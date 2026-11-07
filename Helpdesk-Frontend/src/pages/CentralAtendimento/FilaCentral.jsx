import { Inbox, Timer } from "lucide-react";
import {
  agruparFilaPorPrioridade,
  formatarTempoEmFila,
  obterAssuntoChamado,
  obterIdChamado,
  obterNumeroChamado,
} from "../../utils/centralAtendimentoUtils";
import styles from "./centralAtendimento.module.css";

export default function FilaCentral({
  chamados,
  agora,
  prioridades,
  titulo,
  identificador,
}) {
  const grupos = agruparFilaPorPrioridade(chamados).filter((grupo) =>
    prioridades.includes(grupo.id),
  );
  const totalVisivel = grupos.reduce((total, grupo) => total + grupo.total, 0);

  return (
    <section className={styles.screen} aria-labelledby={identificador}>
      <div className={styles.screenHeading}>
        <div>
          <span>VISÃO DA FILA</span>
          <h2 id={identificador}>{titulo}</h2>
        </div>
        <strong>{totalVisivel} aguardando nesta visão</strong>
      </div>

      <div className={styles.queuePair}>
        {grupos.map((grupo) => (
          <article
            key={grupo.id}
            className={`${styles.queueLane} ${styles[grupo.classe]}`}
          >
            <header className={styles.queueHeader}>
              <div className={styles.queuePriorityTitle}>
                <span className={styles.priorityMarker} />
                <div>
                  <small>Prioridade</small>
                  <h3>{grupo.nome}</h3>
                </div>
              </div>
              <div
                className={`${styles.queueCount} ${
                  grupo.total === 0 ? styles.queueCountEmpty : ""
                }`}
              >
                <strong>{grupo.total}</strong>
                <span>
                  {grupo.total === 1 ? "chamado aguardando" : "chamados aguardando"}
                </span>
              </div>
            </header>

            <div className={styles.queueList}>
              {grupo.itens.length ? (
                grupo.itens.map((chamado) => (
                  <div
                    key={obterIdChamado(chamado)}
                    className={styles.queueRow}
                  >
                    <span className={styles.queueRowAccent} aria-hidden="true" />
                    <div className={styles.queueRowContent}>
                      <strong>#{obterNumeroChamado(chamado)}</strong>
                      <p>{obterAssuntoChamado(chamado)}</p>
                      <small>
                        Solicitante: {chamado.solicitanteNome || "Não informado"}
                      </small>
                    </div>
                    <span className={styles.queueWaitingTime}>
                      <Timer size={16} />
                      {formatarTempoEmFila(chamado.dataAbertura, agora)}
                    </span>
                  </div>
                ))
              ) : (
                <div className={styles.emptyQueue}>
                  <span className={styles.emptyQueueIcon}>
                    <Inbox size={30} />
                  </span>
                  <div>
                    <strong>Fila livre</strong>
                    <span>Nenhum chamado nesta prioridade</span>
                  </div>
                </div>
              )}
            </div>

            {grupo.restantes > 0 ? (
              <footer className={styles.queueFooter}>
                <strong>+{grupo.restantes}</strong>
                <span>chamado(s) além dos exibidos</span>
              </footer>
            ) : null}
          </article>
        ))}
      </div>
    </section>
  );
}
