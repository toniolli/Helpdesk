using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enum
{
    public enum StatusEventoSignalROutboxEnum
    {
        Pendente = 1,
        Processando = 2,
        Enviado = 3,
        Falhou = 4
    }
}
