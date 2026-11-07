import { useCallback, useEffect, useMemo, useState } from "react";
import {
  Activity,
  AlertTriangle,
  BarChart3,
  CalendarClock,
  CheckCircle2,
  CircleAlert,
  CircleCheck,
  Clock,
  FileSearch,
  FolderOpen,
  Headphones,
  Hourglass,
  Info,
  LayoutDashboard,
  PauseCircle,
  RefreshCw,
  RotateCcw,
  ShieldAlert,
  ShieldCheck,
  SlidersHorizontal,
  Tags,
  Ticket,
  Timer,
  TrendingUp,
  Users,
  Wrench,
  XCircle,
} from "lucide-react";
import {
  Area,
  AreaChart,
  Bar,
  BarChart,
  CartesianGrid,
  Cell,
  Pie,
  PieChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import dashboardService from "../../services/dashboardService";
import usuarioService from "../../services/usuarioService";
import perfilService from "../../services/perfilService";
import categoriaService from "../../services/categoriaService";
import useAtualizacaoTempoReal from "../../hooks/useAtualizacaoTempoReal";
import { apiErrorMessage } from "../../utils/apiError";
import { hasPermission } from "../../utils/permissoesUtils";
import FiltrosGerenciaisModal from "../../components/FiltrosGerenciais/FiltrosGerenciaisModal";
import AnaliseChamadosTab from "./AnaliseChamados/AnaliseChamadosTab";
import {
  chartColors,
  prioridadeColors,
  statusColors,
} from "../../utils/chartColors";
import styles from "./dashboard.module.css";

const FILTROS_INICIAIS = {
  numeroChamado: "",
  dataInicial: "",
  dataFinal: "",
  tipoUsuario: "1",
  usuarioId: "",
  perfilId: "",
  categoriaAtendimentoId: "",
  prioridade: "",
  status: "",
};

const formatarTempo = (horas) => {
  const valor = Number(horas);

  if (!Number.isFinite(valor) || valor <= 0) return "0 min";

  const totalMinutos = Math.round(valor * 60);
  const dias = Math.floor(totalMinutos / 1440);
  const horasRestantes = Math.floor((totalMinutos % 1440) / 60);
  const minutos = totalMinutos % 60;
  const partes = [];

  if (dias > 0) partes.push(`${dias}d`);
  if (horasRestantes > 0) partes.push(`${horasRestantes}h`);
  if (minutos > 0 || partes.length === 0) partes.push(`${minutos}min`);

  return partes.join(" ");
};

const formatarStatus = (status) => {
  const nomes = {
    Aberto: "Aberto",
    EmAtendimento: "Em Atendimento",
    Fechado: "Fechado",
    Cancelado: "Cancelado",
    AguardandoRetorno: "Aguardando Retorno",
    Distribuido: "Distribuído",
    AguardandoEquipamento: "Aguardando Equipamento",
  };

  return nomes[status] || status;
};

const formatarPrioridade = (prioridade) => {
  const nomes = {
    Baixa: "Baixa",
    Media: "Média",
    Alta: "Alta",
    Critica: "Crítica",
  };

  return nomes[prioridade] || prioridade;
};

function IndicadorCard({ label, value, icon: Icon, tone = "primary" }) {
  return (
    <div className={`${styles.card} ${styles[tone]}`}>
      <div className={styles.cardHeader}>
        <span className={styles.cardLabel}>{label}</span>
        <div className={styles.cardIcon}>
          <Icon size={20} />
        </div>
      </div>
      <h2 className={styles.cardValue}>{value ?? 0}</h2>
    </div>
  );
}

function GrupoTempos({
  title,
  description,
  badge,
  tone,
  items,
  indicadores,
}) {
  return (
    <section className={`${styles.timeGroup} ${styles[tone]}`}>
      <div className={styles.timeGroupHeader}>
        <div>
          <h3>{title}</h3>
          <p>{description}</p>
        </div>
        <span className={styles.timeGroupBadge}>{badge}</span>
      </div>
      <div className={styles.timeGrid}>
        {items.map(({ key, label, description: itemDescription, icon: Icon }) => (
            <article
              key={key}
              className={styles.timeCard}
              tabIndex={0}
              aria-describedby={`descricao-${key}`}
            >
              <div className={styles.timeCardTop}>
                <div className={styles.timeIcon}>
                  <Icon size={22} />
                </div>
                <div className={styles.timeInfoIcon} aria-hidden="true">
                  <Info size={16} />
                </div>
              </div>
              <div className={styles.timeCardContent}>
                <span>{label}</span>
                <strong>{formatarTempo(indicadores[key])}</strong>
                <small>Média dos chamados selecionados</small>
              </div>
              <div
                id={`descricao-${key}`}
                className={styles.timeTooltip}
                role="tooltip"
              >
                <strong>Como este tempo é calculado</strong>
                <span>{itemDescription}</span>
              </div>
            </article>
          ))}
      </div>
    </section>
  );
}

export default function Dashboard() {
  const [dashboard, setDashboard] = useState(null);
  const [loading, setLoading] = useState(true);
  const [erro, setErro] = useState("");
  const [abaAtiva, setAbaAtiva] = useState("resumo");
  const [filtros, setFiltros] = useState(FILTROS_INICIAIS);
  const [filtrosAplicados, setFiltrosAplicados] = useState({});
  const [usuarios, setUsuarios] = useState([]);
  const [perfis, setPerfis] = useState([]);
  const [categorias, setCategorias] = useState([]);
  const [carregandoOpcoes, setCarregandoOpcoes] = useState(false);
  const [opcoesCarregadas, setOpcoesCarregadas] = useState(false);
  const [filtrosAbertos, setFiltrosAbertos] = useState(false);
  const podeAnalisarChamados =
    hasPermission("VISUALIZAR_AUDITORIA_CHAMADO") &&
    hasPermission("VISUALIZAR_TODOS_CHAMADOS");

  const filtrosDashboardAplicados = useMemo(
    () =>
      Object.fromEntries(
        Object.entries(filtrosAplicados).filter(
          ([campo]) => campo !== "numeroChamado",
        ),
      ),
    [filtrosAplicados],
  );

  const carregarDashboard = useCallback(async (filtrosConsulta = {}) => {
    try {
      setLoading(true);
      setErro("");
      const data = await dashboardService.obterDashboard(filtrosConsulta);
      setDashboard(data);
    } catch (error) {
      console.error("Erro ao carregar dashboard:", error);
      setErro(apiErrorMessage(error, "Não foi possível carregar o dashboard."));
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    carregarDashboard(filtrosDashboardAplicados);
  }, [carregarDashboard, filtrosDashboardAplicados]);

  const atualizarDashboardTempoReal = useCallback(() => {
    // Conserva os filtros aplicados e consulta novamente os indicadores quando o backend avisar.
    carregarDashboard(filtrosDashboardAplicados);
  }, [carregarDashboard, filtrosDashboardAplicados]);

  // Escuta tanto atualizações individuais quanto globais, pois ambas usam o mesmo nome de evento.
  useAtualizacaoTempoReal(
    "dashboardAtualizado",
    atualizarDashboardTempoReal,
    true,
    atualizarDashboardTempoReal,
  );

  const podeFiltrar =
    Boolean(dashboard?.indicadoresAdmin) || podeAnalisarChamados;

  useEffect(() => {
    const deveCarregarOpcoes = filtrosAbertos || abaAtiva === "analise";
    if (!podeFiltrar || !deveCarregarOpcoes || opcoesCarregadas) return;

    const carregarOpcoes = async () => {
      try {
        setCarregandoOpcoes(true);
        const [usuariosData, perfisData, categoriasData] = await Promise.all([
          usuarioService.buscarUsuariosHistorico(),
          perfilService.buscarOpcoesFiltro(),
          categoriaService.getAll(),
        ]);

        setUsuarios(usuariosData);
        setPerfis(perfisData);
        setCategorias(categoriasData);
        setOpcoesCarregadas(true);
      } catch (error) {
        console.error("Erro ao carregar filtros do dashboard:", error);
        setErro(
          apiErrorMessage(
            error,
            "O dashboard foi carregado, mas não foi possível buscar as opções dos filtros.",
          ),
        );
      } finally {
        setCarregandoOpcoes(false);
      }
    };

    carregarOpcoes();
  }, [abaAtiva, filtrosAbertos, opcoesCarregadas, podeFiltrar]);

  const alterarFiltro = (campo, valor) => {
    setFiltros((atuais) => ({
      ...atuais,
      [campo]: valor,
      ...(campo === "tipoUsuario" ? { usuarioId: "" } : {}),
    }));
  };

  const aplicarFiltros = () => {
    if (
      filtros.dataInicial &&
      filtros.dataFinal &&
      filtros.dataFinal < filtros.dataInicial
    ) {
      setErro("A data final não pode ser menor que a data inicial.");
      return false;
    }

    setErro("");
    setFiltrosAplicados({
      numeroChamado: filtros.numeroChamado.trim(),
      dataInicial: filtros.dataInicial,
      dataFinal: filtros.dataFinal,
      tipoUsuario: Number(filtros.tipoUsuario),
      usuarioId: filtros.usuarioId,
      perfilId: filtros.perfilId ? Number(filtros.perfilId) : "",
      categoriaAtendimentoId: filtros.categoriaAtendimentoId
        ? Number(filtros.categoriaAtendimentoId)
        : "",
      prioridade: filtros.prioridade ? Number(filtros.prioridade) : "",
      status: filtros.status ? Number(filtros.status) : "",
    });

    return true;
  };

  const limparFiltros = () => {
    setFiltros(FILTROS_INICIAIS);
    setFiltrosAplicados({});
    setErro("");
  };

  const totalFiltrosAtivos = Object.entries(filtrosAplicados).filter(
    ([campo, valor]) =>
      campo !== "tipoUsuario" && valor !== "" && valor != null,
  ).length;

  const dados = useMemo(() => {
    const statusChartData = (dashboard?.status || []).map((item) => ({
      ...item,
      statusFormatado: formatarStatus(item.status),
    }));
    const prioridadesChartData = (dashboard?.prioridades || []).map((item) => ({
      ...item,
      prioridadeFormatada: formatarPrioridade(item.prioridade),
    }));

    return {
      resumo: dashboard?.resumo || {},
      statusChartData,
      prioridadesChartData,
      totalStatus: statusChartData.reduce(
        (total, item) => total + Number(item.quantidade ?? 0),
        0,
      ),
      chamadosMes: dashboard?.chamadosPorMes || [],
      topTecnicos: (dashboard?.rankingTecnicos || []).slice(0, 10),
      topSolicitantes: (dashboard?.rankingSolicitantes || []).slice(0, 10),
      topCategorias: (dashboard?.rankingCategorias || []).slice(0, 10),
    };
  }, [dashboard]);

  if (loading && !dashboard) {
    return (
      <div className={styles.container}>
        <div className={styles.loading}>
          <RefreshCw size={22} className={styles.loadingIcon} />
          <span>Carregando dashboard...</span>
        </div>
      </div>
    );
  }

  if (!dashboard) {
    return (
      <div className={styles.container}>
        <div className={styles.loading}>
          <span>{erro || "Não foi possível carregar o dashboard."}</span>
          <button
            type="button"
            className={styles.retryButton}
            onClick={() => carregarDashboard(filtrosDashboardAplicados)}
          >
            Tentar novamente
          </button>
        </div>
      </div>
    );
  }

  const indicadoresAdmin = dashboard.indicadoresAdmin;
  const indicadoresTecnico = dashboard.indicadoresTecnico;
  const indicadoresSolicitante = dashboard.indicadoresSolicitante;
  const indicadoresTempo =
    indicadoresAdmin ?? indicadoresTecnico ?? indicadoresSolicitante ?? {};
  const dataAtualizacao = new Date(dashboard.dataAtualizacao).toLocaleString(
    "pt-BR",
  );
  const indicadoresOperacionais = [
    indicadoresAdmin?.chamadosForaSlaEmAberto ??
      indicadoresSolicitante?.meusChamadosForaSla,
    indicadoresAdmin?.chamadosProximosVencimento ??
      indicadoresTecnico?.meusChamadosProximosVencimento,
    indicadoresAdmin?.chamadosResolvidosHoje,
  ];

  const indicadoresResumo = indicadoresAdmin
    ? [
        ["Criados Hoje", indicadoresAdmin.chamadosCriadosHoje, Ticket, "info"],
        [
          "Resolvidos Hoje",
          indicadoresAdmin.chamadosResolvidosHoje,
          CircleCheck,
          "success",
        ],
        ["SLA Cumprido", indicadoresAdmin.slaCumprido, ShieldCheck, "success"],
        ["SLA Violado", indicadoresAdmin.slaViolado, ShieldAlert, "danger"],
        [
          "Percentual de SLA",
          `${indicadoresAdmin.percentualSlaCumprido ?? 0}%`,
          TrendingUp,
          "primary",
        ],
        [
          "Próximos do Vencimento",
          indicadoresAdmin.chamadosProximosVencimento,
          Clock,
          "warning",
        ],
        [
          "Críticos Abertos",
          indicadoresAdmin.chamadosCriticosAbertos,
          AlertTriangle,
          "danger",
        ],
        [
          "Fora do SLA",
          indicadoresAdmin.chamadosForaSlaEmAberto,
          CircleAlert,
          "danger",
        ],
      ]
    : indicadoresTecnico
      ? [
          [
            "Chamados Pendentes",
            indicadoresTecnico.chamadosPendentes,
            FolderOpen,
            "warning",
          ],
          [
            "Meus Resolvidos",
            indicadoresTecnico.meusChamadosResolvidos,
            CheckCircle2,
            "success",
          ],
          [
            "Chamados Vencidos",
            indicadoresTecnico.meusChamadosVencidos,
            AlertTriangle,
            "danger",
          ],
          [
            "Próximos do Vencimento",
            indicadoresTecnico.meusChamadosProximosVencimento,
            Clock,
            "warning",
          ],
          [
            "Aguardando Retorno",
            indicadoresTecnico.meusChamadosAguardandoRetorno,
            Clock,
            "info",
          ],
          [
            "Meu SLA Cumprido",
            indicadoresTecnico.meuSlaCumprido,
            ShieldCheck,
            "success",
          ],
          [
            "Meus Chamados Críticos",
            indicadoresTecnico.meusChamadosCriticos,
            CircleAlert,
            "danger",
          ],
        ]
      : [
          [
            "Chamados Abertos",
            indicadoresSolicitante?.meusChamadosAbertos,
            FolderOpen,
            "warning",
          ],
          [
            "Em Atendimento",
            indicadoresSolicitante?.meusChamadosEmAtendimento,
            Headphones,
            "info",
          ],
          [
            "Aguardando Retorno",
            indicadoresSolicitante?.meusChamadosAguardandoRetorno,
            Clock,
            "warning",
          ],
          [
            "Resolvidos",
            indicadoresSolicitante?.meusChamadosResolvidos,
            CheckCircle2,
            "success",
          ],
          [
            "Fora do SLA",
            indicadoresSolicitante?.meusChamadosForaSla,
            ShieldAlert,
            "danger",
          ],
        ];

  const indicadoresSlaDetalhado = indicadoresAdmin
    ? [
        [
          "1º atendimento cumprido",
          indicadoresAdmin.slaPrimeiroAtendimentoCumprido,
          Headphones,
          "success",
        ],
        [
          "1º atendimento violado",
          indicadoresAdmin.slaPrimeiroAtendimentoViolado,
          Hourglass,
          "danger",
        ],
        [
          "Resolução cumprida",
          indicadoresAdmin.slaResolucaoCumprido,
          ShieldCheck,
          "success",
        ],
        [
          "Resolução violada",
          indicadoresAdmin.slaResolucaoViolado,
          ShieldAlert,
          "danger",
        ],
      ]
    : [];

  const gruposTempos = [
    {
      title: "Tempos gerais",
      description: "Visão completa do ciclo e do início do atendimento.",
      badge: "Ciclo do chamado",
      tone: "timeGeneral",
      items: [
        [
          "tempoMedioCorridoHoras",
          "Tempo médio corrido",
          "Conta todo o período desde a abertura até agora ou até o " +
            "encerramento, sem descontar pausas. O cálculo considera 24 " +
            "horas por dia, inclusive finais de semana.",
          CalendarClock,
        ],
        [
          "tempoMedioPrimeiroAtendimentoHoras",
          "Primeiro atendimento corrido",
          "Conta o período entre a abertura e o primeiro atendimento " +
            "realizado nos chamados que já começaram a ser atendidos. " +
            "Considera 24 horas por dia, inclusive finais de semana.",
          Hourglass,
        ],
      ],
    },
    {
      title: "Tempos pausados",
      description: "Períodos que não consomem o prazo de resolução do SLA.",
      badge: "Pausas do SLA",
      tone: "timePaused",
      items: [
        [
          "tempoMedioAguardandoRetornoHoras",
          "Espera pelo solicitante",
          "Soma os períodos em Aguardando Retorno, incluindo a pausa " +
            "atual quando ela ainda estiver em andamento.",
          PauseCircle,
        ],
        [
          "tempoMedioAguardandoEquipamentoHoras",
          "Espera pelo equipamento",
          "Soma os períodos em Aguardando Equipamento, desde o envio " +
            "até a chegada à matriz ou a próxima ação técnica.",
          Wrench,
        ],
        [
          "tempoMedioTotalPausadoHoras",
          "Total pausado",
          "Soma as esperas pelo solicitante e pelo equipamento, incluindo " +
            "pausas que ainda estejam em andamento.",
          Timer,
        ],
      ],
    },
    {
      title: "Tempos de SLA e atendimento",
      description: "Prazos válidos do SLA e tempo efetivo de trabalho.",
      badge: "Prazos do SLA",
      tone: "timeResolution",
      items: [
        [
          "tempoMedioPrimeiroAtendimentoSlaHoras",
          "Primeiro atendimento pelo SLA",
          "Conta somente as horas do período de trabalho entre a abertura " +
            "e o primeiro atendimento. Não desconta pausas, pois elas só " +
            "existem depois que o chamado foi assumido.",
          Headphones,
        ],
        [
          "tempoMedioResolucaoSlaHoras",
          "Resolução pelo SLA",
          "Conta somente as horas do período de trabalho e desconta as " +
            "esperas pelo solicitante e pelo equipamento. É o tempo " +
            "utilizado para avaliar o SLA de resolução.",
          ShieldCheck,
        ],
        [
          "tempoMedioAtendimentoEfetivoHoras",
          "Atendimento efetivo",
          "Conta do primeiro atendimento até agora ou até o encerramento, " +
            "descontando as esperas pelo solicitante e pelo equipamento.",
          Activity,
        ],
      ],
    },
  ].map((grupo) => ({
    ...grupo,
    items: grupo.items.map(([key, label, description, icon]) => ({
      key,
      label,
      description,
      icon,
    })),
  }));

  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <div className={styles.headerContent}>
          <div className={styles.headerIcon}>
            <LayoutDashboard size={24} />
          </div>
          <div>
            <h1>Dashboard</h1>
            <div className={styles.headerInfo}>
              <span className={styles.profileBadge}>{dashboard.perfil}</span>
              <span className={styles.update}>
                Atualizado em {dataAtualizacao}
              </span>
              {loading && (
                <span className={styles.updating}>
                  <RefreshCw size={13} className={styles.loadingIcon} />
                  Atualizando
                </span>
              )}
            </div>
          </div>
        </div>

        {podeFiltrar && (
          <div className={styles.headerActions}>
            {totalFiltrosAtivos > 0 && (
              <button
                type="button"
                className={styles.clearFiltersButton}
                onClick={limparFiltros}
                disabled={loading}
              >
                <RotateCcw size={16} />
                Limpar
              </button>
            )}

            <button
              type="button"
              className={styles.openFiltersButton}
              onClick={() => setFiltrosAbertos(true)}
              aria-haspopup="dialog"
            >
              <SlidersHorizontal size={17} />
              {totalFiltrosAtivos > 0
                ? `Filtros ativos · ${totalFiltrosAtivos}`
                : abaAtiva === "analise"
                  ? "Filtrar chamados"
                  : "Filtrar dashboard"}
            </button>
          </div>
        )}
      </div>

      {podeFiltrar && filtrosAbertos && (
        <FiltrosGerenciaisModal
          titulo={
            abaAtiva === "analise"
              ? "Filtrar análise de chamados"
              : "Filtrar dashboard"
          }
          descricao={
            abaAtiva === "analise"
              ? "Refine a lista para localizar e analisar atendimentos específicos."
              : "Refine os indicadores e gráficos exibidos no dashboard."
          }
          mostrarNumeroChamado={abaAtiva === "analise"}
          filtros={filtros}
          usuarios={usuarios}
          perfis={perfis}
          categorias={categorias}
          erro={erro}
          carregando={loading}
          carregandoOpcoes={carregandoOpcoes}
          onChange={alterarFiltro}
          onApply={aplicarFiltros}
          onClear={limparFiltros}
          onClose={() => setFiltrosAbertos(false)}
        />
      )}

      {erro && <div className={styles.errorMessage}>{erro}</div>}

      <nav className={styles.tabs} aria-label="Seções do dashboard">
        <button
          type="button"
          className={abaAtiva === "resumo" ? styles.activeTab : ""}
          onClick={() => setAbaAtiva("resumo")}
        >
          <LayoutDashboard size={17} />
          Resumo
        </button>
        <button
          type="button"
          className={abaAtiva === "tempos" ? styles.activeTab : ""}
          onClick={() => setAbaAtiva("tempos")}
        >
          <Timer size={17} />
          Tempos
        </button>
        {indicadoresAdmin && (
          <button
            type="button"
            className={abaAtiva === "desempenho" ? styles.activeTab : ""}
            onClick={() => setAbaAtiva("desempenho")}
          >
            <BarChart3 size={17} />
            Desempenho
          </button>
        )}
        {podeAnalisarChamados && (
          <button
            type="button"
            className={abaAtiva === "analise" ? styles.activeTab : ""}
            onClick={() => setAbaAtiva("analise")}
          >
            <FileSearch size={17} />
            Análise de chamados
          </button>
        )}
      </nav>

      {abaAtiva === "resumo" && (
        <div className={styles.tabContent}>
          <div className={styles.cardsGrid}>
            <IndicadorCard
              label="Total de Chamados"
              value={dados.resumo.total}
              icon={Ticket}
              tone="total"
            />
            <IndicadorCard
              label="Abertos"
              value={dados.resumo.abertos}
              icon={FolderOpen}
              tone="abertos"
            />
            <IndicadorCard
              label="Em Atendimento"
              value={dados.resumo.atendimento}
              icon={Headphones}
              tone="atendimento"
            />
            <IndicadorCard
              label="Fechados"
              value={dados.resumo.fechados}
              icon={CheckCircle2}
              tone="fechados"
            />
            <IndicadorCard
              label="Cancelados"
              value={dados.resumo.cancelados}
              icon={XCircle}
              tone="cancelados"
            />
          </div>

          {indicadoresOperacionais.some((valor) => valor !== undefined) && (
            <section className={styles.operationalStrip} aria-label="Resumo operacional">
              <div className={styles.operationalIntro}>
                <span>Atividade operacional</span>
                <small>Resumo do que merece atenção agora</small>
              </div>
              {indicadoresOperacionais[0] !== undefined && (
                <div className={`${styles.operationalItem} ${styles.operationalDanger}`}>
                  <CircleAlert size={19} />
                  <span>
                    <strong>{indicadoresOperacionais[0] ?? 0}</strong>{" "}
                    chamado(s) fora do SLA
                  </span>
                </div>
              )}
              {indicadoresOperacionais[1] !== undefined && (
                <div className={`${styles.operationalItem} ${styles.operationalWarning}`}>
                  <Clock size={19} />
                  <span>
                    <strong>{indicadoresOperacionais[1] ?? 0}</strong>{" "}
                    próximo(s) do vencimento
                  </span>
                </div>
              )}
              {indicadoresOperacionais[2] !== undefined && (
                <div className={`${styles.operationalItem} ${styles.operationalSuccess}`}>
                  <CircleCheck size={19} />
                  <span><strong>{indicadoresOperacionais[2] ?? 0}</strong> resolvido(s) hoje</span>
                </div>
              )}
            </section>
          )}

          <div className={styles.sectionHeader}>
            <div>
              <span className={styles.sectionEyebrow}>
                {indicadoresAdmin
                  ? "VISÃO GERENCIAL"
                  : indicadoresTecnico
                    ? "MINHA OPERAÇÃO"
                    : "MEUS CHAMADOS"}
              </span>
              <h2 className={styles.sectionTitle}>
                {indicadoresAdmin
                  ? "Indicadores da Operação"
                  : indicadoresTecnico
                    ? "Indicadores Técnicos"
                    : "Acompanhamento dos Chamados"}
              </h2>
            </div>
          </div>
          <div className={styles.indicatorsGrid}>
            {indicadoresResumo.map(([label, value, icon, tone]) => (
              <IndicadorCard key={label} label={label} value={value} icon={icon} tone={tone} />
            ))}
          </div>

          {indicadoresAdmin && (
            <>
              <div className={styles.slaBreakdownHeader}>
                <span className={styles.sectionEyebrow}>COMPOSIÇÃO DO SLA</span>
                <h3 className={styles.cardTitle}>
                  Primeiro atendimento e resolução
                </h3>
                <p>
                  O SLA geral só é cumprido quando os dois prazos são atendidos.
                </p>
              </div>
              <div className={`${styles.indicatorsGrid} ${styles.slaBreakdownGrid}`}>
                {indicadoresSlaDetalhado.map(([label, value, icon, tone]) => (
                  <IndicadorCard
                    key={label}
                    label={label}
                    value={value}
                    icon={icon}
                    tone={tone}
                  />
                ))}
              </div>
            </>
          )}

          <div className={styles.sectionHeader}>
            <div>
              <span className={styles.sectionEyebrow}>VISÃO ANALÍTICA</span>
              <h2 className={styles.sectionTitle}>Distribuição dos Chamados</h2>
            </div>
          </div>
          <div className={styles.chartsGrid}>
            <div className={styles.chartCard}>
              <div className={styles.chartHeader}>
                <h3 className={styles.cardTitle}>Status dos Chamados</h3>
              </div>
              {dados.statusChartData.length > 0 ? (
                <div className={styles.statusChartLayout}>
                  <div className={styles.donutChart}>
                    <ResponsiveContainer width="100%" height={280}>
                      <PieChart>
                        <Pie
                          data={dados.statusChartData}
                          dataKey="quantidade"
                          nameKey="statusFormatado"
                          innerRadius={70}
                          outerRadius={105}
                          paddingAngle={3}
                        >
                          {dados.statusChartData.map((item) => (
                            <Cell
                              key={item.status}
                              fill={
                                statusColors[item.status]?.() ||
                                chartColors.primary()
                              }
                            />
                          ))}
                        </Pie>
                        <Tooltip />
                      </PieChart>
                    </ResponsiveContainer>
                    <div className={styles.donutTotal}>
                      <span>Total</span>
                      <strong>{dados.resumo.total ?? dados.totalStatus}</strong>
                    </div>
                  </div>
                  <div className={styles.chartLegend}>
                    {dados.statusChartData.map((item) => {
                      const quantidade = Number(item.quantidade ?? 0);
                      const percentual = dados.totalStatus
                        ? Math.round((quantidade / dados.totalStatus) * 100)
                        : 0;
                      return (
                        <div key={item.status} className={styles.legendItem}>
                          <span
                            className={styles.legendColor}
                            style={{
                              background:
                                statusColors[item.status]?.() ||
                                chartColors.primary(),
                            }}
                          />
                          <span>{item.statusFormatado}</span>
                          <strong>{quantidade}</strong>
                          <small>{percentual}%</small>
                        </div>
                      );
                    })}
                  </div>
                </div>
              ) : <div className={styles.chartPlaceholder}>Nenhum dado disponível.</div>}
            </div>

            <div className={styles.chartCard}>
              <div className={styles.chartHeader}>
                <h3 className={styles.cardTitle}>Distribuição por Prioridade</h3>
              </div>
              {dados.prioridadesChartData.length > 0 ? (
                <ResponsiveContainer width="100%" height={300}>
                  <BarChart data={dados.prioridadesChartData}>
                    <CartesianGrid vertical={false} strokeDasharray="3 3" stroke="var(--border)" />
                    <XAxis
                      dataKey="prioridadeFormatada"
                      axisLine={false}
                      tickLine={false}
                      tick={{ fill: "var(--text-light)", fontSize: 12 }}
                    />
                    <YAxis
                      axisLine={false}
                      tickLine={false}
                      tick={{ fill: "var(--text-light)", fontSize: 12 }}
                    />
                    <Tooltip />
                    <Bar dataKey="quantidade" radius={[8, 8, 0, 0]}>
                      {dados.prioridadesChartData.map((item) => (
                        <Cell
                          key={item.prioridade}
                          fill={
                            prioridadeColors[item.prioridadeFormatada]?.() ||
                            chartColors.primary()
                          }
                        />
                      ))}
                    </Bar>
                  </BarChart>
                </ResponsiveContainer>
              ) : <div className={styles.chartPlaceholder}>Nenhum dado disponível.</div>}
            </div>
          </div>

          <div className={styles.fullWidthCard}>
            <div className={styles.chartHeader}>
              <div>
                <span className={styles.sectionEyebrow}>HISTÓRICO</span>
                <h3 className={styles.cardTitle}>Evolução de Chamados</h3>
              </div>
            </div>
            {dados.chamadosMes.length > 0 ? (
              <ResponsiveContainer width="100%" height={380}>
                <AreaChart data={dados.chamadosMes}>
                  <CartesianGrid vertical={false} strokeDasharray="3 3" stroke="var(--border)" />
                  <XAxis
                    dataKey="periodo"
                    axisLine={false}
                    tickLine={false}
                    tick={{ fill: "var(--text-light)", fontSize: 12 }}
                  />
                  <YAxis
                    axisLine={false}
                    tickLine={false}
                    tick={{ fill: "var(--text-light)", fontSize: 12 }}
                  />
                  <Tooltip />
                  <Area
                    type="monotone"
                    dataKey="quantidade"
                    stroke={chartColors.primary()}
                    fill={chartColors.primary()}
                    fillOpacity={0.12}
                    strokeWidth={3}
                  />
                </AreaChart>
              </ResponsiveContainer>
            ) : <div className={styles.chartLargePlaceholder}>Nenhum dado disponível.</div>}
          </div>
        </div>
      )}

      {abaAtiva === "tempos" && (
        <div className={styles.tabContent}>
          <div className={styles.sectionHeader}>
            <div>
              <span className={styles.sectionEyebrow}>INDICADORES DE TEMPO</span>
              <h2 className={styles.sectionTitle}>Médias dos chamados selecionados</h2>
            </div>
          </div>
          <div className={styles.timeOverview}>
            <div className={styles.timeOverviewIcon}>
              <Timer size={22} />
            </div>
            <div>
              <strong>Entenda onde o tempo do atendimento foi utilizado</strong>
              <span>
                Os valores são médias dos chamados exibidos no dashboard.
                Passe o mouse ou selecione um card para ver como cada tempo
                é calculado.
              </span>
            </div>
          </div>
          <div className={styles.timeGroups}>
            {gruposTempos.map((grupo) => (
              <GrupoTempos
                key={grupo.title}
                {...grupo}
                indicadores={indicadoresTempo}
              />
            ))}
          </div>
        </div>
      )}

      {abaAtiva === "desempenho" && indicadoresAdmin && (
        <div className={styles.tabContent}>
          <div className={styles.sectionHeader}>
            <div>
              <span className={styles.sectionEyebrow}>DESEMPENHO</span>
              <h2 className={styles.sectionTitle}>Equipe, solicitantes e demanda</h2>
            </div>
          </div>
          {dados.topTecnicos.length === 0 &&
          dados.topSolicitantes.length === 0 &&
          dados.topCategorias.length === 0 ? (
            <div className={styles.emptyState}>
              Nenhum dado de desempenho disponível para os filtros aplicados.
            </div>
          ) : (
            <>
              <div className={styles.rankingsGrid}>
                {dados.topSolicitantes.length > 0 && (
                  <div className={styles.chartCard}>
                    <div className={styles.chartHeader}>
                      <div className={styles.chartTitleGroup}>
                        <Users size={19} />
                        <h3 className={styles.cardTitle}>
                          Top 10 Solicitantes
                        </h3>
                      </div>
                    </div>
                    <ResponsiveContainer width="100%" height={350}>
                      <BarChart layout="vertical" data={dados.topSolicitantes}>
                        <XAxis type="number" />
                        <YAxis dataKey="nome" type="category" width={150} />
                        <Tooltip />
                        <Bar
                          dataKey="quantidade"
                          fill={chartColors.primary()}
                          radius={[0, 7, 7, 0]}
                        />
                      </BarChart>
                    </ResponsiveContainer>
                  </div>
                )}
                {dados.topTecnicos.length > 0 && (
                  <div className={styles.chartCard}>
                    <div className={styles.chartHeader}>
                      <div className={styles.chartTitleGroup}>
                        <Wrench size={19} />
                        <h3 className={styles.cardTitle}>Top 10 Técnicos</h3>
                      </div>
                    </div>
                    <ResponsiveContainer width="100%" height={350}>
                      <BarChart layout="vertical" data={dados.topTecnicos}>
                        <XAxis type="number" />
                        <YAxis dataKey="nome" type="category" width={150} />
                        <Tooltip />
                        <Bar
                          dataKey="quantidade"
                          fill={chartColors.success()}
                          radius={[0, 7, 7, 0]}
                        />
                      </BarChart>
                    </ResponsiveContainer>
                  </div>
                )}
              </div>
              {dados.topCategorias.length > 0 && (
                <div className={styles.fullWidthCard}>
                  <div className={styles.chartHeader}>
                    <div className={styles.chartTitleGroup}>
                      <Tags size={19} />
                      <h3 className={styles.cardTitle}>
                        Volume de Chamados por Categoria
                      </h3>
                    </div>
                  </div>
                  <ResponsiveContainer width="100%" height={380}>
                    <BarChart
                      layout="vertical"
                      data={dados.topCategorias}
                      margin={{ top: 10, right: 20, left: 20, bottom: 10 }}
                    >
                      <XAxis type="number" />
                      <YAxis type="category" dataKey="nome" width={180} />
                      <Tooltip />
                      <Bar
                        dataKey="quantidade"
                        fill={chartColors.warning()}
                        radius={[0, 7, 7, 0]}
                      />
                    </BarChart>
                  </ResponsiveContainer>
                </div>
              )}
            </>
          )}
        </div>
      )}

      {abaAtiva === "analise" && podeAnalisarChamados && (
        <div className={styles.tabContent}>
          <AnaliseChamadosTab
            filtros={filtrosAplicados}
            categorias={categorias}
          />
        </div>
      )}
    </div>
  );
}
