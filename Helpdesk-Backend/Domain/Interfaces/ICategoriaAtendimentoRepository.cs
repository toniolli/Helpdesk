using Domain.Entities;

namespace Domain.Interfaces
{
    public interface ICategoriaAtendimentoRepository
    {
        Task<CategoriaAtendimento> Adicionar(CategoriaAtendimento categoriaAtendimento);
        Task<CategoriaAtendimento?> BuscarPorId(int id);
        Task<IEnumerable<CategoriaAtendimento>> BuscarTodos();
        Task<CategoriaAtendimento> Atualizar(CategoriaAtendimento categoriaAtendimento);
        Task Remover(int id);
    }
}