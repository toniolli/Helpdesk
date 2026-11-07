using Domain.Entities;
using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
    public class DashboardFiltroViewModel
    {
        public DateTime? DataInicial { get; set; }
        public DateTime? DataFinal { get; set; }
        public TipoUsuarioDashboardEnum TipoUsuario { get; set; }  = TipoUsuarioDashboardEnum.Tecnico;
        public string? UsuarioId { get; set; }
        public int? PerfilId { get; set; }
        public int? CategoriaAtendimentoId { get; set; }
        public PrioridadeEnum? Prioridade { get; set; }
        public StatusChamadoEnum? Status { get; set; }
    }
}
