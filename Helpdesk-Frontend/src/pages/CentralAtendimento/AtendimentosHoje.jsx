import { CheckCircle2, Headphones, TrendingUp } from "lucide-react";
import styles from "./centralAtendimento.module.css";

export default function AtendimentosHoje({ iniciados, finalizados }) {
  const total = iniciados + finalizados;

  return (
    <section className={styles.screen} aria-labelledby="atendimentos-hoje-titulo">
      <div className={styles.screenHeading}>
        <div>
          <span>PRODUÇÃO DO DIA</span>
          <h2 id="atendimentos-hoje-titulo">Atendimentos de hoje</h2>
        </div>
        <strong>Atualização contínua</strong>
      </div>

      <div className={styles.todayLayout}>
        <article className={`${styles.todayMetric} ${styles.todayStarted}`}>
          <div className={styles.todayIcon}>
            <Headphones size={42} />
          </div>
          <span>Iniciados hoje</span>
          <strong>{iniciados}</strong>
          <p>Chamados que receberam o primeiro atendimento neste dia.</p>
        </article>

        <article className={`${styles.todayMetric} ${styles.todayFinished}`}>
          <div className={styles.todayIcon}>
            <CheckCircle2 size={42} />
          </div>
          <span>Finalizados hoje</span>
          <strong>{finalizados}</strong>
          <p>Chamados encerrados pela equipe durante o dia.</p>
        </article>

        <div className={styles.todaySummary}>
          <TrendingUp size={22} />
          <span>Movimentações de atendimento registradas hoje</span>
          <strong>{total}</strong>
        </div>
      </div>
    </section>
  );
}
