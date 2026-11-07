using Application.Dto.SlaDto;
using Application.Interfaces;
using Application.ViewModel;
using Domain.Entities;
using Domain.Enum;
using Domain.Interfaces;
using Domain.Validation;

namespace Application.Services
{
    // Monta os dashboards conforme as permissões do usuário e centraliza os
    // indicadores, rankings, filtros e cálculos de acompanhamento do SLA.
    public class DashboardService : IDashboardService
    {
        private readonly IChamadoRepository _chamadoRepository;
        private readonly IusuarioService _usuarioService;
        private readonly ISlaService _slaService;
        private readonly IUsuarioPerfilService _usuarioPerfilService;

        public DashboardService(
            IChamadoRepository chamadoRepository,
            IusuarioService usuarioService,
            ISlaService slaService,
            IUsuarioPerfilService usuarioPerfilService)
        {
            _chamadoRepository = chamadoRepository;
            _usuarioService = usuarioService;
            _slaService = slaService;
            _usuarioPerfilService = usuarioPerfilService;
        }

        // ENTRADA DO DASHBOARD

        // Seleciona a visão correspondente às permissões do usuário logado.
        public async Task<DashboardViewModel> ObterDashboard(
            DashboardFiltroViewModel filtro)
        {
            var usuario = await _usuarioService.ObterUsuarioLogado();

            var dashboard = new DashboardViewModel
            {
                Perfil = usuario.Perfil,
                DataAtualizacao = DateTime.Now
            };

            if (usuario.Permissoes.Contains("DASHBOARD_ADMIN"))
            {
                return await MontarDashboardAdmin(dashboard, filtro);
            }
            else if (usuario.Permissoes.Contains("DASHBOARD_TECNICO"))
            {
                return await MontarDashboardTecnico(dashboard, usuario.Id);
            }
            else if (usuario.Permissoes.Contains("DASHBOARD_SOLICITANTE"))
            {
                return await MontarDashboardSolicitante(dashboard, usuario.Id);
            }

            throw new DomainExceptionValidation(
                "Usuário sem dashboard configurado.");
        }

        // MONTAGEM POR PERFIL

        // Monta a visão administrativa com filtros e rankings globais.
        private async Task<DashboardViewModel> MontarDashboardAdmin(
            DashboardViewModel dashboard,
            DashboardFiltroViewModel filtro)
        {
            var todosChamados =
                await _chamadoRepository.BuscarChamadosParaIndicadoresAdmin();

            var chamados =
                await AplicarFiltrosDashboard(todosChamados, filtro);

            dashboard.Resumo = ObterResumo(chamados);
            dashboard.Status = ObterStatus(chamados);
            dashboard.Prioridades = ObterPrioridades(chamados);
            dashboard.ChamadosPorMes = ObterChamadosPorMes(chamados);

            dashboard.RankingTecnicos =
                await ObterRankingTecnicos(chamados);

            dashboard.RankingSolicitantes =
                await ObterRankingSolicitantes(chamados);

            dashboard.RankingCategorias =
                ObterRankingCategorias(chamados);

            dashboard.IndicadoresAdmin =
                await ObterIndicadoresAdmin(chamados);

            return dashboard;
        }

        // Monta a visão usando somente os chamados relacionados ao técnico.
        private async Task<DashboardViewModel> MontarDashboardTecnico(
            DashboardViewModel dashboard,
            string usuarioId)
        {
            var chamados = (await _chamadoRepository
                .BuscarChamadosParaIndicadoresTecnico(usuarioId)).ToList();

            dashboard.Resumo = ObterResumo(chamados);
            dashboard.Status = ObterStatus(chamados);
            dashboard.Prioridades = ObterPrioridades(chamados);
            dashboard.ChamadosPorMes = ObterChamadosPorMes(chamados);
            dashboard.RankingCategorias = ObterRankingCategorias(chamados);
            dashboard.IndicadoresTecnico =
                await ObterIndicadoresTecnico(chamados);

            return dashboard;
        }

        // Monta a visão usando somente os chamados abertos pelo solicitante.
        private async Task<DashboardViewModel> MontarDashboardSolicitante(
            DashboardViewModel dashboard,
            string usuarioId)
        {
            var chamados = (await _chamadoRepository
                .BuscarChamadosParaIndicadoresSolicitante(usuarioId)).ToList();

            dashboard.Resumo = ObterResumo(chamados);
            dashboard.Status = ObterStatus(chamados);
            dashboard.Prioridades = ObterPrioridades(chamados);
            dashboard.ChamadosPorMes = ObterChamadosPorMes(chamados);
            dashboard.RankingCategorias = ObterRankingCategorias(chamados);
            dashboard.IndicadoresSolicitante =
                await ObterIndicadoresSolicitante(chamados);

            return dashboard;
        }

        // INDICADORES POR PERFIL

