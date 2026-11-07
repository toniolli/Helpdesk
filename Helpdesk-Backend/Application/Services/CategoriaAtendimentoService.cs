using Application.Dto.CategoriaAtendimentoDto;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Validation;

namespace Application.Services
{
    // Coordena o cadastro, a consulta e a situação das categorias de
    // atendimento, mantendo a lista de consulta armazenada em cache.
    public class CategoriaAtendimentoService : ICategoriaAtendimentoService
    {
        private const string ChaveCacheCategorias = "categorias:todos";

        private static readonly TimeSpan ExpiracaoCacheCategorias =
            TimeSpan.FromMinutes(30);

        private readonly IMapper _mapper;
        private readonly ICategoriaAtendimentoRepository _categoriaRepository;
        private readonly ICacheService _cacheService;

        public CategoriaAtendimentoService(
            IMapper mapper,
            ICategoriaAtendimentoRepository categoriaRepository,
            ICacheService cacheService)
        {
            _mapper = mapper;
            _categoriaRepository = categoriaRepository;
            _cacheService = cacheService;
        }

        // CADASTRO E ALTERAÇÃO

        // Cria a categoria e invalida a lista armazenada em cache.
        public async Task<CategoriaAtendimentoResponseDto> Criar(
            CategoriaAtendimentoRequestDto dto)
        {
            var categoria = _mapper.Map<CategoriaAtendimento>(dto);
            var criar = await _categoriaRepository.Adicionar(categoria);

            await _cacheService.Remover(ChaveCacheCategorias);

            return _mapper.Map<CategoriaAtendimentoResponseDto>(criar);
        }

        // Atualiza os dados da categoria e invalida a lista em cache.
        public async Task<CategoriaAtendimentoResponseDto> Atualizar(
            int id,
            CategoriaAtendimentoRequestDto dto)
        {
            var categoria = await _categoriaRepository.BuscarPorId(id);

            if (categoria == null)
            {
                throw new DomainExceptionValidation(
                    "Categoria de atendimento não encontrada.");
            }

            categoria.Atualizar(
                dto.Nome,
                dto.Descricao,
                dto.Prioridade);

            var atualizar = await _categoriaRepository.Atualizar(categoria);

            await _cacheService.Remover(ChaveCacheCategorias);

            return _mapper.Map<CategoriaAtendimentoResponseDto>(atualizar);
        }

        // CONSULTAS

        // Procura a categoria dentro da mesma lista utilizada pela consulta geral.
        public async Task<CategoriaAtendimentoResponseDto?> BuscarPorId(int id)
        {
            var categorias = await ObterCategoriasCacheadas();

            return categorias.FirstOrDefault(
                categoria => categoria.Id_CategoriaAtendimento == id);
        }

        public async Task<IEnumerable<CategoriaAtendimentoResponseDto>>
            BuscarTodos()
        {
            return await ObterCategoriasCacheadas();
        }

        // SITUAÇÃO

        // Ativa a categoria e invalida a lista em cache.
        public async Task Ativar(int id)
        {
            var categoria = await _categoriaRepository.BuscarPorId(id);

            if (categoria == null)
            {
                throw new DomainExceptionValidation(
                    "Categoria de atendimento não encontrada.");
            }

            categoria.Ativar();

            await _categoriaRepository.Atualizar(categoria);
            await _cacheService.Remover(ChaveCacheCategorias);
        }

        // Desativa a categoria e invalida a lista em cache.
        public async Task Desativar(int id)
        {
            var categoria = await _categoriaRepository.BuscarPorId(id);

            if (categoria == null)
            {
                throw new DomainExceptionValidation(
                    "Categoria de atendimento não encontrada.");
            }

            categoria.Desativar();

            await _categoriaRepository.Atualizar(categoria);
            await _cacheService.Remover(ChaveCacheCategorias);
        }

        public async Task Remover(int id)
        {
            await _categoriaRepository.Remover(id);
        }

        // MÉTODOS INTERNOS

        // Mantém a lista de categorias em cache por 30 minutos e consulta o
        // repositório somente quando a chave ainda não possui um valor.
        private async Task<IEnumerable<CategoriaAtendimentoResponseDto>>
            ObterCategoriasCacheadas()
        {
            return await _cacheService.ObterOuCriar(
                ChaveCacheCategorias,
                async () =>
                {
                    var categorias =
                        await _categoriaRepository.BuscarTodos();

                    return _mapper.Map<
                        IEnumerable<CategoriaAtendimentoResponseDto>>(
                            categorias);
                },
                ExpiracaoCacheCategorias);
        }
    }
}
