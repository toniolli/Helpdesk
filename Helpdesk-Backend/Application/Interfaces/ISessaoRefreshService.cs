using Application.Dto.Autenticacao;
using Domain.Enum;

namespace Application.Interfaces
{
    public interface ISessaoRefreshService
    {
        // Cria a primeira sessão durante o login.
        Task<ResultadoAutenticacaoDto> CriarSessaoAsync(
            string usuarioId,
            TipoSessaoAutenticacaoEnum tipo,
            CancellationToken cancellationToken = default);

        // Valida e substitui o refresh token normal por um novo.
        Task<ResultadoAutenticacaoDto> RenovarSessaoNormalAsync(
            string refreshToken,
            CancellationToken cancellationToken = default);

        // Revoga a sessão correspondente ao refresh token informado.
        Task RevogarSessaoAsync(
            string refreshToken,
            string motivo,
            CancellationToken cancellationToken = default);

        // Revoga todas as sessões pertencentes a um usuário.
        Task RevogarSessoesDoUsuarioAsync(
            string usuarioId,
            string motivo,
            CancellationToken cancellationToken = default);
    }
}