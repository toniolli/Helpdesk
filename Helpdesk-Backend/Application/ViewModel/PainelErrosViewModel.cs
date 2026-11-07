using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
    public class PainelErrosViewModel
    {
        public DateTime DataAtualizacao { get; set; }

        public ResumoErrosViewModel Resumo { get; set; } = new();

        public IEnumerable<IncidenteErroViewModel> IncidentesAtivos { get; set; }
            = Enumerable.Empty<IncidenteErroViewModel>();

        public IEnumerable<ErroMonitoramentoViewModel> ErrosRecentes { get; set; }
            = Enumerable.Empty<ErroMonitoramentoViewModel>();

        public IEnumerable<EndpointLentoViewModel> EndpointsLentos { get; set; }
            = Enumerable.Empty<EndpointLentoViewModel>();
    }
}