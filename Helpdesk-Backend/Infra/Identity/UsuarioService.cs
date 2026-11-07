using Application.Dto.Autenticacao;
using Application.Dto.UsuarioDto;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enum;
using Domain.Interfaces;
using Domain.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;

namespace Infra.Identity
{
    // Coordena usuários do Identity, autenticação, JWT, senhas, perfis,
    // permissões e auditorias de segurança.
    public class UsuarioService : IusuarioService
    {
        private readonly UserManager<ApplicationUser> _applicationUser;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IusuarioPerfilRepository _usuarioPerfilRepository;
        private readonly IperfilPermissaoRepository _perfilPermissaoRepository;
        private readonly IValidator<CreateUserDto> _createUserValidator;
        private readonly IValidator<LoginDto> _loginValidator;
        private readonly IValidator<ChangePasswordDto> _changePasswordValidator;
        private readonly IAuditoriaIdentityService _auditoriaIdentityService;
        private readonly IEnviarEmail _enviarEmail;
        private readonly ISessaoRefreshService _sessaoRefreshService;

        private const string CaracteresSenhaTemporaria =
            "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@#$%*_-+";

        public UsuarioService(
            UserManager<ApplicationUser> applicationUser,
            SignInManager<ApplicationUser> signInManager,
            IusuarioPerfilRepository usuarioPerfilRepository,
            IperfilPermissaoRepository perfilPermissaoRepository,
            IValidator<CreateUserDto> createUserValidator,
            IValidator<LoginDto> loginValidator,
            IValidator<ChangePasswordDto> changePasswordValidator,
            IHttpContextAccessor httpContextAccessor,
            IEnviarEmail enviarEmail,
            IAuditoriaIdentityService auditoriaIdentityService,
            ISessaoRefreshService sessaoRefreshService)
        {
            _applicationUser = applicationUser;
            _signInManager = signInManager;
            _usuarioPerfilRepository = usuarioPerfilRepository;
            _perfilPermissaoRepository = perfilPermissaoRepository;
            _createUserValidator = createUserValidator;
            _loginValidator = loginValidator;
            _changePasswordValidator = changePasswordValidator;
            _httpContextAccessor = httpContextAccessor;
            _enviarEmail = enviarEmail;
            _auditoriaIdentityService = auditoriaIdentityService;
            _sessaoRefreshService = sessaoRefreshService;
        }

        // SENHAS

        // Altera a senha com a credencial atual ou, no fluxo administrativo,
        // utiliza um token de redefinição.
        public async Task<string> ChangePassword(
            string userIdLogado,
            ChangePasswordDto dto,
            bool isAdmin)
        {
            await _changePasswordValidator
                .ValidateAndThrowAsync(dto);

            var user =
                await _applicationUser.FindByIdAsync(
                    userIdLogado);

            if (user == null)
            {
                throw new DomainExceptionValidation(
                    "Usuário não encontrado");
            }

            IdentityResult result;

            user.ConcluirTrocaSenha();

            if (!isAdmin)
            {
                result =
                    await _applicationUser
                        .ChangePasswordAsync(
                            user,
                            dto.PasswordAtual,
                            dto.NovaSenha);
            }
            else
            {
                var token =
                    await _applicationUser
                        .GeneratePasswordResetTokenAsync(
                            user);

                result =
                    await _applicationUser
                        .ResetPasswordAsync(
                            user,
                            token,
                            dto.NovaSenha);
            }

            ValidarResultadoIdentity(result);

            await _sessaoRefreshService
                .RevogarSessoesDoUsuarioAsync(
                    user.Id,
                    "SENHA_ALTERADA");

            await RegistrarAuditoriaUsuario(
                user,
                isAdmin
                    ? EventoAuditoriaIdentityEnum
                        .RedefinicaoSenha
                    : EventoAuditoriaIdentityEnum
                        .AlteracaoSenha,
                isAdmin
                    ? "SENHA_REDEFINIDA"
                    : "SENHA_ALTERADA");

            return "Senha alterada com sucesso";
        }

        // CONSULTAS

