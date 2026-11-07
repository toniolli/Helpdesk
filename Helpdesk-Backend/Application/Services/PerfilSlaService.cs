using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Validation;

namespace Application.Services
{
    // Resolve o período de trabalho vigente utilizado nos cálculos de SLA,
    // validando o perfil e, quando necessário, o usuário responsável.
    public class PerfilSlaService : IPerfilSlaService
    {
        private readonly IperfilRepository _perfilRepository;
        private readonly IusuarioService _usuarioService;

        public PerfilSlaService(
            IperfilRepository perfilRepository,
            IusuarioService usuarioService)
        {
            _perfilRepository = perfilRepository;
            _usuarioService = usuarioService;
        }

        // CONSULTA POR PERFIL

        // Retorna o período vigente somente quando o perfil existe, está
        // ativo e possui uma configuração de trabalho disponível.
        public async Task<PeriodoTrabalhoPerfil>
            ObterPeriodoVigentePorPerfil(int perfilId)
        {
            var perfil = await _perfilRepository.BuscarPorId(perfilId);

            if (perfil == null)
                throw new DomainExceptionValidation(
                    "Perfil de atendimento não encontrado.");

            if (!perfil.Ativo)
                throw new DomainExceptionValidation(
                    "O perfil de atendimento está inativo.");

            var periodo = perfil.PeriodoTrabalhoVigente;

            if (periodo == null)
                throw new DomainExceptionValidation(
                    "Configure o período de trabalho do perfil antes de distribuir ou assumir chamados.");

            return periodo;
        }

        // CONSULTA POR USUÁRIO

        // Valida o usuário e utiliza seu perfil atual para resolver o período
        // de trabalho que será aplicado ao SLA.
        public async Task<PeriodoTrabalhoPerfil>
            ObterPeriodoVigentePorUsuario(string usuarioId)
        {
            if (string.IsNullOrWhiteSpace(usuarioId))
                throw new DomainExceptionValidation(
                    "Usuário é obrigatório para definir o período do SLA.");

            var usuario = await _usuarioService
                .BuscarPorIdInterno(usuarioId);

            if (usuario == null)
                throw new DomainExceptionValidation(
                    "Usuário não encontrado.");

            if (!usuario.Ativo)
                throw new DomainExceptionValidation(
                    "O usuário está inativo.");

            if (!usuario.Id_Perfil.HasValue)
                throw new DomainExceptionValidation(
                    "O usuário precisa possuir um perfil para definir o período do SLA.");

            return await ObterPeriodoVigentePorPerfil(
                usuario.Id_Perfil.Value);
        }
    }
}
