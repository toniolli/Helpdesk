using Domain.Entities;

namespace Application.Interfaces
{
    public interface IPerfilSlaService
    {
        Task<PeriodoTrabalhoPerfil> ObterPeriodoVigentePorPerfil(
            int perfilId);

        Task<PeriodoTrabalhoPerfil> ObterPeriodoVigentePorUsuario(
            string usuarioId);
    }
}