        // Consulta interna usada por outros serviços, sem validação do usuário
        // que iniciou a requisição.
        public async Task<UsuarioReponseDto> BuscarPorIdInterno(string id)
        {
            var usuario =
                await _applicationUser.FindByIdAsync(id);

            if (usuario == null)
                return null;

            var perfis =
                await _usuarioPerfilRepository.BuscarPerfisUsuario(
                    usuario.Id);

            var perfil = perfis.FirstOrDefault();

            var usuarioresponse = new UsuarioReponseDto()
            {
                Id = usuario.Id,
                Username = usuario.UserName,
                Email = usuario.Email,
                Ativo = usuario.Ativo,
                Id_Perfil = perfil?.Id_Perfil ?? 0
            };

            return usuarioresponse;
        }

        public async Task<UsuarioReponseDto> BuscarPorId(
            string id,
            string usuarioLogadoId,
            bool podeGerenciarUsuarios)
        {
            if (id != usuarioLogadoId && !podeGerenciarUsuarios)
                throw new AcessoNegadoException(
                    "Você não possui acesso aos dados deste usuário.");

            return await BuscarPorIdInterno(id);
        }

        public async Task<IEnumerable<UsuarioReponseDto>> BuscarTodos()
        {
            return await _applicationUser.Users
                .Select(u => new UsuarioReponseDto
                {
                    Id = u.Id,
                    Username = u.UserName!,
                    Email = u.Email!,
                    Ativo = u.Ativo
                })
                .ToListAsync();
        }

        // CADASTRO E ALTERAÇÃO

        public async Task<CreateUserDto> CriarUsuario(
            CreateUserDto createDTO)
        {
            await _createUserValidator
                .ValidateAndThrowAsync(createDTO);

            var user = new ApplicationUser
            {
                UserName = createDTO.Username,
                Email = createDTO.Email,
                Ativo = true
            };

            var result =
                await _applicationUser.CreateAsync(
                    user,
                    createDTO.Password);

            ValidarResultadoIdentity(result);

            await RegistrarAuditoriaUsuario(
                user,
                EventoAuditoriaIdentityEnum.CriacaoUsuario,
                "USUARIO_CRIADO",
                null,
                SerializarUsuario(user));

            return new CreateUserDto
            {
                Id = user.Id,
                Username = user.UserName!,
                Email = user.Email!,
                Password = string.Empty,
                PasswordConfirmation = string.Empty,
                Ativo = user.Ativo
            };
        }

        public async Task<UpdateUserDto> Atualizar(
            string id,
            UpdateUserDto updateUserDto)
        {
            var usuario =
                await _applicationUser.FindByIdAsync(id);

            if (usuario == null)
            {
                throw new DomainExceptionValidation(
                    "Usuario não encontrado.");
            }

            var valoresAnteriores =
                SerializarUsuario(usuario);

            var ativoAnterior =
                usuario.Ativo;

            usuario.Atualizar(
                updateUserDto.Username,
                updateUserDto.Email,
                updateUserDto.Ativo);

            var result =
                await _applicationUser.UpdateAsync(
                    usuario);

            ValidarResultadoIdentity(result);

            if (ativoAnterior && !usuario.Ativo)
            {
                await _sessaoRefreshService
                    .RevogarSessoesDoUsuarioAsync(
                        usuario.Id,
                        "USUARIO_DESATIVADO");
            }

            var evento =
                ativoAnterior != usuario.Ativo
                    ? usuario.Ativo
                        ? EventoAuditoriaIdentityEnum
                            .AtivacaoUsuario
                        : EventoAuditoriaIdentityEnum
                            .DesativacaoUsuario
                    : EventoAuditoriaIdentityEnum
                        .AlteracaoUsuario;

            var motivo =
                ativoAnterior != usuario.Ativo
                    ? usuario.Ativo
                        ? "USUARIO_ATIVADO"
                        : "USUARIO_DESATIVADO"
                    : "USUARIO_ALTERADO";

            await RegistrarAuditoriaUsuario(
                usuario,
                evento,
                motivo,
                valoresAnteriores,
                SerializarUsuario(usuario));

            return new UpdateUserDto
            {
                Id = usuario.Id,
                Username = usuario.UserName!,
                Email = usuario.Email!,
                Ativo = usuario.Ativo
            };
        }

