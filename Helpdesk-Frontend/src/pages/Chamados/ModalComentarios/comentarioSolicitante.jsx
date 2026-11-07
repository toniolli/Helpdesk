import {
  useCallback,
  useEffect,
  useId,
  useLayoutEffect,
  useRef,
  useState,
} from "react";
import {
  ChevronDown,
  Download,
  FileText,
  LoaderCircle,
  Maximize2,
  MessageCircle,
  Minus,
  Paperclip,
  Send,
  X,
} from "lucide-react";
import styles from "./comentarioTecnico.module.css";
import comentarioService from "../../../services/comentarioService";
import anexoService from "../../../services/anexoService";
import useAtualizacaoTempoReal from "../../../hooks/useAtualizacaoTempoReal";
import { getUserId } from "../../../utils/authUtils";

const LIMITE_ANEXOS = 5;
const DISTANCIA_FINAL_CHAT = 120;

const obterIniciais = (nome = "Usuário") =>
  nome
    .trim()
    .split(/\s+/)
    .slice(0, 2)
    .map((parte) => parte[0])
    .join("")
    .toUpperCase();

const formatarData = (data) =>
  data ? new Date(data).toLocaleString() : "";

export default function ComentarioSolicitante({
  aberto,
  onClose,
  chamado,
  offset = 0,
}) {
  const [minimizado, setMinimizado] = useState(false);
  const [comentarioTexto, setComentarioTexto] = useState("");
  const [anexos, setAnexos] = useState([]);
  const [comentarios, setComentarios] = useState([]);
  const [pagina, setPagina] = useState(1);
  const [totalPaginas, setTotalPaginas] = useState(0);
  const [salvando, setSalvando] = useState(false);
  const [carregando, setCarregando] = useState(false);
  const [erroCarregamento, setErroCarregamento] = useState("");
  const [mostrarNovaMensagem, setMostrarNovaMensagem] = useState(false);

  const tituloId = useId();
  const modalRef = useRef(null);
  const chatRef = useRef(null);
  const textareaRef = useRef(null);
  const inputArquivoRef = useRef(null);
  const primeiraCargaRef = useRef(true);
  const deveRolarAoFinalRef = useRef(true);
  const posicaoAntesDaPaginacaoRef = useRef(null);
  const usuarioLogado = { id: getUserId() };

  const tamanhoPagina = 5;
  const chamadoId = chamado?.id_Chamado;

  const limparEditor = useCallback(() => {
    setComentarioTexto("");
    setAnexos([]);

    if (textareaRef.current) {
      textareaRef.current.style.height = "auto";
    }
  }, []);

  const estaPertoDoFinal = useCallback(() => {
    const chat = chatRef.current;
    if (!chat) return true;

    return (
      chat.scrollHeight - chat.scrollTop - chat.clientHeight <
      DISTANCIA_FINAL_CHAT
    );
  }, []);

  const irParaUltimaMensagem = useCallback((behavior = "smooth") => {
    const chat = chatRef.current;
    if (!chat) return;

    chat.scrollTo({ top: chat.scrollHeight, behavior });
    setMostrarNovaMensagem(false);
  }, []);

  const buscarComentarios = useCallback(
    async (paginaSolicitada = pagina) => {
      try {
        setCarregando(true);
        setErroCarregamento("");

        const response =
          await comentarioService.buscarComentariosPorChamadoPaginado(
            chamadoId,
            paginaSolicitada,
            tamanhoPagina,
          );
        const data = response.dados ?? [];
        setTotalPaginas(response.totalPaginas);

        const comentariosComAnexos = data.map((comentario) => ({
          ...comentario,
          anexos: comentario.anexos ?? [],
        }));
        const comentariosOrdenados = [...comentariosComAnexos].reverse();

        setComentarios((atuais) => {
          if (paginaSolicitada === 1) return comentariosOrdenados;

          const idsExistentes = new Set(
            atuais.map((comentario) => comentario.id_Comentario),
          );
          const comentariosNovos = comentariosOrdenados.filter(
            (comentario) => !idsExistentes.has(comentario.id_Comentario),
          );

          return [...comentariosNovos, ...atuais];
        });
      } catch (error) {
        if (error?.response?.status === 404) {
          setComentarios([]);
          return;
        }

        console.error(error);
        setErroCarregamento("Não foi possível carregar a conversa.");
      } finally {
        setCarregando(false);
      }
    },
    [chamadoId, pagina, tamanhoPagina],
  );

  useEffect(() => {
    setPagina(1);
    setComentarios([]);
    setTotalPaginas(0);
    setMostrarNovaMensagem(false);
    setErroCarregamento("");
    primeiraCargaRef.current = true;
    deveRolarAoFinalRef.current = true;
    posicaoAntesDaPaginacaoRef.current = null;
  }, [chamadoId]);

  useEffect(() => {
    if (chamadoId) buscarComentarios();
  }, [chamadoId, buscarComentarios]);

  useLayoutEffect(() => {
    const chat = chatRef.current;
    if (!chat) return;

    const posicaoAnterior = posicaoAntesDaPaginacaoRef.current;
    if (posicaoAnterior) {
      chat.scrollTop =
        chat.scrollHeight -
        posicaoAnterior.scrollHeight +
        posicaoAnterior.scrollTop;
      posicaoAntesDaPaginacaoRef.current = null;
      return;
    }

    if (primeiraCargaRef.current || deveRolarAoFinalRef.current) {
      chat.scrollTop = chat.scrollHeight;
      primeiraCargaRef.current = false;
      deveRolarAoFinalRef.current = false;
    }
  }, [comentarios]);

  useEffect(() => {
    if (!aberto || minimizado) return undefined;

    const frame = window.requestAnimationFrame(() =>
      textareaRef.current?.focus(),
    );
    return () => window.cancelAnimationFrame(frame);
  }, [aberto, minimizado]);

  useEffect(() => {
    if (!aberto) return undefined;

    const elementoAnterior = document.activeElement;
    return () => elementoAnterior?.focus?.();
  }, [aberto]);

  useEffect(() => {
    if (!aberto || minimizado) return undefined;

    const controlarTeclado = (event) => {
      if (event.key === "Tab") {
        const elementosFocaveis = Array.from(
          modalRef.current?.querySelectorAll(
            'button:not(:disabled), textarea:not(:disabled), input:not(:disabled), [tabindex]:not([tabindex="-1"])',
          ) ?? [],
        );

        if (elementosFocaveis.length === 0) return;

        const primeiroElemento = elementosFocaveis[0];
        const ultimoElemento = elementosFocaveis.at(-1);

        if (event.shiftKey && document.activeElement === primeiroElemento) {
          event.preventDefault();
          ultimoElemento.focus();
        } else if (
          !event.shiftKey &&
          document.activeElement === ultimoElemento
        ) {
          event.preventDefault();
          primeiroElemento.focus();
        }
        return;
      }

      if (event.key === "Escape") onClose();
    };

    window.addEventListener("keydown", controlarTeclado);
    return () => window.removeEventListener("keydown", controlarTeclado);
  }, [aberto, minimizado, onClose]);

  const atualizarComentariosAposReconexao = useCallback(() => {
    if (pagina === 1) return buscarComentarios(1);
    setPagina(1);
  }, [buscarComentarios, pagina]);

  const adicionarComentarioSeNovo = useCallback((comentario) => {
    if (!comentario?.id_Comentario) return;

    const comentarioNormalizado = {
      ...comentario,
      anexos: comentario.anexos ?? [],
    };

    setComentarios((atuais) =>
      atuais.some(
        (item) => item.id_Comentario === comentarioNormalizado.id_Comentario,
      )
        ? atuais
        : [...atuais, comentarioNormalizado],
    );
  }, []);

  useAtualizacaoTempoReal(
    "comentarioCriado",
    ({ chamadoId: chamadoAtualizadoId, comentario }) => {
      if (chamadoAtualizadoId !== chamadoId || !comentario) return;

      const acompanharMensagem = estaPertoDoFinal();
      deveRolarAoFinalRef.current = acompanharMensagem;
      setMostrarNovaMensagem(!acompanharMensagem);
      adicionarComentarioSeNovo(comentario);
    },
    Boolean(aberto && chamadoId),
    atualizarComentariosAposReconexao,
  );

  const baixarAnexo = async (anexo) => {
    try {
      const response = await anexoService.download(anexo.id_Anexo);
      const blob = new Blob([response.data], {
        type: response.headers["content-type"],
      });
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement("a");

      link.href = url;
      link.download = anexo.nomeOriginal;
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);
    } catch (error) {
      console.error("Erro ao baixar anexo:", error);
    }
  };

  const handleArquivos = (event) => {
    const arquivos = Array.from(event.target.files);

    if (anexos.length + arquivos.length > LIMITE_ANEXOS) {
      alert(`É permitido selecionar no máximo ${LIMITE_ANEXOS} arquivos.`);
      event.target.value = "";
      return;
    }

    setAnexos((atuais) => [...atuais, ...arquivos]);
    event.target.value = "";
  };

  const removerAnexo = (indice) => {
    setAnexos((atuais) =>
      atuais.filter((_, indiceAtual) => indiceAtual !== indice),
    );
  };

  const ajustarAlturaTextarea = (event) => {
    const textarea = event.currentTarget;
    textarea.style.height = "auto";
    textarea.style.height = `${Math.min(textarea.scrollHeight, 120)}px`;
    setComentarioTexto(textarea.value);
  };

  const salvarComentario = async () => {
    if (!comentarioTexto.trim() && anexos.length === 0) {
      alert("Digite uma mensagem ou selecione pelo menos um arquivo.");
      return;
    }

    try {
      setSalvando(true);

      const comentarioSalvo = await comentarioService.create({
        chamadoId: chamado.id_Chamado,
        mensagem: comentarioTexto,
        arquivos: anexos,
      });

      deveRolarAoFinalRef.current = true;
      setMostrarNovaMensagem(false);
      adicionarComentarioSeNovo(comentarioSalvo);
      limparEditor();
      window.requestAnimationFrame(() => irParaUltimaMensagem());
    } catch (error) {
      console.error(
        "Erro ao salvar comentário:",
        error.response?.data || error,
      );
      alert("Erro ao enviar comentário.");
    } finally {
      setSalvando(false);
    }
  };

  const enviarComEnter = (event) => {
    if (
      event.key === "Enter" &&
      !event.shiftKey &&
      !event.nativeEvent.isComposing
    ) {
      event.preventDefault();
      if (!salvando && (comentarioTexto.trim() || anexos.length > 0)) {
        salvarComentario();
      }
    }
  };

  const carregarComentariosAnteriores = () => {
    const chat = chatRef.current;
    if (chat) {
      posicaoAntesDaPaginacaoRef.current = {
        scrollHeight: chat.scrollHeight,
        scrollTop: chat.scrollTop,
      };
    }
    setPagina((paginaAtual) => paginaAtual + 1);
  };

  const acompanharRolagem = () => {
    if (estaPertoDoFinal()) setMostrarNovaMensagem(false);
  };

  if (!aberto) return null;

  return (
    <div className={minimizado ? styles.overlayMinimized : styles.overlay}>
      <section
        ref={modalRef}
        className={`${styles.modal} ${minimizado ? styles.minimized : ""}`}
        style={minimizado ? { right: 20 + offset * 360 } : {}}
        role="dialog"
        aria-modal={minimizado ? undefined : "true"}
        aria-labelledby={tituloId}
      >
        <header className={styles.header}>
          <div className={styles.headerIdentity}>
            <div className={styles.headerIcon} aria-hidden="true">
              <MessageCircle size={19} />
            </div>
            <div className={styles.headerText}>
              <span>Conversa do chamado</span>
              <h2 id={tituloId}>{chamado?.numero}</h2>
              <p title={chamado?.equipamentoAfetado}>
                {chamado?.equipamentoAfetado}
              </p>
            </div>
          </div>

          <div className={styles.headerActions}>
            <button
              type="button"
              className={styles.iconButton}
              onClick={() => setMinimizado((estadoAtual) => !estadoAtual)}
              aria-label={
                minimizado ? "Restaurar conversa" : "Minimizar conversa"
              }
              title={minimizado ? "Restaurar" : "Minimizar"}
            >
              {minimizado ? <Maximize2 size={17} /> : <Minus size={18} />}
            </button>
            <button
              type="button"
              className={styles.iconButton}
              onClick={onClose}
              aria-label="Fechar conversa"
              title="Fechar"
            >
              <X size={18} />
            </button>
          </div>
        </header>

        {!minimizado ? (
          <>
            <div
              ref={chatRef}
              className={styles.body}
              onScroll={acompanharRolagem}
              role="log"
              aria-label="Histórico de comentários"
              aria-live="polite"
            >
              <div className={styles.chatArea}>
                {pagina < totalPaginas ? (
                  <button
                    type="button"
                    className={styles.carregarMais}
                    onClick={carregarComentariosAnteriores}
                    disabled={carregando}
                  >
                    {carregando ? (
                      <LoaderCircle size={14} className={styles.spinning} />
                    ) : null}
                    Carregar comentários anteriores
                  </button>
                ) : null}

                <div className={styles.systemMessage}>
                  <span>Descrição inicial do chamado</span>
                  <p>{chamado?.descricao}</p>
                  <small>{formatarData(chamado?.dataAbertura)}</small>
                </div>

                {erroCarregamento ? (
                  <div className={styles.inlineError} role="alert">
                    <span>{erroCarregamento}</span>
                    <button
                      type="button"
                      onClick={() => buscarComentarios(1)}
                    >
                      Tentar novamente
                    </button>
                  </div>
                ) : null}

                {carregando && comentarios.length === 0 ? (
                  <div className={styles.loadingState}>
                    <LoaderCircle size={18} className={styles.spinning} />
                    Carregando conversa...
                  </div>
                ) : null}

                {!carregando && comentarios.length === 0 ? (
                  <div className={styles.emptyState}>
                    <MessageCircle size={18} />
                    <span>A conversa começa aqui.</span>
                  </div>
                ) : null}

                {comentarios.map((comentario) => {
                  const mensagemPropria =
                    comentario.usuarioId === usuarioLogado.id;
                  const nomeUsuario = comentario.usuarioNome || "Usuário";

                  return (
                    <div
                      key={comentario.id_Comentario}
                      className={
                        mensagemPropria
                          ? styles.messageRight
                          : styles.messageLeft
                      }
                    >
                      {!mensagemPropria ? (
                        <div className={styles.avatar} aria-hidden="true">
                          {obterIniciais(nomeUsuario)}
                        </div>
                      ) : null}

                      <article className={styles.messageBubble}>
                        <strong>{mensagemPropria ? "Você" : nomeUsuario}</strong>
                        {comentario.mensagem ? (
                          <p>{comentario.mensagem}</p>
                        ) : null}

                        {comentario.anexos?.length > 0 ? (
                          <div className={styles.anexosComentario}>
                            {comentario.anexos.map((anexo) => (
                              <button
                                key={anexo.id_Anexo}
                                type="button"
                                className={styles.anexoBtn}
                                onClick={() => baixarAnexo(anexo)}
                                title={`Baixar ${anexo.nomeOriginal}`}
                              >
                                <FileText size={16} />
                                <span>{anexo.nomeOriginal}</span>
                                <Download size={15} />
                              </button>
                            ))}
                          </div>
                        ) : null}

                        <small>{formatarData(comentario.dataCriacao)}</small>
                      </article>
                    </div>
                  );
                })}
              </div>

              {mostrarNovaMensagem ? (
                <button
                  type="button"
                  className={styles.newMessageButton}
                  onClick={() => irParaUltimaMensagem()}
                >
                  <ChevronDown size={16} />
                  Nova mensagem
                </button>
              ) : null}
            </div>

            <footer className={styles.composer}>
              {anexos.length > 0 ? (
                <div className={styles.selectedFiles}>
                  <div className={styles.selectedFilesHeader}>
                    <span>Arquivos selecionados</span>
                    <small>
                      {anexos.length} de {LIMITE_ANEXOS}
                    </small>
                  </div>
                  <div className={styles.selectedFilesList}>
                    {anexos.map((arquivo, indice) => (
                      <div
                        key={`${arquivo.name}-${arquivo.size}-${arquivo.lastModified}-${indice}`}
                        className={styles.selectedFile}
                      >
                        <FileText size={14} />
                        <span title={arquivo.name}>{arquivo.name}</span>
                        <button
                          type="button"
                          onClick={() => removerAnexo(indice)}
                          aria-label={`Remover ${arquivo.name}`}
                        >
                          <X size={14} />
                        </button>
                      </div>
                    ))}
                  </div>
                </div>
              ) : null}

              <div className={styles.composerRow}>
                <input
                  ref={inputArquivoRef}
                  type="file"
                  multiple
                  accept=".png,.jpg,.jpeg,.pdf,.txt,.docx"
                  hidden
                  onChange={handleArquivos}
                />
                <button
                  type="button"
                  className={styles.attachButton}
                  onClick={() => inputArquivoRef.current?.click()}
                  disabled={salvando || anexos.length >= LIMITE_ANEXOS}
                  aria-label="Anexar arquivos"
                  title="Anexar arquivos"
                >
                  <Paperclip size={19} />
                </button>
                <textarea
                  ref={textareaRef}
                  rows={1}
                  value={comentarioTexto}
                  onChange={ajustarAlturaTextarea}
                  onKeyDown={enviarComEnter}
                  placeholder="Digite uma mensagem..."
                  aria-label="Mensagem do comentário"
                  disabled={salvando}
                />
                <button
                  type="button"
                  className={styles.sendButton}
                  onClick={salvarComentario}
                  disabled={
                    salvando ||
                    (!comentarioTexto.trim() && anexos.length === 0)
                  }
                  aria-label={
                    salvando ? "Enviando mensagem" : "Enviar mensagem"
                  }
                >
                  {salvando ? (
                    <LoaderCircle size={19} className={styles.spinning} />
                  ) : (
                    <Send size={19} />
                  )}
                </button>
              </div>
              <div className={styles.composerHint}>
                <span>Mensagem ou anexo</span>
                <span>Enter para enviar · Shift + Enter para quebrar linha</span>
              </div>
            </footer>
          </>
        ) : null}
      </section>
    </div>
  );
}
