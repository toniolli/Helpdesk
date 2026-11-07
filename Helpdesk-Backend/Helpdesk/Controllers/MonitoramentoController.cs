using Application.Interfaces;
using Application.ViewModel;
using Helpdesk.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helpdesk.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    /// <summary>
    /// Disponibiliza o painel administrativo de monitoramento de erros.
    /// </summary>
    public class MonitoramentoController
        : ControllerBase
    {
        private readonly IMonitoramentoService
            _monitoramentoService;

        public MonitoramentoController(IMonitoramentoService monitoramentoService)
        {
            _monitoramentoService =
                monitoramentoService;
        }

        // CONSULTAS

        [HttpGet("painel")]
        [Permissao("VISUALIZAR_AUDITORIA")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PainelErrosViewModel))]
        [ProducesResponseType( StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PainelErrosViewModel>> ObterPainel()
        {
            var painel =
                await _monitoramentoService
                    .ObterPainel();

            return Ok(painel);
        }
    }
}
