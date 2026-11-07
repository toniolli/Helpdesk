using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dto.NotificacaoDto
{
    public class NotificacaoRequestDto
    {

        public string usuarioId { get; set; } 
        public string usuarioOrigemId { get; set; }
        public int chamadoId { get; set; }
        public TipoNotificacaoEnum tipo { get; set; }
        public string titulo { get; set; }
        public string mensagem { get; set; }
        public StatusNotificacaoEnum status{ get; set; }

    }
}
