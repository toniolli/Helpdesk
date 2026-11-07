import { useEffect } from "react";
import { X } from "lucide-react";
import {
  acaoAuditoriaTexto,
  eventoAuditoriaChamadoTexto,
  eventoAuditoriaTexto,
  formatarDataHora,
  lerValoresAuditoria,
  origemAuditoriaTexto,
} from "../../utils/auditoriaUtils";
import { getStatusChamado } from "../../utils/statusChamadoUtils";
import styles from "./auditoria.module.css";

export default function AuditoriaDetalhesModal({ auditoria, tipo, onClose }) {
  useEffect(() => {
    const fecharComEscape = (event) => {
      if (event.key === "Escape") onClose();
    };

    window.addEventListener("keydown", fecharComEscape);
    return () => window.removeEventListener("keydown", fecharComEscape);
  }, [onClose]);

  if (!auditoria) return null;

  const ehLogin = tipo === "login";
  const ehChamado = tipo === "chamado";
  const titulo = ehChamado
    ? "Detalhes da auditoria de chamados"
    : ehLogin
      ? "Detalhes da auditoria de login"
      : "Detalhes da auditoria geral";
  const dataRegistro = ehChamado
    ? auditoria.dataEvento
    : auditoria.dataCriacao;
  const valoresAnteriores = ehChamado
    ? auditoria.detalhesAnteriores
    : auditoria.valoresAnteriores;
  const valoresAtuais = ehChamado
    ? auditoria.detalhesNovos
    : auditoria.valoresAtuais;

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
        aria-labelledby="auditoria-detalhes-titulo"
      >
        <div className={styles.modalHeader}>
          <div>
            <span className={styles.eyebrow}>REGISTRO DE AUDITORIA</span>
            <h2 id="auditoria-detalhes-titulo">{titulo}</h2>
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
          <div><span>Data</span><strong>{formatarDataHora(dataRegistro)}</strong></div>
          <div><span>Usuário</span><strong>{auditoria.usuarioNome || "-"}</strong></div>
          {ehChamado ? (
            <>
              <div><span>Perfil</span><strong>{auditoria.perfilUsuario || "-"}</strong></div>
              <div><span>Evento</span><strong>{eventoAuditoriaChamadoTexto(auditoria.evento)}</strong></div>
              <div><span>Chamado</span><strong>{auditoria.numeroChamado || "-"}</strong></div>
              <div><span>ID do chamado</span><strong>{auditoria.chamadoId || "-"}</strong></div>
              <div><span>Status anterior</span><strong>{getStatusChamado(auditoria.statusAnterior)}</strong></div>
              <div><span>Status novo</span><strong>{getStatusChamado(auditoria.statusNovo)}</strong></div>
              <div className={styles.wideDetail}><span>Descrição</span><strong>{auditoria.descricao || "-"}</strong></div>
            </>
          ) : (
            <>
              <div><span>{ehLogin ? "Evento" : "Ação"}</span><strong>{ehLogin ? eventoAuditoriaTexto(auditoria.evento) : acaoAuditoriaTexto(auditoria.acao)}</strong></div>
              <div><span>Origem</span><strong>{origemAuditoriaTexto(auditoria.origem)}</strong></div>
              <div><span>Entidade</span><strong>{auditoria.entidade || "-"}</strong></div>
              <div><span>ID do registro</span><strong className={styles.breakText}>{auditoria.registroId || "-"}</strong></div>
              {ehLogin && <div><span>Resultado</span><strong>{auditoria.sucesso ? "Sucesso" : "Falha"}</strong></div>}
              {ehLogin && <div><span>Motivo</span><strong>{auditoria.motivo || "-"}</strong></div>}
              <div><span>Endereço IP</span><strong>{auditoria.enderecoIp || "-"}</strong></div>
              <div><span>Método HTTP</span><strong>{auditoria.metodoHttp || "-"}</strong></div>
              <div className={styles.wideDetail}><span>Rota</span><strong className={styles.breakText}>{auditoria.rota || "-"}</strong></div>
              {ehLogin && <div className={styles.wideDetail}><span>User Agent</span><strong className={styles.breakText}>{auditoria.userAgent || "-"}</strong></div>}
              <div className={styles.wideDetail}><span>TraceId</span><strong className={styles.breakText}>{auditoria.traceId || "-"}</strong></div>
            </>
          )}
        </div>

        <div className={styles.valuesGrid}>
          <div>
            <h3>Valores anteriores</h3>
            <pre>{lerValoresAuditoria(valoresAnteriores)}</pre>
          </div>
          <div>
            <h3>Valores atuais</h3>
            <pre>{lerValoresAuditoria(valoresAtuais)}</pre>
          </div>
        </div>
      </section>
    </div>
  );
}