        // Indicadores administrativos calculados sobre o conjunto filtrado.
        private async Task<DashboardIndicadoresAdminViewModel> ObterIndicadoresAdmin(
            IEnumerable<Chamado> chamados)
        {
            var slasPorPrioridade = (await _slaService.BuscarTodos())
                .Where(sla => sla.Ativo)
                .ToDictionary(sla => sla.Prioridade);

            var agora = DateTime.Now;
            var hoje = DateTime.Today;

            var slaCumprido = ObterSlaCumprido(chamados, slasPorPrioridade);

            var slaViolado = ObterSlaViolado(chamados, slasPorPrioridade);

            return new DashboardIndicadoresAdminViewModel
            {
                ChamadosCriadosHoje = chamados.Count(chamado =>
                    chamado.DataAbertura.Date == hoje),

                ChamadosResolvidosHoje = chamados.Count(chamado =>
                    chamado.Status == StatusChamadoEnum.Fechado &&
                    chamado.DataFechamento.HasValue &&
                    chamado.DataFechamento.Value.Date == hoje),

                SlaCumprido = slaCumprido,

                SlaViolado = slaViolado,

                SlaPrimeiroAtendimentoCumprido =
                    ObterSlaPrimeiroAtendimentoCumprido(
                        chamados,
                        slasPorPrioridade),

                SlaPrimeiroAtendimentoViolado =
                    ObterSlaPrimeiroAtendimentoViolado(
                        chamados,
                        slasPorPrioridade),

                SlaResolucaoCumprido =
                    ObterSlaResolucaoCumprido(
                        chamados,
                        slasPorPrioridade),

                SlaResolucaoViolado =
                    ObterSlaResolucaoViolado(
                        chamados,
                        slasPorPrioridade),

                ChamadosProximosVencimento =
                    ObterChamadosProximosVencimento(
                        chamados,
                        slasPorPrioridade),

                PercentualSlaCumprido =
                    ObterPercentualSlaCumprido(
                        slaCumprido,
                        slaViolado),

                ChamadosCriticosAbertos =
                    ObterChamadosCriticosAbertos(
                        chamados),

                ChamadosForaSlaEmAberto =
                    ObterChamadosForaSlaEmAberto(
                        chamados,
                        slasPorPrioridade),

                TempoMedioCorridoHoras =
                    ObterTempoMedioCorridoHoras(
                        chamados,
                        agora),

                TempoMedioPrimeiroAtendimentoHoras =
                    ObterTempoMedioPrimeiroAtendimentoHoras(
                        chamados),

                TempoMedioPrimeiroAtendimentoSlaHoras =
                    ObterTempoMedioPrimeiroAtendimentoSlaHoras(
                        chamados,
                        agora),

                TempoMedioAguardandoRetornoHoras =
                    ObterTempoMedioAguardandoRetornoHoras(
                        chamados,
                        agora),

                TempoMedioAguardandoEquipamentoHoras =
                    ObterTempoMedioAguardandoEquipamentoHoras(
                        chamados,
                        agora),

                TempoMedioTotalPausadoHoras =
                    ObterTempoMedioTotalPausadoHoras(
                        chamados,
                        agora),

                TempoMedioResolucaoSlaHoras =
                    ObterTempoMedioResolucaoSlaHoras(
                        chamados,
                        slasPorPrioridade,
                        agora),

                TempoMedioAtendimentoEfetivoHoras =
                    ObterTempoMedioAtendimentoEfetivoHoras(
                        chamados,
                        agora)
            };
        }

        // Indicadores calculados sobre os chamados associados ao técnico.
        private async Task<DashboardIndicadoresTecnicoViewModel> ObterIndicadoresTecnico(
            IEnumerable<Chamado> chamados)
        {
            var slasPorPrioridade = (await _slaService.BuscarTodos())
                .Where(sla => sla.Ativo)
                .ToDictionary(sla => sla.Prioridade);

            var agora = DateTime.Now;

            return new DashboardIndicadoresTecnicoViewModel
            {
                ChamadosPendentes = chamados.Count(chamado =>
                    chamado.Status == StatusChamadoEnum.Aberto),

                MeusChamadosVencidos = ObterMeusChamadosVencidos(chamados, slasPorPrioridade),
                MeusChamadosProximosVencimento = ObterMeusChamadosProximosVencimento(chamados, slasPorPrioridade),
                MeusChamadosAguardandoRetorno = ObterMeusChamadosAguardandoRetorno(chamados),
                MeusChamadosResolvidos = ObterMeusChamadosResolvidosTecnico(chamados),
                MeuSlaCumprido = ObterMeuSlaCumprido(chamados, slasPorPrioridade),
                MeusChamadosCriticos = ObterMeusChamadosCriticos(chamados),
                TempoMedioCorridoHoras = ObterTempoMedioCorridoHoras(chamados, agora),
                TempoMedioPrimeiroAtendimentoHoras = ObterTempoMedioPrimeiroAtendimentoHoras(chamados),
                TempoMedioPrimeiroAtendimentoSlaHoras = ObterTempoMedioPrimeiroAtendimentoSlaHoras(chamados, agora),
                TempoMedioAguardandoRetornoHoras = ObterTempoMedioAguardandoRetornoHoras(chamados, agora),
                TempoMedioAguardandoEquipamentoHoras = ObterTempoMedioAguardandoEquipamentoHoras(chamados, agora),
                TempoMedioTotalPausadoHoras = ObterTempoMedioTotalPausadoHoras(chamados, agora),
                TempoMedioResolucaoSlaHoras = ObterTempoMedioResolucaoSlaHoras(chamados, slasPorPrioridade, agora),
                TempoMedioAtendimentoEfetivoHoras = ObterTempoMedioAtendimentoEfetivoHoras(chamados, agora),
            };
        }