        // AUTENTICAÇÃO E TOKEN

        // Valida as credenciais, registra a tentativa e cria
        // a sessão renovável do usuário.
        public async Task<ResultadoAutenticacaoDto> Login(LoginDto loginDTO,CancellationToken cancellationToken = default)
        {
            await _loginValidator.ValidateAndThrowAsync(
                loginDTO,
                cancellationToken);

            var user =
                await _applicationUser.FindByNameAsync(
                    loginDTO.UserName);

            if (user == null)
            {
                await RegistrarAuditoriaLogin(
                    null,
                    loginDTO.UserName,
                    EventoAuditoriaIdentityEnum.LoginFalha,
                    false,
                    "CREDENCIAIS_INVALIDAS");

                throw new DomainExceptionValidation(
                    "Usuário ou senha inválidos");
            }

            if (!user.Ativo)
            {
                await RegistrarAuditoriaLogin(
                    user,
                    loginDTO.UserName,
                    EventoAuditoriaIdentityEnum.LoginFalha,
                    false,
                    "USUARIO_INATIVO");

                throw new DomainExceptionValidation(
                    "Usuário ou senha inválidos");
            }

            var result =
                await _signInManager.CheckPasswordSignInAsync(
                    user,
                    loginDTO.Password,
                    true);

            if (result.IsLockedOut)
            {
                await RegistrarAuditoriaLogin(
                    user,
                    loginDTO.UserName,
                    EventoAuditoriaIdentityEnum.LoginBloqueado,
                    false,
                    "USUARIO_BLOQUEADO");

                throw new DomainExceptionValidation(
                    "Muitas tentativas inválidas. Tente novamente após 10 minutos.");
            }

            if (!result.Succeeded)
            {
                await RegistrarAuditoriaLogin(
                    user,
                    loginDTO.UserName,
                    EventoAuditoriaIdentityEnum.LoginFalha,
                    false,
                    "CREDENCIAIS_INVALIDAS");

                throw new DomainExceptionValidation(
                    "Usuário ou senha inválidos");
            }

            var resultado =
                await _sessaoRefreshService.CriarSessaoAsync(
                    user.Id,
                    TipoSessaoAutenticacaoEnum.Normal,
                    cancellationToken);

            await RegistrarAuditoriaLogin(
                user,
                loginDTO.UserName,
                EventoAuditoriaIdentityEnum.LoginSucesso,
                true,
                "LOGIN_REALIZADO");

            return resultado;
        }

        // REMOÇÃO

        public async Task<CreateUserDto?> Excluir(string id)
        {
            var user =
                await _applicationUser.FindByIdAsync(id);

            if (user == null)
            {
                throw new DomainExceptionValidation(
                    "Usuário não encontrado");
            }

            var valoresAnteriores =
                SerializarUsuario(user);

            var usuarioDTO = new CreateUserDto
            {
                Id = user.Id,
                Username = user.UserName!,
                Email = user.Email!,
                Password = string.Empty,
                PasswordConfirmation = string.Empty,
                Ativo = user.Ativo
            };

            var result =
                await _applicationUser.DeleteAsync(user);

            ValidarResultadoIdentity(result);

            await RegistrarAuditoriaUsuario(
                user,
                EventoAuditoriaIdentityEnum.ExclusaoUsuario,
                "USUARIO_EXCLUIDO",
                valoresAnteriores,
                null);

            return usuarioDTO;
        }
        // ESTADO DO USUÁRIO

        public async Task Ativar(string id)
        {
            var usuario =
                await _applicationUser.FindByIdAsync(id);

            if (usuario == null)
            {
                throw new DomainExceptionValidation(
                    "Usuario de atendimento não encontrado.");
            }

            var valoresAnteriores =
                SerializarUsuario(usuario);

            usuario.Ativar();

            var result =
                await _applicationUser.UpdateAsync(
                    usuario);

            ValidarResultadoIdentity(result);

            await RegistrarAuditoriaUsuario(
                usuario,
                EventoAuditoriaIdentityEnum.AtivacaoUsuario,
                "USUARIO_ATIVADO",
                valoresAnteriores,
                SerializarUsuario(usuario));
        }

