using Domain.Entities;
using Domain.Enum;

namespace Domain.Interfaces
{
    public interface IAuditoriaIdentityRepository
    {
        Task<AuditoriaIdentity> Adicionar(AuditoriaIdentity auditoriaIdentity);
        Task<AuditoriaIdentity?> BuscarPorId(int id);
        Task<(IEnumerable<AuditoriaIdentity> Auditorias,
            int TotalRegistros)> BuscarPaginado(
                DateTime dataInicial,
                DateTime dataFinal,
                string? usuarioId,
                EventoAuditoriaIdentityEnum? evento,
                bool? sucesso,
                int pagina,
                int tamanhoPagina);
        Task<int> RemoverAnterioresA(DateTime dataLimite,int tamanhoLote);
    }
}