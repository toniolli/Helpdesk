using Application.Interfaces;
using Domain.Enum;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace Helpdesk.Hubs
{
    // Publica fisicamente no SignalR os eventos que foram
    // recuperados e reservados pelo processador do Outbox.
    public class PublicadorSignalRService
        : IPublicadorSignalRService
    {
        private readonly IHubContext<AtualizacoesHub>
            _hubContext;

        private readonly ILogger<PublicadorSignalRService>
            _logger;

        public PublicadorSignalRService(
            IHubContext<AtualizacoesHub> hubContext,
            ILogger<PublicadorSignalRService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        // Publica o evento para usuários específicos ou para
        // um grupo do SignalR, conforme o destino armazenado.
        public async Task PublicarAsync(
            string nomeEvento,
            TipoDestinoEventoSignalREnum tipoDestino,
            IReadOnlyCollection<string> destinatarios,
            string? grupoDestino,
            string? payloadJson,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var clientes =
                    ObterClientes(
                        tipoDestino,
                        destinatarios,
                        grupoDestino);

                await EnviarEventoAsync(
                    clientes,
                    nomeEvento,
                    payloadJson,
                    cancellationToken);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao publicar o evento SignalR {NomeEvento}.",
                    nomeEvento);

                // O erro precisa chegar ao processador para que ele
                // registre a falha e reagende o evento.
                throw;
            }
        }

        // Localiza os clientes do SignalR conforme o tipo
        // de destino armazenado no evento do Outbox.
        private IClientProxy ObterClientes(
            TipoDestinoEventoSignalREnum tipoDestino,
            IReadOnlyCollection<string> destinatarios,
            string? grupoDestino)
        {
            if (tipoDestino ==
                TipoDestinoEventoSignalREnum.Usuarios)
            {
                if (destinatarios.Count == 0)
                {
                    throw new InvalidOperationException(
                        "O evento destinado a usuários não possui destinatários.");
                }

                return _hubContext.Clients.Users(
                    destinatarios.ToList());
            }

            if (tipoDestino ==
                TipoDestinoEventoSignalREnum.Grupo)
            {
                if (string.IsNullOrWhiteSpace(grupoDestino))
                {
                    throw new InvalidOperationException(
                        "O evento destinado a grupo não possui o nome do grupo.");
                }

                return _hubContext.Clients.Group(
                    grupoDestino);
            }

            throw new InvalidOperationException(
                "O tipo de destino do evento SignalR é inválido.");
        }

        // Envia o evento sem argumentos quando não existe payload
        // ou converte o JSON armazenado em um objeto JSON real.
        private static Task EnviarEventoAsync(
            IClientProxy clientes,
            string nomeEvento,
            string? payloadJson,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(payloadJson))
            {
                return clientes.SendCoreAsync(
                    nomeEvento,
                    Array.Empty<object?>(),
                    cancellationToken);
            }

            using var documentoJson =
                JsonDocument.Parse(payloadJson);

            var payload =
                documentoJson.RootElement.Clone();

            return clientes.SendCoreAsync(
                nomeEvento,
                new object?[] { payload },
                cancellationToken);
        }
    }
}