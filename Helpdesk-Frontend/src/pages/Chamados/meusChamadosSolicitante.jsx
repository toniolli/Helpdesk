import { useCallback, useEffect, useMemo, useState } from "react";
import chamadoService from "../../services/chamadoService";
import categoriaService from "../../services/categoriaService";
import styles from "./meusChamadosSolicitante.module.css";
import ComentarioSolicitante from "./ModalComentarios/comentarioSolicitante";
import { getUserId } from "../../utils/authUtils";
import { getPrioridade, getPrioridadeClass } from "../../utils/prioridadeUtils";
import { hasPermission } from "../../utils/permissoesUtils";
import useAtualizacaoTempoReal from "../../hooks/useAtualizacaoTempoReal";
import { apiErrorMessage } from "../../utils/apiError";
import {
  criarMapaCategorias,
  formatarDataChamado,
  formatarTempoChamado,
  formatarTempoDesdeAbertura,
  obterClasseStatusCard,
  obterMensagemStatus,
  obterNomeCategoria,
  obterStatusSolicitante,
} from "../../utils/chamadoCardUtils";

export default function MeusChamadosSolicitante() {
  const [chamados, setChamados] = useState([]);
  const [chamadoSelecionado, setChamadoSelecionado] = useState(null);
  const [pesquisa, setPesquisa] = useState("");
  const [ordenacao, setOrdenacao] = useState("recentes");
  const [comentariosAbertos, setComentariosAbertos] = useState([]);
  const [pagina, setPagina] = useState(1);
  const [totalPaginas, setTotalPaginas] = useState(0);
  const [enviandoEquipamento, setEnviandoEquipamento] = useState(false);
  const [categorias, setCategorias] = useState([]);

  const tamanhoPagina = 10;

  const categoriasPorId = useMemo(
    () => criarMapaCategorias(categorias),
    [categorias],
  );

  const carregarChamados = useCallback(async () => {
    try {
      const userId = getUserId();

      if (!userId) {
        console.error("Token não encontrado");
        return;
      }
      let resultado;

      if (hasPermission("VISUALIZAR_TODOS_CHAMADOS")) {
        resultado = await chamadoService.buscarMeusChamadosSolicitantePaginado(
          userId,
          {
            pagina,
            tamanhoPagina,
          },
        );
      } else {
        resultado = await chamadoService.buscarMeusChamadosSolicitantePaginado(
          userId,
          {
            pagina,
            tamanhoPagina,
          },
        );
      }

      let data = resultado.dados;

      setTotalPaginas(resultado.totalPaginas);

      data = data.filter(
        (chamado) =>
          chamado.status === 1 ||
          chamado.status === 2 ||
          chamado.status === 5 ||
          chamado.status === 7,
      );

      setChamados((prev) => (pagina === 1 ? data : [...prev, ...data]));

      setChamadoSelecionado((atual) => {
        if (!atual) return data[0] ?? null;

        const chamadoAtualizado = data.find(
          (chamado) => chamado.id_Chamado === atual.id_Chamado,
        );

        if (chamadoAtualizado) return chamadoAtualizado;

        return pagina === 1 ? (data[0] ?? null) : atual;
      });
    } catch (error) {
      console.error(error);
    }
  }, [pagina]);

  useEffect(() => {
    carregarChamados();
  }, [carregarChamados]);

  const recuperarChamadosAposReconexao = useCallback(() => {
    if (pagina === 1) {
      return carregarChamados();
    }

    setPagina(1);
  }, [carregarChamados, pagina]);

  useEffect(() => {
    let ativo = true;

    categoriaService
      .getAll()
      .then((resultado) => {
        if (ativo) setCategorias(Array.isArray(resultado) ? resultado : []);
      })
      .catch((error) =>
        console.error("Não foi possível carregar as categorias:", error),
      );

    return () => {
      ativo = false;
    };
  }, []);

  // Atualiza a lista do solicitante quando um de seus chamados for modificado.
  useAtualizacaoTempoReal(
    "chamadoAtualizado",
    carregarChamados,
    true,
    recuperarChamadosAposReconexao,
  );

  const podeEnviarEquipamento =
    hasPermission("ABRIR_CHAMADO") &&
    (chamadoSelecionado?.status === 2 || chamadoSelecionado?.status === 5);

  const enviarEquipamento = async () => {
    if (!chamadoSelecionado || enviandoEquipamento) return;

    try {
      setEnviandoEquipamento(true);

      const chamadoAtualizado = await chamadoService.enviarEquipamento(
        chamadoSelecionado.id_Chamado,
      );

      setChamados((atuais) =>
        atuais.map((chamado) =>
          chamado.id_Chamado === chamadoAtualizado.id_Chamado
            ? chamadoAtualizado
            : chamado,
        ),
      );
      setChamadoSelecionado(chamadoAtualizado);

      alert("Envio do equipamento registrado com sucesso.");
    } catch (error) {
      console.error("Erro ao registrar envio do equipamento:", error);
      alert(
        apiErrorMessage(
          error,
          "Não foi possível registrar o envio do equipamento.",
        ),
      );
    } finally {
      setEnviandoEquipamento(false);
    }
  };

  const chamadosFiltrados = chamados
    .filter(
      (x) =>
        x.numero?.toLowerCase().includes(pesquisa.toLowerCase()) ||
        x.equipamentoAfetado?.toLowerCase().includes(pesquisa.toLowerCase()) ||
        x.solicitanteNome?.toLowerCase().includes(pesquisa.toLowerCase()) ||
        x.tecnicoNome?.toLowerCase().includes(pesquisa.toLowerCase()),
    )
    .sort((a, b) => {
      switch (ordenacao) {
        case "antigos":
          return new Date(a.dataAbertura) - new Date(b.dataAbertura);

        case "prioridade":
          return b.prioridade - a.prioridade;

        default:
          return new Date(b.dataAbertura) - new Date(a.dataAbertura);
      }
    });

  return (
    <div className={styles.container}>
      <div className={styles.topBar}>
        <input
          type="text"
          placeholder="Pesquisar chamados..."
          value={pesquisa}
          onChange={(e) => setPesquisa(e.target.value)}
        />

        <select
          value={ordenacao}
          onChange={(e) => setOrdenacao(e.target.value)}
        >
          <option value="recentes">Mais Recentes</option>

          <option value="antigos">Mais Antigos</option>

          <option value="prioridade">Prioridade</option>
        </select>
      </div>

      <div className={styles.mainContent}>
        <div className={styles.sidebar}>
          {chamadosFiltrados.map((chamado) => (
            <div
              key={chamado.id_Chamado}
              className={`${styles.ticket}
                            ${
                              chamadoSelecionado?.id_Chamado ===
                              chamado.id_Chamado
                                ? styles.active
                                : ""
                            }`}
              onClick={() => setChamadoSelecionado(chamado)}
            >
              <div className={styles.ticketTop}>
                <strong>{chamado.numero}</strong>

                <span
                  className={`${styles.statusBadge} ${
                    styles[obterClasseStatusCard(chamado.status)]
                  }`}
                >
                  {obterStatusSolicitante(chamado.status)}
                </span>
              </div>

              <span>{chamado.equipamentoAfetado}</span>

              <div className={styles.ticketCategory}>
                {obterNomeCategoria(
                  categoriasPorId,
                  chamado.categoriaAtendimentoId,
                )}
              </div>

              <div className={styles.usuarioInfo}>
                <small>
                  <strong>Responsável:</strong>{" "}
                  {chamado.tecnicoNome || "Não atribuído"}
                </small>
              </div>

              <div className={styles.ticketFooter}>
                <span
                  className={`${styles.prioridade}
                                    ${getPrioridadeClass(
                                      chamado.prioridade,
                                      styles,
                                    )}`}
                >
                  {getPrioridade(chamado.prioridade)}
                </span>

                <small>
                  {formatarTempoDesdeAbertura(chamado.tempoCorridoHoras)}
                </small>
              </div>
            </div>
          ))}

          {pagina < totalPaginas && (
            <div
              className={styles.carregarMais}
              onClick={() => setPagina((prev) => prev + 1)}
            >
              Carregar mais chamados
            </div>
          )}
        </div>

        <div className={styles.content}>
          {!chamadoSelecionado && (
            <div className={styles.empty}>Nenhum chamado selecionado.</div>
          )}

          {chamadoSelecionado && (
            <div className={styles.ticketView}>
              <div className={styles.ticketHeader}>
                <div>
                  <h2>{chamadoSelecionado.numero}</h2>

                  <span>{chamadoSelecionado.equipamentoAfetado}</span>
                </div>

                <span
                  className={`${styles.headerStatus} ${
                    styles[obterClasseStatusCard(chamadoSelecionado.status)]
                  }`}
                >
                  {obterStatusSolicitante(chamadoSelecionado.status)}
                </span>
              </div>

              <div className={styles.statusContext}>
                {obterMensagemStatus(chamadoSelecionado.status, true)}
              </div>

              <div className={styles.chamadoInfo}>
                <div>
                  <strong>Solicitante:</strong>{" "}
                  {chamadoSelecionado.solicitanteNome}
                </div>

                <div>
                  <strong>Responsável:</strong>{" "}
                  {chamadoSelecionado.tecnicoNome || "Não atribuído"}
                </div>
              </div>
              <div className={styles.messageCard}>
                <p>{chamadoSelecionado.descricao}</p>
              </div>

              <div className={styles.actions}>
                <button
                  type="button"
                  className={styles.commentsBtn}
                  onClick={() => {
                    const jaExiste = comentariosAbertos.some(
                      (x) => x.id_Chamado === chamadoSelecionado.id_Chamado,
                    );

                    if (jaExiste) return;

                    setComentariosAbertos((prev) => [
                      ...prev,
                      chamadoSelecionado,
                    ]);
                  }}
                >
                  Ver Comentários
                </button>

                {podeEnviarEquipamento && (
                  <button
                    type="button"
                    className={styles.equipmentBtn}
                    onClick={enviarEquipamento}
                    disabled={enviandoEquipamento}
                  >
                    {enviandoEquipamento
                      ? "Registrando envio..."
                      : "Equipamento enviado para a matriz"}
                  </button>
                )}
              </div>

              <div className={styles.detailsCard}>
                <div>
                  <strong>Status</strong>

                  <span
                    className={`${styles.statusBadge} ${
                      styles[obterClasseStatusCard(chamadoSelecionado.status)]
                    }`}
                  >
                    {obterStatusSolicitante(chamadoSelecionado.status)}
                  </span>
                </div>

                <div>
                  <strong>Prioridade</strong>

                  <span
                    className={`${styles.prioridade}
                                    ${getPrioridadeClass(
                                      chamadoSelecionado.prioridade,
                                      styles,
                                    )}`}
                  >
                    {getPrioridade(chamadoSelecionado.prioridade)}
                  </span>
                </div>

                <div>
                  <strong>Categoria</strong>

                  <span>
                    {obterNomeCategoria(
                      categoriasPorId,
                      chamadoSelecionado.categoriaAtendimentoId,
                    )}
                  </span>
                </div>

                <div>
                  <strong>Abertura</strong>

                  <span>
                    {formatarDataChamado(chamadoSelecionado.dataAbertura)}
                  </span>
                </div>

                <div>
                  <strong>Primeiro atendimento</strong>

                  <span>
                    {formatarDataChamado(
                      chamadoSelecionado.dataPrimeiroAtendimento,
                    )}
                  </span>
                </div>
              </div>

              <div className={styles.timeSection}>
                <div className={styles.sectionHeading}>
                  <span>Acompanhamento</span>
                  <small>Resumo do andamento do seu chamado</small>
                </div>

                <div className={styles.timeGrid}>
                  <div className={styles.timeCard}>
                    <span>Tempo desde a abertura</span>
                    <strong>
                      {formatarTempoChamado(
                        chamadoSelecionado.tempoCorridoHoras,
                      )}
                    </strong>
                  </div>

                  <div className={styles.timeCard}>
                    <span>Primeiro atendimento</span>
                    <strong>
                      {formatarDataChamado(
                        chamadoSelecionado.dataPrimeiroAtendimento,
                      )}
                    </strong>
                  </div>

                  {Number(
                    chamadoSelecionado.tempoAguardandoRetornoHoras,
                  ) > 0 && (
                    <div className={styles.timeCard}>
                      <span>Seu tempo de resposta</span>
                      <strong>
                        {formatarTempoChamado(
                          chamadoSelecionado.tempoAguardandoRetornoHoras,
                        )}
                      </strong>
                    </div>
                  )}

                  {Number(
                    chamadoSelecionado.tempoAguardandoEquipamentoHoras,
                  ) > 0 && (
                    <div className={styles.timeCard}>
                      <span>Tempo do equipamento em trânsito</span>
                      <strong>
                        {formatarTempoChamado(
                          chamadoSelecionado.tempoAguardandoEquipamentoHoras,
                        )}
                      </strong>
                    </div>
                  )}
                </div>
              </div>
            </div>
          )}
        </div>
      </div>
      {comentariosAbertos.map((chamado, index) => (
        <ComentarioSolicitante
          key={chamado.id_Chamado}
          chamado={chamado}
          offset={index}
          aberto={true}
          onClose={() =>
            setComentariosAbertos((prev) =>
              prev.filter((item) => item.id_Chamado !== chamado.id_Chamado),
            )
          }
        />
      ))}
    </div>
  );
}
