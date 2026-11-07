import { useCallback, useEffect, useState } from "react";
import {
  Activity,
  CircleCheck,
  Eye,
  Gauge,
  RefreshCw,
  Server,
  Timer,
  TriangleAlert,
} from "lucide-react";
import monitoramentoService from "../../services/monitoramentoService";
import { apiErrorMessage } from "../../utils/apiError";
import MonitoramentoDetalhesModal from "./MonitoramentoDetalhesModal";
import styles from "./monitoramento.module.css";

const intervaloAtualizacaoMs = 30_000;
const formatadorNumero = new Intl.NumberFormat("pt-BR");

const formatarDataHora = (data) => {
  if (!data) return "-";

  return new Intl.DateTimeFormat("pt-BR", {
    dateStyle: "short",
    timeStyle: "medium",
  }).format(new Date(data));
};

const formatarTempoResposta = (tempoMs) => {
  const valor = Number(tempoMs) || 0;

  if (valor < 1000) return `${Math.round(valor)} ms`;

  return `${(valor / 1000).toLocaleString("pt-BR", {
    minimumFractionDigits: 1,
    maximumFractionDigits: 1,
  })} s`;
};

const formatarDuracao = (segundos) => {
  const total = Math.max(0, Math.floor(Number(segundos) || 0));

  if (total < 60) return `${total} s`;
  if (total < 3600) return `${Math.floor(total / 60)} min`;
  if (total < 86400) return `${Math.floor(total / 3600)} h ${Math.floor((total % 3600) / 60)} min`;
  return `${Math.floor(total / 86400)} d ${Math.floor((total % 86400) / 3600)} h`;
};

const classeStatus = (statusCode) => {
  if (!statusCode) return styles.statusNeutral;
  if (statusCode >= 500) return styles.statusCritical;
  if (statusCode >= 400) return styles.statusWarning;
  return styles.statusNeutral;
};

