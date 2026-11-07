using Application.Interfaces;
using Domain.Entities;
using Domain.Enum;
using Domain.Interfaces;
using System.Text.Json;
using Application.Dto.Comentario;

namespace Application.Services
{
    // Transforma as solicitações de atualização em eventos persistidos
    // no Outbox, sem enviar diretamente pelo SignalR.
    public class AtualizacaoTempoRealOutboxService
        : IAtualizacaoTempoRealService
    {
        private const string GrupoFilaAtendimento =
            "fila-atendimento";

        private const string GrupoDashboardGlobal =
            "dashboard-global";

        private readonly IEventoSignalROutboxRepository
            _eventoRepository;

        public AtualizacaoTempoRealOutboxService(
            IEventoSignalROutboxRepository eventoRepository)
        {
            _eventoRepository = eventoRepository;
        }

        // Registra a atualização das notificações de um usuário.
        public Task NotificarNotificacaoAtualizadaAsync(
            string usuarioId)
        {
            return RegistrarParaUsuariosAsync(
                new[] { usuarioId },
                "notificacaoAtualizada");
        }

        // Registra a criação de um comentário para o outro participante
        // do chamado, sem enviar o evento para o próprio autor.
        public Task NotificarComentarioCriadoAsync(
            Chamado chamado,
            string autorId,
             ComentarioResponseDto comentario)
        {
            var destinatarios = new[]
            {
                chamado.SolicitanteId,
                chamado.TecnicoResponsavelId
            }
            .Where(usuarioId =>
                !string.IsNullOrWhiteSpace(usuarioId) &&
                usuarioId != autorId)
            .Select(usuarioId => usuarioId!);

            return RegistrarParaUsuariosAsync(
                destinatarios,
                "comentarioCriado",
                new
                {
                    chamadoId = chamado.Id_Chamado,
                    comentario
                });
        }

        // Registra a atualização de um chamado para os usuários
        // diretamente envolvidos com ele.
        public Task NotificarChamadoAtualizadoAsync(
            IEnumerable<string> usuarioIds,
            int chamadoId)
        {
            return RegistrarParaUsuariosAsync(
                usuarioIds,
                "chamadoAtualizado",
                new
                {
                    chamadoId
                });
        }

        // Registra a atualização da fila para todos os usuários
        // conectados ao grupo da fila de atendimento.
        public Task NotificarFilaAtualizadaAsync(
            int chamadoId)
        {
            return RegistrarParaGrupoAsync(
                GrupoFilaAtendimento,
                "filaAtualizada",
                new
                {
                    chamadoId
                });
        }

        // Registra a atualização dos indicadores individuais
        // dos usuários envolvidos com o chamado.
        public Task NotificarDashboardAtualizadoAsync(
            IEnumerable<string> usuarioIds)
        {
            return RegistrarParaUsuariosAsync(
                usuarioIds,
                "dashboardAtualizado");
        }
        //registra varios usuario dentro do evento, quando e enviado uma notificação 
        public Task NotificarNotificacoesAtualizadasAsync(
            IEnumerable<string> usuarioIds)
        {
            return RegistrarParaUsuariosAsync(
                usuarioIds,
                "notificacaoAtualizada");
        }

        // Registra a atualização dos indicadores globais para
        // administradores e líderes conectados ao grupo gerencial.
        public Task NotificarDashboardGlobalAtualizadoAsync()
        {
            return RegistrarParaGrupoAsync(
                GrupoDashboardGlobal,
                "dashboardAtualizado");
        }

        // Normaliza os usuários, serializa os dados e cria um evento
        // destinado a uma coleção específica de usuários.
        private async Task RegistrarParaUsuariosAsync(
            IEnumerable<string> usuarioIds,
            string nomeEvento,
            object? payload = null)
        {
            var destinatarios = usuarioIds
                .Where(usuarioId =>
                    !string.IsNullOrWhiteSpace(usuarioId))
                .Select(usuarioId =>
                    usuarioId.Trim())
                .Distinct()
                .ToArray();

            if (destinatarios.Length == 0)
                return;

            var destinatariosJson =
                JsonSerializer.Serialize(
                    destinatarios);

            var payloadJson =
                SerializarPayload(payload);

            var evento =
                new EventoSignalROutbox(
                    nomeEvento,
                    TipoDestinoEventoSignalREnum.Usuarios,
                    destinatariosJson,
                    grupoDestino: null,
                    payloadJson);

            await _eventoRepository.Adicionar(
                evento);
        }

        // Serializa os dados e cria um evento destinado
        // a um grupo registrado no SignalR.
        private async Task RegistrarParaGrupoAsync(
            string grupoDestino,
            string nomeEvento,
            object? payload = null)
        {
            var payloadJson =
                SerializarPayload(payload);

            var evento =
                new EventoSignalROutbox(
                    nomeEvento,
                    TipoDestinoEventoSignalREnum.Grupo,
                    destinatariosJson: null,
                    grupoDestino,
                    payloadJson);

            await _eventoRepository.Adicionar(
                evento);
        }

        // Serializa somente eventos que possuem conteúdo adicional.
        // Eventos utilizados apenas como aviso permanecem sem payload.
        private static string? SerializarPayload(
            object? payload)
        {
            return payload is null
                ? null
                : JsonSerializer.Serialize(payload);
        }
    }
}