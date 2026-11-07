using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dto.Paginacao
{
    public class PaginacaoResponseDto<T>
    {
        public DateTime? dataInicial { get; set; }
        public DateTime? dataFinal { get; set; }
        public int Pagina { get; set; }
        public int TamanhoPagina { get; set; }
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
        public IEnumerable<T> Dados { get; set; } = Enumerable.Empty<T>();
    }
}
