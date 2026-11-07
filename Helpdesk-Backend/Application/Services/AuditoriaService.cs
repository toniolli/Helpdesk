using Application.Dto.Auditoria;
using Application.Dto.Paginacao;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enum;
using Domain.Interfaces;
using Domain.Validation;

namespace Application.Services
{
    // Coordena o registro, as consultas administrativas e a retenção dos
    // eventos da auditoria geral do sistema.
    public class AuditoriaService : IAuditoriaService
    {
        private const int RetencaoDias = 730;
        private const int TamanhoLoteExclusao = 500;

        private readonly IAuditoriaRepository _auditoriaRepository;
        private readonly IMapper _mapper;

        public AuditoriaService(
            IAuditoriaRepository auditoriaRepository,
            IMapper mapper)
        {
            _auditoriaRepository = auditoriaRepository;
            _mapper = mapper;
        }

        // REGISTRO

        // Persiste um evento produzido pelos fluxos internos do backend.
        public async Task<AuditoriaResponseDto> Adicionar(Auditoria auditoria)
        {
            if (auditoria == null)
            {
                throw new DomainExceptionValidation(
                    "A auditoria é obrigatória.");
            }

            var auditoriaCriada =
                await _auditoriaRepository.Adicionar(auditoria);

            return _mapper.Map<AuditoriaResponseDto>(auditoriaCriada);
        }

        // CONSULTAS

        // Busca um evento específico pelo identificador da auditoria.
        public async Task<AuditoriaResponseDto?> BuscarPorId(int id)
        {
            if (id <= 0)
            {
                throw new DomainExceptionValidation(
                    "O identificador da auditoria é inválido.");
            }

            var auditoria = await _auditoriaRepository.BuscarPorId(id);

            if (auditoria == null)
            {
                return null;
            }

            return _mapper.Map<AuditoriaResponseDto>(auditoria);
        }

        // Retorna os eventos da auditoria geral de forma paginada e filtrada.
        public async Task<PaginacaoResponseDto<AuditoriaResponseDto>>
            BuscarPaginado(
                DateTime dataInicial,
                DateTime dataFinal,
                string? usuarioId,
                AcaoAuditoriaEnum? acao,
                string? entidade,
                PaginacaoRequestDto request)
        {
            ValidarPeriodo(dataInicial, dataFinal);

            var usuarioIdNormalizado = string.IsNullOrWhiteSpace(usuarioId)
                ? null
                : usuarioId.Trim();

            var entidadeNormalizada = string.IsNullOrWhiteSpace(entidade)
                ? null
                : entidade.Trim();

            var resultado = await _auditoriaRepository.BuscarPaginado(
                dataInicial,
                dataFinal,
                usuarioIdNormalizado,
                acao,
                entidadeNormalizada,
                request.Pagina,
                request.TamanhoPagina);

            var auditoriasDto = _mapper.Map<IEnumerable<AuditoriaResponseDto>>(
                resultado.Auditorias);

            return new PaginacaoResponseDto<AuditoriaResponseDto>
            {
                Pagina = request.Pagina,
                TamanhoPagina = request.TamanhoPagina,
                TotalRegistros = resultado.TotalRegistros,
                TotalPaginas = (int)Math.Ceiling(
                    resultado.TotalRegistros /
                    (double)request.TamanhoPagina),
                Dados = auditoriasDto
            };
        }

        // RETENÇÃO

        // Remove registros anteriores ao período de retenção em lotes para
        // evitar uma operação de exclusão excessivamente grande.
        public async Task<int> RemoverExpiradas()
        {
            var dataLimite = DateTime.UtcNow.AddDays(-RetencaoDias);

            var totalRemovido = 0;
            int quantidadeRemovida;

            do
            {
                quantidadeRemovida =
                    await _auditoriaRepository.RemoverAnterioresA(
                        dataLimite,
                        TamanhoLoteExclusao);

                totalRemovido += quantidadeRemovida;
            }
            while (quantidadeRemovida == TamanhoLoteExclusao);

            return totalRemovido;
        }

        // MÉTODOS INTERNOS

        // Impede consultas com período ausente ou cronologicamente inválido.
        private static void ValidarPeriodo(
            DateTime dataInicial,
            DateTime dataFinal)
        {
            if (dataInicial == default)
            {
                throw new DomainExceptionValidation(
                    "A data inicial é obrigatória.");
            }

            if (dataFinal == default)
            {
                throw new DomainExceptionValidation(
                    "A data final é obrigatória.");
            }

            if (dataInicial.Date > dataFinal.Date)
            {
                throw new DomainExceptionValidation(
                    "A data inicial não pode ser maior que a data final.");
            }
        }
    }
}
