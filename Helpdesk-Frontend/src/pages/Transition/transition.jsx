import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import styles from "./transition.module.css";

const messages = [
  "Inicializando ambiente...",
  "Conectando ao servidor...",
  "Carregando chamados...",
  "Preparando painel...",
  "Bem-vindo!",
];

export default function Transition() {
  const navigate = useNavigate();
  const [messageIndex, setMessageIndex] = useState(0);

  useEffect(() => {
    const interval = setInterval(() => {
      setMessageIndex((prev) => (prev < messages.length - 1 ? prev + 1 : prev));
    }, 450);

    const timeout = setTimeout(() => {
      navigate("/");
    }, 3000);

    return () => {
      clearInterval(interval);
      clearTimeout(timeout);
    };
  }, [navigate]);

  const progress = ((messageIndex + 1) / messages.length) * 100;

  return (
    <main className={styles.container}>
      {/* Elementos decorativos */}
      <div className={styles.backgroundGlow}></div>
      <div className={styles.backgroundGlowSecondary}></div>
      <div className={styles.backgroundGrid}></div>
      <div className={styles.backgroundCircle}></div>

      <section className={styles.content}>
        {/* =========================
            CARD DE TRANSIÇÃO
        ========================== */}

        <div className={styles.card}>
          <div className={styles.cardGlow}></div>

          <div className={styles.brand}>
            <img
              src="/branding/helpdesk-icon.svg"
              alt="Help Desk"
              className={styles.logo}
            />

            <div>
              <span className={styles.brandSmall}>
                CENTRAL DE SUPORTE
              </span>
              <h1>Help Desk</h1>
            </div>
          </div>

          <div className={styles.divider}></div>

          {/* Status */}
          <div className={styles.status}>
            <span className={styles.statusDot}></span>

            <div>
              <span className={styles.statusLabel}>SISTEMA ONLINE</span>
              <span className={styles.statusText}>Preparando seu ambiente</span>
            </div>
          </div>

          {/* Mensagem atual */}
          <div className={styles.messageArea}>
            <span className={styles.messageLabel}>PROCESSAMENTO</span>

            <p className={styles.message} key={messageIndex}>
              {messages[messageIndex]}
            </p>
          </div>

          {/* Progresso */}
          <div className={styles.progressWrapper}>
            <div className={styles.progressInfo}>
              <span>Carregando sistema</span>
              <span>{Math.round(progress)}%</span>
            </div>

            <div className={styles.progress}>
              <div
                className={styles.progressBar}
                style={{
                  width: `${progress}%`,
                }}
              >
                <span className={styles.progressLight}></span>
              </div>
            </div>
          </div>

          {/* Steps */}
          <div className={styles.steps}>
            {messages.slice(0, 4).map((_, index) => (
              <div
                key={index}
                className={
                  index <= messageIndex ? styles.stepActive : styles.step
                }
              >
                <span className={styles.stepNumber}>0{index + 1}</span>
              </div>
            ))}
          </div>

          <div className={styles.footer}>
            <span>Central de Suporte</span>

            <div className={styles.version}>
              <span className={styles.versionDot}></span>
              Suporte para sua operação • v1.0.0
            </div>
          </div>
        </div>

        {/* =========================
            AVATAR
        ========================== */}

        <div className={styles.characterArea}>
          <div className={styles.connectionLine}></div>

          <div className={styles.characterGlow}></div>
          <div className={styles.characterRing}></div>
          <div className={styles.characterRingSecondary}></div>

          <img
            src="/Avatar.PNG"
            alt="Assistente do Help Desk"
            className={styles.character}
          />

          {/* Status flutuante */}
          <div className={styles.floatingCard}>
            <div className={styles.floatingIcon}>
              <svg
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2.5"
              >
                <path d="m5 12 4 4L19 6" />
              </svg>
            </div>

            <div>
              <span className={styles.floatingLabel}>SERVIÇOS ONLINE</span>
              <strong>Ambiente conectado</strong>
              <span className={styles.floatingDescription}>
                Tudo pronto para começar
              </span>
            </div>
          </div>

          {/* Pequeno indicador decorativo */}
          <div className={styles.systemBadge}>
            <span className={styles.systemBadgeDot}></span>
            <span>TI</span>
          </div>
        </div>
      </section>
    </main>
  );
}
