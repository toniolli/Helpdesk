using Application.Interfaces;

namespace Helpdesk.BackgroundServices
{
    /// <summary>
    /// Processa os eventos pendentes, recupera processamentos
    /// interrompidos e remove eventos enviados expirados.
    /// </summary>
    public sealed class EventoSignalROutboxProcessamentoBackgroundService: BackgroundService
    {
        private static readonly TimeSpan IntervaloProcessamento =
            TimeSpan.FromSeconds(2);

        private static readonly TimeSpan IntervaloRecuperacao =
            TimeSpan.FromMinutes(1);

        private static readonly TimeSpan IntervaloLimpeza =
            TimeSpan.FromMinutes(10);

        private DateTime _proximaRecuperacaoUtc =
            DateTime.MinValue;

        private readonly IServiceScopeFactory
            _scopeFactory;

        private readonly ILogger<
            EventoSignalROutboxProcessamentoBackgroundService>
            _logger;

        public EventoSignalROutboxProcessamentoBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<EventoSignalROutboxProcessamentoBackgroundService>
                logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        // CICLO PRINCIPAL

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            try
            {
                // Mantém o processamento e a limpeza independentes,
                // embora ambos pertençam ao mesmo BackgroundService.
                await Task.WhenAll(
                    ExecutarCicloProcessamento(
                        stoppingToken),
                    ExecutarCicloLimpeza(
                        stoppingToken));
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                // Encerramento normal da aplicação.
            }
        }

        // CICLO DE PROCESSAMENTO

        // Processa imediatamente ao iniciar e repete
        // a busca por eventos pendentes a cada dois segundos.
        private async Task ExecutarCicloProcessamento(
            CancellationToken stoppingToken)
        {
            using var timer =
                new PeriodicTimer(
                    IntervaloProcessamento);

            await ExecutarProcessamento(
                stoppingToken);

            while (await timer.WaitForNextTickAsync(
                stoppingToken))
            {
                await ExecutarProcessamento(
                    stoppingToken);
            }
        }

        // Cria um escopo novo para processar eventos pendentes
        // e recuperar processamentos interrompidos.
        private async Task ExecutarProcessamento(
            CancellationToken stoppingToken)
        {
            try
            {
                await using var scope =
                    _scopeFactory.CreateAsyncScope();

                var processador =
                    scope.ServiceProvider
                        .GetRequiredService<
                            IProcessadorEventoSignalROutboxService>();

                var agoraUtc =
                    DateTime.UtcNow;

                // A recuperação de eventos travados ocorre
                // somente uma vez por minuto.
                if (agoraUtc >= _proximaRecuperacaoUtc)
                {
                    await processador
                        .RecuperarProcessamentosExpiradosAsync(
                            stoppingToken);

                    _proximaRecuperacaoUtc =
                        agoraUtc.Add(
                            IntervaloRecuperacao);
                }

                // Eventos pendentes continuam sendo
                // consultados a cada dois segundos.
                await processador
                    .ProcessarPendentesAsync(
                        stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao processar eventos SignalR do Outbox.");
            }
        }

        // CICLO DE LIMPEZA

        // Executa imediatamente ao iniciar e repete
        // a limpeza dos eventos enviados a cada dez minutos.
        private async Task ExecutarCicloLimpeza(
            CancellationToken stoppingToken)
        {
            await ExecutarLimpeza(
                stoppingToken);

            using var timer =
                new PeriodicTimer(
                    IntervaloLimpeza);

            while (await timer.WaitForNextTickAsync(
                stoppingToken))
            {
                await ExecutarLimpeza(
                    stoppingToken);
            }
        }

        // Cria um escopo exclusivo para remover os eventos que já
        // foram enviados com sucesso pelo SignalR.
        private async Task ExecutarLimpeza(
            CancellationToken stoppingToken)
        {
            try
            {
                await using var scope =
                    _scopeFactory.CreateAsyncScope();

                var processador =
                    scope.ServiceProvider
                        .GetRequiredService<
                            IProcessadorEventoSignalROutboxService>();

                await processador
                    .RemoverEnviadosExpiradosAsync(
                        stoppingToken);

                _logger.LogInformation(
                    "Limpeza dos eventos SignalR do Outbox concluída.");
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao remover eventos SignalR expirados do Outbox.");
            }
        }
    }
}
