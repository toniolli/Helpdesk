import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import chamadoService from "../../services/chamadoService";
import useAtualizacaoTempoReal from "../../hooks/useAtualizacaoTempoReal";
import styles from "./filaAtendimento.module.css";
import ConsultasPrioridadesModal from "./ConsultasPrioridadesModal";
import { getUserId } from "../../utils/authUtils";

function formatarTempoEmFila(dataAbertura) {
  const abertura = new Date(dataAbertura);

  if (Number.isNaN(abertura.getTime())) {
    return "Data não informada";
  }

  const diferencaMinutos = Math.max(
    0,
    Math.floor((Date.now() - abertura.getTime()) / 60000),
  );

  if (diferencaMinutos < 60) {
    return `Há ${diferencaMinutos}min`;
  }

  const diferencaHoras = Math.floor(diferencaMinutos / 60);

  if (diferencaHoras < 24) {
    return `Há ${diferencaHoras}h`;
  }

  return `Há ${Math.floor(diferencaHoras / 24)}d`;
}

function formatarDataAbertura(dataAbertura) {
  const abertura = new Date(dataAbertura);

  if (Number.isNaN(abertura.getTime())) {
    return "Data não informada";
  }

  return abertura.toLocaleString("pt-BR", {
    day: "2-digit",
    month: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
  });
}

