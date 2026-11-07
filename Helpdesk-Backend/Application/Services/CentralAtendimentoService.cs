using Application.Dto.CentralAtendimento;
using Application.Dto.SlaDto;
using Application.Dto.UsuarioDto;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    // Monta a fotografia completa utilizada exclusivamente
    // pela Central de Atendimento.
    public class CentralAtendimentoService: ICentralAtendimentoService
    {
        private readonly IChamadoRepository _chamadoRepository;
        private readonly IusuarioService _usuarioService;
        private readonly ISlaService _slaService;
        private readonly ICentralAtendimentoTempoRealService _centralAtendimentoTempoRealService;

        public CentralAtendimentoService(
            IChamadoRepository chamadoRepository,
            IusuarioService usuarioService,
            ISlaService slaService,
            ICentralAtendimentoTempoRealService centralAtendimentoTempoRealService)
        {
            _chamadoRepository = chamadoRepository;
            _usuarioService = usuarioService;
            _slaService = slaService;
            _centralAtendimentoTempoRealService = centralAtendimentoTempoRealService;
        }

        // Coordena a busca dos dados e monta uma fotografia completa
        // da Central de Atendimento.
        public async Task<CentralAtendimentoResponseDto>
            ObterFotografiaAsync(
                CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var agora = DateTime.Now;
            var inicioDia = agora.Date;
            var fimDia = inicioDia.AddDays(1);

            var chamados = (
                await _chamadoRepository
                    .BuscarDadosCentralAtendimento(
                        inicioDia,
                        fimDia,
                        cancellationToken)
            ).ToArray();

            cancellationToken.ThrowIfCancellationRequested();

            var usuarios = (
                await _usuarioService.BuscarTodos()
            ).ToArray();

            cancellationToken.ThrowIfCancellationRequested();

            var slas = (
                await _slaService.BuscarTodos()
            ).ToArray();

            var usuariosPorId =
                CriarMapaUsuarios(usuarios);

            var slasPorPrioridade =
                CriarMapaSlas(slas);

            return new CentralAtendimentoResponseDto
            {
                InstanciaId = _centralAtendimentoTempoRealService.InstanciaId,

                Versao =_centralAtendimentoTempoRealService.GerarProximaVersao(),

                DataAtualizacaoUtc = DateTime.UtcNow,

                Fila = MontarFila(
                    chamados,
                    usuariosPorId),

                ChamadosMonitorados =
                    MontarChamadosMonitorados(
                        chamados,
                        usuariosPorId),

                AtendimentosHoje =
                    MontarAtendimentosHoje(
                        chamados,
                        inicioDia,
                        fimDia),

                ResumoOperacional =
                    MontarResumoOperacional(
                        chamados,
                        slasPorPrioridade,
                        agora)
            };
        }

        //METOODS PRIVADOS

        // Cria um mapa de usuários para resolver os nomes sem fazer
        // uma consulta separada para cada chamado.
        private static IReadOnlyDictionary<string, string>
            CriarMapaUsuarios(
                IEnumerable<UsuarioReponseDto> usuarios)
        {
            return usuarios
                .Where(usuario =>
                    !string.IsNullOrWhiteSpace(usuario.Id))
                .GroupBy(
                    usuario => usuario.Id,
                    StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    grupo => grupo.Key,
                    grupo => grupo.First().Username,
                    StringComparer.OrdinalIgnoreCase);
        }

        // Mantém somente os SLAs ativos e permite localizar
        // rapidamente a configuração de cada prioridade.
        private static IReadOnlyDictionary<
            PrioridadeEnum,
            SlaResponseDto> CriarMapaSlas(
                IEnumerable<SlaResponseDto> slas)
        {
            return slas
                .Where(sla => sla.Ativo)
                .GroupBy(sla => sla.Prioridade)
                .ToDictionary(
                    grupo => grupo.Key,
                    grupo => grupo.First());
        }

        // Monta a fila ordenada da maior prioridade para a menor
        // e, dentro da prioridade, pelo chamado mais antigo.
        private static IReadOnlyCollection<
            CentralAtendimentoChamadoDto> MontarFila(
                IEnumerable<Chamado> chamados,
                IReadOnlyDictionary<string, string> usuarios)
        {
            return chamados
                .Where(chamado =>
                    chamado.Status == StatusChamadoEnum.Aberto)
                .OrderByDescending(chamado =>
                    chamado.Prioridade)
                .ThenBy(chamado =>
                    chamado.DataAbertura)
                .Select(chamado =>
                    MontarChamado(
                        chamado,
                        usuarios))
                .ToArray();
        }

        // Mantém a fotografia compacta usada pelo frontend
        // para identificar novos chamados e distribuições.
        private static IReadOnlyCollection<
            CentralAtendimentoChamadoDto>
            MontarChamadosMonitorados(
                IEnumerable<Chamado> chamados,
                IReadOnlyDictionary<string, string> usuarios)
        {
            return chamados
                .Where(chamado =>
                    chamado.Status ==
                        StatusChamadoEnum.Aberto ||
                    chamado.Status ==
                        StatusChamadoEnum.Distribuido)
                .Select(chamado =>
                    MontarChamado(
                        chamado,
                        usuarios))
                .ToArray();
        }

        // Converte a entidade no formato compacto utilizado
        // exclusivamente pela Central de Atendimento.
        private static CentralAtendimentoChamadoDto
            MontarChamado(
                Chamado chamado,
                IReadOnlyDictionary<string, string> usuarios)
        {
            return new CentralAtendimentoChamadoDto
            {
                ChamadoId = chamado.Id_Chamado,
                Numero = chamado.Numero,
                SolicitanteNome = ObterNomeUsuario(
                    chamado.SolicitanteId,
                    usuarios,
                    "Solicitante não informado"),
                Assunto =
                    !string.IsNullOrWhiteSpace(
                        chamado.EquipamentoAfetado)
                        ? chamado.EquipamentoAfetado
                        : chamado.Descricao,
                Prioridade = chamado.Prioridade,
                Status = chamado.Status,
                DataAbertura = chamado.DataAbertura,
                Destino = ObterDestino(
                    chamado,
                    usuarios)
            };
        }

        // Resolve o nome de um usuário utilizando o mapa
        // carregado uma única vez para toda a fotografia.
        private static string ObterNomeUsuario(
            string? usuarioId,
            IReadOnlyDictionary<string, string> usuarios,
            string valorPadrao)
        {
            if (string.IsNullOrWhiteSpace(usuarioId))
                return valorPadrao;

            return usuarios.TryGetValue(
                usuarioId,
                out var nome)
                    ? nome
                    : valorPadrao;
        }

        // Informa para onde o chamado foi encaminhado
        // sem obrigar o frontend a consultar auditoria ou perfis.
        private static string ObterDestino(
            Chamado chamado,
            IReadOnlyDictionary<string, string> usuarios)
        {
            if (chamado.Status ==
                StatusChamadoEnum.Aberto)
            {
                return "Fila geral";
            }

            if (!string.IsNullOrWhiteSpace(
                    chamado.TecnicoResponsavelId))
            {
                var nomeTecnico = ObterNomeUsuario(
                    chamado.TecnicoResponsavelId,
                    usuarios,
                    "Usuário de destino");

                return $"Usuário: {nomeTecnico}";
            }

            var nomePerfil =
                chamado.PeriodoTrabalhoPerfil?
                    .Perfil?
                    .Nome;

            if (!string.IsNullOrWhiteSpace(nomePerfil))
                return $"Perfil: {nomePerfil}";

            return "Perfil de destino";
        }

        // Calcula quantos chamados começaram e foram
        // finalizados dentro do dia atual.
        private static CentralAtendimentoHojeDto
            MontarAtendimentosHoje(
                IEnumerable<Chamado> chamados,
                DateTime inicioDia,
                DateTime fimDia)
        {
            return new CentralAtendimentoHojeDto
            {
                Iniciados = chamados.Count(chamado =>
                    chamado.DataPrimeiroAtendimento.HasValue &&
                    chamado.DataPrimeiroAtendimento.Value >=
                        inicioDia &&
                    chamado.DataPrimeiroAtendimento.Value <
                        fimDia),

                Finalizados = chamados.Count(chamado =>
                    chamado.Status ==
                        StatusChamadoEnum.Fechado &&
                    chamado.DataFechamento.HasValue &&
                    chamado.DataFechamento.Value >=
                        inicioDia &&
                    chamado.DataFechamento.Value <
                        fimDia)
            };
        }

        // Monta as quantidades operacionais e os dois
        // indicadores de risco de SLA da Central.
        private static CentralAtendimentoResumoDto
            MontarResumoOperacional(
                IEnumerable<Chamado> chamados,
                IReadOnlyDictionary<
                    PrioridadeEnum,
                    SlaResponseDto> slas,
                DateTime agora)
        {
            var chamadosAtivos = chamados
                .Where(chamado =>
                    chamado.Status !=
                        StatusChamadoEnum.Fechado &&
                    chamado.Status !=
                        StatusChamadoEnum.Cancelado)
                .ToArray();

            return new CentralAtendimentoResumoDto
            {
                Fila = chamadosAtivos.Count(chamado =>
                    chamado.Status ==
                        StatusChamadoEnum.Aberto),

                EmAtendimento = chamadosAtivos.Count(chamado =>
                    chamado.Status ==
                        StatusChamadoEnum.EmAtendimento),

                AguardandoRetorno = chamadosAtivos.Count(chamado =>
                    chamado.Status ==
                        StatusChamadoEnum.AguardandoRetorno),

                AguardandoEquipamento =
                    chamadosAtivos.Count(chamado =>
                        chamado.Status ==
                            StatusChamadoEnum
                                .AguardandoEquipamento),

                ProximosVencimento =
                    ObterChamadosProximosVencimento(
                        chamadosAtivos,
                        slas,
                        agora),

                ForaSla =
                    ObterChamadosForaSla(
                        chamadosAtivos,
                        slas,
                        agora)
            };
        }

        private static int ObterChamadosProximosVencimento(
            IEnumerable<Chamado> chamados,
            IReadOnlyDictionary<
                PrioridadeEnum,
                SlaResponseDto> slas,
            DateTime agora)
        {
            return chamados.Count(chamado =>
                slas.TryGetValue(
                    chamado.Prioridade,
                    out var sla) &&
                EstaProximoDoVencimento(
                    chamado,
                    sla,
                    agora));
        }

        private static int ObterChamadosForaSla(
            IEnumerable<Chamado> chamados,
            IReadOnlyDictionary<
                PrioridadeEnum,
                SlaResponseDto> slas,
            DateTime agora)
        {
            return chamados.Count(chamado =>
                slas.TryGetValue(
                    chamado.Prioridade,
                    out var sla) &&
                PossuiSlaGeralViolado(
                    chamado,
                    sla,
                    agora));
        }

        // Um chamado está fora do SLA quando ultrapassou
        // o prazo de atendimento ou de resolução.
        private static bool PossuiSlaGeralViolado(
            Chamado chamado,
            SlaResponseDto sla,
            DateTime agora)
        {
            return PossuiViolacaoPrimeiroAtendimento(
                       chamado,
                       sla,
                       agora) ||
                   PossuiViolacaoResolucao(
                       chamado,
                       sla,
                       agora);
        }

        private static bool PossuiViolacaoPrimeiroAtendimento(
            Chamado chamado,
            SlaResponseDto sla,
            DateTime agora)
        {
            if (!PodeClassificarSla(chamado))
                return false;

            var tempo =
                chamado.CalcularTempoPrimeiroAtendimentoSlaHoras(
                    agora);

            return tempo.HasValue &&
                   tempo.Value >
                       sla.TempoAtendimentoHoras;
        }

        private static bool PossuiViolacaoResolucao(
            Chamado chamado,
            SlaResponseDto sla,
            DateTime agora)
        {
            if (!PodeClassificarSla(chamado))
                return false;

            var tempo =
                chamado.CalcularTempoResolucaoSlaHoras(
                    agora);

            return tempo.HasValue &&
                   tempo.Value >
                       sla.TempoResolucaoHoras;
        }

        // Identifica chamados que já consumiram pelo menos
        // 80% do prazo, mas que ainda não violaram o SLA.
        private static bool EstaProximoDoVencimento(
            Chamado chamado,
            SlaResponseDto sla,
            DateTime agora)
        {
            if (chamado.Status ==
                    StatusChamadoEnum.Fechado ||
                chamado.Status ==
                    StatusChamadoEnum.Cancelado ||
                sla.TempoAtendimentoHoras <= 0 ||
                sla.TempoResolucaoHoras <= 0 ||
                PossuiSlaGeralViolado(
                    chamado,
                    sla,
                    agora))
            {
                return false;
            }

            var tempoResolucao =
                chamado.CalcularTempoResolucaoSlaHoras(
                    agora);

            if (!tempoResolucao.HasValue)
                return false;

            var percentualResolucao =
                tempoResolucao.Value /
                sla.TempoResolucaoHoras;

            if (EstaEntreOitentaECemPorCento(
                    percentualResolucao))
            {
                return true;
            }

            if (chamado.DataPrimeiroAtendimento.HasValue)
                return false;

            var tempoPrimeiroAtendimento =
                chamado
                    .CalcularTempoPrimeiroAtendimentoSlaHoras(
                        agora);

            if (!tempoPrimeiroAtendimento.HasValue)
                return false;

            var percentualPrimeiroAtendimento =
                tempoPrimeiroAtendimento.Value /
                sla.TempoAtendimentoHoras;

            return EstaEntreOitentaECemPorCento(
                percentualPrimeiroAtendimento);
        }

        private static bool PodeClassificarSla(
            Chamado chamado)
        {
            return chamado.Status !=
                       StatusChamadoEnum.Cancelado &&
                   (
                       chamado.Status !=
                           StatusChamadoEnum.Fechado ||
                       chamado.DataPrimeiroAtendimento.HasValue
                   );
        }

        private static bool EstaEntreOitentaECemPorCento(
            double percentual)
        {
            return percentual >= 0.8 &&
                   percentual <= 1.0;
        }
    }
}