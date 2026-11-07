using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dto.EmailNotification
{
    public class EmailNotificationRequestDto
    {
        public int ChamadoId { get; set; }
        public TipoNotificacaoEmailEnum Tipo { get; set; }
        public string Destinatario { get; set; } = string.Empty;
        public string Assunto { get; set; } = string.Empty;


    }
}
