using Application.Interfaces;
using Application.Services;
using Helpdesk.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace Helpdesk.Hubs
{
    // Ponto de conexão do SignalR. Somente usuários autenticados conseguem abrir a conexão.
    // O Hub não executa regras de negócio: ele apenas organiza cada conexão nos grupos corretos.
    [Authorize]
    public class AtualizacoesHub : Hub
    {
        private const string GrupoCentralAtendimento = "central-atendimento";
        private const string EventoCentralAtendimentoAtualizada = "centralAtendimentoAtualizada";
        private readonly ICentralAtendimentoTempoRealService _centralAtendimentoTempoRealService;
        private readonly ICentralAtendimentoService _centralAtendimentoService;

        public AtualizacoesHub(ICentralAtendimentoTempoRealService centralAtendimentoTempoRealService,
            ICentralAtendimentoService centralAtendimentoService)
        {
            _centralAtendimentoTempoRealService = centralAtendimentoTempoRealService;
            _centralAtendimentoService = centralAtendimentoService;
        }

        // Executado automaticamente sempre que uma nova conexão é estabelecida.
        public override async Task OnConnectedAsync()
        {
            // Usuários com acesso ao dashboard entram no grupo que recebe atualizações gerenciais.
            var podeVisualizarTodosChamados = Context.User?
                .HasClaim("Permissao", "VISUALIZAR_DASHBOARD") == true;

            // Usuários com acesso à fila entram no grupo que recebe alterações da fila de atendimento.
            var podeVisualizarFila =Context.User?
                .HasClaim("Permissao", "VISUALIZAR_FILA") == true;

            if (podeVisualizarTodosChamados)
            {
                // O grupo permite enviar um único evento para todos os dashboards conectados.
                await Groups.AddToGroupAsync(
                    Context.ConnectionId,
                    "dashboard-global");
            }

            if (podeVisualizarFila)
            {
                // O grupo permite atualizar a fila sem conhecer individualmente cada usuário conectado.
                await Groups.AddToGroupAsync(
                    Context.ConnectionId, "fila-atendimento"); 
            }

            // Finaliza o ciclo padrão de conexão do SignalR.
            await base.OnConnectedAsync();
        }

        [Permissao("VISUALIZAR_CENTRAL_ATENDIMENTO")]
        public async Task EntrarCentralAtendimento()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GrupoCentralAtendimento, Context.ConnectionAborted);

            _centralAtendimentoTempoRealService.AdicionarConexao(Context.ConnectionId);


            var fotografia = await _centralAtendimentoService.ObterFotografiaAsync(Context.ConnectionAborted);

            await Clients.Caller.SendAsync(EventoCentralAtendimentoAtualizada,
                fotografia,
                Context.ConnectionAborted); 
        }


        // Remove a conexão quando o frontend sai normalmente da Central.
        [Permissao("VISUALIZAR_CENTRAL_ATENDIMENTO")]
        public async Task SairCentralAtendimento()
        {
            try
            {
                await Groups.RemoveFromGroupAsync(
                    Context.ConnectionId,
                    GrupoCentralAtendimento,
                    Context.ConnectionAborted);
            }
            finally
            {
                _centralAtendimentoTempoRealService
                    .RemoverConexao(
                        Context.ConnectionId);
            }
        }

        // Remove o registro da conexão quando ocorre fechamento,
        // queda de internet ou encerramento inesperado do navegador.
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _centralAtendimentoTempoRealService
                .RemoverConexao(
                    Context.ConnectionId);

            await base.OnDisconnectedAsync(exception);
        }

    }
}
