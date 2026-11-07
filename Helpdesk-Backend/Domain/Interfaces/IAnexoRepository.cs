using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IAnexoRepository
    {
        Task<Anexo> Adicionar(Anexo anexo);
        Task<Anexo?> BuscarPorId(int id);
        Task<IEnumerable<Anexo>> BuscarPorComentario(int comentarioId);
        Task<Anexo> Atualizar(Anexo anexo);
        Task Remover(int id);
    }
}