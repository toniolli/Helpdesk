import {
  ArrowRight,
  AlertTriangle,
  Box,
  ClockAlert,
  Headset,
  Inbox,
  MessagesSquare,
  PauseCircle,
} from "lucide-react";
import styles from "./centralAtendimento.module.css";

export default function ResumoOperacional({ resumo }) {
  const possuiRiscoSla = resumo.proximosVencimento > 0 || resumo.foraSla > 0;

  return (
    <section className={styles.screen} aria-labelledby="resumo-operacional-titulo">
      <div className={styles.screenHeading}>
        <div>
          <span>PANORAMA EM TEMPO REAL</span>
          <h2 id="resumo-operacional-titulo">Resumo operacional</h2>
        </div>
        <strong>Operação atual</strong>
      </div>

      <div className={styles.operationCanvas}>
        <div className={styles.operationFlow}>
          <div className={`${styles.flowNode} ${styles.queueNode}`}>
            <div className={styles.flowIcon}>
              <Inbox size={31} />
            </div>
            <div className={styles.flowValue}>
              <strong>{resumo.fila ?? 0}</strong>
              <span>Aguardando na fila</span>
            </div>
          </div>

          <div className={styles.flowConnector} aria-hidden="true">
            <span />
            <ArrowRight size={25} />
          </div>

          <div className={`${styles.flowNode} ${styles.serviceNode}`}>
            <div className={styles.flowIcon}>
              <Headset size={31} />
            </div>
            <div className={styles.flowValue}>
              <strong>{resumo.emAtendimento ?? 0}</strong>
              <span>Em atendimento</span>
            </div>
          </div>

          <div className={styles.flowConnector} aria-hidden="true">
            <span />
            <ArrowRight size={25} />
          </div>

          <div className={styles.pauseFlow}>
            <div className={styles.pauseHub}>
              <PauseCircle size={27} />
              <div>
                <span>Chamados pausados</span>
                <strong>
                  {(resumo.aguardandoRetorno ?? 0) +
                    (resumo.aguardandoEquipamento ?? 0)}
                </strong>
              </div>
            </div>

            <div className={styles.pauseBranches}>
              <div className={styles.pauseBranch}>
                <span className={styles.branchLine} aria-hidden="true" />
                <MessagesSquare size={24} />
                <div>
                  <strong>{resumo.aguardandoRetorno ?? 0}</strong>
                  <span>Aguardando retorno</span>
                </div>
              </div>

              <div className={styles.pauseBranch}>
                <span className={styles.branchLine} aria-hidden="true" />
                <Box size={24} />
                <div>
                  <strong>{resumo.aguardandoEquipamento ?? 0}</strong>
                  <span>Aguardando equipamento</span>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div
          className={`${styles.slaRail} ${
            possuiRiscoSla ? styles.slaRailAttention : styles.slaRailStable
          }`}
        >
          <div className={styles.slaRailTitle}>
            <span className={styles.slaStatusLight} />
            <div>
              <strong>Monitoramento de SLA</strong>
              <span>
                {possuiRiscoSla
                  ? "Existem chamados que exigem atenção"
                  : "Operação dentro dos limites"}
              </span>
            </div>
          </div>

          <div className={styles.slaMetric}>
            <ClockAlert size={29} />
            <div>
              <span>Próximos do vencimento</span>
              <strong>{resumo.proximosVencimento ?? 0}</strong>
            </div>
          </div>

          <div
            className={`${styles.slaMetric} ${
              resumo.foraSla > 0 ? styles.slaMetricDanger : ""
            }`}
          >
            <AlertTriangle size={29} />
            <div>
              <span>Fora do SLA</span>
              <strong>{resumo.foraSla ?? 0}</strong>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}
