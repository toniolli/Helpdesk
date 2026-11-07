using Application.Dto.CategoriaAtendimentoDto;
using Application.Dto.Paginacao;
using Application.Dto.UsuarioDto;
using Application.Interfaces;
using Domain.Validation;
using Helpdesk.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace Helpdesk.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    /// <summary>
    /// Centraliza consultas e operações de conta dos usuários.
    /// </summary>
    public class UsuarioController : ControllerBase
    {
        private readonly IusuarioService _usuarioService;
        private const string CookieRefreshNormal = "helpdesk_refresh_normal";
        private readonly ISessaoRefreshService _sessaoRefreshService;
        private readonly IConfiguration _configuration;

        public UsuarioController(
            IusuarioService usuarioService,
            ISessaoRefreshService sessaoRefreshService,
            IConfiguration configuration)
        {
            _usuarioService = usuarioService;
            _sessaoRefreshService = sessaoRefreshService;
            _configuration = configuration;
        }

      
        // CONSULTAS

        [HttpGet]
        [Permissao("GERENCIAR_USUARIOS")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<UsuarioReponseDto>))]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<UsuarioReponseDto>>> BuscarTodos()
        {
            var usuarios = await _usuarioService.BuscarTodos();

            if (usuarios == null || !usuarios.Any())
                return NotFound("Nenhum usuário encontrado");

            return Ok(usuarios);
        }

        [HttpGet("usuarios-transferencia")]
        [Permissao("GERENCIAR_USUARIOS", "ASSUMIR_CHAMADO")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<UsuarioResumoDto>))]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<UsuarioResumoDto>>> BuscarUsuariosTecnicos()
        {
            var usuarios = await _usuarioService.BuscarUsuariosTecnicos();

            if (usuarios == null || !usuarios.Any())
                return NotFound("Nenhum usuário encontrado para transferência.");

            return Ok(usuarios);
        }

        [HttpGet("usuarios-distribuicao")]
        [Permissao("GERENCIAR_DISTRIBUICAO")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<UsuarioResumoDto>))]
        public async Task<ActionResult<IEnumerable<UsuarioResumoDto>>>
            BuscarUsuariosDistribuicao()
        {
            var usuarios = await _usuarioService.BuscarUsuariosDistribuicao();
            return Ok(usuarios);
        }

        [HttpGet("usuarios-historico")]
        [Permissao("VISUALIZAR_TODOS_CHAMADOS")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<UsuarioResumoDto>))]
        public async Task<ActionResult<IEnumerable<UsuarioResumoDto>>>
            BuscarUsuariosHistorico()
        {
            var usuarios = await _usuarioService.BuscarUsuariosHistorico();
            return Ok(usuarios);
        }

        [HttpGet("usuario/{id}")]
        [Permissao(
            "GERENCIAR_USUARIOS",
             "ABRIR_CHAMADO",
            "ASSUMIR_CHAMADO"
        )]
        [ProducesResponseType(200, Type = typeof(UsuarioReponseDto))]
        [ProducesResponseType(404)]
        public async Task<ActionResult<UsuarioReponseDto>> BuscarPorId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Usuário inválido.");

            var usuarioLogadoId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(usuarioLogadoId))
                return Unauthorized();

            var podeGerenciarUsuarios =
                User.HasClaim("Permissao", "GERENCIAR_USUARIOS");

            var usuarios = await _usuarioService.BuscarPorId(
                id,
                usuarioLogadoId,
                podeGerenciarUsuarios);

            if (usuarios == null)   
                return NotFound("Nenhum usuário encontrado");

            return Ok(usuarios);
        }

        // AÇÕES

        // Cadastro e autenticação

        [HttpPost("registrar")]
        [Permissao("GERENCIAR_USUARIOS")]
        [ProducesResponseType(200, Type = typeof(CreateUserDto))]
        [ProducesResponseType(400)]
        public async Task<ActionResult> CriarUsuario(
            [FromBody] CreateUserDto createUserDto)
        {
            if (createUserDto == null)
                return BadRequest();

            var usuario =
                await _usuarioService.CriarUsuario(
                    createUserDto);

            return Ok(usuario);
        }


        [HttpPost("esqueci-senha")]
        [AllowAnonymous]
        [EnableRateLimiting("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult> RecuperarSenha([FromBody] RecuperarSenhaDto? dto)
        {
            if (dto is not null)
            {
                await _usuarioService.RecuperarSenha(dto);
            }

            return Ok(
                "Se houver uma conta ativa com e-mail cadastrado, " +
                "as instruções de recuperação foram enviadas.");
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting("login")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult> Login([FromBody] LoginDto loginDto, CancellationToken cancellationToken)
        {
            if (loginDto == null)
                return BadRequest();

            var resultado =
                await _usuarioService.Login(
                    loginDto,
                    cancellationToken);

            DefinirCookieRefreshNormal(
                resultado.RefreshToken);

            return Ok(
                resultado.Resposta);
        }

        // Senhas

        [HttpPost("alterar-senha")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult> ChangePassword(
            [FromBody] ChangePasswordDto dto)
        {
            if (dto == null)
                return BadRequest();

            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var isAdmin =
                User.IsInRole("Admin");

            var resultado =
                await _usuarioService.ChangePassword(
                    userId,
                    dto,
                    isAdmin);

            return Ok(resultado);
        }

        [HttpPut("{id}/restaurar-senha")]
        [Permissao("GERENCIAR_USUARIOS")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> RestaurarSenhaPadrao(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("Usuário inválido.");
            }

            var resultado =
                await _usuarioService.RestaurarSenhaPadrao(id);

            return Ok(resultado);
        }

        // Administração do usuário

        [HttpDelete("{id}")]
        [Permissao("GERENCIAR_USUARIOS")]
        [ProducesResponseType(200, Type = typeof(CreateUserDto))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Excluir(
            string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Código inválido");

            var usuario =
                await _usuarioService.Excluir(id);

            if (usuario == null)
                return NotFound(
                    "Usuário não encontrado");

            return Ok(usuario);
        }


        [HttpPut("{id}")]
        [Permissao("GERENCIAR_USUARIOS")]
        [ProducesResponseType(200, Type = typeof(UpdateUserDto))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Atualizar(string id, [FromBody] UpdateUserDto dto)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("Id inválido.");

            var usuario =
                await _usuarioService.Atualizar(id, dto);

            return Ok(usuario);
        }



        [HttpPut("{id}/ativar")]
        [Permissao("GERENCIAR_USUARIOS")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult>
            Ativar(string id)
        {
            if(string.IsNullOrWhiteSpace(id))
                return BadRequest("Id inválido.");


            await _usuarioService.Ativar(id);

            return Ok("Usuario ativado com sucesso.");
        }

        [HttpPut("{id}/desativar")]
        [Permissao("GERENCIAR_USUARIOS")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult>
            Desativar(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Id inválido.");

            await _usuarioService.Desativar(id);

            return Ok("Usuario ativado com sucesso.");
        }

        //Endpoint de renovação do token
        [HttpPost("renovar")]
        [AllowAnonymous]
        [EnableRateLimiting("login")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult> RenovarSessao(CancellationToken cancellationToken)
        {
            if (!RequisicaoDeCookiePermitida())
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    "Origem da requisição não permitida.");
            }

            if (!Request.Cookies.TryGetValue(
                    CookieRefreshNormal,
                    out var refreshToken) ||
                string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new AutenticacaoException(
                    "Sessão de autenticação inválida.");
            }

            var resultado =
                await _sessaoRefreshService
                    .RenovarSessaoNormalAsync(
                        refreshToken,
                        cancellationToken);

            DefinirCookieRefreshNormal(
                resultado.RefreshToken);

            return Ok(
                resultado.Resposta);
        }

        // Encerra a sessão atual e remove o cookie do navegador.
        [HttpPost("logout")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        [ProducesResponseType(403)]
        public async Task<ActionResult> Logout(
            CancellationToken cancellationToken)
        {
            if (!RequisicaoDeCookiePermitida())
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    "Origem da requisição não permitida.");
            }

            try
            {
                if (Request.Cookies.TryGetValue(
                        CookieRefreshNormal,
                        out var refreshToken) &&
                    !string.IsNullOrWhiteSpace(refreshToken))
                {
                    await _sessaoRefreshService
                        .RevogarSessaoAsync(
                            refreshToken,
                            "LOGOUT_USUARIO",
                            cancellationToken);
                }
            }
            finally
            {
                RemoverCookieRefreshNormal();
            }

            return Ok(
                "Logout realizado com sucesso.");
        }

        // Armazena o refresh token em um cookie de sessão protegido.
        // O JavaScript do frontend não consegue acessar seu conteúdo.
        private void DefinirCookieRefreshNormal(string refreshToken)
        {
            Response.Cookies.Append(
                CookieRefreshNormal,
                refreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    IsEssential = true,
                    Path = "/api/Usuario"
                });
        }

        // Confirma que a requisição foi iniciada pelo frontend autorizado.
        // Essa validação protege os endpoints que utilizam o cookie HttpOnly.
        private bool RequisicaoDeCookiePermitida()
        {
            if (!Request.Headers.TryGetValue(
                    "X-Helpdesk-Request",
                    out var cabecalho) ||
                cabecalho.ToString() != "1")
            {
                return false;
            }

            var origem =
                Request.Headers.Origin
                    .ToString()
                    .TrimEnd('/');

            if (string.IsNullOrWhiteSpace(origem))
                return false;

            var origensPermitidas =
                _configuration
                    .GetSection("Cors:AllowedOrigins")
                    .Get<string[]>()
                ?? Array.Empty<string>();

            return origensPermitidas.Any(
                origemPermitida =>
                    string.Equals(
                        origemPermitida.TrimEnd('/'),
                        origem,
                        StringComparison.OrdinalIgnoreCase));
        }

        // Remove o cookie usando o mesmo nome e caminho utilizados na criação.
        private void RemoverCookieRefreshNormal()
        {
            Response.Cookies.Delete(
                CookieRefreshNormal,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    IsEssential = true,
                    Path = "/api/Usuario"
                });
        }




    }
}
