using Application.Dto.Comentario;
using Application.Dto.NotificacaoDto;
using Application.Dto.Paginacao;
using Application.Interfaces;
using Helpdesk.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Helpdesk.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    /// <summary>
    /// Consulta notificações e gerencia suas alterações de estado.
    /// </summary>
    public class NotificacaoController : ControllerBase
    {
        private readonly INotificacaoService _notificacaoService;

        public NotificacaoController(INotificacaoService notificacaoService)
        {
            _notificacaoService = notificacaoService;
        }

        // CONSULTAS

        [HttpGet("{id}", Name = "GetNotificacaoPorId")]
        [Permissao("VISUALIZAR_NOTIFICACAO")]
        [ProducesResponseType(200, Type = typeof(NotificacaoResponseDto))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<NotificacaoResponseDto>> BuscarPorId(int id)
        {
            if (!TryObterContextoUsuario(out var usuarioLogadoId, out var podeVisualizar))
                return Unauthorized();

            if (id <= 0)
                return BadRequest("Notificação inválida");

            var notificacao = await _notificacaoService.BuscarPorId(id, usuarioLogadoId, podeVisualizar);

            if (notificacao == null)
                return NotFound("Notificação não encontrada");

            return Ok(notificacao);
        }

        [HttpGet]
        [Permissao("VISUALIZAR_NOTIFICACAO")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<NotificacaoResponseDto>))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<NotificacaoResponseDto>>> BuscarTodos()
        {
            if (!TryObterContextoUsuario(out var usuarioLogadoId, out var podeVisualizar))
                return Unauthorized();

            var notificacoes = await _notificacaoService.BuscarTodos(usuarioLogadoId, podeVisualizar);

            if (notificacoes == null)
                return NotFound("Nenhuma notificação encontrada");

            return Ok(notificacoes);
        }

        [HttpGet("buscar-notificacao-paginado")]
        [Permissao("VISUALIZAR_NOTIFICACAO")]
        [ProducesResponseType(200, Type = typeof(PaginacaoResponseDto<ComentarioResponseDto>))]
        [ProducesResponseType(400)]
        public async Task<ActionResult<PaginacaoResponseDto<ComentarioResponseDto>>>
        BuscarMeusChamadosPaginado([FromQuery] PaginacaoRequestDto request)
        {
            if (!TryObterContextoUsuario(out var usuarioLogadoId, out var podeVisualizar))
                return Unauthorized();

            var resultado =
                await _notificacaoService.BuscarNotificacaoPaginado(
                    request, usuarioLogadoId, podeVisualizar);

            return Ok(resultado);
        }





        [HttpGet("usuario/{usuarioId}")]
        [Permissao("VISUALIZAR_NOTIFICACAO", "CRIAR_NOTIFICACAO")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<NotificacaoResponseDto>))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<NotificacaoResponseDto>>> ObterPorUsuario(string usuarioId)
        {
            if (!TryObterContextoUsuario(out var usuarioLogadoId, out var podeVisualizar))
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(usuarioId))
                return BadRequest("Usuário inválido");

            var notificacoes = await _notificacaoService.ObterPorUsuario(usuarioId, usuarioLogadoId, podeVisualizar);

            return Ok(notificacoes);
        }

        [HttpGet("nao-lidas")]
        [Permissao("VISUALIZAR_NOTIFICACAO", "CRIAR_NOTIFICACAO")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<NotificacaoResponseDto>))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<NotificacaoResponseDto>>> ObterNaoLidas([FromQuery] bool lida = false)
        {
            if (!TryObterContextoUsuario(out var usuarioLogadoId, out var podeVisualizar))
                return Unauthorized();

            var notificacoes = await _notificacaoService.ObterNaoLidas(lida, usuarioLogadoId, podeVisualizar);

            return Ok(notificacoes);
        }

        [HttpGet("usuario/{usuarioId}/total-nao-lidas")]
        [Permissao("VISUALIZAR_NOTIFICACAO")]
        [ProducesResponseType(200, Type = typeof(int))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<int>> TotalNotificacaoNaoLidas(string usuarioId)
        {
            if (!TryObterContextoUsuario(out var usuarioLogadoId, out var podeVisualizar))
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(usuarioId))
                return BadRequest("Usuário inválido.");

            var total = await _notificacaoService.TotalNotificacaoNaoLidas(usuarioId, usuarioLogadoId, podeVisualizar);

            return Ok(total);
        }

        // AÇÕES


        [HttpPut("{id}/marcar-como-lida")]
        [Permissao("VISUALIZAR_NOTIFICACAO")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> MarcarComoLida(int id)
        {
            if (!TryObterContextoUsuario(out var usuarioLogadoId, out var podeVisualizar))
                return Unauthorized();

            if (id <= 0)
                return BadRequest("Id inválido.");

            await _notificacaoService.MarcarComoLida(id, usuarioLogadoId, podeVisualizar);

            return Ok("Notificação marcada como lida.");
        }

        [HttpPut("{id}/aceitar")]
        [Permissao("VISUALIZAR_NOTIFICACAO")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Aceitar(int id)
        {
            if (!TryObterContextoUsuario(out var usuarioLogadoId, out var podeVisualizar))
                return Unauthorized();

            if (id <= 0)
                return BadRequest("Id inválido.");

            await _notificacaoService.Aceitar(id, usuarioLogadoId, podeVisualizar);

            return Ok("Notificação aceita com sucesso.");
        }

        [HttpPut("{id}/recusar")]
        [Permissao("VISUALIZAR_NOTIFICACAO")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Recusar(int id)
        {
            if (!TryObterContextoUsuario(out var usuarioLogadoId, out var podeVisualizar))
                return Unauthorized();

            if (id <= 0)
                return BadRequest("Id inválido.");

            await _notificacaoService.Recusar(id, usuarioLogadoId, podeVisualizar);

            return Ok("Notificação recusada com sucesso.");
        }

        [HttpPut("{id}/expirar")]
        [Permissao("VISUALIZAR_NOTIFICACAO")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Expirar(int id)
        {
            if (!TryObterContextoUsuario(out var usuarioLogadoId, out var podeVisualizar))
                return Unauthorized();

            if (id <= 0)
                return BadRequest("Id inválido.");

            await _notificacaoService.Expirar(id, usuarioLogadoId, podeVisualizar);

            return Ok("Notificação expirada com sucesso.");
        }

        [HttpPut("{id}/pendente")]
        [Permissao("VISUALIZAR_NOTIFICACAO")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Pendente(int id)
        {
            if (!TryObterContextoUsuario(out var usuarioLogadoId, out var podeVisualizar))
                return Unauthorized();

            if (id <= 0)
                return BadRequest("Id inválido.");

            await _notificacaoService.Pendente(id, usuarioLogadoId, podeVisualizar);

            return Ok("Notificação alterada para pendente.");
        }

        // MÉTODOS PRIVADOS

        private bool TryObterContextoUsuario(out string usuarioLogadoId, out bool podeVisualizar)
        {
            usuarioLogadoId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            podeVisualizar = User.HasClaim("Permissao", "VISUALIZAR_TODOS_CHAMADOS");
            return !string.IsNullOrWhiteSpace(usuarioLogadoId);
        }
    }
}