        public async Task Desativar(string id)
        {
            var usuario =
                await _applicationUser.FindByIdAsync(id);

            if (usuario == null)
            {
                throw new DomainExceptionValidation(
                    "Usuario de atendimento não encontrado.");
            }

            var valoresAnteriores =
                SerializarUsuario(usuario);

            usuario.Desativar();

            var result =
                await _applicationUser.UpdateAsync(
                    usuario);

            ValidarResultadoIdentity(result);

            await _sessaoRefreshService
                    .RevogarSessoesDoUsuarioAsync(
                     usuario.Id,
                    "USUARIO_DESATIVADO");

            await RegistrarAuditoriaUsuario(
                usuario,
                EventoAuditoriaIdentityEnum.DesativacaoUsuario,
                "USUARIO_DESATIVADO",
                valoresAnteriores,
                SerializarUsuario(usuario));
        }

        public async Task<UsuarioLogadoDto> ObterUsuarioLogado()
        {
            var usuarioId = _httpContextAccessor.HttpContext?
                .User?
                .FindFirst(ClaimTypes.NameIdentifier)?
                .Value;

            if (string.IsNullOrEmpty(usuarioId))
                throw new DomainExceptionValidation("Usuário não autenticado.");

            var usuario = await _applicationUser.FindByIdAsync(usuarioId);

            if (usuario == null)
                throw new DomainExceptionValidation("Usuário não encontrado.");

            var perfilId = _httpContextAccessor.HttpContext?
                .User?
                .FindFirst("PerfilId")?
                .Value;

            var perfil = _httpContextAccessor.HttpContext?
                .User?
                .FindFirst("Perfil")?
                .Value;

            var permissoes = _httpContextAccessor.HttpContext?
                .User?
                .FindAll("Permissao")
                .Select(x => x.Value)
                .ToList() ?? new List<string>();

            return new UsuarioLogadoDto
            {
                Id = usuario.Id,
                Username = usuario.UserName ?? string.Empty,
                Email = usuario.Email ?? string.Empty,
                PerfilId = int.TryParse(perfilId, out var id) ? id : 0,
                Perfil = perfil ?? string.Empty,
                Permissoes = permissoes
            };
        }

        // LISTAS AUXILIARES

        // Retorna usuários ativos que não possuem permissão de solicitante.
        public async Task<List<UsuarioResumoDto>> BuscarUsuariosTecnicos()
        {
            var usuarios = await BuscarTodos();

            var usuariosTransferencia = new List<UsuarioResumoDto>();

            foreach (var usuario in usuarios)
            {
                if (usuario == null)
                    continue;

                var perfis = await _usuarioPerfilRepository
                    .BuscarPerfisUsuario(usuario.Id);

                bool podeAbrirChamado = false;

                foreach (var perfil in perfis)
                {
                    var permissao = await _perfilPermissaoRepository
                        .BuscarPermissoesPerfil(perfil.Id_Perfil);

                    if (permissao.Any(x =>
                        x.Nome == "ABRIR_CHAMADO"))
                    {
                        podeAbrirChamado = true;
                        break;
                    }
                }

                if (!podeAbrirChamado && usuario.Ativo)
                {
                    usuariosTransferencia.Add(MapearResumo(usuario));
                }
            }

            return usuariosTransferencia;
        }