        // Indicadores calculados sobre os chamados abertos pelo solicitante.
        private async Task<DashboardIndicadoresSolicitanteViewModel> ObterIndicadoresSolicitante(
            IEnumerable<Chamado> chamados)
        {
            var slasPorPrioridade = (await _slaService.BuscarTodos())
                .Where(sla => sla.Ativo)
                .ToDictionary(sla => sla.Prioridade);

            var agora = DateTime.Now;

            return new DashboardIndicadoresSolicitanteViewModel
            {
                MeusChamadosAguardandoRetorno = ObterChamadosAguardandoMinhaResposta(chamados),
                MeusChamadosForaSla = ObterMeusChamadosForaSla(chamados, slasPorPrioridade),
                MeusChamadosAbertos = ObterMeusChamadosAbertos(chamados),
                MeusChamadosEmAtendimento = ObterMeusChamadosEmAtendimento(chamados),
                MeusChamadosResolvidos = ObterMeusChamadosResolvidosSolicitante(chamados),
                TempoMedioCorridoHoras = ObterTempoMedioCorridoHoras(chamados, agora),
                TempoMedioPrimeiroAtendimentoHoras = ObterTempoMedioPrimeiroAtendimentoHoras(chamados),
                TempoMedioPrimeiroAtendimentoSlaHoras = ObterTempoMedioPrimeiroAtendimentoSlaHoras(chamados, agora),
                TempoMedioAguardandoRetornoHoras = ObterTempoMedioAguardandoRetornoHoras(chamados, agora),
                TempoMedioAguardandoEquipamentoHoras = ObterTempoMedioAguardandoEquipamentoHoras(chamados, agora),
                TempoMedioTotalPausadoHoras = ObterTempoMedioTotalPausadoHoras(chamados, agora),
                TempoMedioResolucaoSlaHoras = ObterTempoMedioResolucaoSlaHoras(chamados, slasPorPrioridade, agora),
                TempoMedioAtendimentoEfetivoHoras = ObterTempoMedioAtendimentoEfetivoHoras(chamados, agora),
            };
        }

        // RANKINGS

        private async Task<IEnumerable<DashboardRankingViewModel>> ObterRankingTecnicos(
            IEnumerable<Chamado> chamados)
        {
            var resultado = chamados
                .Where(chamado =>
                    !string.IsNullOrWhiteSpace(chamado.TecnicoResponsavelId))
                .GroupBy(chamado => chamado.TecnicoResponsavelId!)
                .OrderByDescending(grupo => grupo.Count())
                .ToList();

            var ranking = new List<DashboardRankingViewModel>();

            foreach (var grupo in resultado)
            {
                var usuario =
                    await _usuarioService.BuscarPorIdInterno(grupo.Key);

                ranking.Add(new DashboardRankingViewModel
                {
                    Nome = usuario.Username,
                    Quantidade = grupo.Count()
                });
            }

            return ranking;
        }

        private async Task<IEnumerable<DashboardRankingViewModel>> ObterRankingSolicitantes(
            IEnumerable<Chamado> chamados)
        {
            var resultado = chamados
                .Where(chamado =>
                    !string.IsNullOrWhiteSpace(chamado.SolicitanteId))
                .GroupBy(chamado => chamado.SolicitanteId!)
                .OrderByDescending(grupo => grupo.Count())
                .ToList();

            var ranking = new List<DashboardRankingViewModel>();

            foreach (var grupo in resultado)
            {
                var usuario =
                    await _usuarioService.BuscarPorIdInterno(grupo.Key);

                ranking.Add(new DashboardRankingViewModel
                {
                    Nome = usuario.Username,
                    Quantidade = grupo.Count()
                });
            }

            return ranking;
        }

        // RESUMOS E AGRUPAMENTOS

        private static DashboardResumoViewModel ObterResumo(
            IEnumerable<Chamado> chamados)
        {
            var chamadosLista = chamados.ToList();

            return new DashboardResumoViewModel
            {
                Total = chamadosLista.Count,
                Abertos = chamadosLista.Count(chamado =>
                    chamado.Status == StatusChamadoEnum.Aberto),
                Atendimento = chamadosLista.Count(chamado =>
                    chamado.Status == StatusChamadoEnum.EmAtendimento),
                Fechados = chamadosLista.Count(chamado =>
                    chamado.Status == StatusChamadoEnum.Fechado),
                Cancelados = chamadosLista.Count(chamado =>
                    chamado.Status == StatusChamadoEnum.Cancelado)
            };
        }

