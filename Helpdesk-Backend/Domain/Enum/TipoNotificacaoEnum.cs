using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enum
{
    public enum TipoNotificacaoEnum
    {
        ChamadoDistribuidoPerfil = 1,
        ChamadoDistribuidoUsuario = 2,
        ChamadoAssumido = 3,
        ChamadoTransferidoPerfil = 4,
        ChamadoTransferidoUsuario = 5,
        TransferenciaAceita = 6,
        TransferenciaRecusada = 7,
        NovoComentario = 8,
        ChamadoEncerrado = 9,
        ChamadoCriado = 10,
        AguardandoRetorno = 11,
        RetornoRecebido = 12,
        ChamadoDistribuicaoVencida = 13,
        EquipamentoEnviado = 14
    }

}
