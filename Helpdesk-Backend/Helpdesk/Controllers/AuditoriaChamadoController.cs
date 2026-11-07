using Application.Dto.AuditoriaChamado;
using Application.Dto.Paginacao;
using Application.Interfaces;
using Helpdesk.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helpdesk.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    /// <summary>
    /// Disponibiliza consultas ao histórico de auditoria dos chamados.
    /// </summary>
    public class AuditoriaChamadoController : ControllerBase
    {
        private readonly IAuditoriaChamadoService _auditoriaChamadoService;

        public AuditoriaChamadoController(
            IAuditoriaChamadoService auditoriaChamadoService)
        {
            _auditoriaChamadoService = auditoriaChamadoService;
        }

        // CONSULTAS

        [HttpGet("{id:int}")]
        [Permissao("VISUALIZAR_AUDITORIA_CHAMADO")]
        [ProducesResponseType(
            StatusCodes.Status200OK,
            Type = typeof(AuditoriaChamadoResponseDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AuditoriaChamadoResponseDto>>
            BuscarPorId(int id)
        {
            var auditoria = await _auditoriaChamadoService.BuscarPorId(id);

            if (auditoria == null)
            {
                return NotFound(new
                {
                    error = "Auditoria de chamado não encontrada."
                });
            }

            return Ok(auditoria);
        }

        [HttpGet("chamado/{chamadoId:int}")]
        [Permissao("VISUALIZAR_AUDITORIA_CHAMADO")]
        [ProducesResponseType(
            StatusCodes.Status200OK,
            Type = typeof(IEnumerable<AuditoriaChamadoResponseDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<AuditoriaChamadoResponseDto>>>
            BuscarPorChamado(int chamadoId)
        {
            var auditorias = await _auditoriaChamadoService
                .BuscarPorChamado(chamadoId);

            return Ok(auditorias);
        }

        [HttpGet("paginado")]
        [Permissao("VISUALIZAR_AUDITORIA_CHAMADO")]
        [ProducesResponseType(
            StatusCodes.Status200OK,
            Type = typeof(
                PaginacaoResponseDto<AuditoriaChamadoResponseDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<
            PaginacaoResponseDto<AuditoriaChamadoResponseDto>>>
            BuscarPaginado(
                [FromQuery] AuditoriaChamadoFiltroDto filtro,
                [FromQuery] PaginacaoRequestDto paginacao)
        {
            var resultado = await _auditoriaChamadoService.BuscarPaginado(
                filtro,
                paginacao);

            return Ok(resultado);
        }
    }
}
