using Application.Dto.CentralAtendimento;
using Application.Interfaces;
using Helpdesk.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helpdesk.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CentralAtendimentoController
        : ControllerBase
    {
        private readonly ICentralAtendimentoService
            _centralAtendimentoService;

        public CentralAtendimentoController(
            ICentralAtendimentoService centralAtendimentoService)
        {
            _centralAtendimentoService =
                centralAtendimentoService;
        }

        // Retorna uma fotografia completa da Central.
        // Este endpoint será utilizado apenas como contingência
        // e diagnóstico, sem polling periódico no frontend.
        [HttpGet]
        [Permissao("VISUALIZAR_CENTRAL_ATENDIMENTO")]
        [ProducesResponseType(
            StatusCodes.Status200OK,
            Type = typeof(CentralAtendimentoResponseDto))]
        [ProducesResponseType(
            StatusCodes.Status403Forbidden)]
        public async Task<
            ActionResult<CentralAtendimentoResponseDto>>
            Obter(
                CancellationToken cancellationToken)
        {
            var fotografia =
                await _centralAtendimentoService
                    .ObterFotografiaAsync(
                        cancellationToken);

            return Ok(fotografia);
        }
    }
}