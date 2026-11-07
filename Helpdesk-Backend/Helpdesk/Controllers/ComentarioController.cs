using Application.Dto.Chamado;
using Application.Dto.Comentario;
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
    /// Consulta e mantém os comentários associados aos chamados.
    /// </summary>
    public class ComentarioController : ControllerBase
    {
        private readonly IComentarioService _comentarioService;

        public ComentarioController(
            IComentarioService comentarioService)
        {
            _comentarioService = comentarioService;
        }

        
        // CONSULTAS

        [HttpGet("{id}", Name = "GetComentarioPorId")]
        [Permissao(
            "ABRIR_CHAMADO",
            "ASSUMIR_CHAMADO",
            "VISUALIZAR_TODOS_CHAMADOS"
        )]
        [ProducesResponseType(200, Type = typeof(ComentarioResponseDto))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ComentarioResponseDto>> BuscarPorId(int id)
        {
            var usuarioLogadoId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(usuarioLogadoId))
                return Unauthorized();

            var podeVizualizar = User.HasClaim("Permissao", "VISUALIZAR_TODOS_CHAMADOS");



            if (id <= 0)
                return BadRequest("Comentário inválido.");

            var comentario =
                await _comentarioService.BuscarPorId(id, usuarioLogadoId,podeVizualizar);

            if (comentario == null)
                return NotFound("Comentário não encontrado.");

            return Ok(comentario);
        }

        [HttpGet("chamado/{chamadoId}")]
        [Permissao(
            "ABRIR_CHAMADO",
            "ASSUMIR_CHAMADO",
            "VISUALIZAR_TODOS_CHAMADOS"
        )]
        [ProducesResponseType(200, Type = typeof(IEnumerable<ComentarioResponseDto>))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<ComentarioResponseDto>>>BuscarPorChamado(int chamadoId)
        {
            var usuarioLogadoId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(usuarioLogadoId))
                return Unauthorized();

            var podeVizualizar = User.HasClaim("Permissao", "VISUALIZAR_TODOS_CHAMADOS");

            if (chamadoId <= 0)
                return BadRequest("Chamado inválido.");

            var comentarios =
                await _comentarioService.BuscarPorChamado(chamadoId, usuarioLogadoId, podeVizualizar);

            if (!comentarios.Any())
                return NotFound("Nenhum comentário encontrado.");

            return Ok(comentarios);
        }

        [HttpGet("buscar-por-chamado-paginado/{chamadoId}")]
        [Permissao(
            "ABRIR_CHAMADO",
            "ASSUMIR_CHAMADO",
            "VISUALIZAR_TODOS_CHAMADOS"
        )]
        [ProducesResponseType(200, Type = typeof(PaginacaoResponseDto<ComentarioResponseDto>))]
        [ProducesResponseType(400)]
        public async Task<ActionResult<PaginacaoResponseDto<ComentarioResponseDto>>>
        BuscarMeusChamadosPaginado(int chamadoId,[FromQuery] PaginacaoRequestDto request)
        {
            var usuarioLogadoId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(usuarioLogadoId))
                return Unauthorized();

            var podeVizualizar = User.HasClaim("Permissao", "VISUALIZAR_TODOS_CHAMADOS");

            var resultado = await _comentarioService.BuscarPorChamadoPaginado(chamadoId, request, usuarioLogadoId, podeVizualizar);

            return Ok(resultado);
        }

        // AÇÕES

        [HttpPost]
        [Permissao("COMENTAR_CHAMADO")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(200, Type = typeof(ComentarioResponseDto))]
        [ProducesResponseType(400)]
        public async Task<ActionResult> Criar([FromForm] ComentarioRequestDto dto)
        {
            var usuarioLogadoId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(usuarioLogadoId))
                return Unauthorized();

            var podeVizualizar = User.HasClaim("Permissao", "VISUALIZAR_TODOS_CHAMADOS");

            var comentario =
                await _comentarioService.Criar(dto,usuarioLogadoId, podeVizualizar);

            return Ok(comentario);
        }

        [HttpPut("{id}")]
        [Permissao("COMENTAR_CHAMADO")]
        [ProducesResponseType(200, Type = typeof(ComentarioResponseDto))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Atualizar(int id, [FromBody] ComentarioRequestDto dto)
        {
            var usuarioLogadoId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(usuarioLogadoId))
                return Unauthorized();

            var podeVizualizar = User.HasClaim("Permissao", "VISUALIZAR_TODOS_CHAMADOS");



            if (id <= 0)
                return BadRequest("Id inválido.");

            var comentario =
                await _comentarioService.Atualizar(id, dto, usuarioLogadoId, podeVizualizar);

            return Ok(comentario);
        }

        [HttpDelete("{id}")]
        [Permissao("VISUALIZAR_TODOS_CHAMADOS")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult>
            Remover(int id)
        {
            var usuarioLogadoId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(usuarioLogadoId))
                return Unauthorized();

            var podeVisualizar = User.HasClaim(
                "Permissao",
                "VISUALIZAR_TODOS_CHAMADOS");

            if (id <= 0)
                return BadRequest("Id inválido.");

            await _comentarioService.Remover(
                id,
                usuarioLogadoId,
                podeVisualizar);

            return Ok();
        }

    }
}
