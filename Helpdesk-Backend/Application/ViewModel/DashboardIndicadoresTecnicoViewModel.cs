using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
    public class DashboardIndicadoresTecnicoViewModel
    {
        public int ChamadosPendentes { get; set; }

        //sla
        public int MeusChamadosVencidos { get; set; }
        public int MeusChamadosProximosVencimento { get; set; }
        public int MeusChamadosAguardandoRetorno { get; set; }
        public int MeusChamadosResolvidos { get; set; }
        public int MeuSlaCumprido { get; set; }
        public int MeusChamadosCriticos { get; set; }
        public double TempoMedioCorridoHoras { get; set; }
        public double TempoMedioPrimeiroAtendimentoHoras { get; set; }
        public double TempoMedioPrimeiroAtendimentoSlaHoras { get; set; }
        public double TempoMedioAguardandoRetornoHoras { get; set; }
        public double TempoMedioAguardandoEquipamentoHoras { get; set; }
        public double TempoMedioTotalPausadoHoras { get; set; }
        public double TempoMedioResolucaoSlaHoras { get; set; }
        public double TempoMedioAtendimentoEfetivoHoras { get; set; }


    }
}