        // Inclui destinos que não são solicitantes e preserva administradores.
        public async Task<List<UsuarioResumoDto>> BuscarUsuariosDistribuicao()
        {
            var usuarios = await BuscarTodos();
            var usuariosDestino = new List<UsuarioResumoDto>();

            foreach (var usuario in usuarios)
            {
                var perfis = await _usuarioPerfilRepository
                    .BuscarPerfisUsuario(usuario.Id);

                var ehAdministrador = perfis.Any(perfil =>
                    perfil.Nome?.Contains(
                        "administrador",
                        StringComparison.OrdinalIgnoreCase) == true);

                var podeAbrirChamado = false;

                foreach (var perfil in perfis)
                {
                    var permissoes = await _perfilPermissaoRepository
                        .BuscarPermissoesPerfil(perfil.Id_Perfil);

                    if (permissoes.Any(x => x.Nome == "ABRIR_CHAMADO"))
                    {
                        podeAbrirChamado = true;
                        break;
                    }
                }

                if (!podeAbrirChamado || ehAdministrador)
                    usuariosDestino.Add(MapearResumo(usuario));
            }

            return usuariosDestino;
        }

        public async Task<List<UsuarioResumoDto>> BuscarUsuariosHistorico()
        {
            var usuarios = await BuscarTodos();

            return usuarios.Select(MapearResumo).ToList();
        }

        // MÉTODOS INTERNOS

        private static UsuarioResumoDto MapearResumo(UsuarioReponseDto usuario)
        {
            return new UsuarioResumoDto
            {
                Id = usuario.Id,
                Username = usuario.Username,
                Ativo = usuario.Ativo
            };
        }


        // Registra tentativas anônimas e autenticadas com os dados técnicos da
        // requisição atual.
        private async Task RegistrarAuditoriaLogin(
            ApplicationUser? usuario,
            string usuarioInformado,
            EventoAuditoriaIdentityEnum evento,
            bool sucesso,
            string motivo)
        {
            var httpContext =
                _httpContextAccessor.HttpContext;

            var usuarioNome =
                usuario?.UserName ??
                usuarioInformado;

            var registroId =
                usuario?.Id ??
                usuarioInformado;

            var traceId =
                httpContext?.TraceIdentifier;

            if (string.IsNullOrWhiteSpace(traceId))
            {
                traceId =
                    Guid.NewGuid().ToString("N");
            }

            var auditoria =
                new AuditoriaIdentity(
                    usuario?.Id,
                    usuarioNome,
                    evento,
                    OrigemAuditoriaEnum.Anonimo,
                    nameof(ApplicationUser),
                    registroId,
                    sucesso,
                    motivo,
                    null,
                    null,
                    httpContext?
                        .Connection
                        .RemoteIpAddress?
                        .ToString(),
                    httpContext?
                        .Request
                        .Method,
                    httpContext?
                        .Request
                        .Path
                        .Value,
                    httpContext?
                        .Request
                        .Headers
                        .UserAgent
                        .ToString(),
                    traceId);

            await _auditoriaIdentityService
                .Adicionar(auditoria);
        }

        // Registra alterações administrativas preservando os valores anterior
        // e atual quando eles forem fornecidos.
        private async Task RegistrarAuditoriaUsuario(
            ApplicationUser usuarioAlvo,
            EventoAuditoriaIdentityEnum evento,
            string motivo,
            string? valoresAnteriores = null,
            string? valoresAtuais = null)
        {
            var httpContext =
                _httpContextAccessor.HttpContext;

            var usuarioId =
                httpContext?
                    .User?
                    .FindFirst(
                        ClaimTypes.NameIdentifier)?
                    .Value;

            var usuarioNome =
                httpContext?
                    .User?
                    .FindFirst(
                        ClaimTypes.Name)?
                    .Value;

            var origem =
                string.IsNullOrWhiteSpace(usuarioId)
                    ? OrigemAuditoriaEnum.Sistema
                    : OrigemAuditoriaEnum.Usuario;

            if (string.IsNullOrWhiteSpace(usuarioNome))
            {
                usuarioNome =
                    origem == OrigemAuditoriaEnum.Sistema
                        ? "SISTEMA"
                        : "USUARIO_DESCONHECIDO";
            }

            var traceId =
                httpContext?.TraceIdentifier;

            if (string.IsNullOrWhiteSpace(traceId))
            {
                traceId =
                    Guid.NewGuid().ToString("N");
            }

            var auditoria =
                new AuditoriaIdentity(
                    usuarioId,
                    usuarioNome,
                    evento,
                    origem,
                    nameof(ApplicationUser),
                    usuarioAlvo.Id,
                    true,
                    motivo,
                    valoresAnteriores,
                    valoresAtuais,
                    httpContext?
                        .Connection
                        .RemoteIpAddress?
                        .ToString(),
                    httpContext?
                        .Request
                        .Method,
                    httpContext?
                        .Request
                        .Path
                        .Value,
                    httpContext?
                        .Request
                        .Headers
                        .UserAgent
                        .ToString(),
                    traceId);

            await _auditoriaIdentityService
                .Adicionar(auditoria);
        }

