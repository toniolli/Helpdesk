using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public enum StatusChamadoEnum
    {
        Aberto = 1,
        EmAtendimento = 2,
        Fechado = 3,
        Cancelado = 4,
        AguardandoRetorno = 5,
        Distribuido = 6 ,
        AguardandoEquipamento = 7 
    }
}