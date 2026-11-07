using Application.Dto.AuditoriaChamado;
using Application.Dto.Paginacao;

namespace Application.Interfaces
{
    public interface IAuditoriaChamadoService
    {
        Task<AuditoriaChamadoResponseDto?> BuscarPorId(int id);
        Task<int> RemoverExpiradas();

        Task<IEnumerable<AuditoriaChamadoResponseDto>> BuscarPorChamado(
            int chamadoId);

        Task<PaginacaoResponseDto<AuditoriaChamadoResponseDto>> BuscarPaginado(
            AuditoriaChamadoFiltroDto filtro,
            PaginacaoRequestDto paginacao);
    }
}
