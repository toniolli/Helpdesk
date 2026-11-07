import {
  Clock3,
  Expand,
  Radio,
  Volume2,
  VolumeX,
  WifiOff,
} from "lucide-react";
import styles from "./centralAtendimento.module.css";

function formatarUltimaAtualizacao(valor) {
  if (!valor) return "Aguardando dados";

  return `Atualizado às ${valor.toLocaleTimeString("pt-BR", {
    hour: "2-digit",
    minute: "2-digit",
    second: "2-digit",
  })}`;
}

export default function CentralHeader({
  agora,
  ultimaAtualizacao,
  conectado,
  somAtivo,
  exibirBotaoTelaCheia,
  onAlternarSom,
  onAtivarTelaCheia,
}) {
  return (
    <header className={styles.header}>
      <div className={styles.brand}>
        <img
          src="/branding/helpdesk-logo.svg"
          alt="Help Desk"
        />
        <div>
          <span>MONITORAMENTO OPERACIONAL</span>
          <h1>Central de Atendimento</h1>
        </div>
      </div>

      <div className={styles.headerStatus}>
        <div className={styles.connectionStatus}>
          {conectado ? <Radio size={18} /> : <WifiOff size={18} />}
          <div>
            <strong>{conectado ? "Atualização ativa" : "Reconectando"}</strong>
            <span>{formatarUltimaAtualizacao(ultimaAtualizacao)}</span>
          </div>
        </div>

        <button
          type="button"
          className={`${styles.soundButton} ${
            somAtivo ? styles.soundButtonActive : ""
          }`}
          onClick={onAlternarSom}
          aria-pressed={somAtivo}
        >
          {somAtivo ? <Volume2 size={18} /> : <VolumeX size={18} />}
          {somAtivo ? "Som ativo" : "Ativar som"}
        </button>

        {exibirBotaoTelaCheia ? (
          <button
            type="button"
            className={styles.fullscreenButton}
            onClick={onAtivarTelaCheia}
          >
            <Expand size={18} />
            Ativar tela cheia
          </button>
        ) : null}

        <div className={styles.clock}>
          <Clock3 size={22} />
          <div>
            <strong>
              {agora.toLocaleTimeString("pt-BR", {
                hour: "2-digit",
                minute: "2-digit",
                second: "2-digit",
              })}
            </strong>
            <span>
              {agora.toLocaleDateString("pt-BR", {
                weekday: "long",
                day: "2-digit",
                month: "long",
              })}
            </span>
          </div>
        </div>
      </div>
    </header>
  );
}
