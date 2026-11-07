using Domain.Entities;
using Domain.Enum;

namespace Application.ViewModel
{
    public class AnaliseChamadoFiltroViewModel
    {
        public string? NumeroChamado { get; set; }
        public DateTime? DataInicial { get; set; }
        public DateTime? DataFinal { get; set; }
        public TipoUsuarioDashboardEnum TipoUsuario { get; set; } = TipoUsuarioDashboardEnum.Tecnico;
        public string? UsuarioId { get; set; }
        public int? PerfilId { get; set; }
        public int? CategoriaAtendimentoId { get; set; }
        public PrioridadeEnum? Prioridade { get; set; }
        public StatusChamadoEnum? Status { get; set; }
    }
}