        // Cria a fotografia segura utilizada na auditoria do usuário.
        private static string SerializarUsuario(
            ApplicationUser usuario)
        {
            return JsonSerializer.Serialize(
                new
                {
                    UsuarioNome = usuario.UserName,
                    usuario.Email,
                    usuario.Ativo
                });
        }

        private static void ValidarResultadoIdentity(IdentityResult result)
        {
            if (result.Succeeded)
            {
                return;
            }

            var erros =
                string.Join(
                    " | ",
                    result.Errors.Select(
                        erro => erro.Description));

            throw new DomainExceptionValidation(erros);
        }

        // RECUPERAÇÃO DE SENHA

        // Mantém resposta silenciosa quando o usuário não pode ser recuperado,
        // evitando revelar contas existentes.
        public async Task RecuperarSenha(RecuperarSenhaDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.UserName))
            {
                return;
            }

            var usuario = await _applicationUser.FindByNameAsync(
                dto.UserName.Trim());

            if (usuario == null ||
                !usuario.Ativo ||
                string.IsNullOrWhiteSpace(usuario.Email))
            {
                return;
            }

            var senhaTemporaria = GerarSenhaTemporaria();

            var token = await _applicationUser
                .GeneratePasswordResetTokenAsync(usuario);

            usuario.ExigirTrocaSenha();

            var resultado = await _applicationUser.ResetPasswordAsync(
                usuario,
                token,
                senhaTemporaria);

            ValidarResultadoIdentity(resultado);

            await _sessaoRefreshService
                .RevogarSessoesDoUsuarioAsync(
                 usuario.Id,
                "SENHA_TEMPORARIA_GERADA");

            await RegistrarAuditoriaUsuario(
                usuario,
                EventoAuditoriaIdentityEnum.RedefinicaoSenha,
                "SENHA_TEMPORARIA_ENVIADA_POR_EMAIL");

            var email = _enviarEmail.CriarEmailRecuperacaoSenha(
                usuario.Email,
                usuario.UserName ?? dto.UserName,
                senhaTemporaria);

            await _enviarEmail.EnviarEmail(email);
        }

        public async Task<string> RestaurarSenhaPadrao(string usuarioId)
        {
            var usuario = await _applicationUser
                .FindByIdAsync(usuarioId);

            if (usuario == null)
            {
                throw new DomainExceptionValidation(
                    "Usuário não encontrado");
            }

            var token = await _applicationUser
                .GeneratePasswordResetTokenAsync(usuario);

            usuario.ExigirTrocaSenha();

            var resultado = await _applicationUser
                .ResetPasswordAsync(
                    usuario,
                    token,
                    ConfiguracaoSenha.SenhaPadrao);

            ValidarResultadoIdentity(resultado);

            await _sessaoRefreshService
                    .RevogarSessoesDoUsuarioAsync(
                     usuario.Id,
                     "SENHA_PADRAO_RESTAURADA");

            await RegistrarAuditoriaUsuario(
                usuario,
                EventoAuditoriaIdentityEnum.RedefinicaoSenha,
                "SENHA_PADRAO_RESTAURADA");

            return "Senha restaurada para o padrão com sucesso";
        }

        // Gera uma senha temporária com prefixo que satisfaz a política e uma
        // parte aleatória criptograficamente segura.
        private static string GerarSenhaTemporaria()
        {
            var parteAleatoria = RandomNumberGenerator.GetString(
                CaracteresSenhaTemporaria,
                12);

            return $"Aa1!{parteAleatoria}";
        }
    }
}
