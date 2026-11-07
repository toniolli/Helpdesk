import { useCallback, useEffect, useMemo, useState } from "react";
import chamadoService from "../../services/chamadoService";
import categoriaService from "../../services/categoriaService";
import styles from "./meusChamadosTecnico.module.css";
import ComentarioTecnico from "./ModalComentarios/comentarioTecnico";
import { getUserId } from "../../utils/authUtils";
import { getPrioridade, getPrioridadeClass } from "../../utils/prioridadeUtils";
import { hasPermission } from "../../utils/permissoesUtils";
import usuarioService from "../../services/usuarioService";
import useAtualizacaoTempoReal from "../../hooks/useAtualizacaoTempoReal";
import EncerrarChamadoModal from "./EncerrarChamadoModal";
import { apiErrorMessage } from "../../utils/apiError";
import { getStatusChamado } from "../../utils/statusChamadoUtils";
import {
  criarMapaCategorias,
  formatarDataChamado,
  formatarTempoChamado,
  formatarTempoDesdeAbertura,
  formatarTempoSla,
  obterClasseStatusCard,
  obterMensagemStatus,
  obterNomeCategoria,
} from "../../utils/chamadoCardUtils";

export default function MeusChamadosTecnico() {
  const [chamados, setChamados] = useState([]);
  const [chamadoSelecionado, setChamadoSelecionado] = useState(null);
  const [pesquisa, setPesquisa] = useState("");
  const [ordenacao, setOrdenacao] = useState("recentes");
  const [comentariosAbertos, setComentariosAbertos] = useState([]);
  const [pagina, setPagina] = useState(1);
  const [totalPaginas, setTotalPaginas] = useState(0);
  const [openTransferModal, setOpenTransferModal] = useState(false);
  const [chamadoTransferencia, setChamadoTransferencia] = useState(null);
  const [tecnicoDestinoId, setTecnicoDestinoId] = useState("");
  const [usuarios, setUsuarios] = useState([]);
  const [openCloseModal, setOpenCloseModal] = useState(false);
  const [recebendoEquipamento, setRecebendoEquipamento] = useState(false);
  const [verificandoTransferencia, setVerificandoTransferencia] =
    useState(false);
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
      console.log("userId", userId);
      if (hasPermission("VISUALIZAR_TODOS_CHAMADOS")) {
        resultado = await chamadoService.buscarMeusChamadosPaginado(userId, {
          pagina,
          tamanhoPagina,
        });
      } else if (hasPermission("ASSUMIR_CHAMADO")) {
        resultado = await chamadoService.buscarMeusChamadosPaginado(userId, {
          pagina,
          tamanhoPagina,
        });
      } else if (hasPermission("ABRIR_CHAMADO")) {
        resultado = await chamadoService.buscarMeusChamadosSolicitantePaginado(
          userId,
          {
            pagina,
            tamanhoPagina,
          },
        );
      } else {
        resultado = {
          dados: [],
          totalPaginas: 0,
        };
      }
      console.log("resultado", resultado);
      console.log("totalPaginas", resultado.totalPaginas);
      console.log("dados", resultado.dados);
      console.log("quantidade", resultado.dados?.length);
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

  // Atualiza a lista do técnico quando um chamado relacionado a ele for modificado.
  useAtualizacaoTempoReal(
    "chamadoAtualizado",
    carregarChamados,
    true,
    recuperarChamadosAposReconexao,
  );

  console.log("Quantidade carregada", chamados.length);
  console.log("Pesquisa", pesquisa);
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

  const carregarUsuarios = async () => {
    try {
      const data = await usuarioService.buscarUsuariosTransferencia();

      setUsuarios(data);
    } catch (error) {
      console.error(error);
    }
  };

  const podeEncerrarChamado =
    hasPermission("ENCERRAR_CHAMADO") &&
    (chamadoSelecionado?.status === 2 ||
      chamadoSelecionado?.status === 5 ||
      chamadoSelecionado?.status === 7);

  const podeTransferirChamado =
    hasPermission("ASSUMIR_CHAMADO") && chamadoSelecionado?.status === 2;

  const podeReceberEquipamento =
    chamadoSelecionado?.status === 7 &&
    (hasPermission("ASSUMIR_CHAMADO") ||
      hasPermission("VISUALIZAR_TODOS_CHAMADOS"));

  const receberEquipamento = async () => {
    if (!chamadoSelecionado || recebendoEquipamento) return;

    try {
      setRecebendoEquipamento(true);

      const chamadoAtualizado = await chamadoService.receberEquipamento(
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

      alert("Recebimento do equipamento registrado com sucesso.");
    } catch (error) {
      console.error("Erro ao registrar recebimento do equipamento:", error);
      alert(
        apiErrorMessage(
          error,
          "Não foi possível registrar o recebimento do equipamento.",
        ),
      );
    } finally {
      setRecebendoEquipamento(false);
    }
  };

  const abrirTransferencia = async () => {
    if (!chamadoSelecionado || verificandoTransferencia) return;

    try {
      setVerificandoTransferencia(true);

      const chamadoAtualizado = await chamadoService.getById(
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

      if (chamadoAtualizado.status !== 2) {
        alert(
          "Chamado já foi iniciado. Não é possível fazer a transferência.",
        );
        return;
      }

      setChamadoTransferencia(chamadoAtualizado);
      await carregarUsuarios();
      setOpenTransferModal(true);
    } catch (error) {
      console.error("Erro ao preparar transferência do chamado:", error);
      alert(
        apiErrorMessage(
          error,
          "Não foi possível verificar o chamado para transferência.",
        ),
      );
    } finally {
      setVerificandoTransferencia(false);
    }
  };

  const chamadoEncerrado = (chamadoId) => {
    const chamadosRestantes = chamados.filter(
      (chamado) => chamado.id_Chamado !== chamadoId,
    );

    setChamados(chamadosRestantes);
    setChamadoSelecionado((atual) =>
      atual?.id_Chamado === chamadoId ? (chamadosRestantes[0] ?? null) : atual,
    );
    setComentariosAbertos((abertos) =>
      abertos.filter((chamado) => chamado.id_Chamado !== chamadoId),
    );
    setOpenCloseModal(false);

    alert("Chamado encerrado com sucesso.");
  };

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
                  {getStatusChamado(chamado.status)}
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
                  <strong>Solicitante:</strong> {chamado.solicitanteNome}
                </small>
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
                  {getStatusChamado(chamadoSelecionado.status)}
                </span>
              </div>

              <div className={styles.statusContext}>
                {obterMensagemStatus(chamadoSelecionado.status)}
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

                {podeTransferirChamado && (
                  <button
                    type="button"
                    className={styles.transferBtn}
                    onClick={abrirTransferencia}
                    disabled={verificandoTransferencia}
                  >
                    {verificandoTransferencia
                      ? "Verificando..."
                      : "Transferir"}
                  </button>
                )}

                {podeReceberEquipamento && (
                  <button
                    type="button"
                    className={styles.receiveEquipmentBtn}
                    onClick={receberEquipamento}
                    disabled={recebendoEquipamento}
                  >
                    {recebendoEquipamento
                      ? "Registrando recebimento..."
                      : "Equipamento chegou"}
                  </button>
                )}

                {podeEncerrarChamado && (
                  <button
                    type="button"
                    className={styles.closeTicketBtn}
                    onClick={() => setOpenCloseModal(true)}
                  >
                    Encerrar chamado
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
                    {getStatusChamado(chamadoSelecionado.status)}
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
                  <span>Tempos do atendimento</span>
                  <small>Informações calculadas para o chamado selecionado</small>
                </div>

                <div className={styles.timeGrid}>
                  <div className={styles.timeCard}>
                    <span>Tempo corrido</span>
                    <strong>
                      {formatarTempoChamado(
                        chamadoSelecionado.tempoCorridoHoras,
                      )}
                    </strong>
                  </div>

                  <div className={styles.timeCard}>
                    <span>Primeiro atendimento pelo SLA</span>
                    <strong>
                      {formatarTempoSla(
                        chamadoSelecionado,
                        "tempoPrimeiroAtendimentoSlaHoras",
                      )}
                    </strong>
                  </div>

                  <div className={styles.timeCard}>
                    <span>Resolução pelo SLA</span>
                    <strong>
                      {formatarTempoSla(
                        chamadoSelecionado,
                        "tempoResolucaoSlaHoras",
                      )}
                    </strong>
                  </div>

                  <div className={styles.timeCard}>
                    <span>Espera pelo solicitante</span>
                    <strong>
                      {formatarTempoChamado(
                        chamadoSelecionado.tempoAguardandoRetornoHoras,
                      )}
                    </strong>
                  </div>

                  <div className={styles.timeCard}>
                    <span>Espera pelo equipamento</span>
                    <strong>
                      {formatarTempoChamado(
                        chamadoSelecionado.tempoAguardandoEquipamentoHoras,
                      )}
                    </strong>
                  </div>
                </div>
              </div>
            </div>
          )}
        </div>
      </div>
      {openTransferModal && (
        <div className={styles.modalOverlay}>
          <div className={styles.modal}>
            <h3>Transferir Chamado</h3>

            <p>Chamado: {chamadoTransferencia?.numero}</p>

            <select
              value={tecnicoDestinoId}
              onChange={(e) => setTecnicoDestinoId(e.target.value)}
            >
              <option value="">Selecione um usuário</option>

              {usuarios.map((usuario) => (
                <option key={usuario.id} value={usuario.id}>
                  {usuario.username}
                </option>
              ))}
            </select>

            <div className={styles.modalActions}>
              <button
                onClick={() => {
                  setOpenTransferModal(false);
                  setTecnicoDestinoId("");
                }}
              >
                Cancelar
              </button>

              <button
                onClick={async () => {
                  await chamadoService.transferir(
                    chamadoTransferencia.id_Chamado,
                    tecnicoDestinoId,
                  );

                  setOpenTransferModal(false);
                  setTecnicoDestinoId("");

                  alert("Solicitação de transferência enviada.");
                }}
              >
                Transferir
              </button>
            </div>
          </div>
        </div>
      )}

      {openCloseModal && chamadoSelecionado && (
        <EncerrarChamadoModal
          chamado={chamadoSelecionado}
          onClose={() => setOpenCloseModal(false)}
          onSuccess={chamadoEncerrado}
        />
      )}

      {comentariosAbertos.map((chamado, index) => (
        <ComentarioTecnico
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
