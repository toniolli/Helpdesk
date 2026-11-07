using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IEventoSignalROutboxRepository
    {
        Task<EventoSignalROutbox> Adicionar(
            EventoSignalROutbox evento);

        Task<IEnumerable<EventoSignalROutbox>> BuscarPendentes(
            DateTime agoraUtc,
            int tamanhoLote);

        Task<bool> TentarMarcarProcessando(
            int eventoId,
            DateTime inicioUtc);

        Task<EventoSignalROutbox> Atualizar(
            EventoSignalROutbox evento);

        Task<int> RecuperarProcessamentosExpirados(
            DateTime limiteUtc);

        Task<int> RemoverEnviadosAnterioresA(
            DateTime limiteUtc,
            int tamanhoLote);
    }
}