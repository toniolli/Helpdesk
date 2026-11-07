using Microsoft.AspNetCore.Http;

namespace Application.Dto.Comentario
{
    public class ComentarioRequestDto
    {
        public int ChamadoId { get; set; }
        public string? Mensagem { get; set; }
        public List<IFormFile> Arquivos { get; set; } = new();

    }
}