        private static IEnumerable<DashboardStatusViewModel> ObterStatus(
            IEnumerable<Chamado> chamados)
        {
            return chamados
                .GroupBy(chamado => chamado.Status)
                .Select(grupo => new DashboardStatusViewModel
                {
                    Status = grupo.Key.ToString(),
                    Quantidade = grupo.Count()
                });
        }

        private static IEnumerable<DashboardPrioridadeViewModel> ObterPrioridades(
            IEnumerable<Chamado> chamados)
        {
            return chamados
                .GroupBy(chamado => chamado.Prioridade)
                .Select(grupo => new DashboardPrioridadeViewModel
                {
                    Prioridade = grupo.Key.ToString(),
                    Quantidade = grupo.Count()
                });
        }

        private static IEnumerable<DashboardLinhaTempoViewModel> ObterChamadosPorMes(
            IEnumerable<Chamado> chamados)
        {
            return chamados
                .GroupBy(chamado => new
                {
                    chamado.DataAbertura.Year,
                    chamado.DataAbertura.Month
                })
                .OrderBy(grupo => grupo.Key.Year)
                .ThenBy(grupo => grupo.Key.Month)
                .Select(grupo => new DashboardLinhaTempoViewModel
                {
                    Periodo = $"{grupo.Key.Month:D2}/{grupo.Key.Year}",
                    Quantidade = grupo.Count()
                });
        }

        private static IEnumerable<DashboardRankingViewModel> ObterRankingCategorias(
            IEnumerable<Chamado> chamados)
        {
            return chamados
                .Where(chamado => chamado.CategoriaAtendimento != null)
                .GroupBy(chamado => chamado.CategoriaAtendimento!.Nome)
                .OrderByDescending(grupo => grupo.Count())
                .Select(grupo => new DashboardRankingViewModel
                {
                    Nome = grupo.Key,
                    Quantidade = grupo.Count()
                });
        }

        // REGRAS DE CLASSIFICAÇÃO DO SLA

        // Um chamado fechado só cumpre o SLA geral quando os prazos de
        // primeiro atendimento e resolução foram cumpridos.
        private static bool PossuiSlaGeralCumprido(
            Chamado chamado,
            SlaResponseDto sla,
            DateTime agora)
        {
            if (chamado.Status != StatusChamadoEnum.Fechado ||
                !chamado.DataFechamento.HasValue ||
                !chamado.DataPrimeiroAtendimento.HasValue)
            {
                return false;
            }

            var tempoPrimeiroAtendimento =
                chamado.CalcularTempoPrimeiroAtendimentoSlaHoras(agora);

            var tempoResolucao =
                chamado.CalcularTempoResolucaoSlaHoras(agora);

            return tempoPrimeiroAtendimento.HasValue &&
                   tempoResolucao.HasValue &&
                   tempoPrimeiroAtendimento.Value <= sla.TempoAtendimentoHoras &&
                   tempoResolucao.Value <= sla.TempoResolucaoHoras;
        }

        // Considera o SLA geral violado quando qualquer um dos dois prazos
        // ultrapassa o limite configurado para a prioridade.
        private static bool PossuiSlaGeralViolado(
            Chamado chamado,
            SlaResponseDto sla,
            DateTime agora)
        {
            return PossuiViolacaoPrimeiroAtendimento(chamado, sla, agora) ||
                   PossuiViolacaoResolucao(chamado, sla, agora);
        }

        private static bool PodeClassificarSla(Chamado chamado)
        {
            return chamado.Status != StatusChamadoEnum.Cancelado &&
                   (chamado.Status != StatusChamadoEnum.Fechado ||
                    chamado.DataPrimeiroAtendimento.HasValue);
        }

        private static bool PossuiViolacaoPrimeiroAtendimento(
            Chamado chamado,
            SlaResponseDto sla,
            DateTime agora)
        {
            if (!PodeClassificarSla(chamado))
                return false;

            var tempo =
                chamado.CalcularTempoPrimeiroAtendimentoSlaHoras(agora);

            return tempo.HasValue &&
                   tempo.Value > sla.TempoAtendimentoHoras;
        }

        private static bool PossuiViolacaoResolucao(
            Chamado chamado,
            SlaResponseDto sla,
            DateTime agora)
        {
            if (!PodeClassificarSla(chamado))
                return false;

            var tempo = chamado.CalcularTempoResolucaoSlaHoras(agora);

            return tempo.HasValue &&
                   tempo.Value > sla.TempoResolucaoHoras;
        }

