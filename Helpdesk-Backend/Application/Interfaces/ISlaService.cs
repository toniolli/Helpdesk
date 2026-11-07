using Application.Dto.SlaDto;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ISlaService
    {
        Task<SlaResponseDto> Adicionar(SlaRequestDto slaDto);
        Task<SlaResponseDto?> BuscarPorId(int id);
        Task<SlaResponseDto?> BuscarPrioridade(PrioridadeEnum prioridade);
        Task<IEnumerable<SlaResponseDto>> BuscarTodos();
        Task<SlaResponseDto> Atualizar(int id, SlaRequestDto slaDto);
        Task Remover(int id);
        Task Ativar(int id);
        Task Desativar(int id);
        Task<SlaResponseDto> CalcularTempoAtendimento(
            int chamadoId,
            int id,
            string usuarioLogadoId,
            bool podeVisualizar);
        Task<SlaResponseDto> CalcularTempoResolucao(
            int chamadoId,
            int id,
            string usuarioLogadoId,
            bool podeVisualizar);

    }
}
