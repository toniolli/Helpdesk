using Domain.Entities;
using Domain.Enum;

namespace Domain.Interfaces
{
    public interface IAuditoriaChamadoRepository
    {
        Task<AuditoriaChamado?> BuscarPorId(int id);

        Task<IEnumerable<AuditoriaChamado>> BuscarPorChamado(int chamadoId);
        Task<int> RemoverAnterioresA( DateTime dataLimite, int tamanhoLote);
        Task<(IEnumerable<AuditoriaChamado> Auditorias,int TotalRegistros)> BuscarPaginado(
                DateTime dataInicial,
                DateTime dataFinal,
                string? numeroChamado,
                string? usuarioId,
                string? perfilUsuario,
                EventoAuditoriaChamadoEnum? evento,
                StatusChamadoEnum? status,
                int? categoriaAtendimentoId,
                int pagina,
                int tamanhoPagina);
    }
}