        // Identifica chamados ativos que consumiram entre 80% e 100% de um
        // prazo ainda em andamento, sem incluir chamados que já violaram SLA.
        private static bool EstaProximoDoVencimento(
            Chamado chamado,
            SlaResponseDto sla,
            DateTime agora)
        {
            if (chamado.Status == StatusChamadoEnum.Fechado ||
                chamado.Status == StatusChamadoEnum.Cancelado ||
                sla.TempoAtendimentoHoras <= 0 ||
                sla.TempoResolucaoHoras <= 0 ||
                PossuiSlaGeralViolado(chamado, sla, agora))
            {
                return false;
            }

            var tempoResolucao =
                chamado.CalcularTempoResolucaoSlaHoras(agora);

            if (!tempoResolucao.HasValue)
                return false;

            var percentualResolucao =
                tempoResolucao.Value / sla.TempoResolucaoHoras;

            if (EstaEntreOitentaECemPorCento(percentualResolucao))
                return true;

            if (chamado.DataPrimeiroAtendimento.HasValue)
                return false;

            var tempoPrimeiroAtendimento =
                chamado.CalcularTempoPrimeiroAtendimentoSlaHoras(agora);

            if (!tempoPrimeiroAtendimento.HasValue)
                return false;

            var percentualPrimeiroAtendimento =
                tempoPrimeiroAtendimento.Value / sla.TempoAtendimentoHoras;

            return EstaEntreOitentaECemPorCento(
                percentualPrimeiroAtendimento);
        }

        private static bool EstaEntreOitentaECemPorCento(
            double percentual)
        {
            return percentual >= 0.8 && percentual <= 1.0;
        }

        //sla admin
        private int ObterSlaCumprido(
            IEnumerable<Chamado> chamados,
            IReadOnlyDictionary<PrioridadeEnum, SlaResponseDto> slasPorPrioridade)
        {
            var agora = DateTime.Now;

            return chamados.Count(chamado =>
                slasPorPrioridade.TryGetValue(chamado.Prioridade, out var sla) &&
                PossuiSlaGeralCumprido(chamado, sla, agora));
        }

        private int ObterSlaViolado(
            IEnumerable<Chamado> chamados,
            IReadOnlyDictionary<PrioridadeEnum, SlaResponseDto> slasPorPrioridade)
        {
            var agora = DateTime.Now;

            return chamados.Count(chamado =>
                chamado.Status == StatusChamadoEnum.Fechado &&
                chamado.DataFechamento.HasValue &&
                slasPorPrioridade.TryGetValue(chamado.Prioridade, out var sla) &&
                PossuiSlaGeralViolado(chamado, sla, agora));
        }

        // Conta os chamados que receberam o primeiro atendimento dentro
        // do prazo configurado para a prioridade.
        private int ObterSlaPrimeiroAtendimentoCumprido(
            IEnumerable<Chamado> chamados,
            IReadOnlyDictionary<PrioridadeEnum, SlaResponseDto> slasPorPrioridade)
        {
            var agora = DateTime.Now;

            return chamados.Count(chamado =>
            {
                if (chamado.Status == StatusChamadoEnum.Cancelado ||
                    !chamado.DataPrimeiroAtendimento.HasValue ||
                    !slasPorPrioridade.TryGetValue(chamado.Prioridade, out var sla))
                {
                    return false;
                }

                var tempo =
                    chamado.CalcularTempoPrimeiroAtendimentoSlaHoras(agora);

                return tempo.HasValue &&
                       tempo.Value <= sla.TempoAtendimentoHoras;
            });
        }

        // Conta os chamados cujo primeiro atendimento ultrapassou o prazo
        // configurado para a prioridade.
        private int ObterSlaPrimeiroAtendimentoViolado(
            IEnumerable<Chamado> chamados,
            IReadOnlyDictionary<PrioridadeEnum, SlaResponseDto> slasPorPrioridade)
        {
            var agora = DateTime.Now;

            return chamados.Count(chamado =>
            {
                if (chamado.Status == StatusChamadoEnum.Cancelado ||
                    !chamado.DataPrimeiroAtendimento.HasValue ||
                    !slasPorPrioridade.TryGetValue(chamado.Prioridade, out var sla))
                {
                    return false;
                }

                var tempo =
                    chamado.CalcularTempoPrimeiroAtendimentoSlaHoras(agora);

                return tempo.HasValue &&
                       tempo.Value > sla.TempoAtendimentoHoras;
            });
        }

        // Conta os chamados fechados cuja resolução cumpriu o prazo.
        private int ObterSlaResolucaoCumprido(
            IEnumerable<Chamado> chamados,
            IReadOnlyDictionary<PrioridadeEnum, SlaResponseDto> slasPorPrioridade)
        {
            var agora = DateTime.Now;

            return chamados.Count(chamado =>
            {
                if (chamado.Status != StatusChamadoEnum.Fechado ||
                    !chamado.DataFechamento.HasValue ||
                    !slasPorPrioridade.TryGetValue(chamado.Prioridade, out var sla))
                {
                    return false;
                }

                var tempo = chamado.CalcularTempoResolucaoSlaHoras(agora);

                return tempo.HasValue &&
                       tempo.Value <= sla.TempoResolucaoHoras;
            });
        }

