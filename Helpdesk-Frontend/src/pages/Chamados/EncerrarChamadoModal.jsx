import { useEffect, useState } from "react";
import { CircleCheckBig, Info, X } from "lucide-react";
import chamadoService from "../../services/chamadoService";
import styles from "./EncerrarChamadoModal.module.css";

export default function EncerrarChamadoModal({ chamado, onClose, onSuccess }) {
  const [solucao, setSolucao] = useState("");
  const [encerrando, setEncerrando] = useState(false);
  const [erro, setErro] = useState("");

  useEffect(() => {
    const fecharComEscape = (event) => {
      if (event.key === "Escape" && !encerrando) {
        onClose();
      }
    };

    document.addEventListener("keydown", fecharComEscape);

    return () => document.removeEventListener("keydown", fecharComEscape);
  }, [encerrando, onClose]);

  const encerrarChamado = async () => {
    const solucaoTratada = solucao.trim();

    if (!solucaoTratada) {
      setErro("Informe a solução aplicada antes de encerrar o chamado.");
      return;
    }

    try {
      setEncerrando(true);
      setErro("");

      await chamadoService.fechar(chamado.id_Chamado, {
        tipoAtendimento: 1,
        tipoEncerramento: 1,
        solucao: solucaoTratada,
      });

      onSuccess(chamado.id_Chamado);
    } catch (error) {
      console.error("Erro ao encerrar chamado:", error);
      setErro("Não foi possível encerrar o chamado. Tente novamente.");
      setEncerrando(false);
    }
  };

  return (
    <div className={styles.overlay}>
      <div
        className={styles.modal}
        role="dialog"
        aria-modal="true"
        aria-labelledby="encerrar-chamado-titulo"
        aria-describedby="encerrar-chamado-descricao"
      >
        <div className={styles.header}>
          <div className={styles.heading}>
            <div className={styles.iconContainer} aria-hidden="true">
              <CircleCheckBig size={24} strokeWidth={2.2} />
            </div>
            <div>
              <span className={styles.eyebrow}>FINALIZAR ATENDIMENTO</span>
              <h3 id="encerrar-chamado-titulo">Encerrar chamado</h3>
              <p id="encerrar-chamado-descricao">
                Registre a solução aplicada para concluir o atendimento.
              </p>
            </div>
          </div>
          <button
            type="button"
            className={styles.closeButton}
            onClick={onClose}
            disabled={encerrando}
            aria-label="Fechar modal"
          >
            <X size={19} strokeWidth={2.2} />
          </button>
        </div>

        <div className={styles.ticketSummary}>
          <div>
            <span>Chamado</span>
            <strong>{chamado.numero}</strong>
          </div>
          <div className={styles.summaryDivider} aria-hidden="true" />
          <div>
            <span>Equipamento</span>
            <strong>{chamado.equipamentoAfetado || "Não informado"}</strong>
          </div>
        </div>

        <div className={styles.notice}>
          <Info size={18} strokeWidth={2.2} aria-hidden="true" />
          <p>
            Após o encerramento, o chamado será movido para o histórico e não
            poderá receber novos comentários.
          </p>
        </div>

        <label className={styles.field} htmlFor="solucao-chamado">
          <span>
            Solução aplicada <em>Obrigatório</em>
          </span>
          <textarea
            id="solucao-chamado"
            value={solucao}
            onChange={(event) => {
              setSolucao(event.target.value);
              if (erro) setErro("");
            }}
            placeholder="Descreva de forma clara o que foi realizado para solucionar o chamado..."
            rows={5}
            autoFocus
            disabled={encerrando}
          />
        </label>

        {erro && (
          <p className={styles.error} role="alert">
            {erro}
          </p>
        )}

        <div className={styles.actions}>
          <button type="button" onClick={onClose} disabled={encerrando}>
            Cancelar
          </button>
          <button
            type="button"
            className={styles.confirmButton}
            onClick={encerrarChamado}
            disabled={encerrando}
          >
            {encerrando ? "Encerrando..." : "Encerrar chamado"}
          </button>
        </div>
      </div>
    </div>
  );
}
