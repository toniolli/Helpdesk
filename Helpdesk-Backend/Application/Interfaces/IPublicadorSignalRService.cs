using Domain.Enum;

namespace Application.Interfaces
{
    // Define a publicação física de um evento no SignalR.
    // A implementação ficará na camada Helpdesk, junto ao Hub.
    public interface IPublicadorSignalRService
    {
        Task PublicarAsync(
            string nomeEvento,
            TipoDestinoEventoSignalREnum tipoDestino,
            IReadOnlyCollection<string> destinatarios,
            string? grupoDestino,
            string? payloadJson,
            CancellationToken cancellationToken = default);
    }
}