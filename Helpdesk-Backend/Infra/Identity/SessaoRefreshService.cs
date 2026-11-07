using Application.Dto.Autenticacao;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enum;
using Domain.Interfaces;
using Domain.Validation;
using Infra.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Infra.Identity
{
    // Gerencia criação, rotação e revogação das sessões de autenticação.
    public class SessaoRefreshService : ISessaoRefreshService
    {
        private const int TamanhoRefreshTokenBytes = 64;

        private readonly IdentityContext _identityContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IusuarioPerfilRepository _usuarioPerfilRepository;
        private readonly IperfilPermissaoRepository _perfilPermissaoRepository;
        private readonly IAuditoriaIdentityService _auditoriaIdentityService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<SessaoRefreshService> _logger;

        public SessaoRefreshService(
            IdentityContext identityContext,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            IusuarioPerfilRepository usuarioPerfilRepository,
            IperfilPermissaoRepository perfilPermissaoRepository,
            IAuditoriaIdentityService auditoriaIdentityService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<SessaoRefreshService> logger)
        {
            _identityContext = identityContext;
            _userManager = userManager;
            _configuration = configuration;
            _usuarioPerfilRepository = usuarioPerfilRepository;
            _perfilPermissaoRepository = perfilPermissaoRepository;
            _auditoriaIdentityService = auditoriaIdentityService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }



        // Cria a sessão inicial após a validação do login.
        public async Task<ResultadoAutenticacaoDto>
            CriarSessaoAsync(
                string usuarioId,
                TipoSessaoAutenticacaoEnum tipo,
                CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (tipo != TipoSessaoAutenticacaoEnum.Normal)
            {
                throw new DomainExceptionValidation(
                    "O tipo de sessão informado ainda não está disponível.");
            }

            var usuario =
                await _userManager.FindByIdAsync(
                    usuarioId);

            if (usuario == null || !usuario.Ativo)
            {
                throw new DomainExceptionValidation(
                    "Não foi possível criar a sessão de autenticação.");
            }

            var agoraUtc =
                DateTime.UtcNow;

            var inatividadeMinutos =
                ObterConfiguracaoPositiva(
                    "NormalInactivityMinutes");

            var refreshTokenExpiraEmUtc =
                agoraUtc.AddMinutes(
                    inatividadeMinutos);

            var refreshToken =
                GerarRefreshToken();

            var tokenHash =
                CalcularHashRefreshToken(
                    refreshToken);

            var sessao =
                new SessaoRefresh(
                    usuario.Id,
                    tokenHash,
                    tipo,
                    agoraUtc,
                    refreshTokenExpiraEmUtc);

            var resposta =
                await GerarAccessTokenAsync(
                    usuario,
                    sessao.Id,
                    tipo,
                    cancellationToken);

            await _identityContext.SessoesRefresh
                .AddAsync(
                    sessao,
                    cancellationToken);

            await _identityContext.SaveChangesAsync(
                cancellationToken);

            return new ResultadoAutenticacaoDto
            {
                Resposta = resposta,
                RefreshToken = refreshToken,
                RefreshTokenExpiraEmUtc =
                    refreshTokenExpiraEmUtc
            };
        }


        // Revoga somente a sessão correspondente ao refresh token informado.
        // A operação é idempotente: se não existir ou já estiver revogada,
        // o logout continua sendo considerado concluído.
        public async Task RevogarSessaoAsync(
            string refreshToken,
            string motivo,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(refreshToken))
                return;

            var tokenHash =
                CalcularHashRefreshToken(
                    refreshToken);

            var sessao =
                await _identityContext.SessoesRefresh
                    .AsNoTracking()
                    .SingleOrDefaultAsync(
                        item =>
                            item.TokenHash == tokenHash,
                        cancellationToken);

            if (sessao == null)
            {
                return;
            }

            var motivoRevogacao =
                string.IsNullOrWhiteSpace(motivo)
                    ? "SESSAO_REVOGADA"
                    : motivo.Trim();

            if (motivoRevogacao.Length > 255)
            {
                motivoRevogacao =
                    motivoRevogacao[..255];
            }

            var sessoesRevogadas =
                await _identityContext.SessoesRefresh
                    .Where(item =>
                        item.Id == sessao.Id &&
                        !item.RevogadaEmUtc.HasValue)
                    .ExecuteUpdateAsync(
                        atualizacao =>
                            atualizacao
                                .SetProperty(
                                    item =>
                                        item.RevogadaEmUtc,
                                    DateTime.UtcNow)
                                .SetProperty(
                                    item =>
                                        item.MotivoRevogacao,
                                    motivoRevogacao),
                        cancellationToken);

            if (sessoesRevogadas == 0)
                return;

            if (motivoRevogacao == "LOGOUT_USUARIO")
            {
                await RegistrarAuditoriaSessaoAsync(
                    sessao.UsuarioId,
                    null,
                    sessao.Id.ToString(),
                    EventoAuditoriaIdentityEnum.Logout,
                    true,
                    motivoRevogacao,
                    new
                    {
                        TipoSessao = sessao.Tipo.ToString()
                    });
            }
        }


        // Revoga todas as sessões ativas de um usuário.
        // Utilizado em alteração de senha, restauração, desativação e exclusão.
        public async Task RevogarSessoesDoUsuarioAsync(
            string usuarioId,
            string motivo,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(usuarioId))
            {
                throw new ArgumentException(
                    "O usuário é obrigatório.",
                    nameof(usuarioId));
            }

            var motivoRevogacao =
                string.IsNullOrWhiteSpace(motivo)
                    ? "SESSOES_DO_USUARIO_REVOGADAS"
                    : motivo.Trim();

            if (motivoRevogacao.Length > 255)
            {
                motivoRevogacao =
                    motivoRevogacao[..255];
            }

            var agoraUtc =
                DateTime.UtcNow;

            var quantidadeSessoesRevogadas =
                await _identityContext.SessoesRefresh
                    .Where(sessao =>
                        sessao.UsuarioId == usuarioId &&
                        !sessao.RevogadaEmUtc.HasValue)
                    .ExecuteUpdateAsync(
                        atualizacao =>
                            atualizacao
                                .SetProperty(
                                    sessao =>
                                        sessao.RevogadaEmUtc,
                                    agoraUtc)
                                .SetProperty(
                                    sessao =>
                                        sessao.MotivoRevogacao,
                                    motivoRevogacao),
                        cancellationToken);

            if (quantidadeSessoesRevogadas == 0)
                return;

            await RegistrarAuditoriaSessaoAsync(
                usuarioId,
                null,
                "NAO_IDENTIFICADA",
                EventoAuditoriaIdentityEnum.RevogacaoSessoes,
                true,
                motivoRevogacao,
                new
                {
                    UsuarioAlvoId = usuarioId,
                    QuantidadeSessoesRevogadas =
                        quantidadeSessoesRevogadas
                });
        }

        // Valida o refresh token atual, utiliza-o uma única vez
        // e cria uma nova sessão com novos tokens.
        public async Task<ResultadoAutenticacaoDto>
            RenovarSessaoNormalAsync(
                string refreshToken,
                CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new AutenticacaoException(
                    "Sessão de autenticação inválida.");
            }

            var tokenHash =
                CalcularHashRefreshToken(
                    refreshToken);

            var agoraUtc =
                DateTime.UtcNow;

            var sessaoAtual =
                await _identityContext.SessoesRefresh
                    .AsNoTracking()
                    .SingleOrDefaultAsync(
                        sessao =>
                            sessao.TokenHash == tokenHash,
                        cancellationToken);

            if (sessaoAtual == null ||
                sessaoAtual.Tipo !=
                    TipoSessaoAutenticacaoEnum.Normal)
            {
                await RegistrarAuditoriaSessaoAsync(
                    sessaoAtual?.UsuarioId,
                    null,
                    sessaoAtual?.Id.ToString() ??
                        "NAO_IDENTIFICADA",
                    EventoAuditoriaIdentityEnum
                        .RenovacaoSessaoFalha,
                    false,
                    "SESSAO_INVALIDA",
                    sessaoAtual == null
                        ? null
                        : new
                        {
                            TipoSessao =
                                sessaoAtual.Tipo.ToString()
                        });

                throw new AutenticacaoException(
                    "Sessão de autenticação inválida.");
            }

            // Uma sessão substituída recebeu uma chave nova anteriormente.
            // Usá-la novamente indica possível reutilização indevida.
            if (sessaoAtual.SubstituidaPorId.HasValue)
            {
                await RevogarCadeiaSubstituicaoAsync(
                    sessaoAtual.Id,
                    "REUTILIZACAO_DE_REFRESH_TOKEN",
                    cancellationToken);

                await RegistrarAuditoriaSessaoAsync(
                    sessaoAtual.UsuarioId,
                    null,
                    sessaoAtual.Id.ToString(),
                    EventoAuditoriaIdentityEnum
                        .ReutilizacaoRefreshToken,
                    false,
                    "REUTILIZACAO_DE_REFRESH_TOKEN",
                    new
                    {
                        TipoSessao =
                            sessaoAtual.Tipo.ToString()
                    });

                throw new AutenticacaoException(
                    "Sessão de autenticação inválida.");
            }

            if (sessaoAtual.RevogadaEmUtc.HasValue)
            {
                await RegistrarAuditoriaSessaoAsync(
                    sessaoAtual.UsuarioId,
                    null,
                    sessaoAtual.Id.ToString(),
                    EventoAuditoriaIdentityEnum
                        .RenovacaoSessaoFalha,
                    false,
                    "SESSAO_REVOGADA",
                    new
                    {
                        TipoSessao =
                            sessaoAtual.Tipo.ToString(),
                        sessaoAtual.RevogadaEmUtc
                    });

                throw new AutenticacaoException(
                    "Sessão de autenticação inválida.");
            }

            if (sessaoAtual.ExpiraEmUtc <= agoraUtc)
            {
                await RevogarSessaoAsync(
                    refreshToken,
                    "SESSAO_EXPIRADA",
                    cancellationToken);

                await RegistrarAuditoriaSessaoAsync(
                    sessaoAtual.UsuarioId,
                    null,
                    sessaoAtual.Id.ToString(),
                    EventoAuditoriaIdentityEnum
                        .RenovacaoSessaoFalha,
                    false,
                    "SESSAO_EXPIRADA",
                    new
                    {
                        TipoSessao =
                            sessaoAtual.Tipo.ToString(),
                        sessaoAtual.ExpiraEmUtc
                    });

                throw new AutenticacaoException(
                    "Sessão de autenticação expirada.");
            }

            var usuario =
                await _userManager.FindByIdAsync(
                    sessaoAtual.UsuarioId);

            if (usuario == null || !usuario.Ativo)
            {
                await RevogarSessoesDoUsuarioAsync(
                    sessaoAtual.UsuarioId,
                    "USUARIO_INATIVO",
                    cancellationToken);

                await RegistrarAuditoriaSessaoAsync(
                    sessaoAtual.UsuarioId,
                    usuario?.UserName,
                    sessaoAtual.Id.ToString(),
                    EventoAuditoriaIdentityEnum
                        .RenovacaoSessaoFalha,
                    false,
                    "USUARIO_INATIVO",
                    new
                    {
                        TipoSessao =
                            sessaoAtual.Tipo.ToString()
                    });

                throw new AutenticacaoException(
                    "Sessão de autenticação inválida.");
            }

            var inatividadeMinutos =
                ObterConfiguracaoPositiva(
                    "NormalInactivityMinutes");

            var novoRefreshToken =
                GerarRefreshToken();

            var novoTokenHash =
                CalcularHashRefreshToken(
                    novoRefreshToken);

            var novaExpiracaoUtc =
                agoraUtc.AddMinutes(
                    inatividadeMinutos);

            var novaSessao =
                new SessaoRefresh(
                    usuario.Id,
                    novoTokenHash,
                    TipoSessaoAutenticacaoEnum.Normal,
                    agoraUtc,
                    novaExpiracaoUtc);

            var resposta =
                await GerarAccessTokenAsync(
                    usuario,
                    novaSessao.Id,
                    TipoSessaoAutenticacaoEnum.Normal,
                    cancellationToken);

            var rotacaoConcluida =
                false;

            await using (
                var transacao =
                    await _identityContext.Database
                        .BeginTransactionAsync(
                            cancellationToken))
            {
                // A atualização condicional garante que duas requisições
                // não consigam utilizar a mesma chave simultaneamente.
                var sessoesAtualizadas =
                    await _identityContext.SessoesRefresh
                        .Where(sessao =>
                            sessao.Id == sessaoAtual.Id &&
                            !sessao.RevogadaEmUtc.HasValue &&
                            !sessao.SubstituidaPorId.HasValue &&
                            sessao.ExpiraEmUtc > agoraUtc)
                        .ExecuteUpdateAsync(
                            atualizacao =>
                                atualizacao
                                    .SetProperty(
                                        sessao =>
                                            sessao.UltimaRenovacaoUtc,
                                        agoraUtc)
                                    .SetProperty(
                                        sessao =>
                                            sessao.SubstituidaPorId,
                                        novaSessao.Id)
                                    .SetProperty(
                                        sessao =>
                                            sessao.RevogadaEmUtc,
                                        agoraUtc)
                                    .SetProperty(
                                        sessao =>
                                            sessao.MotivoRevogacao,
                                        "REFRESH_TOKEN_ROTACIONADO"),
                            cancellationToken);

                if (sessoesAtualizadas == 1)
                {
                    await _identityContext.SessoesRefresh
                        .AddAsync(
                            novaSessao,
                            cancellationToken);

                    await _identityContext.SaveChangesAsync(
                        cancellationToken);

                    await transacao.CommitAsync(
                        cancellationToken);

                    rotacaoConcluida = true;
                }
                else
                {
                    await transacao.RollbackAsync(
                        cancellationToken);
                }
            }

            if (!rotacaoConcluida)
            {
                _identityContext.ChangeTracker.Clear();

                await RevogarCadeiaSubstituicaoAsync(
                    sessaoAtual.Id,
                    "REUTILIZACAO_SIMULTANEA_DE_REFRESH_TOKEN",
                    cancellationToken);

                await RegistrarAuditoriaSessaoAsync(
                    sessaoAtual.UsuarioId,
                    usuario.UserName,
                    sessaoAtual.Id.ToString(),
                    EventoAuditoriaIdentityEnum
                        .ReutilizacaoRefreshToken,
                    false,
                    "REUTILIZACAO_SIMULTANEA_DE_REFRESH_TOKEN",
                    new
                    {
                        TipoSessao =
                            sessaoAtual.Tipo.ToString()
                    });

                throw new AutenticacaoException(
                    "Sessão de autenticação inválida.");
            }

            await RegistrarAuditoriaSessaoAsync(
                usuario.Id,
                usuario.UserName,
                sessaoAtual.Id.ToString(),
                EventoAuditoriaIdentityEnum
                    .RenovacaoSessaoSucesso,
                true,
                "SESSAO_RENOVADA",
                new
                {
                    TipoSessao =
                        sessaoAtual.Tipo.ToString(),
                    NovaSessaoId = novaSessao.Id,
                    ExpiraEmUtc = novaExpiracaoUtc
                });

            return new ResultadoAutenticacaoDto
            {
                Resposta = resposta,
                RefreshToken = novoRefreshToken,
                RefreshTokenExpiraEmUtc =
                    novaExpiracaoUtc
            };
        }










       
        /// //METODO PRIVADOS

        // Registra somente metadados seguros das sessões. Uma falha na
        // auditoria não desfaz uma rotação ou revogação já confirmada.
        private async Task RegistrarAuditoriaSessaoAsync(
            string? usuarioSessaoId,
            string? usuarioSessaoNome,
            string registroId,
            EventoAuditoriaIdentityEnum evento,
            bool sucesso,
            string motivo,
            object? valoresAtuais = null)
        {
            try
            {
                var httpContext =
                    _httpContextAccessor.HttpContext;

                var usuarioAutenticadoId =
                    httpContext?
                        .User?
                        .FindFirst(
                            ClaimTypes.NameIdentifier)?
                        .Value;

                var usuarioAutenticadoNome =
                    httpContext?
                        .User?
                        .FindFirst(
                            ClaimTypes.Name)?
                        .Value;

                if (string.IsNullOrWhiteSpace(
                        usuarioSessaoNome) &&
                    !string.IsNullOrWhiteSpace(
                        usuarioSessaoId))
                {
                    var usuarioSessao =
                        await _userManager.FindByIdAsync(
                            usuarioSessaoId);

                    usuarioSessaoNome =
                        usuarioSessao?.UserName;
                }

                var origem =
                    !string.IsNullOrWhiteSpace(
                        usuarioAutenticadoId)
                        ? OrigemAuditoriaEnum.Usuario
                        : httpContext == null
                            ? OrigemAuditoriaEnum.Sistema
                            : OrigemAuditoriaEnum.Anonimo;

                var usuarioId =
                    !string.IsNullOrWhiteSpace(
                        usuarioAutenticadoId)
                        ? usuarioAutenticadoId
                        : origem ==
                            OrigemAuditoriaEnum.Anonimo
                            ? usuarioSessaoId
                            : null;

                var usuarioNome =
                    !string.IsNullOrWhiteSpace(
                        usuarioAutenticadoNome)
                        ? usuarioAutenticadoNome
                        : origem ==
                            OrigemAuditoriaEnum.Sistema
                            ? "SISTEMA"
                            : !string.IsNullOrWhiteSpace(
                                usuarioSessaoNome)
                                ? usuarioSessaoNome
                                : "USUARIO_DESCONHECIDO";

                var traceId =
                    httpContext?.TraceIdentifier;

                if (string.IsNullOrWhiteSpace(traceId))
                {
                    traceId =
                        Guid.NewGuid().ToString("N");
                }

                var auditoria =
                    new AuditoriaIdentity(
                        LimitarTexto(
                            usuarioId,
                            450),
                        LimitarTexto(
                            usuarioNome,
                            255)!,
                        evento,
                        origem,
                        nameof(SessaoRefresh),
                        LimitarTexto(
                            registroId,
                            450)!,
                        sucesso,
                        LimitarTexto(
                            motivo,
                            255)!,
                        null,
                        valoresAtuais == null
                            ? null
                            : JsonSerializer.Serialize(
                                valoresAtuais),
                        LimitarTexto(
                            httpContext?
                                .Connection
                                .RemoteIpAddress?
                                .ToString(),
                            45),
                        LimitarTexto(
                            httpContext?
                                .Request
                                .Method,
                            10),
                        LimitarTexto(
                            httpContext?
                                .Request
                                .Path
                                .Value,
                            500),
                        LimitarTexto(
                            httpContext?
                                .Request
                                .Headers
                                .UserAgent
                                .ToString(),
                            1000),
                        LimitarTexto(
                            traceId,
                            200)!);

                await _auditoriaIdentityService
                    .Adicionar(auditoria);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Falha ao registrar auditoria {Evento} da sessão {RegistroId}.",
                    evento,
                    registroId);
            }
        }

        // Respeita os limites definidos no mapeamento da auditoria sem
        // permitir que dados enviados pelo cliente impeçam sua gravação.
        private static string? LimitarTexto(
            string? valor,
            int tamanhoMaximo)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return null;

            var valorNormalizado =
                valor.Trim();

            return valorNormalizado.Length <= tamanhoMaximo
                ? valorNormalizado
                : valorNormalizado[..tamanhoMaximo];
        }
      
        // Recarrega funções, perfis e permissões para que cada novo
        // access token represente o estado atual do usuário.
        private async Task<List<Claim>> MontarClaimsAsync(
            ApplicationUser usuario,
            Guid sessaoId,
            TipoSessaoAutenticacaoEnum tipoSessao,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var roles =
                await _userManager.GetRolesAsync(usuario);

            var perfis =
                await _usuarioPerfilRepository
                    .BuscarPerfisUsuario(usuario.Id);

            var claims = new List<Claim>
            {
                new(
                    ClaimTypes.Email,
                    usuario.Email ?? string.Empty),

                new(
                    ClaimTypes.Name,
                    usuario.UserName ?? string.Empty),

                new(
                    ClaimTypes.NameIdentifier,
                    usuario.Id),

                new(
                    "DeveTrocarSenha",
                    usuario.DeveTrocarSenha
                        .ToString()
                        .ToLowerInvariant()),

                new(
                    "SessaoId",
                    sessaoId.ToString()),

                new(
                    "TipoSessao",
                    tipoSessao.ToString())
            };

            claims.AddRange(
                roles.Select(role =>
                    new Claim(
                        ClaimTypes.Role,
                        role)));

            foreach (var perfil in perfis)
            {
                cancellationToken.ThrowIfCancellationRequested();

                claims.Add(
                    new Claim(
                        "PerfilId",
                        perfil.Id_Perfil.ToString()));

                claims.Add(
                    new Claim(
                        "Perfil",
                        perfil.Nome));

                var permissoes =
                    await _perfilPermissaoRepository
                        .BuscarPermissoesPerfil(
                            perfil.Id_Perfil);

                foreach (var permissao in permissoes)
                {
                    var permissaoJaAdicionada =
                        claims.Any(claim =>
                            claim.Type == "Permissao" &&
                            claim.Value == permissao.Nome);

                    if (!permissaoJaAdicionada)
                    {
                        claims.Add(
                            new Claim(
                                "Permissao",
                                permissao.Nome));
                    }
                }
            }

            return claims;
        }

        // Gera uma chave aleatória e segura para ser armazenada
        // no cookie HttpOnly do navegador.
        private static string GerarRefreshToken()
        {
            var bytes =
                RandomNumberGenerator.GetBytes(
                    TamanhoRefreshTokenBytes);

            return WebEncoders.Base64UrlEncode(bytes);
        }

        // Transforma o refresh token em um hash SHA-256.
        // Somente esse hash será armazenado no banco.
        private static string CalcularHashRefreshToken(
            string refreshToken)
        {
            var bytes =
                Encoding.UTF8.GetBytes(refreshToken);

            var hash =
                SHA256.HashData(bytes);

            return Convert.ToHexString(hash);
        }

        // Lê uma configuração numérica obrigatória da seção Jwt
        // e impede que a aplicação utilize uma duração inválida.
        private int ObterConfiguracaoPositiva(
            string nomeConfiguracao)
        {
            var valor =
                _configuration.GetValue<int?>(
                    $"Jwt:{nomeConfiguracao}");

            if (!valor.HasValue || valor.Value <= 0)
            {
                throw new InvalidOperationException(
                    $"A configuração Jwt:{nomeConfiguracao} deve possuir um valor maior que zero.");
            }

            return valor.Value;
        }

        // Gera o access token de curta duração que será utilizado
        // nas requisições HTTP e na conexão SignalR.
        private async Task<AutenticacaoResponseDto>
            GerarAccessTokenAsync(
                ApplicationUser usuario,
                Guid sessaoId,
                TipoSessaoAutenticacaoEnum tipoSessao,
                CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var chaveJwt =
                _configuration["Jwt:Key"];

            var emissor =
                _configuration["Jwt:Issuer"];

            var destinatario =
                _configuration["Jwt:Audience"];

            if (string.IsNullOrWhiteSpace(chaveJwt) ||
                chaveJwt.Length < 32 ||
                string.IsNullOrWhiteSpace(emissor) ||
                string.IsNullOrWhiteSpace(destinatario))
            {
                throw new InvalidOperationException(
                    "As configurações do JWT estão incompletas.");
            }

            var duracaoMinutos =
                ObterConfiguracaoPositiva(
                    "AccessTokenExpirationMinutes");

            var agoraUtc =
                DateTime.UtcNow;

            var expiraEmUtc =
                agoraUtc.AddMinutes(
                    duracaoMinutos);

            var claims =
                await MontarClaimsAsync(
                    usuario,
                    sessaoId,
                    tipoSessao,
                    cancellationToken);

            var chave =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        chaveJwt));

            var credenciais =
                new SigningCredentials(
                    chave,
                    SecurityAlgorithms.HmacSha256);

            var token =
                new JwtSecurityToken(
                    issuer: emissor,
                    audience: destinatario,
                    claims: claims,
                    notBefore: agoraUtc,
                    expires: expiraEmUtc,
                    signingCredentials: credenciais);

            return new AutenticacaoResponseDto
            {
                AccessToken =
                    new JwtSecurityTokenHandler()
                        .WriteToken(token),

                AccessTokenExpiraEmUtc =
                    expiraEmUtc,

                ModoPainel =
                    tipoSessao ==
                    TipoSessaoAutenticacaoEnum.Painel
            };
        }

        // Percorre as sessões geradas pela rotação de um refresh token
        // reutilizado e revoga qualquer sessão posterior ainda ativa.
        private async Task RevogarCadeiaSubstituicaoAsync(
            Guid sessaoInicialId,
            string motivo,
            CancellationToken cancellationToken)
        {
            var sessaoAtualId =
                (Guid?)sessaoInicialId;

            var sessoesVisitadas =
                new HashSet<Guid>();

            var agoraUtc =
                DateTime.UtcNow;

            while (sessaoAtualId.HasValue &&
                   sessoesVisitadas.Add(
                       sessaoAtualId.Value))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var sessao =
                    await _identityContext.SessoesRefresh
                        .SingleOrDefaultAsync(
                            item =>
                                item.Id ==
                                sessaoAtualId.Value,
                            cancellationToken);

                if (sessao == null)
                    break;

                var proximaSessaoId =
                    sessao.SubstituidaPorId;

                if (!sessao.RevogadaEmUtc.HasValue)
                {
                    sessao.Revogar(
                        agoraUtc,
                        motivo);
                }

                sessaoAtualId =
                    proximaSessaoId;
            }

            await _identityContext.SaveChangesAsync(
                cancellationToken);
        }

    }
}
