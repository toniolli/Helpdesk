using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using System.Text.Json;

namespace Application.Services
{
    // Coordena a recuperação, reserva, publicação, tentativa
    // e limpeza dos eventos SignalR persistidos no Outbox.
    public class ProcessadorEventoSignalROutboxService
        : IProcessadorEventoSignalROutboxService
    {
        private const int TamanhoLoteProcessamento = 100;
        private const int TamanhoLoteLimpeza = 500;
        private const int MinutosParaRecuperarProcessamento = 5;

        private readonly IEventoSignalROutboxRepository
            _eventoRepository;

        private readonly IPublicadorSignalRService
            _publicadorSignalR;

        public ProcessadorEventoSignalROutboxService(
            IEventoSignalROutboxRepository eventoRepository,
            IPublicadorSignalRService publicadorSignalR)
        {
            _eventoRepository = eventoRepository;
            _publicadorSignalR = publicadorSignalR;
        }


        // Busca e processa até 100 eventos pendentes cuja
        // data da próxima tentativa já tenha sido alcançada.
        public async Task ProcessarPendentesAsync(
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var eventosPendentes =
                await _eventoRepository.BuscarPendentes(
                    DateTime.UtcNow,
                    TamanhoLoteProcessamento);

            foreach (var evento in eventosPendentes)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await ProcessarEventoAsync(
                    evento,
                    cancellationToken);
            }
        }

        // Remove todos os eventos já enviados, utilizando lotes
        // de 500 registros para não sobrecarregar o banco.
        public async Task RemoverEnviadosExpiradosAsync(
            CancellationToken cancellationToken)
        {
            var limiteUtc = DateTime.UtcNow;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var quantidadeRemovida =
                    await _eventoRepository
                        .RemoverEnviadosAnterioresA(
                            limiteUtc,
                            TamanhoLoteLimpeza);

                if (quantidadeRemovida <
                    TamanhoLoteLimpeza)
                {
                    break;
                }
            }
        }

        // Recupera eventos que permaneceram Processando por mais
        // de cinco minutos após uma interrupção inesperada.
        public async Task RecuperarProcessamentosExpiradosAsync(
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var limiteProcessamentoUtc =
                DateTime.UtcNow.AddMinutes(
                    -MinutosParaRecuperarProcessamento);

            await _eventoRepository
                .RecuperarProcessamentosExpirados(
                    limiteProcessamentoUtc);
        }

        // Tenta reservar o evento para esta instância, publica pelo
        // SignalR e registra o resultado da tentativa no banco.
        private async Task ProcessarEventoAsync(
            EventoSignalROutbox evento,
            CancellationToken cancellationToken)
        {
            var inicioProcessamentoUtc =
                DateTime.UtcNow;

            var reservado =
                await _eventoRepository
                    .TentarMarcarProcessando(
                        evento.Id_EventoSignalROutbox,
                        inicioProcessamentoUtc);

            if (!reservado)
                return;

            evento.MarcarProcessando();

            try
            {
                var destinatarios =
                    DeserializarDestinatarios(
                        evento.DestinatariosJson);

                await _publicadorSignalR.PublicarAsync(
                    evento.NomeEvento,
                    evento.TipoDestino,
                    destinatarios,
                    evento.GrupoDestino,
                    evento.PayloadJson,
                    cancellationToken);

                evento.MarcarEnviado();
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                // Em uma parada da aplicação, o evento permanece
                // Processando e será recuperado depois de cinco minutos.
                throw;
            }
            catch (Exception ex)
            {
                // A exceção é persistida no próprio evento e ele será
                // reagendado ou marcado como Falhou após dez tentativas.
                evento.RegistrarFalha(
                    ex.ToString());
            }

            await _eventoRepository.Atualizar(
                evento);
        }

        // Converte o JSON armazenado no banco em uma coleção de IDs,
        // removendo valores vazios e destinatários repetidos.
        private static IReadOnlyCollection<string>
            DeserializarDestinatarios(
                string? destinatariosJson)
        {
            if (string.IsNullOrWhiteSpace(
                destinatariosJson))
            {
                return Array.Empty<string>();
            }

            var destinatarios =
                JsonSerializer.Deserialize<List<string>>(
                    destinatariosJson);

            if (destinatarios is null)
                return Array.Empty<string>();

            return destinatarios
                .Where(destinatario =>
                    !string.IsNullOrWhiteSpace(
                        destinatario))
                .Select(destinatario =>
                    destinatario.Trim())
                .Distinct()
                .ToArray();
        }
    }
}
