using Domain.Enum;
using Domain.Entities;

namespace Application.Dto.AuditoriaChamado
{
    public class AuditoriaChamadoResponseDto
    {
        public int Id_AuditoriaChamado { get; set; }
        public int ChamadoId { get; set; }
        public string NumeroChamado { get; set; } = string.Empty;
        public EventoAuditoriaChamadoEnum Evento { get; set; }
        public DateTime DataEvento { get; set; }
        public string? UsuarioId { get; set; }
        public string UsuarioNome { get; set; } = string.Empty;
        public string? PerfilUsuario { get; set; }
        public StatusChamadoEnum? StatusAnterior { get; set; }
        public StatusChamadoEnum? StatusNovo { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public string? DetalhesAnteriores { get; set; }
        public string? DetalhesNovos { get; set; }
    }
}
