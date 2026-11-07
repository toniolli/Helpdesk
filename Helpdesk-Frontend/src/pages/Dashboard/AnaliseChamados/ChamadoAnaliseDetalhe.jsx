import { useState } from "react";
import {
  Activity,
  ArrowLeft,
  CalendarClock,
  Clock3,
  Headphones,
  History,
  Hourglass,
  MessageCircle,
  PauseCircle,
  ShieldCheck,
  Timer,
  UserRound,
  Wrench,
} from "lucide-react";
import { getStatusChamado } from "../../../utils/statusChamadoUtils";
import AuditoriaChamadoAnaliseModal from "./AuditoriaChamadoAnaliseModal";
import ComentariosAnaliseModal from "./ComentariosAnaliseModal";
import {
  classePrioridade,
  classeStatus,
  formatarDataChamado,
  formatarPrioridadeChamado,
  formatarTempoChamado,
} from "../../../utils/analiseChamadosUtils";
import styles from "./analiseChamados.module.css";

const METRICAS = [
  {
    campo: "tempoResolucaoSlaHoras",
    titulo: "Resolução pelo SLA",
    descricao: "Horas do período de trabalho, descontando espera por retorno e equipamento.",
    icon: ShieldCheck,
    destaque: true,
    dependePeriodoSla: true,
  },
  {
    campo: "tempoPrimeiroAtendimentoSlaHoras",
    titulo: "Primeiro atendimento pelo SLA",
    descricao: "Da abertura ao primeiro atendimento, somente nas horas do período de trabalho.",
    icon: Headphones,
    destaque: true,
    dependePeriodoSla: true,
  },
  {
    campo: "tempoCorridoHoras",
    titulo: "Tempo corrido",
    descricao: "Da abertura até agora ou até o encerramento, sem descontar pausas.",
    icon: CalendarClock,
  },
  {
    campo: "tempoEfetivoAtendimentoHoras",
    titulo: "Atendimento efetivo",
    descricao: "Do primeiro atendimento ao fim, descontando as duas pausas.",
    icon: Activity,
  },
  {
    campo: "tempoPrimeiroAtendimentoHoras",
    titulo: "Primeiro atendimento corrido",
    descricao: "Da abertura até o primeiro atendimento, considerando 24 horas por dia.",
    icon: Hourglass,
  },
  {
    campo: "tempoAguardandoRetornoHoras",
    titulo: "Espera pelo solicitante",
    descricao: "Total acumulado em Aguardando Retorno.",
    icon: PauseCircle,
  },
  {
    campo: "tempoAguardandoEquipamentoHoras",
    titulo: "Espera pelo equipamento",
    descricao: "Total acumulado em Aguardando Equipamento.",
    icon: Wrench,
  },
  {
    campo: "tempoTotalPausadoHoras",
    titulo: "Total pausado",
    descricao: "Soma das esperas pelo solicitante e pelo equipamento.",
    icon: Timer,
  },
];

export default function ChamadoAnaliseDetalhe({
  chamado,
  categoriaNome,
  onBack,
}) {
  const [modalAberto, setModalAberto] = useState(null);

  return (
    <div className={styles.detailView}>
      <button type="button" className={styles.backButton} onClick={onBack}>
        <ArrowLeft size={17} />
        Voltar para os chamados
      </button>

      <section className={styles.detailHero}>
        <div className={styles.detailHeroMain}>
          <span className={styles.detailEyebrow}>ANÁLISE DO ATENDIMENTO</span>
          <h2>{chamado.numero}</h2>
          <p>{chamado.descricao}</p>
          <div className={styles.ticketBadges}>
            <span
              className={`${styles.badge} ${styles[classeStatus(chamado.status)]}`}
            >
              {getStatusChamado(chamado.status)}
            </span>
            <span
              className={`${styles.badge} ${styles[classePrioridade(chamado.prioridade)]}`}
            >
              {formatarPrioridadeChamado(
                chamado.prioridade,
                chamado.prioridadeNome,
              )}
            </span>
          </div>
        </div>

        <div className={styles.detailActions}>
          <button type="button" onClick={() => setModalAberto("comentarios")}>
            <MessageCircle size={18} />
            Ver comentários
          </button>
          <button type="button" onClick={() => setModalAberto("auditoria")}>
            <History size={18} />
            Ver auditoria
          </button>
        </div>
      </section>

      <section className={styles.metricsSection}>
        <div className={styles.sectionHeading}>
          <span>TEMPOS DO CHAMADO</span>
          <h3>Onde o tempo do atendimento foi utilizado</h3>
          <p>
            Tempos corridos consideram 24 horas por dia. Os tempos de SLA
            consideram o período de trabalho fixado no chamado.
          </p>
        </div>
        <div className={styles.metricsGrid}>
          {METRICAS.map(({
            campo,
            titulo,
            descricao,
            icon: Icon,
            destaque,
            dependePeriodoSla,
          }) => (
            <article
              key={campo}
              className={`${styles.metricCard} ${destaque ? styles.metricFeatured : ""}`}
            >
              <div className={styles.metricIcon}>
                <Icon size={20} />
              </div>
              <span>{titulo}</span>
              <strong>
                {dependePeriodoSla && chamado.slaPendenteDefinicaoPerfil
                  ? "Pendente"
                  : formatarTempoChamado(chamado[campo])}
              </strong>
              <p>{descricao}</p>
            </article>
          ))}
        </div>
      </section>

      <div className={styles.detailColumns}>
        <section className={styles.infoPanel}>
          <div className={styles.sectionHeading}>
            <span>RESPONSÁVEIS</span>
            <h3>Pessoas envolvidas</h3>
          </div>
          <div className={styles.infoRows}>
            <div>
              <UserRound size={18} />
              <span><small>Solicitante</small>{chamado.solicitanteNome || "-"}</span>
            </div>
            <div>
              <Headphones size={18} />
              <span><small>Técnico responsável</small>{chamado.tecnicoNome || "Não atribuído"}</span>
            </div>
            <div>
              <Wrench size={18} />
              <span><small>Equipamento afetado</small>{chamado.equipamentoAfetado || "-"}</span>
            </div>
          </div>
        </section>

        <section className={styles.infoPanel}>
          <div className={styles.sectionHeading}>
            <span>CONTEXTO</span>
            <h3>Dados do chamado</h3>
          </div>
          <div className={styles.infoRows}>
            <div>
              <Clock3 size={18} />
              <span><small>Abertura</small>{formatarDataChamado(chamado.dataAbertura)}</span>
            </div>
            <div>
              <Headphones size={18} />
              <span><small>Primeiro atendimento</small>{formatarDataChamado(chamado.dataPrimeiroAtendimento)}</span>
            </div>
            <div>
              <CalendarClock size={18} />
              <span><small>Encerramento</small>{formatarDataChamado(chamado.dataFechamento)}</span>
            </div>
            <div>
              <Timer size={18} />
              <span><small>Categoria</small>{categoriaNome || "-"}</span>
            </div>
          </div>
        </section>
      </div>

      {chamado.solucao ? (
        <section className={styles.solutionPanel}>
          <span>SOLUÇÃO REGISTRADA</span>
          <p>{chamado.solucao}</p>
        </section>
      ) : null}

      {modalAberto === "comentarios" ? (
        <ComentariosAnaliseModal
          chamado={chamado}
          onClose={() => setModalAberto(null)}
        />
      ) : null}

      {modalAberto === "auditoria" ? (
        <AuditoriaChamadoAnaliseModal
          chamado={chamado}
          onClose={() => setModalAberto(null)}
        />
      ) : null}
    </div>
  );
}
