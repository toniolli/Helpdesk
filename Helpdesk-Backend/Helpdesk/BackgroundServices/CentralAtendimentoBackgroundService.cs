using Application.Interfaces;
using Domain.Enum;
using System.Text.Json;

namespace Helpdesk.BackgroundServices
{
    /// <summary>
    /// Mantém a Central de Atendimento atualizada pelo SignalR.
    /// Processa sinais de mudanças e atualizações periódicas de SLA.
    /// </summary>
    public sealed class CentralAtendimentoBackgroundService
        : BackgroundService
    {
        // Grupo SignalR utilizado somente pelos navegadores
        // que estão exibindo a Central de Atendimento.
        private const string GrupoCentralAtendimento =
            "central-atendimento";

        // Evento que entrega uma fotografia completa da Central
        // para os navegadores conectados ao grupo.
        private const string EventoCentralAtendimentoAtualizada =
            "centralAtendimentoAtualizada";

        // Atualiza a Central mesmo sem mudança de chamado,
        // pois os indicadores de SLA mudam com o passar do tempo.
        private static readonly TimeSpan IntervaloTemporal =
            TimeSpan.FromMinutes(1);

        // Agrupa várias mudanças próximas para evitar montar
        // uma fotografia para cada chamado alterado.
        private static readonly TimeSpan IntervaloAgrupamento =
            TimeSpan.FromMilliseconds(300);

        // Tempo aguardado antes de solicitar uma nova tentativa
        // quando ocorrer uma falha ao montar ou publicar a fotografia.
        private static readonly TimeSpan IntervaloNovaTentativa =
            TimeSpan.FromSeconds(5);

        // Utiliza a configuração web para serializar propriedades
        // em camelCase, que é o formato esperado pelo frontend.
        private static readonly JsonSerializerOptions OpcoesJson =
            new(JsonSerializerDefaults.Web);

        // Permite criar um escopo para utilizar serviços Scoped,
        // como CentralAtendimentoService e seus repositórios.
        private readonly IServiceScopeFactory _scopeFactory;

        // Serviço Singleton que guarda as conexões e recebe
        // os sinais de atualização em memória.
        private readonly ICentralAtendimentoTempoRealService
            _tempoRealService;

        private readonly ILogger<CentralAtendimentoBackgroundService>
            _logger;

        // Impede que o ciclo de sinais e o ciclo temporal montem
        // duas fotografias simultaneamente.
        private readonly SemaphoreSlim _bloqueioAtualizacao =
            new(1, 1);

        public CentralAtendimentoBackgroundService(
            IServiceScopeFactory scopeFactory,
            ICentralAtendimentoTempoRealService tempoRealService,
            ILogger<CentralAtendimentoBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _tempoRealService = tempoRealService;
            _logger = logger;
        }

        /// <summary>
        /// Inicia os dois ciclos da Central:
        /// um reage às mudanças e outro atualiza os dados por tempo.
        /// </summary>
        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            try
            {
                await Task.WhenAll(
                    ExecutarCicloSinaisAsync(stoppingToken),
                    ExecutarCicloTemporalAsync(stoppingToken));
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                // O cancelamento é esperado quando a API é encerrada.
            }
        }

        /// <summary>
        /// Aguarda um aviso de que algum dado da Central mudou.
        /// Sinais recebidos próximos são agrupados em uma atualização.
        /// </summary>
        private async Task ExecutarCicloSinaisAsync(
            CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Permanece aguardando sem ocupar uma thread.
                await _tempoRealService
                    .AguardarAtualizacaoAsync(stoppingToken);

                // Aguarda 300 ms para permitir que mudanças próximas,
                // como uma distribuição em lote, sejam agrupadas.
                await Task.Delay(
                    IntervaloAgrupamento,
                    stoppingToken);

                // Remove outros sinais que chegaram durante os 300 ms.
                // Uma fotografia completa já representará todos eles.
                while (_tempoRealService
                    .TentarConsumirAtualizacao())
                {
                    // O conteúdo do sinal não precisa ser processado.
                }

                await AtualizarCentralAsync(stoppingToken);
            }
        }

        /// <summary>
        /// Atualiza a Central a cada minuto enquanto existir
        /// pelo menos um navegador utilizando a tela.
        /// </summary>
        private async Task ExecutarCicloTemporalAsync(
            CancellationToken stoppingToken)
        {
            using var timer =
                new PeriodicTimer(IntervaloTemporal);

            while (await timer.WaitForNextTickAsync(
                stoppingToken))
            {
                await AtualizarCentralAsync(stoppingToken);
            }
        }

        /// <summary>
        /// Monta uma fotografia atual e publica no grupo SignalR.
        /// Não consulta o banco quando não existe Central conectada.
        /// </summary>
        private async Task AtualizarCentralAsync(
            CancellationToken stoppingToken)
        {
            // Evita qualquer consulta quando ninguém estiver
            // visualizando a Central de Atendimento.
            if (!_tempoRealService.PossuiConexoes)
                return;

            // Aguarda outra atualização terminar, caso os ciclos
            // de sinal e de tempo sejam executados juntos.
            await _bloqueioAtualizacao.WaitAsync(
                stoppingToken);

            try
            {
                // Verifica novamente porque a última conexão pode
                // ter saído enquanto aguardávamos o bloqueio.
                if (!_tempoRealService.PossuiConexoes)
                    return;

                // O BackgroundService é Singleton. Por isso ele cria
                // um escopo antes de utilizar serviços Scoped.
                await using var scope =
                    _scopeFactory.CreateAsyncScope();

                var centralAtendimentoService =
                    scope.ServiceProvider
                        .GetRequiredService<
                            ICentralAtendimentoService>();

                var publicadorSignalR =
                    scope.ServiceProvider
                        .GetRequiredService<
                            IPublicadorSignalRService>();

                // Consulta os dados e monta uma fotografia completa
                // com InstanciaId, versão e horário da atualização.
                var fotografia =
                    await centralAtendimentoService
                        .ObterFotografiaAsync(
                            stoppingToken);

                // Converte a fotografia para JSON em camelCase.
                var payloadJson =
                    JsonSerializer.Serialize(
                        fotografia,
                        OpcoesJson);

                // Publica diretamente pelo SignalR.
                // Essa atualização não é registrada no Outbox.
                await publicadorSignalR.PublicarAsync(
                    EventoCentralAtendimentoAtualizada,
                    TipoDestinoEventoSignalREnum.Grupo,
                    Array.Empty<string>(),
                    GrupoCentralAtendimento,
                    payloadJson,
                    stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                // Permite que a API encerre normalmente.
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao atualizar a Central de Atendimento.");

                // Evita uma repetição contínua em caso de falha.
                // Depois de cinco segundos, coloca um novo sinal
                // para que outra tentativa seja realizada.
                if (_tempoRealService.PossuiConexoes)
                {
                    await Task.Delay(
                        IntervaloNovaTentativa,
                        stoppingToken);

                    _tempoRealService
                        .SinalizarAtualizacao();
                }
            }
            finally
            {
                // Libera a próxima atualização mesmo quando
                // ocorrer erro ou retorno antecipado.
                _bloqueioAtualizacao.Release();
            }
        }
    }
}