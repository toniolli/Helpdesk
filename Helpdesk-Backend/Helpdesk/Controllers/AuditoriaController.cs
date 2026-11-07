using Application.Dto.Auditoria;
using Application.Dto.Paginacao;
using Application.Interfaces;
using Domain.Enum;
using Helpdesk.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helpdesk.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    /// <summary>
    /// Disponibiliza consultas à auditoria geral da aplicação.
    /// </summary>
    public class AuditoriaController : ControllerBase
    {
        private readonly IAuditoriaService _auditoriaService;

        public AuditoriaController(
            IAuditoriaService auditoriaService)
        {
            _auditoriaService = auditoriaService;
        }

        // CONSULTAS

        [HttpGet("{id:int}")]
        [Permissao("VISUALIZAR_AUDITORIA")]
        [ProducesResponseType(
            StatusCodes.Status200OK,
            Type = typeof(AuditoriaResponseDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AuditoriaResponseDto>>
            BuscarPorId(int id)
        {
            var auditoria =
                await _auditoriaService.BuscarPorId(id);

            if (auditoria == null)
            {
                return NotFound(new
                {
                    error = "Auditoria não encontrada."
                });
            }

            return Ok(auditoria);
        }

        [HttpGet("paginado")]
        [Permissao("VISUALIZAR_AUDITORIA")]
        [ProducesResponseType(
            StatusCodes.Status200OK,
            Type = typeof(
                PaginacaoResponseDto<AuditoriaResponseDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<
            PaginacaoResponseDto<AuditoriaResponseDto>>>
            BuscarPaginado(
                [FromQuery] DateTime dataInicial,
                [FromQuery] DateTime dataFinal,
                [FromQuery] string? usuarioId,
                [FromQuery] AcaoAuditoriaEnum? acao,
                [FromQuery] string? entidade,
                [FromQuery] PaginacaoRequestDto request)
        {
            var resultado =
                await _auditoriaService.BuscarPaginado(
                    dataInicial,
                    dataFinal,
                    usuarioId,
                    acao,
                    entidade,
                    request);

            return Ok(resultado);
        }
    }
}
