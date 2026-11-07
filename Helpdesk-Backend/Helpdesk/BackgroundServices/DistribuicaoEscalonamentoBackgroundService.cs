using Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Helpdesk.BackgroundServices
{
    /// <summary>
    /// Processa periodicamente os chamados distribuídos cujo prazo de
    /// escalonamento foi atingido.
    /// </summary>
    public class DistribuicaoEscalonamentoBackgroundService : BackgroundService
    {
        private static readonly TimeSpan IntervaloExecucao =
            TimeSpan.FromMinutes(30);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DistribuicaoEscalonamentoBackgroundService> _logger;

        public DistribuicaoEscalonamentoBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<DistribuicaoEscalonamentoBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        // CICLO DE EXECUÇÃO

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ExecutarEscalonamento();

                await Task.Delay(IntervaloExecucao, stoppingToken);
            }
        }

        // MÉTODOS PRIVADOS

        private async Task ExecutarEscalonamento()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var escalonamentoService =
                    scope.ServiceProvider.GetRequiredService<
                        IDistribuicaoEscalonamentoService>();

                await escalonamentoService
                    .ProcessarChamadosDistribuidosVencidos();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao processar escalonamento de distribuição.");
            }
        }
    }
}
