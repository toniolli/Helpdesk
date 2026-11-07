import { useEffect } from "react";
import { X } from "lucide-react";
import styles from "./monitoramento.module.css";

const formatarDataHora = (data) => {
  if (!data) return "-";

  return new Intl.DateTimeFormat("pt-BR", {
    dateStyle: "short",
    timeStyle: "medium",
  }).format(new Date(data));
};

export default function MonitoramentoDetalhesModal({ erro, onClose }) {
  useEffect(() => {
    const fecharComEscape = (event) => {
      if (event.key === "Escape") onClose();
    };

    window.addEventListener("keydown", fecharComEscape);
    return () => window.removeEventListener("keydown", fecharComEscape);
  }, [onClose]);

  if (!erro) return null;

  return (
    <div
      className={styles.overlay}
      onMouseDown={(event) => {
        if (event.target === event.currentTarget) onClose();
      }}
    >
      <section
        className={styles.modal}
        role="dialog"
        aria-modal="true"
        aria-labelledby="monitoramento-detalhes-titulo"
      >
        <div className={styles.modalHeader}>
          <div>
            <span className={styles.eyebrow}>ERRO MONITORADO</span>
            <h2 id="monitoramento-detalhes-titulo">Detalhes do erro</h2>
          </div>

          <button
            type="button"
            className={styles.closeButton}
            onClick={onClose}
            aria-label="Fechar detalhes"
          >
            <X size={20} />
          </button>
        </div>

        <div className={styles.detailsGrid}>
          <div><span>Data</span><strong>{formatarDataHora(erro.dataOcorrencia)}</strong></div>
          <div><span>Origem</span><strong>{erro.origem || "-"}</strong></div>
          <div><span>Método HTTP</span><strong>{erro.metodoHttp || "-"}</strong></div>
          <div><span>Status HTTP</span><strong>{erro.statusCode ?? "-"}</strong></div>
          <div><span>Tipo do erro</span><strong>{erro.tipoErro || "-"}</strong></div>
          <div><span>Código</span><strong>{erro.codigoErro || "-"}</strong></div>
          <div className={styles.wideDetail}>
            <span>Endpoint</span>
            <strong className={styles.breakText}>{erro.endpoint || "-"}</strong>
          </div>
          <div className={styles.wideDetail}>
            <span>TraceId</span>
            <strong className={styles.breakText}>{erro.traceId || "-"}</strong>
          </div>
        </div>

        <div className={styles.technicalDetails}>
          <div>
            <h3>Mensagem</h3>
            <pre>{erro.mensagem || "Sem mensagem registrada."}</pre>
          </div>
          <div>
            <h3>Exceção interna</h3>
            <pre>{erro.excecaoInterna || "Nenhuma exceção interna registrada."}</pre>
          </div>
        </div>
      </section>
    </div>
  );
}

