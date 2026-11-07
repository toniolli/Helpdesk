
using Application.DTOs.Anexo;


namespace Application.Interfaces
{
    public interface IAnexoService
    {
        Task<AnexoResponseDto?> BuscarPorId(int id, string usuarioLogadoId, bool podeVisualizar);
        Task<IEnumerable<AnexoResponseDto>> BuscarPorComentario(int comentarioId, string usuarioLogadoId, bool podeVisualizar);
        Task Remover(int id, string usuarioLogadoId, bool podeVisualizar);
    }
}
