using Infra.Context;
using Microsoft.EntityFrameworkCore;

namespace Helpdesk.BackgroundServices
{
    /// <summary>
    /// Remove periodicamente sessões de autenticação antigas,
    /// evitando o crescimento indefinido da tabela SessoesRefresh.
    /// </summary>
    public sealed class SessaoRefreshLimpezaBackgroundService
        : BackgroundService
    {
        private static readonly TimeSpan IntervaloExecucao =
            TimeSpan.FromMinutes(10);

        private const int RetencaoDias = 1;
        private const int TamanhoLote = 500;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<
            SessaoRefreshLimpezaBackgroundService> _logger;

        public SessaoRefreshLimpezaBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<SessaoRefreshLimpezaBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        // CICLO DE EXECUÇÃO

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            try
            {
                // Executa uma vez quando a API iniciar.
                await ExecutarLimpeza(
                    stoppingToken);

                using var timer =
                    new PeriodicTimer(
                        IntervaloExecucao);

                while (await timer.WaitForNextTickAsync(
                    stoppingToken))
                {
                    await ExecutarLimpeza(
                        stoppingToken);
                }
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                // Encerramento normal da aplicação.
            }
        }

        // MÉTODOS PRIVADOS

        private async Task ExecutarLimpeza(
            CancellationToken stoppingToken)
        {
            try
            {
                await using var scope =
                    _scopeFactory.CreateAsyncScope();

                var identityContext =
                    scope.ServiceProvider
                        .GetRequiredService<IdentityContext>();

                var limiteRetencaoUtc =
                    DateTime.UtcNow.AddDays(
                        -RetencaoDias);

                var totalRemovido =
                    0;

                int quantidadeRemovida;

                do
                {
                    stoppingToken
                        .ThrowIfCancellationRequested();

                    var sessoesParaRemover =
                        await identityContext.SessoesRefresh
                            .AsNoTracking()
                            .Where(sessao =>
                                (
                                    sessao.RevogadaEmUtc.HasValue &&
                                    sessao.RevogadaEmUtc.Value <=
                                        limiteRetencaoUtc
                                ) ||
                                (
                                    !sessao.RevogadaEmUtc.HasValue &&
                                    sessao.ExpiraEmUtc <=
                                        limiteRetencaoUtc
                                ))
                            .OrderBy(sessao =>
                                sessao.ExpiraEmUtc)
                            .Select(sessao =>
                                sessao.Id)
                            .Take(TamanhoLote)
                            .ToListAsync(
                                stoppingToken);

                    if (sessoesParaRemover.Count == 0)
                        break;

                    quantidadeRemovida =
                        await identityContext.SessoesRefresh
                            .Where(sessao =>
                                sessoesParaRemover.Contains(
                                    sessao.Id))
                            .ExecuteDeleteAsync(
                                stoppingToken);

                    totalRemovido +=
                        quantidadeRemovida;
                }
                while (quantidadeRemovida ==
                       TamanhoLote);

                if (totalRemovido > 0)
                {
                    _logger.LogInformation(
                        "Limpeza das sessões de autenticação concluída. " +
                        "{TotalRemovido} registros removidos.",
                        totalRemovido);
                }
                else
                {
                    _logger.LogDebug(
                        "Limpeza das sessões de autenticação concluída. " +
                        "Nenhum registro antigo encontrado.");
                }
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
                    "Erro ao remover sessões de autenticação antigas.");
            }
        }
    }
}