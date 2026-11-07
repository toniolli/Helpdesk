using Application.Dto.AuditoriaChamado;
using Application.Dto.Paginacao;
using Application.Interfaces;
using AutoMapper;
using Domain.Interfaces;
using Domain.Validation;

namespace Application.Services
{
    // Organiza as consultas do histórico de chamados, valida os filtros
    // recebidos e converte as entidades para os modelos de resposta.
    public class AuditoriaChamadoService : IAuditoriaChamadoService
    {
        private const int RetencaoDias = 730;
        private const int TamanhoLoteExclusao = 500;
        private readonly IAuditoriaChamadoRepository _auditoriaChamadoRepository;
        private readonly IMapper _mapper;

        public AuditoriaChamadoService(
            IAuditoriaChamadoRepository auditoriaChamadoRepository,
            IMapper mapper)
        {
            _auditoriaChamadoRepository = auditoriaChamadoRepository;
            _mapper = mapper;
        }

        // CONSULTAS

        // Busca um evento específico da auditoria pelo seu identificador.
        public async Task<AuditoriaChamadoResponseDto?> BuscarPorId(int id)
        {
            if (id <= 0)
            {
                throw new DomainExceptionValidation(
                    "O identificador da auditoria é inválido.");
            }

            var auditoria = await _auditoriaChamadoRepository.BuscarPorId(id);

            return auditoria == null
                ? null
                : _mapper.Map<AuditoriaChamadoResponseDto>(auditoria);
        }

        // Retorna todo o histórico de um chamado na ordem em que ocorreu.
        public async Task<IEnumerable<AuditoriaChamadoResponseDto>>
            BuscarPorChamado(int chamadoId)
        {
            if (chamadoId <= 0)
            {
                throw new DomainExceptionValidation(
                    "O identificador do chamado é inválido.");
            }

            var auditorias = await _auditoriaChamadoRepository
                .BuscarPorChamado(chamadoId);

            return _mapper.Map<IEnumerable<AuditoriaChamadoResponseDto>>(
                auditorias);
        }

        // Retorna o histórico geral paginado conforme os filtros informados.
        public async Task<PaginacaoResponseDto<AuditoriaChamadoResponseDto>>
            BuscarPaginado(
                AuditoriaChamadoFiltroDto filtro,
                PaginacaoRequestDto paginacao)
        {
            ValidarPeriodo(filtro.DataInicial, filtro.DataFinal);

            var resultado = await _auditoriaChamadoRepository.BuscarPaginado(
                filtro.DataInicial,
                filtro.DataFinal,
                Normalizar(filtro.NumeroChamado),
                Normalizar(filtro.UsuarioId),
                Normalizar(filtro.PerfilUsuario),
                filtro.Evento,
                filtro.Status,
                filtro.CategoriaAtendimentoId,
                paginacao.Pagina,
                paginacao.TamanhoPagina);

            return new PaginacaoResponseDto<AuditoriaChamadoResponseDto>
            {
                dataInicial = filtro.DataInicial,
                dataFinal = filtro.DataFinal,
                Pagina = paginacao.Pagina,
                TamanhoPagina = paginacao.TamanhoPagina,
                TotalRegistros = resultado.TotalRegistros,
                TotalPaginas = (int)Math.Ceiling(
                    resultado.TotalRegistros /
                    (double)paginacao.TamanhoPagina),
                Dados = _mapper.Map<IEnumerable<AuditoriaChamadoResponseDto>>(
                    resultado.Auditorias)
            };
        }

        // RETENÇÃO

        // Remove auditorias de chamado anteriores ao período de retenção,
        // processando a exclusão em lotes.
        public async Task<int> RemoverExpiradas()
        {
            var dataLimite = DateTime.UtcNow.AddDays(-RetencaoDias);

            var totalRemovido = 0;
            int quantidadeRemovida;

            do
            {
                quantidadeRemovida =
                    await _auditoriaChamadoRepository.RemoverAnterioresA(
                        dataLimite,
                        TamanhoLoteExclusao);

                totalRemovido += quantidadeRemovida;
            }
            while (quantidadeRemovida == TamanhoLoteExclusao);

            return totalRemovido;
        }



        // MÉTODOS INTERNOS

        // Remove espaços excedentes e converte textos vazios em ausência de filtro.
        private static string? Normalizar(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor)
                ? null
                : valor.Trim();
        }

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
