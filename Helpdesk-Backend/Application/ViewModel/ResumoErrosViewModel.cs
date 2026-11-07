using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
    public class ResumoErrosViewModel
    {
        public int IncidentesAtivos { get; set; }
        public int ErrosUltimaHora { get; set; }
        public int EndpointsLentos { get; set; }
        public int TotalRequisicoesMonitoradas { get; set; }
    }
}
