using Application.Interfaces;

namespace Helpdesk.BackgroundServices
{
    /// <summary>
    /// Remove periodicamente os registros de auditoria que ultrapassaram
    /// o período de retenção configurado pela aplicação.
    /// </summary>
    public sealed class AuditoriaLimpezaBackgroundService : BackgroundService
    {
        private static readonly TimeSpan IntervaloExecucao = TimeSpan.FromHours(24);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AuditoriaLimpezaBackgroundService> _logger;

        public AuditoriaLimpezaBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<AuditoriaLimpezaBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        // CICLO DE EXECUÇÃO

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            // Executa uma vez quando a API iniciar.
            await ExecutarLimpeza();

            using var timer = new PeriodicTimer(IntervaloExecucao);

            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await ExecutarLimpeza();
                }
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                // Encerramento normal da aplicação.
            }
        }

        // MÉTODOS PRIVADOS

        private async Task ExecutarLimpeza()
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();

                var auditoriaService =
                    scope.ServiceProvider.GetRequiredService<IAuditoriaService>();

                var auditoriaIdentityService =
                    scope.ServiceProvider.GetRequiredService<IAuditoriaIdentityService>();

                var auditoriaChamadoService =
                    scope.ServiceProvider.GetRequiredService<IAuditoriaChamadoService>();

                var totalRemovidoAuditoria =
                    await auditoriaService.RemoverExpiradas();

                var totalRemovidoIdentity =
                    await auditoriaIdentityService.RemoverExpiradas();

                var totalRemovidoChamado =
                    await auditoriaChamadoService.RemoverExpiradas();

                var totalRemovido =
                    totalRemovidoAuditoria +
                    totalRemovidoAuditoria +
                    totalRemovidoIdentity;

                if (totalRemovido > 0)
                {
                    _logger.LogInformation(
                     "Limpeza de auditoria concluída. " +
                     "{AuditoriaNormal} registros normais, " +
                     "{AuditoriaIdentity} registros de Identity e " +
                     "{AuditoriaChamado} registros de chamados removidos.",
                     totalRemovidoAuditoria,
                     totalRemovidoIdentity,
                     totalRemovidoChamado);
                }
                else
                {
                    _logger.LogDebug(
                        "Limpeza de auditoria concluída. Nenhum registro expirado encontrado.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao remover registros de auditoria expirados.");
            }
        }
    }
}
