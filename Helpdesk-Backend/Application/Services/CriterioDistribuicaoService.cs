using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    // Coordena o cadastro e as consultas dos critérios associados às regras
    // de distribuição, convertendo entidades e DTOs pelo AutoMapper.
    public class CriterioDistribuicaoService
        : ICriterioDistribuicaoService
    {
        private readonly IMapper _mapper;
        private readonly ICriterioDistribuicaoRepository _criterioRepository;

        public CriterioDistribuicaoService(
            IMapper mapper,
            ICriterioDistribuicaoRepository criterioDistribuicao)
        {
            _mapper = mapper;
            _criterioRepository = criterioDistribuicao;
        }

        // CADASTRO

        public async Task<CriterioDistribuicaoResponseDto> Adicionar(
            CriterioDistribuicaoRequestDto criterioDto)
        {
            var criterio =
                _mapper.Map<CriterioDistribuicao>(criterioDto);

            var criar = await _criterioRepository.Adicionar(criterio);

            return _mapper.Map<CriterioDistribuicaoResponseDto>(criar);
        }

        // CONSULTAS

        // Retorna somente os critérios vinculados à regra informada.
        public async Task<IEnumerable<CriterioDistribuicaoResponseDto>>
            BuscarCriteriosPorRegra(int regraId)
        {
            var criterios =
                await _criterioRepository.BuscarCriterioPorRegra(regraId);

            return _mapper.Map<
                IEnumerable<CriterioDistribuicaoResponseDto>>(criterios);
        }

        public async Task<CriterioDistribuicaoResponseDto> BuscarPorId(
            int id)
        {
            var criterio = await _criterioRepository.BuscarPorId(id);

            return _mapper.Map<CriterioDistribuicaoResponseDto>(criterio);
        }

        public async Task<IEnumerable<CriterioDistribuicaoResponseDto>>
            BuscarTodos()
        {
            var criterio = await _criterioRepository.BuscarTodos();

            return _mapper.Map<
                IEnumerable<CriterioDistribuicaoResponseDto>>(criterio);
        }

        // REMOÇÃO

        public async Task Remover(int id)
        {
            await _criterioRepository.Remover(id);
        }
    }
}
