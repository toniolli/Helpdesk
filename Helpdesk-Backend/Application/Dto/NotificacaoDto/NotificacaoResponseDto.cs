using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dto.NotificacaoDto
{
    public class NotificacaoResponseDto
    {
        public int Id_Notificacao { get;  set; }
        public string UsuarioId { get;  set; }
        public string? UsuarioOrigemId { get;  set; }
        public int ChamadoId { get;  set; }
        public TipoNotificacaoEnum Tipo { get; set; }
        public string Titulo { get; set; }
        public string Mensagem { get; set; }
        public bool Lida { get; set; }
        public StatusNotificacaoEnum Status { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataLida { get; set; }



    }
}
