using Application.Dto.NotificacaoDto;
using Application.Interfaces;
using Domain.Enum;
using Domain.Interfaces;

namespace Application.Services
{
    // Monitora chamados distribuídos que ainda não foram assumidos e avisa
    // os gestores quando o prazo de primeiro atendimento é ultrapassado.
    public class DistribuicaoEscalonamentoService
        : IDistribuicaoEscalonamentoService
    {
        private readonly IChamadoRepository _chamadoRepository;
        private readonly ISlaService _slaService;
        private readonly IUsuarioPerfilService _usuarioPerfilService;
        private readonly INotificacaoService _notificacaoService;
        private readonly ITransacaoService _transacaoService;

        public DistribuicaoEscalonamentoService(
            IChamadoRepository chamadoRepository,
            ISlaService slaService,
            IUsuarioPerfilService usuarioPerfilService,
            INotificacaoService notificacaoService,
            ITransacaoService transacaoService)
        {
            _chamadoRepository = chamadoRepository;
            _slaService = slaService;
            _usuarioPerfilService = usuarioPerfilService;
            _notificacaoService = notificacaoService;
            _transacaoService = transacaoService;
        }

        // Processa os chamados distribuídos, compara o tempo corrido com o SLA
        // da prioridade e cria uma única notificação por gestor e chamado.
        public async Task ProcessarChamadosDistribuidosVencidos()
        {
            var chamados = await _chamadoRepository
                .BuscarChamadosDistribuido();
            
            var gestores = await _usuarioPerfilService
                .BuscarUsuariosComPermissao(
                    "GERENCIAR_DISTRIBUICAO");

            var agora = DateTime.Now;

            foreach (var chamado in chamados)
            {
                // Cada chamado utiliza o SLA configurado para sua prioridade.
                var sla = await _slaService
                    .BuscarPrioridade(chamado.Prioridade);

                if (sla == null)
                    continue;

                var prazoParaAssumir = chamado.DataAbertura
                    .AddHours(sla.TempoAtendimentoHoras);

                if (agora < prazoParaAssumir)
                    continue;

                // Cada chamado vencido possui uma transação curta própria.
                // As notificações e seus eventos Outbox são confirmados juntos.
                await _transacaoService.ExecutarAsync(async () =>
                {
                    foreach (var gestorId in gestores)
                    {
                        // Evita novas notificações quando o gestor já foi avisado.
                        var jaFoiNotificado =
                            await _notificacaoService
                                .ExisteNotificacaoPorChamadoTipoEUsuario(
                                    chamado.Id_Chamado,
                                    TipoNotificacaoEnum
                                        .ChamadoDistribuicaoVencida,
                                    gestorId);

                        if (jaFoiNotificado)
                            continue;

                        await _notificacaoService.Adicionar(
                            new NotificacaoRequestDto
                            {
                                usuarioId = gestorId,
                                usuarioOrigemId = chamado.SolicitanteId!,
                                chamadoId = chamado.Id_Chamado,
                                tipo = TipoNotificacaoEnum
                                    .ChamadoDistribuicaoVencida,
                                titulo =
                                    "Chamado aguardando atendimento",
                                mensagem =
                                    $"O chamado {chamado.Numero} excedeu o prazo para ser assumido pelo perfil.",
                                status = StatusNotificacaoEnum.Ativa
                            });
                    }
                });
            }
        }
    }
}
