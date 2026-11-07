import { useCallback, useEffect, useState } from "react";
import {
  Download,
  LoaderCircle,
  MessageCircle,
  Paperclip,
  X,
} from "lucide-react";
import anexoService from "../../../services/anexoService";
import comentarioService from "../../../services/comentarioService";
import { apiErrorMessage } from "../../../utils/apiError";
import { formatarDataChamado } from "../../../utils/analiseChamadosUtils";
import styles from "./analiseChamados.module.css";

const TAMANHO_PAGINA = 5;

export default function ComentariosAnaliseModal({ chamado, onClose }) {
  const [comentarios, setComentarios] = useState([]);
  const [pagina, setPagina] = useState(1);
  const [totalPaginas, setTotalPaginas] = useState(0);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState("");

  const buscarPagina = useCallback(
    async (paginaSolicitada, substituir = false) => {
      try {
        setCarregando(true);
        setErro("");

        const resposta =
          await comentarioService.buscarComentariosPorChamadoPaginado(
            chamado.id_Chamado,
            paginaSolicitada,
            TAMANHO_PAGINA,
          );

        const itensComAnexos = (resposta.dados ?? []).map((comentario) => ({
          ...comentario,
          anexos: comentario.anexos ?? [],
        }));

        const ordenados = [...itensComAnexos].reverse();
        setComentarios((atuais) => {
          if (substituir) return ordenados;

          const idsExistentes = new Set(
            atuais.map((comentario) => comentario.id_Comentario),
          );
          const comentariosNovos = ordenados.filter(
            (comentario) => !idsExistentes.has(comentario.id_Comentario),
          );

          return [...comentariosNovos, ...atuais];
        });
        setPagina(resposta.pagina ?? paginaSolicitada);
        setTotalPaginas(resposta.totalPaginas ?? 0);
      } catch (error) {
        if (error.response?.status === 404) {
          if (substituir) setComentarios([]);
          setTotalPaginas(0);
          return;
        }

        setErro(
          apiErrorMessage(
            error,
            "Não foi possível carregar os comentários deste chamado.",
          ),
        );
      } finally {
        setCarregando(false);
      }
    },
    [chamado.id_Chamado],
  );

  useEffect(() => {
    buscarPagina(1, true);
  }, [buscarPagina]);

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

  const baixarAnexo = async (anexo) => {
    try {
      const resposta = await anexoService.download(anexo.id_Anexo);
      const url = window.URL.createObjectURL(new Blob([resposta.data]));
      const link = document.createElement("a");

      link.href = url;
      link.download = anexo.nomeOriginal;
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);
    } catch (error) {
      setErro(apiErrorMessage(error, "Não foi possível baixar o anexo."));
    }
  };

  const alinhamentoComentario = (comentario) => {
    if (comentario.usuarioId === chamado.solicitanteId) return styles.messageLeft;
    if (comentario.usuarioId === chamado.tecnicoResponsavelId) {
      return styles.messageRight;
    }
    return styles.messageNeutral;
  };

  return (
    <div
      className={styles.modalOverlay}
      onMouseDown={(event) => {
        if (event.target === event.currentTarget) onClose();
      }}
    >
      <section
        className={`${styles.modal} ${styles.chatModal}`}
        role="dialog"
        aria-modal="true"
        aria-labelledby="comentarios-analise-titulo"
      >
        <header className={styles.modalHeader}>
          <div className={styles.modalTitle}>
            <MessageCircle size={20} />
            <div>
              <span>HISTÓRICO DA CONVERSA</span>
              <h2 id="comentarios-analise-titulo">
                Comentários · {chamado.numero}
              </h2>
            </div>
          </div>
          <button
            type="button"
            className={styles.iconButton}
            onClick={onClose}
            aria-label="Fechar comentários"
          >
            <X size={20} />
          </button>
        </header>

        <div className={styles.chatBody}>
          {erro ? <div className={styles.inlineError}>{erro}</div> : null}

          {pagina < totalPaginas ? (
            <button
              type="button"
              className={styles.loadOlderButton}
              disabled={carregando}
              onClick={() => buscarPagina(pagina + 1)}
            >
              {carregando ? <LoaderCircle size={15} /> : null}
              Carregar comentários anteriores
            </button>
          ) : null}

          <div className={styles.messageLeft}>
            <div className={styles.messageBubble}>
              <strong>{chamado.solicitanteNome || "Solicitante"}</strong>
              <p>{chamado.descricao}</p>
              <small>{formatarDataChamado(chamado.dataAbertura)}</small>
            </div>
          </div>

          {comentarios.map((comentario) => (
            <div
              key={comentario.id_Comentario}
              className={alinhamentoComentario(comentario)}
            >
              <div className={styles.messageBubble}>
                <strong>{comentario.usuarioNome || "Usuário"}</strong>
                {comentario.mensagem ? <p>{comentario.mensagem}</p> : null}
                {comentario.anexos?.length ? (
                  <div className={styles.attachments}>
                    {comentario.anexos.map((anexo) => (
                      <button
                        type="button"
                        key={anexo.id_Anexo}
                        onClick={() => baixarAnexo(anexo)}
                      >
                        <Paperclip size={14} />
                        <span>{anexo.nomeOriginal}</span>
                        <Download size={14} />
                      </button>
                    ))}
                  </div>
                ) : null}
                <small>{formatarDataChamado(comentario.dataCriacao)}</small>
              </div>
            </div>
          ))}

          {carregando && comentarios.length === 0 ? (
            <div className={styles.modalLoading}>
              <LoaderCircle size={20} />
              Carregando conversa...
            </div>
          ) : null}
        </div>
      </section>
    </div>
  );
}
