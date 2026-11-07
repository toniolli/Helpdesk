using Application.DTOs.Anexo;

namespace Application.Dto.Comentario
{
    public class ComentarioResponseDto
    {
        public int Id_Comentario { get; set; }
        public int ChamadoId { get; set; }
        public string UsuarioId { get; set; }
        public string UsuarioNome { get; set; } = string.Empty;
        public string? Mensagem { get; set; }
        public DateTime DataCriacao { get; set; }
        public List<AnexoResponseDto> Anexos { get; set; } = new();
    }
}
