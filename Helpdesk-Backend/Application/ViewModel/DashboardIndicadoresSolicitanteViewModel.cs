using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
    public class DashboardIndicadoresSolicitanteViewModel
    {
        public int MeusChamadosAguardandoRetorno { get; set; }
        public int MeusChamadosForaSla { get; set; }
        public int MeusChamadosAbertos { get; set; }
        public int MeusChamadosEmAtendimento { get; set; }
        public int MeusChamadosResolvidos { get; set; }


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
