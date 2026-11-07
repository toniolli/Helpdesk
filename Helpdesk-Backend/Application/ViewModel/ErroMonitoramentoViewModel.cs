using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
    public class ErroMonitoramentoViewModel
    {
        public string Origem { get; set; } = string.Empty;
        public string? MetodoHttp { get; set; }
        public string? Endpoint { get; set; }
        public int? StatusCode { get; set; }
        public string TipoErro { get; set; } = string.Empty;
        public string? CodigoErro { get; set; }
        public string Mensagem { get; set; } = string.Empty;
        public string? ExcecaoInterna { get; set; }
        public string? TraceId { get; set; }
        public DateTime DataOcorrencia { get; set; }
    }
}