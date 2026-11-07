using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dto.RegraDistribuicaoDto
{
    public class AplicarRegraChamadosRequestDto
    {

        public int RegraId { get; set; }
        public List<int> ChamadosId { get; set; } = new List<int>();


    }
}