        // Conta os chamados fechados cuja resolução ultrapassou o prazo.
        private int ObterSlaResolucaoViolado(
            IEnumerable<Chamado> chamados,
            IReadOnlyDictionary<PrioridadeEnum, SlaResponseDto> slasPorPrioridade)
        {
            var agora = DateTime.Now;

            return chamados.Count(chamado =>
            {
                if (chamado.Status != StatusChamadoEnum.Fechado ||
                    !chamado.DataFechamento.HasValue ||
                    !slasPorPrioridade.TryGetValue(chamado.Prioridade, out var sla))
                {
                    return false;
                }

                var tempo = chamado.CalcularTempoResolucaoSlaHoras(agora);

                return tempo.HasValue &&
                       tempo.Value > sla.TempoResolucaoHoras;
            });
        }

        private int ObterChamadosProximosVencimento(
            IEnumerable<Chamado> chamados,
            IReadOnlyDictionary<PrioridadeEnum, SlaResponseDto> slasPorPrioridade)
        {
            var agora = DateTime.Now;

            return chamados.Count(chamado =>
                slasPorPrioridade.TryGetValue(chamado.Prioridade, out var sla) &&
                EstaProximoDoVencimento(chamado, sla, agora));
        }

        private double ObterPercentualSlaCumprido(int slaCumprido, int slaViolado)
        {
            var total = slaCumprido + slaViolado;

            if (total == 0)
                return 0;

            return Math.Round(
                ((double)slaCumprido / total) * 100,
                2);
        }

        private int ObterChamadosCriticosAbertos(IEnumerable<Chamado> chamados)
        {
            var total = 0;

            foreach (var chamado in chamados)
            {
                if (chamado.Status == StatusChamadoEnum.Fechado ||
                    chamado.Status == StatusChamadoEnum.Cancelado)
                    continue;

                if (chamado.Prioridade == PrioridadeEnum.Critica)
                {
                    total++;
                }
            }

            return total;
        }

        private int ObterChamadosForaSlaEmAberto(
            IEnumerable<Chamado> chamados,
            IReadOnlyDictionary<PrioridadeEnum, SlaResponseDto> slasPorPrioridade)
        {
            var agora = DateTime.Now;

            return chamados.Count(chamado =>
                chamado.Status != StatusChamadoEnum.Fechado &&
                chamado.Status != StatusChamadoEnum.Cancelado &&
                slasPorPrioridade.TryGetValue(chamado.Prioridade, out var sla) &&
                PossuiSlaGeralViolado(chamado, sla, agora));
        }




        // INDICADORES DO TÉCNICO

        private int ObterMeusChamadosVencidos(
            IEnumerable<Chamado> chamados,
            IReadOnlyDictionary<PrioridadeEnum, SlaResponseDto> slasPorPrioridade)
        {
            return ObterSlaViolado(chamados, slasPorPrioridade);
        }

        private int ObterMeusChamadosProximosVencimento(
            IEnumerable<Chamado> chamados,
            IReadOnlyDictionary<PrioridadeEnum, SlaResponseDto> slasPorPrioridade)
        {
            return ObterChamadosProximosVencimento(
                chamados,
                slasPorPrioridade);
        }

        private int ObterMeusChamadosAguardandoRetorno(IEnumerable<Chamado> chamados)
        {
            return chamados.Count(chamado =>
                chamado.Status == StatusChamadoEnum.AguardandoRetorno);
        }

        private int ObterMeusChamadosResolvidosTecnico(
            IEnumerable<Chamado> chamados)
        {
            return chamados.Count(chamado =>
                chamado.Status == StatusChamadoEnum.Fechado);
        }

        private int ObterMeuSlaCumprido(
            IEnumerable<Chamado> chamados,
            IReadOnlyDictionary<PrioridadeEnum, SlaResponseDto> slasPorPrioridade)
        {
            return ObterSlaCumprido(chamados, slasPorPrioridade);
        }

        private int ObterMeusChamadosCriticos(IEnumerable<Chamado> chamados)
        {
            return chamados.Count(chamado =>
                chamado.Prioridade == PrioridadeEnum.Critica);
        }

        // INDICADORES DO SOLICITANTE

        private int ObterChamadosAguardandoMinhaResposta(IEnumerable<Chamado> chamados)
        {
            return chamados.Count(chamado =>
                chamado.Status == StatusChamadoEnum.AguardandoRetorno);
        }

        private int ObterMeusChamadosForaSla(
            IEnumerable<Chamado> chamados,
            IReadOnlyDictionary<PrioridadeEnum, SlaResponseDto> slasPorPrioridade)
        {
            return ObterSlaViolado(chamados, slasPorPrioridade);
        }

        private int ObterMeusChamadosAbertos(IEnumerable<Chamado> chamados)
        {
            return chamados.Count(chamado =>
                chamado.Status == StatusChamadoEnum.Aberto);
        }

        private int ObterMeusChamadosEmAtendimento(IEnumerable<Chamado> chamados)
        {
            return chamados.Count(chamado =>
                chamado.Status == StatusChamadoEnum.EmAtendimento);
        }

