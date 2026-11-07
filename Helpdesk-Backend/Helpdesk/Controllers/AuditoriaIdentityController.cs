using Application.Dto.AuditoriaIdentity;
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
    /// Disponibiliza consultas aos eventos de auditoria de identidade.
    /// </summary>
    public class AuditoriaIdentityController
        : ControllerBase
    {
        private readonly IAuditoriaIdentityService
            _auditoriaIdentityService;

        public AuditoriaIdentityController(
            IAuditoriaIdentityService
                auditoriaIdentityService)
        {
            _auditoriaIdentityService =
                auditoriaIdentityService;
        }

        // CONSULTAS

        [HttpGet("{id:int}")]
        [Permissao("VISUALIZAR_AUDITORIA")]
        [ProducesResponseType(
            StatusCodes.Status200OK,
            Type = typeof(
                AuditoriaIdentityResponseDto))]
        [ProducesResponseType(
            StatusCodes.Status404NotFound)]
        [ProducesResponseType(
            StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<
            AuditoriaIdentityResponseDto>>
            BuscarPorId(int id)
        {
            var auditoria =
                await _auditoriaIdentityService
                    .BuscarPorId(id);

            if (auditoria == null)
            {
                return NotFound(new
                {
                    error =
                        "Auditoria de Identity não encontrada."
                });
            }

            return Ok(auditoria);
        }

        [HttpGet("paginado")]
        [Permissao("VISUALIZAR_AUDITORIA")]
        [ProducesResponseType(
            StatusCodes.Status200OK,
            Type = typeof(
                PaginacaoResponseDto<
                    AuditoriaIdentityResponseDto>))]
        [ProducesResponseType(
            StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<
            PaginacaoResponseDto<
                AuditoriaIdentityResponseDto>>>
            BuscarPaginado(
                [FromQuery] DateTime dataInicial,
                [FromQuery] DateTime dataFinal,
                [FromQuery] string? usuarioId,
                [FromQuery]
                    EventoAuditoriaIdentityEnum? evento,
                [FromQuery] bool? sucesso,
                [FromQuery]
                    PaginacaoRequestDto request)
        {
            var resultado =
                await _auditoriaIdentityService
                    .BuscarPaginado(
                        dataInicial,
                        dataFinal,
                        usuarioId,
                        evento,
                        sucesso,
                        request);

            return Ok(resultado);
        }
    }
}