export default function Monitoramento() {
  const [painel, setPainel] = useState(null);
  const [carregando, setCarregando] = useState(true);
  const [atualizando, setAtualizando] = useState(false);
  const [erro, setErro] = useState("");
  const [erroSelecionado, setErroSelecionado] = useState(null);

  const carregarPainel = useCallback(async (cargaInicial = false) => {
    if (cargaInicial) setCarregando(true);
    else setAtualizando(true);

    setErro("");

    try {
      const resultado = await monitoramentoService.obterPainel();
      setPainel(resultado);
    } catch (error) {
      setErro(
        apiErrorMessage(error, "Não foi possível atualizar o monitoramento."),
      );
    } finally {
      setCarregando(false);
      setAtualizando(false);
    }
  }, []);

  useEffect(() => {
    carregarPainel(true);

    const intervalo = window.setInterval(() => {
      carregarPainel();
    }, intervaloAtualizacaoMs);

    return () => window.clearInterval(intervalo);
  }, [carregarPainel]);

  const resumo = painel?.resumo ?? {};
  const incidentesAtivos = painel?.incidentesAtivos ?? [];
  const errosRecentes = painel?.errosRecentes ?? [];
  const endpointsLentos = painel?.endpointsLentos ?? [];

  const cards = [
    {
      titulo: "Incidentes ativos",
      valor: resumo.incidentesAtivos ?? 0,
      descricao: "Ocorrências ainda em atividade",
      icone: TriangleAlert,
      tom: "danger",
    },
    {
      titulo: "Erros na última hora",
      valor: resumo.errosUltimaHora ?? 0,
      descricao: "Falhas registradas nos últimos 60 min",
      icone: Activity,
      tom: "warning",
    },
    {
      titulo: "Endpoints lentos",
      valor: resumo.endpointsLentos ?? 0,
      descricao: "Respostas acima do limite definido",
      icone: Timer,
      tom: "info",
    },
    {
      titulo: "Requisições na última hora",
      valor: resumo.totalRequisicoesMonitoradas ?? 0,
      descricao: "Desde a última inicialização",
      icone: Server,
      tom: "neutral",
    },
  ];

  return (
    <div className={styles.container}>
      <header className={styles.pageHeader}>
        <div>
          <span className={styles.eyebrow}>ADMINISTRAÇÃO</span>
          <h1>Monitoramento do sistema</h1>
          <p>Acompanhe erros ativos, falhas recentes e endpoints com resposta lenta.</p>
        </div>

        <div className={styles.headerActions}>
          <div className={styles.updateInfo}>
            <span>Atualização automática a cada 30 segundos</span>
            <strong>Última atualização: {formatarDataHora(painel?.dataAtualizacao)}</strong>
          </div>
          <button
            type="button"
            className={styles.refreshButton}
            onClick={() => carregarPainel()}
            disabled={atualizando || carregando}
          >
            <RefreshCw size={17} className={atualizando ? styles.spinning : ""} />
            {atualizando ? "Atualizando..." : "Atualizar agora"}
          </button>
        </div>
      </header>

      {erro && <div className={styles.errorMessage} role="alert">{erro}</div>}

      {carregando && !painel ? (
        <div className={styles.loadingState}>
          <RefreshCw size={24} className={styles.spinning} />
          Carregando monitoramento...
        </div>
      ) : (
        <>
          <section className={styles.summaryGrid} aria-label="Resumo do monitoramento">
            {cards.map(({ titulo, valor, descricao, icone: Icone, tom }) => (
              <article className={styles.summaryCard} key={titulo}>
                <div className={`${styles.summaryIcon} ${styles[tom]}`}>
                  <Icone size={21} />
                </div>
                <div>
                  <span>{titulo}</span>
                  <strong>{formatadorNumero.format(valor)}</strong>
                  <p>{descricao}</p>
                </div>
              </article>
            ))}
          </section>

          <section className={styles.sectionCard}>
            <div className={styles.sectionHeader}>
              <div>
                <span className={styles.sectionEyebrow}>TEMPO REAL</span>
                <h2>Incidentes ativos</h2>
              </div>
              <span className={styles.countBadge}>{incidentesAtivos.length}</span>
            </div>

            {incidentesAtivos.length === 0 ? (
              <div className={styles.healthyState}>
                <CircleCheck size={30} />
                <div>
                  <strong>Nenhum incidente ativo</strong>
                  <span>O sistema não apresenta falhas em atividade neste momento.</span>
                </div>
              </div>
            ) : (
              <div className={styles.incidentsGrid}>
                {incidentesAtivos.map((incidente) => (
                  <article className={styles.incidentCard} key={incidente.identificador}>
                    <div className={styles.incidentTop}>
                      <div className={styles.endpointLine}>
                        <span className={styles.methodBadge}>{incidente.metodoHttp || "S/M"}</span>
                        <strong title={incidente.endpoint}>{incidente.endpoint || incidente.origem}</strong>
                      </div>
                      <span className={`${styles.statusBadge} ${classeStatus(incidente.statusCode)}`}>
                        {incidente.statusCode ?? incidente.tipoErro}
                      </span>
                    </div>

                    <p className={styles.incidentMessage}>{incidente.mensagem}</p>

                    <div className={styles.incidentMeta}>
                      <span><b>Código</b>{incidente.codigoErro || "-"}</span>
                      <span><b>Ocorrências</b>{incidente.quantidadeOcorrencias}</span>
                      <span><b>Ativo há</b>{formatarDuracao(incidente.duracaoAtivaSegundos)}</span>
                      <span><b>Última ocorrência</b>{formatarDataHora(incidente.ultimaOcorrencia)}</span>
                    </div>
                  </article>
                ))}
              </div>
            )}
          </section>

          <section className={styles.sectionCard}>
            <div className={styles.sectionHeader}>
              <div>
                <span className={styles.sectionEyebrow}>HISTÓRICO RECENTE</span>
                <h2>Erros recentes</h2>
              </div>
              <span className={styles.countBadge}>{errosRecentes.length}</span>
            </div>

            {errosRecentes.length === 0 ? (
              <div className={styles.compactEmpty}>Nenhum erro recente registrado.</div>
            ) : (
              <div className={styles.tableScroll}>
                <table className={styles.table}>
                  <thead>
                    <tr><th>Data</th><th>Origem</th><th>Endpoint</th><th>Status</th><th>Erro</th><th>Código</th><th></th></tr>
                  </thead>
                  <tbody>
                    {errosRecentes.map((item, indice) => (
                      <tr key={`${item.traceId || item.endpoint}-${item.dataOcorrencia}-${indice}`}>
                        <td>{formatarDataHora(item.dataOcorrencia)}</td>
                        <td>{item.origem}</td>
                        <td className={styles.endpointCell} title={item.endpoint}>{item.endpoint || "-"}</td>
                        <td><span className={`${styles.statusBadge} ${classeStatus(item.statusCode)}`}>{item.statusCode ?? "-"}</span></td>
                        <td>{item.tipoErro}</td>
                        <td><span className={styles.codeBadge}>{item.codigoErro || "-"}</span></td>
                        <td>
                          <button type="button" className={styles.detailButton} onClick={() => setErroSelecionado(item)}>
                            <Eye size={16} />Detalhes
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </section>

          <section className={styles.sectionCard}>
            <div className={styles.sectionHeader}>
              <div>
                <span className={styles.sectionEyebrow}>DESEMPENHO</span>
                <h2>Endpoints lentos</h2>
              </div>
              <Gauge size={20} className={styles.sectionIcon} />
            </div>

            {endpointsLentos.length === 0 ? (
              <div className={styles.compactEmpty}>Nenhum endpoint lento registrado.</div>
            ) : (
              <div className={styles.tableScroll}>
                <table className={`${styles.table} ${styles.performanceTable}`}>
                  <thead>
                    <tr><th>Método</th><th>Endpoint</th><th>Ocorrências</th><th>Tempo médio</th><th>Maior tempo</th><th>Último tempo</th><th>Última ocorrência</th></tr>
                  </thead>
                  <tbody>
                    {endpointsLentos.map((item) => (
                      <tr key={`${item.metodoHttp}-${item.endpoint}`}>
                        <td><span className={styles.methodBadge}>{item.metodoHttp}</span></td>
                        <td className={styles.endpointCell} title={item.endpoint}>{item.endpoint}</td>
                        <td>{item.quantidadeOcorrencias}</td>
                        <td>{formatarTempoResposta(item.tempoMedioMs)}</td>
                        <td><strong className={styles.slowTime}>{formatarTempoResposta(item.maiorTempoMs)}</strong></td>
                        <td>{formatarTempoResposta(item.ultimoTempoMs)}</td>
                        <td>{formatarDataHora(item.ultimaOcorrencia)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </section>
        </>
      )}

      {erroSelecionado && (
        <MonitoramentoDetalhesModal
          erro={erroSelecionado}
          onClose={() => setErroSelecionado(null)}
        />
      )}
    </div>
  );
}