        private int ObterMeusChamadosResolvidosSolicitante(IEnumerable<Chamado> chamados)
        {
            return chamados.Count(chamado =>
                chamado.Status == StatusChamadoEnum.Fechado);
        }

        // MÉDIAS DE TEMPO

        // Calcula a média do tempo corrido dos chamados,
        // desde a abertura até agora ou até o fechamento.
        private double ObterTempoMedioCorridoHoras(
            IEnumerable<Chamado> chamados,
            DateTime agora)
        {
            double somaHoras = 0;
            var quantidade = 0;

            foreach (var chamado in chamados)
            {
                if (chamado.Status ==
                    StatusChamadoEnum.Cancelado)
                {
                    continue;
                }

                somaHoras +=
                    chamado.CalcularTempoCorridoHoras(
                        agora);

                quantidade++;
            }

            if (quantidade == 0)
                return 0;

            return somaHoras / quantidade;
        }


        // FILTROS ADMINISTRATIVOS

        // Aplica os filtros em memória sobre os chamados carregados para a
        // visão administrativa, preservando o período final como exclusivo.
        private async Task<List<Chamado>> AplicarFiltrosDashboard(
            IEnumerable<Chamado> chamados,
            DashboardFiltroViewModel filtro)
        {
            if (filtro.DataInicial.HasValue &&
                filtro.DataFinal.HasValue &&
                filtro.DataFinal.Value.Date <
                filtro.DataInicial.Value.Date)
            {
                throw new DomainExceptionValidation(
                    "A data final não pode ser menor que a data inicial.");
            }

            if (!Enum.IsDefined(
                typeof(TipoUsuarioDashboardEnum),
                filtro.TipoUsuario))
            {
                throw new DomainExceptionValidation(
                    "Tipo de usuário do dashboard inválido.");
            }

            var resultado = chamados;

            if (filtro.DataInicial.HasValue)
            {
                var dataInicial =
                    filtro.DataInicial.Value.Date;

                resultado = resultado.Where(chamado =>
                    chamado.DataAbertura >= dataInicial);
            }

            if (filtro.DataFinal.HasValue)
            {
                var dataFinalExclusiva =
                    filtro.DataFinal.Value.Date.AddDays(1);

                resultado = resultado.Where(chamado =>
                    chamado.DataAbertura <
                    dataFinalExclusiva);
            }

            if (filtro.CategoriaAtendimentoId.HasValue)
            {
                resultado = resultado.Where(chamado =>
                    chamado.CategoriaAtendimentoId ==
                    filtro.CategoriaAtendimentoId.Value);
            }

            if (filtro.Prioridade.HasValue)
            {
                resultado = resultado.Where(chamado =>
                    chamado.Prioridade ==
                    filtro.Prioridade.Value);
            }

            if (filtro.Status.HasValue)
            {
                resultado = resultado.Where(chamado =>
                    chamado.Status ==
                    filtro.Status.Value);
            }

            if (filtro.PerfilId.HasValue)
            {
                var usuariosPerfil =
                    await _usuarioPerfilService
                        .BuscarUsuariosPerfil(
                            filtro.PerfilId.Value);

                var usuariosIds = usuariosPerfil
                    .Select(usuario =>
                        usuario.UsuarioId)
                    .ToHashSet();

                if (filtro.TipoUsuario ==
                    TipoUsuarioDashboardEnum.Tecnico)
                {
                    resultado = resultado.Where(chamado =>
                        chamado.TecnicoResponsavelId != null &&
                        usuariosIds.Contains(
                            chamado.TecnicoResponsavelId));
                }
                else
                {
                    resultado = resultado.Where(chamado =>
                        chamado.SolicitanteId != null &&
                        usuariosIds.Contains(
                            chamado.SolicitanteId));
                }
            }

            if (!string.IsNullOrWhiteSpace(
                filtro.UsuarioId))
            {
                if (filtro.TipoUsuario ==
                    TipoUsuarioDashboardEnum.Tecnico)
                {
                    resultado = resultado.Where(chamado =>
                        chamado.TecnicoResponsavelId ==
                        filtro.UsuarioId);
                }
                else
                {
                    resultado = resultado.Where(chamado =>
                        chamado.SolicitanteId ==
                        filtro.UsuarioId);
                }
            }

            return resultado.ToList();
        }

        // MÉDIAS DE TEMPO DETALHADAS

        // Calcula a média entre a abertura do chamado
        // e o primeiro atendimento realizado.
        private double ObterTempoMedioPrimeiroAtendimentoHoras(
            IEnumerable<Chamado> chamados)
        {
            double somaHoras = 0;
            var quantidade = 0;

            foreach (var chamado in chamados)
            {
                if (chamado.Status ==
                    StatusChamadoEnum.Cancelado)
                {
                    continue;
                }

                var tempoPrimeiroAtendimento =
                    chamado
                        .CalcularTempoPrimeiroAtendimentoHoras();

                if (!tempoPrimeiroAtendimento.HasValue)
                    continue;

                somaHoras +=
                    tempoPrimeiroAtendimento.Value;

                quantidade++;
            }

            if (quantidade == 0)
                return 0;

            return somaHoras / quantidade;
        }

