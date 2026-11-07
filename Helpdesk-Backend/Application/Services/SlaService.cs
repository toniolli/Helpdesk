using Application.Dto.SlaDto;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Validation;

namespace Application.Services
{
    // Coordena as configurações de SLA, o cache das consultas e os cálculos
    // aplicados aos chamados.
    public class SlaService : ISlaService
    {
        private readonly ISlaRepository _slaREpository;
        private readonly IMapper _mapper;
        private readonly IChamadoRepository _chamadoRepository;

        private const string ChaveCacheSla = "sla:todos";

        private static readonly TimeSpan ExpiracaoCacheSla =
            TimeSpan.FromMinutes(60);

        private readonly ICacheService _cacheService;

        public SlaService(
            ISlaRepository slaREpository,
            IMapper mapper,
            IChamadoRepository chamadoRepository,
            ICacheService cacheService)
        {
            _slaREpository = slaREpository;
            _mapper = mapper;
            _chamadoRepository = chamadoRepository;
            _cacheService = cacheService;
        }

        // CADASTRO E ALTERAÇÃO

        // Após qualquer mudança, remove o cache para a próxima consulta
        // carregar os valores atualizados.
        public async Task<SlaResponseDto> Adicionar(
            SlaRequestDto slaDto)
        {
            var sla = _mapper.Map<SLA>(slaDto);

            var criar = await _slaREpository.Adicionar(sla);

            await _cacheService.Remover(ChaveCacheSla);

            return _mapper.Map<SlaResponseDto>(criar);
        }

        public async Task<SlaResponseDto> Atualizar(
            int id,
            SlaRequestDto slaDto)
        {
            var Sla = await _slaREpository.BuscarPorId(id);

            if (Sla == null)
                throw new DomainExceptionValidation(
                    "Sla não encontrado!");

            Sla.Atualizar(
                slaDto.TempoAtendimentoHoras,
                slaDto.TempoResolucaoHoras);

            await _slaREpository.Atualizar(Sla);
            await _cacheService.Remover(ChaveCacheSla);

            return _mapper.Map<SlaResponseDto>(Sla);
        }

        // CONSULTAS

        public async Task<SlaResponseDto?> BuscarPrioridade(
            PrioridadeEnum prioridade)
        {
            var slas = await ObterSlaCacheadas();

            return slas.FirstOrDefault(
                sla => sla.Prioridade == prioridade);
        }

        public async Task<SlaResponseDto?> BuscarPorId(int id)
        {
            var sla = await ObterSlaCacheadas();

            return sla.FirstOrDefault(
                sla => sla.Id_Sla == id);
        }

        public async Task<IEnumerable<SlaResponseDto>> BuscarTodos()
        {
            return await ObterSlaCacheadas();
        }

        // ESTADO E REMOÇÃO

        public async Task Ativar(int id)
        {
            var Sla = await _slaREpository.BuscarPorId(id);

            if (Sla == null)
                throw new DomainExceptionValidation(
                    "Permissao de atendimento não encontrada.");

            Sla.Ativar();

            await _slaREpository.Atualizar(Sla);
            await _cacheService.Remover(ChaveCacheSla);
        }

        public async Task Desativar(int id)
        {
            var Sla = await _slaREpository.BuscarPorId(id);

            if (Sla == null)
                throw new DomainExceptionValidation(
                    "Permissao de atendimento não encontrada.");

            Sla.Desativar();

            await _slaREpository.Atualizar(Sla);
            await _cacheService.Remover(ChaveCacheSla);
        }

        public async Task Remover(int id)
        {
            await _slaREpository.Remover(id);
        }

        // CÁLCULO DO PRIMEIRO ATENDIMENTO

