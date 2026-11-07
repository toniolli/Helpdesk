
using Application.Interfaces;
using Application.ViewModel;
using Helpdesk.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helpdesk.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    /// <summary>
    /// Fornece os indicadores consolidados exibidos no dashboard.
    /// </summary>
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
  
        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
           
        }

        // CONSULTAS

        [HttpGet]
        [Permissao("VISUALIZAR_DASHBOARD")]
        [ProducesResponseType(200, Type = typeof(DashboardViewModel))]
        [ProducesResponseType(400)]
        public async Task<ActionResult<DashboardViewModel>> ObterDashboard([FromQuery] DashboardFiltroViewModel filtro)
        {
            var dashboard = await _dashboardService.ObterDashboard(filtro);

            return Ok(dashboard);
        }



    }
}
