using Domain.Entities;
using Domain.Enum;
using Domain.Interfaces;
using Infra.Context;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositores
{
    // Centraliza a persistência e as consultas das notificações.
    public class NotificacaoRepository : INotificacaoRepository
    {
        private readonly AppDbContext _notificacaoContext;

        public NotificacaoRepository(AppDbContext notificacaoContext)
        {
            _notificacaoContext = notificacaoContext;
        }

        // PERSISTÊNCIA

        public async Task<Notificacao> Adicionar(Notificacao notificacao)
        {
            await _notificacaoContext
                .Notificacoes
                .AddAsync(notificacao);

            await _notificacaoContext.SaveChangesAsync();

            return notificacao;
        }

        public async Task<Notificacao> Atualizar(Notificacao notificacao)
        {
            _notificacaoContext
                .Notificacoes
                .Update(notificacao);

            await _notificacaoContext.SaveChangesAsync();

            return notificacao;
        }

        // CONSULTAS

        public async Task<Notificacao> BuscarPorId(int id)
        {
            return await _notificacaoContext
                .Notificacoes
                .FindAsync(id);
        }

        public async Task<IEnumerable<Notificacao>> BuscarTodos()
        {
            return await _notificacaoContext
                .Notificacoes
                .ToListAsync();
        }

        // Restringe a consulta ao destinatário informado e retorna primeiro as
        // notificações mais recentes.
        public async Task<IEnumerable<Notificacao>> BuscarTodos(
            string usuarioId)
        {
            return await _notificacaoContext
                .Notificacoes
                .Where(x =>
                    x.UsuarioId == usuarioId)
                .OrderByDescending(x =>
                    x.DataCriacao)
                .ToListAsync();
        }

        // PAGINAÇÃO

        public async Task<(
            IEnumerable<Notificacao> Notificacao,
            int totalRegistro)> BuscarNotificacaoPaginado(
                int pagina,
                int tamanhoPagina)
        {
            var query = _notificacaoContext
                .Notificacoes
                .AsQueryable();

            var totalRegistro = await query.CountAsync();

            var notificacao = await query
                .OrderByDescending(x =>
                    x.DataCriacao)
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .ToListAsync();

            return (notificacao, totalRegistro);
        }

        public async Task<(
            IEnumerable<Notificacao> Notificacao,
            int totalRegistro)> BuscarNotificacaoPaginado(
                int pagina,
                int tamanhoPagina,
                string usuarioId)
        {
            var query = _notificacaoContext
                .Notificacoes
                .Where(x =>
                    x.UsuarioId == usuarioId);

            var totalRegistro = await query.CountAsync();

            var notificacoes = await query
                .OrderByDescending(x =>
                    x.DataCriacao)
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .ToListAsync();

            return (notificacoes, totalRegistro);
        }

        // LEITURA

        public async Task<IEnumerable<Notificacao>> ObterNaoLidas(
            bool lida)
        {
            return await _notificacaoContext
                .Notificacoes
                .Where(x =>
                    x.Lida == lida)
                .ToListAsync();
        }

        public async Task<IEnumerable<Notificacao>> ObterNaoLidas(
            bool lida,
            string usuarioId)
        {
            return await _notificacaoContext
                .Notificacoes
                .Where(x =>
                    x.Lida == lida &&
                    x.UsuarioId == usuarioId)
                .OrderByDescending(x =>
                    x.DataCriacao)
                .ToListAsync();
        }

        // Retorna somente as notificações ainda não lidas pelo usuário.
        public async Task<IEnumerable<Notificacao>> ObterPorUsuario(
            string usuarioId)
        {
            return await _notificacaoContext
                .Notificacoes
                .Where(x =>
                    x.UsuarioId == usuarioId &&
                    !x.Lida)
                .OrderByDescending(x =>
                    x.DataCriacao)
                .ToListAsync();
        }

        public async Task<int> TotalNotificacaoNaoLidas(
            string usuarioId)
        {
            return await _notificacaoContext
                .Notificacoes
                .Where(x =>
                    x.UsuarioId == usuarioId &&
                    !x.Lida)
                .CountAsync();
        }

        // DISTRIBUIÇÃO

        // Localiza os convites pendentes enviados ao perfil do chamado.
        public async Task<IEnumerable<Notificacao>>
            BuscarPendentesDistribuicaoPerfil(int chamadoId)
        {
            return await _notificacaoContext
                .Notificacoes
                .Where(x =>
                    x.ChamadoId == chamadoId &&
                    x.Status == StatusNotificacaoEnum.Pendente &&
                    x.Tipo == TipoNotificacaoEnum.ChamadoDistribuidoPerfil)
                .ToListAsync();
        }

        // Evita gerar outra notificação do mesmo tipo para o mesmo usuário.
        public async Task<bool> ExisteNotificacaoPorChamadoTipoEUsuario(
            int chamadoId,
            TipoNotificacaoEnum tipo,
            string usuarioId)
        {
            return await _notificacaoContext
                .Notificacoes
                .AnyAsync(notificacao =>
                    notificacao.ChamadoId == chamadoId &&
                    notificacao.Tipo == tipo &&
                    notificacao.UsuarioId == usuarioId);
        }
    }
}
