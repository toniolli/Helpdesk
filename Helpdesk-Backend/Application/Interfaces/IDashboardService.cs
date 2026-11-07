using Application.ViewModel;

namespace Application.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardViewModel> ObterDashboard(
            DashboardFiltroViewModel filtro);
    }
}