        // Valida o acesso ao chamado e compara o tempo útil calculado pela
        // entidade com o limite de primeiro atendimento do SLA.
        public async Task<SlaResponseDto> CalcularTempoAtendimento(
            int chamadoId,
            int id,
            string usuarioLogadoId,
            bool podeVisualizar)
        {
            var chamado =
                await _chamadoRepository.BuscarPorId(chamadoId);

            if (chamado == null)
                throw new DomainExceptionValidation(
                    "Chamado não encontrado!");

            ValidarAcessoAoChamado(
                chamado,
                usuarioLogadoId,
                podeVisualizar);

            var sla =
                await _slaREpository.BuscarPorId(id);

            if (sla == null)
                throw new DomainExceptionValidation(
                    "SLA não encontrado");

            if (!chamado.DataPrimeiroAtendimento.HasValue)
                throw new DomainExceptionValidation(
                    "Chamado ainda não possui atendimento");

            var tempoAtendimentoHoras =
                chamado.CalcularTempoPrimeiroAtendimentoSlaHoras(
                    DateTime.Now);

            if (!tempoAtendimentoHoras.HasValue)
                throw new DomainExceptionValidation(
                    "O SLA do chamado está pendente porque ainda não possui um período de trabalho definido.");

            var tempoAtendimento = TimeSpan.FromHours(
                tempoAtendimentoHoras.Value);

            var dentroPrazo =
                sla.AtendimentoDentroPrazo(
                    tempoAtendimento);

            return new SlaResponseDto
            {
                Id_Sla = sla.Id_SLA,
                Prioridade = sla.Prioridade,
                TempoAtendimentoHoras = sla.TempoAtendimentoHoras,
                TempoResolucaoHoras = sla.TempoResolucaoHoras,
                Ativo = sla.Ativo,
                TempoAtendimentoRealHoras = tempoAtendimentoHoras.Value,
                AtendimentoDentroPrazo = dentroPrazo
            };
        }

        // CÁLCULO DA RESOLUÇÃO

        // Valida o acesso ao chamado e compara o tempo útil de resolução com
        // o limite configurado no SLA.
        public async Task<SlaResponseDto> CalcularTempoResolucao(
            int chamadoId,
            int id,
            string usuarioLogadoId,
            bool podeVisualizar)
        {
            var chamado =
                await _chamadoRepository.BuscarPorId(chamadoId);

            if (chamado == null)
                throw new DomainExceptionValidation(
                    "Chamado não encontrado!");

            ValidarAcessoAoChamado(
                chamado,
                usuarioLogadoId,
                podeVisualizar);

            var sla =
                await _slaREpository.BuscarPorId(id);

            if (sla == null)
                throw new DomainExceptionValidation(
                    "SLA não encontrado");

            if (!chamado.DataFechamento.HasValue)
                throw new DomainExceptionValidation(
                    "Chamado ainda não foi encerrado");

            var tempoResolucaoHoras =
                chamado.CalcularTempoResolucaoSlaHoras(
                    DateTime.Now);

            if (!tempoResolucaoHoras.HasValue)
                throw new DomainExceptionValidation(
                    "O SLA do chamado está pendente porque ainda não possui um período de trabalho definido.");

            var tempoResolucao = TimeSpan.FromHours(
                tempoResolucaoHoras.Value);

            var dentroPrazo =
                sla.ResolucaoDentroPrazo(
                    tempoResolucao);

            return new SlaResponseDto
            {
                Id_Sla = sla.Id_SLA,
                Prioridade = sla.Prioridade,
                TempoAtendimentoHoras = sla.TempoAtendimentoHoras,
                TempoResolucaoHoras = sla.TempoResolucaoHoras,
                Ativo = sla.Ativo,
                TempoResolucaoRealHoras = tempoResolucaoHoras.Value,
                ResolucaoDentroPrazo = dentroPrazo
            };
        }

        // MÉTODOS INTERNOS

        // Permite o acesso ao participante do chamado ou a quem possui a
        // permissão global recebida pelo endpoint.
        private static void ValidarAcessoAoChamado(
            Chamado chamado,
            string usuarioLogadoId,
            bool podeVisualizar)
        {
            var participaDoChamado =
                chamado.SolicitanteId == usuarioLogadoId ||
                chamado.TecnicoResponsavelId == usuarioLogadoId;

            if (!participaDoChamado && !podeVisualizar)
                throw new AcessoNegadoException(
                    "Você não possui acesso a este chamado.");
        }

        // Mantém as consultas de SLA em cache durante o período configurado.
        private async Task<IEnumerable<SlaResponseDto>> ObterSlaCacheadas()
        {
            return await _cacheService.ObterOuCriar(
                ChaveCacheSla,
                async () =>
                {
                    var sla =
                        await _slaREpository.BuscarTodos();

                    return _mapper.Map<
                        IEnumerable<SlaResponseDto>>(
                        sla);
                },
                ExpiracaoCacheSla);
        }
    }
}
