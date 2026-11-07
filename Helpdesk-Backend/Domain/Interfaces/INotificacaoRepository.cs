using Domain.Entities;
using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface INotificacaoRepository
    {
        Task<Notificacao> Adicionar(Notificacao notificacao);
        Task<Notificacao> Atualizar (Notificacao notificacao);
        Task<Notificacao> BuscarPorId(int id);
        Task<IEnumerable<Notificacao>> BuscarTodos();
        Task<IEnumerable<Notificacao>> BuscarTodos(string usuarioId);
        Task<(IEnumerable<Notificacao> Notificacao, int totalRegistro)> BuscarNotificacaoPaginado(int pagina, int tamanhoPagina);
        Task<(IEnumerable<Notificacao> Notificacao, int totalRegistro)> BuscarNotificacaoPaginado(int pagina, int tamanhoPagina, string usuarioId);
        Task<IEnumerable<Notificacao>> ObterPorUsuario(string usuarioId);
        Task<IEnumerable<Notificacao>> ObterNaoLidas(bool lida);
        Task<IEnumerable<Notificacao>> ObterNaoLidas(bool lida, string usuarioId);
        Task<int> TotalNotificacaoNaoLidas(string usuarioId);
        Task <IEnumerable<Notificacao>> BuscarPendentesDistribuicaoPerfil(int chamadoId);
        Task<bool> ExisteNotificacaoPorChamadoTipoEUsuario(int chamadoId,TipoNotificacaoEnum tipo,string usuarioId);
    }
}
