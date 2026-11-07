import { useEffect, useState } from "react";
import {
  ChevronDown,
  ChevronUp,
  History,
  LoaderCircle,
  X,
} from "lucide-react";
import auditoriaChamadoService from "../../../services/auditoriaChamadoService";
import { apiErrorMessage } from "../../../utils/apiError";
import {
  eventoAuditoriaChamadoTexto,
  formatarDataHora,
  lerValoresAuditoria,
} from "../../../utils/auditoriaUtils";
import { getStatusChamado } from "../../../utils/statusChamadoUtils";
import styles from "./analiseChamados.module.css";

export default function AuditoriaChamadoAnaliseModal({ chamado, onClose }) {
  const [auditorias, setAuditorias] = useState([]);
  const [expandida, setExpandida] = useState(null);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState("");

  useEffect(() => {
    let ativo = true;

    const carregar = async () => {
      try {
        const dados = await auditoriaChamadoService.buscarPorChamado(
          chamado.id_Chamado,
        );

        if (!ativo) return;
        setAuditorias(
          [...(dados ?? [])].sort(
            (a, b) => new Date(b.dataEvento) - new Date(a.dataEvento),
          ),
        );
      } catch (error) {
        if (!ativo) return;
        setErro(
          apiErrorMessage(
            error,
            "Não foi possível carregar a auditoria deste chamado.",
          ),
        );
      } finally {
        if (ativo) setCarregando(false);
      }
    };

    carregar();
    return () => {
      ativo = false;
    };
  }, [chamado.id_Chamado]);

  useEffect(() => {
    const overflowAnterior = document.body.style.overflow;
    const fecharComEscape = (event) => {
      if (event.key === "Escape") onClose();
    };

    document.body.style.overflow = "hidden";
    window.addEventListener("keydown", fecharComEscape);

    return () => {
      document.body.style.overflow = overflowAnterior;
      window.removeEventListener("keydown", fecharComEscape);
    };
  }, [onClose]);

  return (
    <div
      className={styles.modalOverlay}
      onMouseDown={(event) => {
        if (event.target === event.currentTarget) onClose();
      }}
    >
      <section
        className={styles.modal}
        role="dialog"
        aria-modal="true"
        aria-labelledby="auditoria-analise-titulo"
      >
        <header className={styles.modalHeader}>
          <div className={styles.modalTitle}>
            <History size={20} />
            <div>
              <span>LINHA DO TEMPO</span>
              <h2 id="auditoria-analise-titulo">
                Auditoria · {chamado.numero}
              </h2>
            </div>
          </div>
          <button
            type="button"
            className={styles.iconButton}
            onClick={onClose}
            aria-label="Fechar auditoria"
          >
            <X size={20} />
          </button>
        </header>

        <div className={styles.auditBody}>
          {erro ? <div className={styles.inlineError}>{erro}</div> : null}

          {carregando ? (
            <div className={styles.modalLoading}>
              <LoaderCircle size={20} />
              Carregando linha do tempo...
            </div>
          ) : null}

          {!carregando && auditorias.length === 0 && !erro ? (
            <div className={styles.modalEmpty}>
              Nenhum evento de auditoria registrado.
            </div>
          ) : null}

          <div className={styles.timeline}>
            {auditorias.map((auditoria, indice) => {
              const identificador = auditoria.id_AuditoriaChamado ?? indice;
              const aberta = expandida === identificador;
              const possuiDetalhes =
                auditoria.detalhesAnteriores || auditoria.detalhesNovos;

              return (
                <article key={identificador} className={styles.timelineItem}>
                  <span className={styles.timelineDot} aria-hidden="true" />
                  <div className={styles.timelineCard}>
                    <div className={styles.timelineHeader}>
                      <div>
                        <strong>
                          {eventoAuditoriaChamadoTexto(auditoria.evento)}
                        </strong>
                        <span>{formatarDataHora(auditoria.dataEvento)}</span>
                      </div>
                      {possuiDetalhes ? (
                        <button
                          type="button"
                          onClick={() =>
                            setExpandida(aberta ? null : identificador)
                          }
                          aria-expanded={aberta}
                        >
                          {aberta ? "Ocultar" : "Ver alterações"}
                          {aberta ? (
                            <ChevronUp size={15} />
                          ) : (
                            <ChevronDown size={15} />
                          )}
                        </button>
                      ) : null}
                    </div>
                    <p>{auditoria.descricao || "Evento registrado."}</p>
                    <div className={styles.timelineMeta}>
                      <span>{auditoria.usuarioNome || "Sistema"}</span>
                      <span>{auditoria.perfilUsuario || "-"}</span>
                      <span>
                        {getStatusChamado(auditoria.statusAnterior)} →{" "}
                        {getStatusChamado(auditoria.statusNovo)}
                      </span>
                    </div>

                    {aberta ? (
                      <div className={styles.auditValues}>
                        <div>
                          <span>Antes</span>
                          <pre>
                            {lerValoresAuditoria(auditoria.detalhesAnteriores)}
                          </pre>
                        </div>
                        <div>
                          <span>Depois</span>
                          <pre>
                            {lerValoresAuditoria(auditoria.detalhesNovos)}
                          </pre>
                        </div>
                      </div>
                    ) : null}
                  </div>
                </article>
              );
            })}
          </div>
        </div>
      </section>
    </div>
  );
}
