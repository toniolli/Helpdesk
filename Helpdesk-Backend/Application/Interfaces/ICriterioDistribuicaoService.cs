using Application.DTOs;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ICriterioDistribuicaoService
    {
        Task<CriterioDistribuicaoResponseDto> Adicionar(CriterioDistribuicaoRequestDto criterioDto);
        Task<CriterioDistribuicaoResponseDto> BuscarPorId(int id);
        Task<IEnumerable<CriterioDistribuicaoResponseDto>> BuscarTodos();
        Task Remover(int id);
        Task<IEnumerable<CriterioDistribuicaoResponseDto>> BuscarCriteriosPorRegra(int regraId);

    }
}