export default function FilaAtendimento() {
  const [chamados, setChamados] = useState([]);
  const [modalOpen, setModalOpen] = useState(false);
  const [chamadoSelecionado, setChamadoSelecionado] = useState(null);
  const [modalPrioridadeOpen, setModalPrioridadeOpen] = useState(false);
  const [prioridadeSelecionada, setPrioridadeSelecionada] = useState("");
  const [showSuccess, setShowSuccess] = useState(false);
  const [prioridadeFiltro, setPrioridadeFiltro] = useState(null);

  const navigate = useNavigate();

  const carregarFila = async () => {
    try {
      const data = await chamadoService.buscarFila();

      setChamados(data);
    } catch (error) {
      console.error(error);
    }
  };

  useEffect(() => {
    carregarFila();
  }, []);

  // Recarrega quando um chamado entra ou sai formalmente da fila.
  useAtualizacaoTempoReal(
    "filaAtualizada",
    carregarFila,
    true,
    carregarFila,
  );

  // Também recarrega quando dados de um chamado visível são alterados.
  useAtualizacaoTempoReal("chamadoAtualizado", carregarFila);

  const assumirChamado = async () => {
    try {
      const tecnicoId = getUserId();

      if (!tecnicoId) return;

      await chamadoService.assumir(chamadoSelecionado.id_Chamado, tecnicoId);

      setModalOpen(false);

      setShowSuccess(true);

      carregarFila();
    } catch (error) {
      console.error(error);
    }
  };

  const getPrioridadeClass = (prioridade) => {
    switch (prioridade) {
      case 4:
        return styles.critico;

      case 3:
        return styles.alta;

      case 2:
        return styles.media;

      default:
        return styles.baixa;
    }
  };

  const criticosTodos = chamados.filter((x) => x.prioridade === 4);
  const altosTodos = chamados.filter((x) => x.prioridade === 3);
  const mediosTodos = chamados.filter((x) => x.prioridade === 2);
  const baixosTodos = chamados.filter((x) => x.prioridade === 1);
  const criticos = criticosTodos.slice(0, 20);
  const altos = altosTodos.slice(0, 20);
  const medios = mediosTodos.slice(0, 20);
  const baixos = baixosTodos.slice(0, 20);

  const renderCards = (lista) =>
    lista.map((chamado) => (
      <button
        type="button"
        key={chamado.id_Chamado}
        className={`${styles.card} ${getPrioridadeClass(chamado.prioridade)}`}
        aria-label={`Abrir chamado ${chamado.numero ?? chamado.id_Chamado}`}
        onClick={() => {
          setChamadoSelecionado(chamado);

          setModalOpen(true);
        }}
      >
        <div className={styles.cardHeader}>
          <div className={styles.cardIdentification}>
            <span
              className={styles.priorityDot}
              title="Prioridade do chamado"
              aria-hidden="true"
            />
            <span className={styles.protocol}>#{chamado.numero}</span>
          </div>

          <span className={styles.waitingTime}>
            {formatarTempoEmFila(chamado.dataAbertura)}
          </span>
        </div>

        <div className={styles.cardBody}>
          <div className={styles.equipment}>
            {chamado.equipamentoAfetado || "Equipamento não informado"}
          </div>

          <div className={styles.requesterInfo}>
            <strong>Solicitante:</strong> {chamado.solicitanteNome}
          </div>
        </div>

        <div className={styles.cardFooter}>
          Aberto em {formatarDataAbertura(chamado.dataAbertura)}
        </div>
      </button>
    ));
  const abrirConsultaPrioridade = (nome, prioridadeId) => {
    setPrioridadeSelecionada(nome);
    setPrioridadeFiltro(prioridadeId);
    setModalPrioridadeOpen(true);
  };
  const assumirChamadoTabela = async (chamado) => {
    try {
      const tecnicoId = getUserId();

      if (!tecnicoId) return;

      await chamadoService.assumir(chamado.id_Chamado, tecnicoId);

      setModalPrioridadeOpen(false);

      setShowSuccess(true);

      carregarFila();
    } catch (error) {
      console.error(error);
    }
  };

  return (
    <div className={styles.container}>
      <h1>Fila de Atendimento</h1>

      <div className={styles.board}>
        <div className={styles.column}>
          <div className={`${styles.columnTitle} ${styles.criticoTitle}`}>
            <span>Críticos</span>

            <div className={styles.headerInfo}>
              <span className={styles.badge}>{criticosTodos.length}</span>

              <button
                type="button"
                className={styles.verTodos}
                onClick={() => abrirConsultaPrioridade("Críticos", 4)}
              >
                Ver todos
              </button>
            </div>
          </div>
          {criticos.length ? (
            renderCards(criticos)
          ) : (
            <p className={styles.emptyColumn}>Nenhum chamado crítico.</p>
          )}
        </div>

        <div className={styles.column}>
          <div className={`${styles.columnTitle} ${styles.altaTitle}`}>
            <span>Alta</span>

            <div className={styles.headerInfo}>
              <span className={styles.badge}>{altosTodos.length}</span>

              <button
                type="button"
                className={styles.verTodos}
                onClick={() => abrirConsultaPrioridade("Alta", 3)}
              >
                Ver todos
              </button>
            </div>
          </div>

          {altos.length ? (
            renderCards(altos)
          ) : (
            <p className={styles.emptyColumn}>Nenhum chamado de alta prioridade.</p>
          )}
        </div>

        <div className={styles.column}>
          <div className={`${styles.columnTitle} ${styles.mediaTitle}`}>
            <span>Média</span>

            <div className={styles.headerInfo}>
              <span className={styles.badge}>{mediosTodos.length}</span>

              <button
                type="button"
                className={styles.verTodos}
                onClick={() => abrirConsultaPrioridade("Média", 2)}
              >
                Ver todos
              </button>
            </div>
          </div>

          {medios.length ? (
            renderCards(medios)
          ) : (
            <p className={styles.emptyColumn}>Nenhum chamado de média prioridade.</p>
          )}
        </div>

        <div className={styles.column}>
          <div className={`${styles.columnTitle} ${styles.baixaTitle}`}>
            <span>Baixa</span>

            <div className={styles.headerInfo}>
              <span className={styles.badge}>{baixosTodos.length}</span>

              <button
                type="button"
                className={styles.verTodos}
                onClick={() => abrirConsultaPrioridade("Baixa", 1)}
              >
                Ver todos
              </button>
            </div>
          </div>
          {baixos.length ? (
            renderCards(baixos)
          ) : (
            <p className={styles.emptyColumn}>Nenhum chamado de baixa prioridade.</p>
          )}
        </div>
      </div>

      {modalOpen && chamadoSelecionado && (
        <div className={styles.overlay}>
          <div className={styles.modal}>
            <h2>{chamadoSelecionado.numero}</h2>

            <p>
              <strong>Equipamento:</strong>{" "}
              {chamadoSelecionado.equipamentoAfetado}
            </p>

            <p>
              <strong>Prioridade:</strong> {chamadoSelecionado.prioridade}
            </p>

            <p>
              <strong>Abertura:</strong>{" "}
              {new Date(chamadoSelecionado.dataAbertura).toLocaleString()}
            </p>

            <hr />

            <p>{chamadoSelecionado.descricao}</p>

            <div className={styles.modalActions}>
              <button onClick={() => setModalOpen(false)}>Cancelar</button>

              <button onClick={assumirChamado}>Iniciar chamado</button>
            </div>
          </div>
        </div>
      )}

      <ConsultasPrioridadesModal
        isOpen={modalPrioridadeOpen}
        onClose={() => setModalPrioridadeOpen(false)}
        prioridade={prioridadeSelecionada}
        prioridadeId={prioridadeFiltro}
        onAssumir={assumirChamadoTabela}
      />

      {showSuccess && (
        <div className={styles.toast}>
          ✅ Chamado assumido com sucesso
          <button onClick={() => navigate("/chamados/meus")}>
            Ver Meus Chamados
          </button>
        </div>
      )}
    </div>
  );
}
