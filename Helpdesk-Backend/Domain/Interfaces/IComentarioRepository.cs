using Domain.Entities;
using System.Runtime.CompilerServices;

namespace Domain.Interfaces
{
    public interface IComentarioRepository
    {
        Task<Comentario> Adicionar( Comentario comentario);
        Task<Comentario?> BuscarPorId(int id);
        Task<IEnumerable<Comentario>> BuscarPorChamado(int chamadoId);
        Task<Comentario> Atualizar(Comentario comentario);
        Task Remover(int id);
        //paginação
        Task<(IEnumerable<Comentario> Comentario, int TotalRegsitro)> BuscarPorChamadoPaginacao (int chamadoId, int pagina, int tamanhoPagina);
        

    }
}