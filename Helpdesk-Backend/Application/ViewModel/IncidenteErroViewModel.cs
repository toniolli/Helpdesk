using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
    public class IncidenteErroViewModel
    {
        public string Identificador { get; set; } = string.Empty;
        public string Origem { get; set; } = string.Empty;
        public string? MetodoHttp { get; set; }
        public string? Endpoint { get; set; }
        public int? StatusCode { get; set; }
        public string TipoErro { get; set; } = string.Empty;
        public string? CodigoErro { get; set; }
        public string Mensagem { get; set; } = string.Empty;
        public DateTime PrimeiraOcorrencia { get; set; }
        public DateTime UltimaOcorrencia { get; set; }
        public int QuantidadeOcorrencias { get; set; }
        public double DuracaoAtivaSegundos { get; set; }
        public bool EstaAtivo { get; set; }
    }
}
