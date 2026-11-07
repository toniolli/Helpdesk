using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
    public class EndpointLentoViewModel
    {
        public string MetodoHttp { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public int QuantidadeOcorrencias { get; set; }
        public double TempoMedioMs { get; set; }
        public double MaiorTempoMs { get; set; }
        public double UltimoTempoMs { get; set; }
        public DateTime UltimaOcorrencia { get; set; }
    }
}