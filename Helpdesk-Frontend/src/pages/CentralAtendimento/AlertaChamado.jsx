import { BellRing, MapPin, UserRound } from "lucide-react";
import {
  PRIORIDADES,
  obterAssuntoChamado,
  obterNumeroChamado,
} from "../../utils/centralAtendimentoUtils";
import styles from "./centralAtendimento.module.css";

export default function AlertaChamado({ alerta }) {
  if (!alerta) return null;

  const prioridade =
    PRIORIDADES.find((item) => item.id === Number(alerta.prioridade)) ??
    PRIORIDADES.at(-1);

  return (
    <div className={styles.alertOverlay} role="alert" aria-live="assertive">
      <article className={`${styles.alertPanel} ${styles[prioridade.classe]}`}>
        <span className={styles.alertPriorityRail} aria-hidden="true" />

        <header className={styles.alertHeader}>
          <div className={styles.alertBrand}>
            <img
              src="/branding/helpdesk-logo.svg"
              alt="Help Desk"
            />
            <div>
              <strong>Central de Atendimento</strong>
              <span>Help Desk</span>
            </div>
          </div>

          <div className={styles.alertLive}>
            <span />
            Alerta em tempo real
          </div>
        </header>

        <div className={styles.alertBody}>
          <div className={styles.alertSignal}>
            <BellRing size={34} />
          </div>
          <div className={styles.alertContent}>
            <span className={styles.alertEyebrow}>NOVO CHAMADO RECEBIDO</span>
            <div className={styles.alertIdentification}>
              <strong>#{obterNumeroChamado(alerta)}</strong>
              <span>{prioridade.nome}</span>
            </div>
            <h2>{obterAssuntoChamado(alerta)}</h2>
          </div>
        </div>

        <div className={styles.alertDetails}>
          <span>
            <UserRound size={20} />
            <span>
              <small>Solicitante</small>
              <strong>
                {alerta.solicitanteNome || "Solicitante não informado"}
              </strong>
            </span>
          </span>
          <span>
            <MapPin size={20} />
            <span>
              <small>Destino</small>
              <strong>{alerta.destino || "Fila geral"}</strong>
            </span>
          </span>
        </div>

        <div className={styles.alertProgress} aria-hidden="true">
          <span />
        </div>
      </article>
    </div>
  );
}
