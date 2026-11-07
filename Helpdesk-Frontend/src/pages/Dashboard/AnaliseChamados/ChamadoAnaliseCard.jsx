import {
  CalendarDays,
  CircleCheck,
  CircleX,
  ChevronRight,
  Clock3,
  Headphones,
  UserRound,
} from "lucide-react";
import { getStatusChamado } from "../../../utils/statusChamadoUtils";
import {
  classePrioridade,
  classeStatus,
  formatarDataChamado,
  formatarPrioridadeChamado,
  formatarTempoChamado,
} from "../../../utils/analiseChamadosUtils";
import styles from "./analiseChamados.module.css";

export default function ChamadoAnaliseCard({
  chamado,
  categoriaNome,
  situacaoSla,
  onSelect,
}) {
  const slaDentro = situacaoSla === "dentro";
  const slaFora = situacaoSla === "fora";

  const classeCardSla = slaFora
    ? styles.ticketCardSlaViolated
    : slaDentro
      ? styles.ticketCardSlaCompliant
      : "";

  const abrir = () => onSelect(chamado);

  const aoPressionar = (event) => {
    if (event.key === "Enter" || event.key === " ") {
      event.preventDefault();
      abrir();
    }
  };

  return (
    <article
      className={`${styles.ticketCard} ${classeCardSla}`}
      role="button"
      tabIndex={0}
      onClick={abrir}
      onKeyDown={aoPressionar}
      aria-label={`Ver análise do chamado ${chamado.numero}`}
    >
      <div className={styles.ticketCardHeader}>
        <div>
          <span className={styles.ticketNumber}>{chamado.numero}</span>
          <span className={styles.ticketDate}>
            <CalendarDays size={13} />
            {formatarDataChamado(chamado.dataAbertura)}
          </span>
        </div>
        <ChevronRight size={20} className={styles.ticketArrow} />
      </div>

      <div className={styles.ticketBadges}>
        <span className={`${styles.badge} ${styles[classeStatus(chamado.status)]}`}>
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
        {situacaoSla ? (
          <span
            className={`${styles.badge} ${styles.slaFlag} ${
              slaDentro
                ? styles.slaFlagCompliant
                : slaFora
                  ? styles.slaFlagViolated
                  : styles.slaFlagPending
            }`}
          >
            {slaDentro ? <CircleCheck size={13} /> : null}
            {slaFora ? <CircleX size={13} /> : null}
            {situacaoSla === "pendente" ? <Clock3 size={13} /> : null}
            {slaDentro
              ? "Dentro do SLA"
              : slaFora
                ? "Fora do SLA"
                : "SLA pendente"}
          </span>
        ) : null}
      </div>

      <p className={styles.ticketDescription}>{chamado.descricao}</p>

      <div className={styles.ticketMeta}>
        <span>{categoriaNome || "Categoria não informada"}</span>
        <span>{chamado.equipamentoAfetado || "Sem equipamento"}</span>
      </div>

      <div className={styles.ticketPeople}>
        <div>
          <UserRound size={15} />
          <span>
            <small>Solicitante</small>
            {chamado.solicitanteNome || "-"}
          </span>
        </div>
        <div>
          <Headphones size={15} />
          <span>
            <small>Técnico</small>
            {chamado.tecnicoNome || "Não atribuído"}
          </span>
        </div>
      </div>

      <div className={styles.ticketTimes}>
        <div>
          <Clock3 size={16} />
          <span>
            <small>Tempo corrido</small>
            <strong>{formatarTempoChamado(chamado.tempoCorridoHoras)}</strong>
          </span>
        </div>
        <div>
          <Headphones size={16} />
          <span>
            <small>Tempo válido de SLA</small>
            <strong>
              {chamado.slaPendenteDefinicaoPerfil
                ? "Pendente"
                : formatarTempoChamado(chamado.tempoResolucaoSlaHoras)}
            </strong>
          </span>
        </div>
      </div>

      <span className={styles.ticketAction}>Ver análise</span>
    </article>
  );
}
