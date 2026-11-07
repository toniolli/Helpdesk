using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
    public class DashboardIndicadoresAdminViewModel
    {
        public int ChamadosCriadosHoje { get; set; }
        public int ChamadosResolvidosHoje { get; set; }

        //Sla
        public int SlaCumprido { get; set; }
        public int SlaViolado { get; set; }
        public int SlaPrimeiroAtendimentoCumprido { get; set; }
        public int SlaPrimeiroAtendimentoViolado { get; set; }
        public int SlaResolucaoCumprido { get; set; }
        public int SlaResolucaoViolado { get; set; }
        public int ChamadosProximosVencimento { get; set; }
        public double PercentualSlaCumprido { get; set; }
        public int ChamadosCriticosAbertos { get; set; }
        public int ChamadosForaSlaEmAberto { get; set; }

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
