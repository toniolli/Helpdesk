using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
    public class DashboardViewModel
    {
        public string Perfil { get; set; } = string.Empty;

        public DateTime DataAtualizacao { get; set; }
        public DashboardResumoViewModel Resumo { get; set; } = new();
        public DashboardIndicadoresAdminViewModel? IndicadoresAdmin { get; set; }
        public DashboardIndicadoresTecnicoViewModel? IndicadoresTecnico { get; set; }
        public DashboardIndicadoresSolicitanteViewModel? IndicadoresSolicitante { get; set; }
        public IEnumerable<DashboardStatusViewModel> Status { get; set; }
            = Enumerable.Empty<DashboardStatusViewModel>();
        public IEnumerable<DashboardPrioridadeViewModel> Prioridades { get; set; }
            = Enumerable.Empty<DashboardPrioridadeViewModel>();
        public IEnumerable<DashboardLinhaTempoViewModel> ChamadosPorMes { get; set; }
            = Enumerable.Empty<DashboardLinhaTempoViewModel>();
        public IEnumerable<DashboardRankingViewModel> RankingTecnicos { get; set; }
            = Enumerable.Empty<DashboardRankingViewModel>();
        public IEnumerable<DashboardRankingViewModel> RankingSolicitantes { get; set; }
            = Enumerable.Empty<DashboardRankingViewModel>();
        public IEnumerable<DashboardRankingViewModel> RankingCategorias { get; set; }
            = Enumerable.Empty<DashboardRankingViewModel>();
    }
}
