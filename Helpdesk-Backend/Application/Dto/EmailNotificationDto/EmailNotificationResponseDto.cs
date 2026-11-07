using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dto.EmailNotificationDto
{
    public class EmailNotificationResponseDto
    {

        public int Id_EmailNotfication { get; set; }
        public int ChamadoId { get; set; }
        public TipoNotificacaoEmailEnum Tipo { get; set; }
        public string Destinatario { get; set; } = string.Empty;
        public string Assunto { get; set; } = string.Empty;
        public StatusNotificacaoEmailEnum Status { get; set; }
        public int QuantidadeTentativas { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataEnvio { get; set; }
        public string? MensagemErro { get; set; }

    }
}
