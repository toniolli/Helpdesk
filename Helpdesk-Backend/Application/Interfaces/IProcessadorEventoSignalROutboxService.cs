namespace Application.Interfaces
{
    // Define o processamento e a limpeza dos eventos persistidos
    // na tabela de Outbox do SignalR.
    public interface IProcessadorEventoSignalROutboxService
    {
        Task ProcessarPendentesAsync(
            CancellationToken cancellationToken);

        Task RemoverEnviadosExpiradosAsync(
            CancellationToken cancellationToken);

        Task RecuperarProcessamentosExpiradosAsync(
             CancellationToken cancellationToken);
    }
}