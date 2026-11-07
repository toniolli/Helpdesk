using Application.Dto.AuditoriaIdentity;
using Application.Dto.Paginacao;
using Domain.Entities;
using Domain.Enum;

namespace Application.Interfaces
{
    public interface IAuditoriaIdentityService
    {
        // Uso interno pelo backend
        Task<AuditoriaIdentityResponseDto> Adicionar(AuditoriaIdentity auditoriaIdentity);
        // Consultas administrativas
        Task<AuditoriaIdentityResponseDto?> BuscarPorId(int id);
        Task<PaginacaoResponseDto<AuditoriaIdentityResponseDto>>
            BuscarPaginado(
                DateTime dataInicial,
                DateTime dataFinal,
                string? usuarioId,
                EventoAuditoriaIdentityEnum? evento,
                bool? sucesso,
                PaginacaoRequestDto request);

        // Uso interno pelo processo de retenção
        Task<int> RemoverExpiradas();
    }
}