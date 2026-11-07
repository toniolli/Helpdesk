using Application.Dto.AuditoriaIdentity;
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
    // eventos de auditoria relacionados à identidade dos usuários.
    public class AuditoriaIdentityService : IAuditoriaIdentityService
    {
        private const int RetencaoDias = 730;
        private const int TamanhoLoteExclusao = 500;

        private readonly IAuditoriaIdentityRepository _auditoriaIdentityRepository;
        private readonly IMapper _mapper;

        public AuditoriaIdentityService(
            IAuditoriaIdentityRepository auditoriaIdentityRepository,
            IMapper mapper)
        {
            _auditoriaIdentityRepository = auditoriaIdentityRepository;
            _mapper = mapper;
        }

        // REGISTRO

        // Persiste um evento produzido pelos fluxos internos do backend.
        public async Task<AuditoriaIdentityResponseDto> Adicionar(
            AuditoriaIdentity auditoriaIdentity)
        {
            if (auditoriaIdentity == null)
            {
                throw new DomainExceptionValidation(
                    "A auditoria de Identity é obrigatória.");
            }

            var auditoriaCriada =
                await _auditoriaIdentityRepository.Adicionar(auditoriaIdentity);

            return _mapper.Map<AuditoriaIdentityResponseDto>(auditoriaCriada);
        }

        // CONSULTAS

        // Busca um evento específico pelo identificador da auditoria.
        public async Task<AuditoriaIdentityResponseDto?> BuscarPorId(int id)
        {
            if (id <= 0)
            {
                throw new DomainExceptionValidation(
                    "O identificador da auditoria de Identity é inválido.");
            }

            var auditoria = await _auditoriaIdentityRepository.BuscarPorId(id);

            if (auditoria == null)
            {
                return null;
            }

            return _mapper.Map<AuditoriaIdentityResponseDto>(auditoria);
        }

        // Retorna os eventos de Identity de forma paginada e filtrada.
        public async Task<PaginacaoResponseDto<AuditoriaIdentityResponseDto>>
            BuscarPaginado(
                DateTime dataInicial,
                DateTime dataFinal,
                string? usuarioId,
                EventoAuditoriaIdentityEnum? evento,
                bool? sucesso,
                PaginacaoRequestDto request)
        {
            ValidarPeriodo(dataInicial, dataFinal);

            var usuarioIdNormalizado = string.IsNullOrWhiteSpace(usuarioId)
                ? null
                : usuarioId.Trim();

            var resultado = await _auditoriaIdentityRepository.BuscarPaginado(
                dataInicial,
                dataFinal,
                usuarioIdNormalizado,
                evento,
                sucesso,
                request.Pagina,
                request.TamanhoPagina);

            var auditoriasDto =
                _mapper.Map<IEnumerable<AuditoriaIdentityResponseDto>>(
                    resultado.Auditorias);

            return new PaginacaoResponseDto<AuditoriaIdentityResponseDto>
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
                    await _auditoriaIdentityRepository.RemoverAnterioresA(
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
