using Application.Dto.NotificacaoDto;
using Application.Dto.Paginacao;
using Domain.Entities;
using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface INotificacaoService
    {
        Task<NotificacaoResponseDto> Adicionar(NotificacaoRequestDto dto, bool agendarAtualizacaoTempoReal = true);
        // Uso interno
        Task<NotificacaoResponseDto> BuscarPorIdInterno(int id);
        Task MarcarComoLidaInterno(int id);
        Task AceitarInterno(int id);
        Task RecusarInterno(int id);
        Task ExpirarInterno(int id);
        // Uso pelos endpoints, com validação de acesso
        Task<NotificacaoResponseDto> BuscarPorId(int id,string usuarioLogadoId,bool podeVisualizar);
        Task<IEnumerable<NotificacaoResponseDto>> BuscarTodos(string usuarioLogadoId,bool podeVisualizar);
        Task<PaginacaoResponseDto<NotificacaoResponseDto>>BuscarNotificacaoPaginado(PaginacaoRequestDto request, string usuarioLogadoId, bool podeVisualizar);
        Task<IEnumerable<NotificacaoResponseDto>> ObterPorUsuario(string usuarioId, string usuarioLogadoId,bool podeVisualizar);
        Task<IEnumerable<NotificacaoResponseDto>> ObterNaoLidas(bool lida, string usuarioLogadoId, bool podeVisualizar);
        Task MarcarComoLida(int id,string usuarioLogadoId, bool podeVisualizar);
        Task Aceitar(int id,string usuarioLogadoId,bool podeVisualizar);
        Task Recusar(int id, string usuarioLogadoId, bool podeVisualizar);
        Task Expirar(int id,  string usuarioLogadoId, bool podeVisualizar);
        Task Pendente(int id,string usuarioLogadoId,bool podeVisualizar);
        Task<int> TotalNotificacaoNaoLidas(string usuarioId,string usuarioLogadoId, bool podeVisualizar);
        // Fluxos internos de distribuição
        Task<IEnumerable<NotificacaoResponseDto>>BuscarPendentesDistribuicaoPerfil(int chamadoId);
        Task<bool> ExisteNotificacaoPorChamadoTipoEUsuario(int chamadoId,TipoNotificacaoEnum tipo,string usuarioId);
    }
}
