using Domain.Entities;
using Domain.Enum;

namespace Domain.Interfaces
{
    public interface IAuditoriaRepository
    {
        Task<Auditoria> Adicionar(Auditoria auditoria);
        Task<Auditoria?> BuscarPorId(int id);
        Task<( IEnumerable<Auditoria> Auditorias,int TotalRegistros )> BuscarPaginado(
            DateTime dataInicial,
            DateTime dataFinal,
            string? usuarioId,
            AcaoAuditoriaEnum? acao,
            string? entidade,
            int pagina,
            int tamanhoPagina);
        Task<int> RemoverAnterioresA(DateTime dataLimite,int tamanhoLote);
    }
}