using Domain.Entities;
using Domain.Enum;
using Domain.Interfaces;
using Infra.Context;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositores
{
    // Controla a persistência, o processamento concorrente e a limpeza
    // dos eventos SignalR armazenados no Outbox.
    public class EventoSignalROutboxRepository
        : IEventoSignalROutboxRepository
    {
        private readonly AppDbContext _context;

        public EventoSignalROutboxRepository(
            AppDbContext context)
        {
            _context = context;
        }

        // PERSISTÊNCIA

        // Registra um novo evento no Outbox utilizando a mesma unidade
        // de trabalho das demais alterações da operação de negócio.
        public async Task<EventoSignalROutbox> Adicionar(
            EventoSignalROutbox evento)
        {
            await _context
                .EventosSignalROutbox
                .AddAsync(evento);

            await _context.SaveChangesAsync();

            return evento;
        }

        // Atualiza o estado do evento depois de uma publicação
        // bem-sucedida ou de uma falha no SignalR.
        public async Task<EventoSignalROutbox> Atualizar(
            EventoSignalROutbox evento)
        {
            _context
                .EventosSignalROutbox
                .Update(evento);

            await _context.SaveChangesAsync();

            return evento;
        }

        // PROCESSAMENTO

        // Retorna somente eventos pendentes cuja data da próxima
        // tentativa já tenha sido alcançada.
        public async Task<IEnumerable<EventoSignalROutbox>>
            BuscarPendentes(
                DateTime agoraUtc,
                int tamanhoLote)
        {
            return await _context
                .EventosSignalROutbox
                .AsNoTracking()
                .Where(evento =>
                    evento.Status ==
                        StatusEventoSignalROutboxEnum.Pendente &&
                    evento.ProximaTentativaUtc <= agoraUtc)
                .OrderBy(evento => evento.DataCriacaoUtc)
                .Take(tamanhoLote)
                .ToListAsync();
        }

        // Tenta reservar atomicamente um evento pendente. O retorno será
        // verdadeiro apenas para a instância que conseguiu atualizá-lo.
        public async Task<bool> TentarMarcarProcessando(
            int eventoId,
            DateTime inicioUtc)
        {
            var registrosAtualizados = await _context
                .EventosSignalROutbox
                .Where(evento =>
                    evento.Id_EventoSignalROutbox == eventoId &&
                    evento.Status ==
                        StatusEventoSignalROutboxEnum.Pendente)
                .ExecuteUpdateAsync(atualizacao =>
                    atualizacao
                        .SetProperty(
                            evento => evento.Status,
                            StatusEventoSignalROutboxEnum.Processando)
                        .SetProperty(
                            evento => evento.ProcessamentoIniciadoUtc,
                            inicioUtc));

            return registrosAtualizados == 1;
        }

        // RECUPERAÇÃO

        // Devolve para Pendente os eventos que permaneceram Processando
        // além do limite, normalmente após uma interrupção da aplicação.
        public async Task<int> RecuperarProcessamentosExpirados(
            DateTime limiteUtc)
        {
            var agoraUtc = DateTime.UtcNow;

            return await _context
                .EventosSignalROutbox
                .Where(evento =>
                    evento.Status ==
                        StatusEventoSignalROutboxEnum.Processando &&
                    evento.ProcessamentoIniciadoUtc.HasValue &&
                    evento.ProcessamentoIniciadoUtc.Value <= limiteUtc)
                .ExecuteUpdateAsync(atualizacao =>
                    atualizacao
                        .SetProperty(
                            evento => evento.Status,
                            StatusEventoSignalROutboxEnum.Pendente)
                        .SetProperty(
                            evento => evento.ProcessamentoIniciadoUtc,
                            (DateTime?)null)
                        .SetProperty(
                            evento => evento.ProximaTentativaUtc,
                            agoraUtc));
        }

        // LIMPEZA

        // Remove somente eventos enviados antes da data limite,
        // respeitando o tamanho máximo do lote de limpeza.
        public async Task<int> RemoverEnviadosAnterioresA(
            DateTime limiteUtc,
            int tamanhoLote)
        {
            var ids = await _context
                .EventosSignalROutbox
                .AsNoTracking()
                .Where(evento =>
                    evento.Status ==
                        StatusEventoSignalROutboxEnum.Enviado &&
                    evento.DataEnvioUtc.HasValue &&
                    evento.DataEnvioUtc.Value < limiteUtc)
                .OrderBy(evento => evento.DataEnvioUtc)
                .Select(evento =>
                    evento.Id_EventoSignalROutbox)
                .Take(tamanhoLote)
                .ToListAsync();

            if (ids.Count == 0)
                return 0;

            return await _context
                .EventosSignalROutbox
                .Where(evento =>
                    ids.Contains(
                        evento.Id_EventoSignalROutbox))
                .ExecuteDeleteAsync();
        }
    }
}