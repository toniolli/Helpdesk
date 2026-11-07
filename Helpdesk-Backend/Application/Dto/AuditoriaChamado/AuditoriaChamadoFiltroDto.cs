using Domain.Enum;
using Domain.Entities;

namespace Application.Dto.AuditoriaChamado
{
    public class AuditoriaChamadoFiltroDto
    {
        public DateTime DataInicial { get; set; }
        public DateTime DataFinal { get; set; }
        public string? NumeroChamado { get; set; }
        public string? UsuarioId { get; set; }
        public string? PerfilUsuario { get; set; }
        public EventoAuditoriaChamadoEnum? Evento { get; set; }
        public StatusChamadoEnum? Status { get; set; }
        public int? CategoriaAtendimentoId { get; set; }
    }
}
