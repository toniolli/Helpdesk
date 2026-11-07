using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enum
{
    public enum TipoEncerramentoEnum
    {
        Resolvido = 1,
        SemRetornoDaLoja = 2,
        InformacoesInsuficientes = 3,
        ChamadoAbertoPorEngano = 4,
        CanceladoPeloSolicitante = 5
    }
}
