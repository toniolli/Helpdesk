using Application.Dto.Chamado;
using Application.Dto.RegraDistribuicaoDto;
using Application.DTOs;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IRegraDistribuicaoService
    {
        Task<RegraDistribuicaoResponseDto> Adicionar(RegraDistribuicaoRequestDto regraDto);
        Task<RegraDistribuicaoResponseDto> BuscarPorId(int id);
        Task<IEnumerable<RegraDistribuicaoResponseDto>> BuscarTodos();
        Task<RegraDistribuicaoResponseDto> Atualizar(int id, RegraDistribuicaoRequestDto regraDto);
        Task Remover(int id);
        Task Ativar (int id);
        Task Desativar(int id);
        Task<RegraDistribuicaoResponseDto> AdicionarCriterio(int id, CriterioDistribuicaoRequestDto criterio);
        Task RemoverCriterio(int criterioId);
        Task ProcessarDistribuicaoAutomatica(Chamado chamado);
        Task<IEnumerable<ChamadoResponseDto>> BuscarChamadosDisponiveis(int regraId);
        Task AplicarRegraChamados(AplicarRegraChamadosRequestDto request);

    }
}
