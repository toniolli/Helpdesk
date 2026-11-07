using Application.Dto.Auditoria;
using Application.Dto.Paginacao;
using Domain.Entities;
using Domain.Enum;

namespace Application.Interfaces
{
    public interface IAuditoriaService
    {
        // Uso interno pelo backend
        Task<AuditoriaResponseDto> Adicionar(Auditoria auditoria);
        // Consultas administrativas
        Task<AuditoriaResponseDto?> BuscarPorId(int id);
        Task<PaginacaoResponseDto<AuditoriaResponseDto>>
            BuscarPaginado(
                DateTime dataInicial,
                DateTime dataFinal,
                string? usuarioId,
                AcaoAuditoriaEnum? acao,
                string? entidade,
                PaginacaoRequestDto request);

        // Uso interno pelo processo de retenção
        Task<int> RemoverExpiradas();
    }
}