        // Calcula a média do primeiro atendimento considerando somente
        // as horas do período de trabalho fixado no chamado.
        private double ObterTempoMedioPrimeiroAtendimentoSlaHoras(
            IEnumerable<Chamado> chamados,
            DateTime agora)
        {
            double somaHoras = 0;
            var quantidade = 0;

            foreach (var chamado in chamados)
            {
                if (chamado.Status == StatusChamadoEnum.Cancelado ||
                    !chamado.DataPrimeiroAtendimento.HasValue)
                {
                    continue;
                }

                var tempoPrimeiroAtendimento =
                    chamado.CalcularTempoPrimeiroAtendimentoSlaHoras(agora);

                if (!tempoPrimeiroAtendimento.HasValue)
                    continue;

                somaHoras += tempoPrimeiroAtendimento.Value;
                quantidade++;
            }

            if (quantidade == 0)
                return 0;

            return somaHoras / quantidade;
        }

        // Calcula a média do tempo que os chamados
        // ficaram aguardando retorno do solicitante.
        private double ObterTempoMedioAguardandoRetornoHoras(
            IEnumerable<Chamado> chamados,
            DateTime agora)
        {
            double somaHoras = 0;
            var quantidade = 0;

            foreach (var chamado in chamados)
            {
                if (chamado.Status ==
                    StatusChamadoEnum.Cancelado)
                {
                    continue;
                }

                somaHoras +=
                    chamado
                        .CalcularTempoAguardandoRetornoHoras(
                            agora);

                quantidade++;
            }

            if (quantidade == 0)
                return 0;

            return somaHoras / quantidade;
        }

        // Calcula a média do tempo que os chamados
        // ficaram aguardando a chegada do equipamento.
        private double ObterTempoMedioAguardandoEquipamentoHoras(
            IEnumerable<Chamado> chamados,
            DateTime agora)
        {
            double somaHoras = 0;
            var quantidade = 0;

            foreach (var chamado in chamados)
            {
                if (chamado.Status ==
                    StatusChamadoEnum.Cancelado)
                {
                    continue;
                }

                somaHoras +=
                    chamado
                        .CalcularTempoAguardandoEquipamentoHoras(
                            agora);

                quantidade++;
            }

            if (quantidade == 0)
                return 0;

            return somaHoras / quantidade;
        }

        // Calcula a média do tempo total pausado,
        // somando retorno e equipamento.
        private double ObterTempoMedioTotalPausadoHoras(
            IEnumerable<Chamado> chamados,
            DateTime agora)
        {
            double somaHoras = 0;
            var quantidade = 0;

            foreach (var chamado in chamados)
            {
                if (chamado.Status ==
                    StatusChamadoEnum.Cancelado)
                {
                    continue;
                }

                somaHoras +=
                    chamado.CalcularTempoTotalPausadoHoras(
                        agora);

                quantidade++;
            }

            if (quantidade == 0)
                return 0;

            return somaHoras / quantidade;
        }

        // Calcula a média do tempo válido para o SLA
        // dos chamados que já foram fechados.
        private double ObterTempoMedioResolucaoSlaHoras(
            IEnumerable<Chamado> chamados,
            IReadOnlyDictionary<PrioridadeEnum, SlaResponseDto> slasPorPrioridade,
            DateTime agora)
        {
            double somaHoras = 0;
            var quantidade = 0;

            foreach (var chamado in chamados)
            {
                if (chamado.Status !=
                        StatusChamadoEnum.Fechado ||
                    !chamado.DataFechamento.HasValue)
                {
                    continue;
                }

                if (!slasPorPrioridade.ContainsKey(
                        chamado.Prioridade))
                {
                    continue;
                }

                var tempoResolucaoSlaHoras =
                    chamado.CalcularTempoResolucaoSlaHoras(
                        agora);

                if (!tempoResolucaoSlaHoras.HasValue)
                    continue;

                somaHoras += tempoResolucaoSlaHoras.Value;

                quantidade++;
            }

            if (quantidade == 0)
                return 0;

            return somaHoras / quantidade;
        }

        // Calcula a média do tempo efetivo de atendimento,
        // começando no primeiro atendimento e descontando pausas.
        private double ObterTempoMedioAtendimentoEfetivoHoras(
            IEnumerable<Chamado> chamados,
            DateTime agora)
        {
            double somaHoras = 0;
            var quantidade = 0;

            foreach (var chamado in chamados)
            {
                if (chamado.Status ==
                    StatusChamadoEnum.Cancelado)
                {
                    continue;
                }

                var tempoEfetivo =
                    chamado
                        .CalcularTempoEfetivoAtendimentoHoras(
                            agora);

                if (!tempoEfetivo.HasValue)
                    continue;

                somaHoras += tempoEfetivo.Value;
                quantidade++;
            }

            if (quantidade == 0)
                return 0;

            return somaHoras / quantidade;
        }
    }
}
