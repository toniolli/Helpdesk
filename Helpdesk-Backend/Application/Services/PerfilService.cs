using Application.Dto.UsuarioDto;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Validation;

namespace Application.Services
{
    // Coordena o cadastro dos perfis e o versionamento dos respectivos
    // períodos de trabalho.
    public class PerfilService : IPerfilService
    {
        private readonly IMapper _mapper;
        private readonly IperfilRepository _perfilRepository;

        public PerfilService(
            IMapper mapper,
            IperfilRepository perfilRepository)
        {
            _mapper = mapper;
            _perfilRepository = perfilRepository;
        }

        // CRIAÇÃO

        // Cria o perfil e, quando configurado, inicia sua primeira versão do
        // período de trabalho.
        public async Task<PerfilDto> Adicionar(PerfilDto perfilDto)
        {
            var perfil = new Perfil(
                perfilDto.Nome,
                perfilDto.Descricao);

            var possuiPeriodoTrabalho =
                perfilDto.PossuiPeriodoTrabalho ??
                perfilDto.PeriodoTrabalho != null;

            if (possuiPeriodoTrabalho)
            {
                if (perfilDto.PeriodoTrabalho == null)
                    throw new DomainExceptionValidation(
                        "O período de trabalho é obrigatório para este perfil.");

                perfil.DefinirPeriodoTrabalho(
                    CriarPeriodoTrabalho(
                        perfilDto.PeriodoTrabalho),
                    DateTime.Now);
            }

            var criar = await _perfilRepository.Adicionar(perfil);

            return _mapper.Map<PerfilDto>(criar);
        }

        // ALTERAÇÃO

        // Atualiza os dados e aplica a decisão recebida sobre o período:
        // encerrar, exigir uma configuração ou criar uma nova versão.
        public async Task<PerfilDto> Atualizar(
            int id,
            PerfilDto perfilDto)
        {
            var perfil = await _perfilRepository.BuscarPorId(id);

            if (perfil == null)
                throw new DomainExceptionValidation(
                    "Perfil não encontrado.");

            perfil.Atualizar(
                perfilDto.Nome,
                perfilDto.Descricao);

            if (perfilDto.PossuiPeriodoTrabalho == false)
            {
                perfil.EncerrarPeriodoTrabalho(DateTime.Now);
            }
            else if (perfilDto.PossuiPeriodoTrabalho == true &&
                     perfilDto.PeriodoTrabalho == null)
            {
                throw new DomainExceptionValidation(
                    "O período de trabalho é obrigatório para este perfil.");
            }
            else if (perfilDto.PeriodoTrabalho != null)
            {
                perfil.DefinirPeriodoTrabalho(
                    CriarPeriodoTrabalho(
                        perfilDto.PeriodoTrabalho),
                    DateTime.Now);
            }

            var criar = await _perfilRepository.Atualizar(perfil);

            return _mapper.Map<PerfilDto>(criar);
        }

        // CONSULTAS

        public async Task<PerfilDto?> BuscarPorId(int id)
        {
            var perfil = await _perfilRepository.BuscarPorId(id);

            return _mapper.Map<PerfilDto>(perfil);
        }

        public async Task<IEnumerable<PerfilDto>> BuscarTodos()
        {
            var perfil = await _perfilRepository.BuscarTodos();

            return _mapper.Map<IEnumerable<PerfilDto>>(perfil);
        }

        // REMOÇÃO E ESTADO

        // Impede a remoção quando existe histórico de período de trabalho,
        // preservando as referências utilizadas pelos chamados.
        public async Task Remover(int id)
        {
            var perfil = await _perfilRepository.BuscarPorId(id);

            if (perfil == null)
                throw new DomainExceptionValidation(
                    "Perfil não encontrado.");

            if (perfil.PeriodosTrabalho.Any())
                throw new DomainExceptionValidation(
                    "O perfil possui histórico de período de trabalho e não pode ser excluído. Desative o perfil.");

            await _perfilRepository.Remover(id);
        }

        public async Task Ativar(int id)
        {
            var perfil = await _perfilRepository.BuscarPorId(id);

            if (perfil == null)
                throw new DomainExceptionValidation(
                    "Perfil de atendimento não encontrada.");

            perfil.Ativar();

            await _perfilRepository.Atualizar(perfil);
        }

        public async Task Desativar(int id)
        {
            var perfil = await _perfilRepository.BuscarPorId(id);

            if (perfil == null)
                throw new DomainExceptionValidation(
                    "Perfil de atendimento não encontrada.");

            perfil.Desativar();

            await _perfilRepository.Atualizar(perfil);
        }

        // MÉTODOS INTERNOS

        // Converte os dados recebidos para a entidade do período sem alterar
        // nenhuma regra de validação do domínio.
        private static PeriodoTrabalhoPerfil CriarPeriodoTrabalho(
            PeriodoTrabalhoPerfilDto dto)
        {
            return new PeriodoTrabalhoPerfil(
                dto.SegundaAtivo,
                dto.TercaAtivo,
                dto.QuartaAtivo,
                dto.QuintaAtivo,
                dto.SextaAtivo,
                dto.HoraInicioSemana,
                dto.HoraFimSemana,
                dto.SabadoAtivo,
                dto.HoraInicioSabado,
                dto.HoraFimSabado,
                dto.DomingoAtivo,
                dto.HoraInicioDomingo,
                dto.HoraFimDomingo);
        }
    }
}
