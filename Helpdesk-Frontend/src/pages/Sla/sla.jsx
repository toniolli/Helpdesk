import { useEffect, useState } from "react";
import slaService from "../../services/slaService";
import { hasPermission } from "../../utils/permissoesUtils";
import styles from "./sla.module.css";

const prioridadeTexto = {
  1: "Baixa",
  2: "Média",
  3: "Alta",
  4: "Crítica",
};

export default function Sla() {
  const [slas, setSlas] = useState([]);

  const [tempoAtendimento, setTempoAtendimento] = useState("");
  const [tempoResolucao, setTempoResolucao] = useState("");
  const [ativo, setAtivo] = useState(true);

  const [openModal, setOpenModal] = useState(false);
  const [slaEmEdicao, setSlaEmEdicao] = useState(null);

  const [filtro, setFiltro] = useState("");
  const [filtroStatus, setFiltroStatus] = useState("todos");
  const [pagina, setPagina] = useState(1);

  const tamanhoPagina = 10;
  const podeConfigurarSla = hasPermission("CONFIGURAR_SLA");

  const getPrioridadeNome = (prioridade) => {
    return prioridadeTexto[Number(prioridade)] ?? "Não definida";
  };

  const carregarSlas = async () => {
    try {
      const data = await slaService.getAll();
      setSlas(data);
    } catch (error) {
      console.error("Erro ao carregar SLAs:", error);
    }
  };

  useEffect(() => {
    carregarSlas();
  }, []);

  const limparFormulario = () => {
    setTempoAtendimento("");
    setTempoResolucao("");
    setAtivo(true);
    setSlaEmEdicao(null);
  };

  const fecharModal = () => {
    setOpenModal(false);
    limparFormulario();
  };

  const abrirModalEditar = async (sla) => {
    try {
      const dados = await slaService.getById(sla.id_Sla);

      setSlaEmEdicao(dados);

      setTempoAtendimento(dados.tempoAtendimentoHoras ?? "");

      setTempoResolucao(dados.tempoResolucaoHoras ?? "");

      setAtivo(dados.ativo);

      setOpenModal(true);
    } catch (error) {
      console.error("Erro ao carregar SLA:", error);
      alert("Erro ao carregar dados do SLA.");
    }
  };

  const salvarSla = async (e) => {
    e.preventDefault();

    if (!slaEmEdicao) return;

    const sla = {
      prioridade: Number(slaEmEdicao.prioridade),
      tempoAtendimentoHoras: Number(tempoAtendimento),
      tempoResolucaoHoras: Number(tempoResolucao),
      ativo,
    };

    try {
      await slaService.update(slaEmEdicao.id_Sla, sla);

      await carregarSlas();

      fecharModal();
    } catch (error) {
      console.error("Erro ao atualizar SLA:", error);

      alert("Erro ao atualizar SLA.");
    }
  };

  const alterarStatus = async (sla) => {
    try {
      if (sla.ativo) {
        await slaService.desativar(sla.id_Sla);
      } else {
        await slaService.ativar(sla.id_Sla);
      }

      await carregarSlas();
    } catch (error) {
      console.error("Erro ao alterar status do SLA:", error);

      alert("Erro ao alterar status do SLA.");
    }
  };

  const formatarHoras = (horas) => {
    if (horas === null || horas === undefined) {
      return "-";
    }

    const valor = Number(horas);

    if (Number.isNaN(valor)) {
      return "-";
    }

    return `${valor}h`;
  };

  const slasFiltrados = slas.filter((sla) => {
    const termo = filtro.toLowerCase();

    const prioridadeNome = getPrioridadeNome(sla.prioridade);

    const correspondeTexto = prioridadeNome.toLowerCase().includes(termo);

    const correspondeStatus =
      filtroStatus === "todos" ||
      (filtroStatus === "ativo" && sla.ativo) ||
      (filtroStatus === "inativo" && !sla.ativo);

    return correspondeTexto && correspondeStatus;
  });

  const totalPaginas = Math.max(
    1,
    Math.ceil(slasFiltrados.length / tamanhoPagina),
  );

  const paginaAtual = Math.min(pagina, totalPaginas);

  const indiceInicial = (paginaAtual - 1) * tamanhoPagina;

  const slasPaginados = slasFiltrados.slice(
    indiceInicial,
    indiceInicial + tamanhoPagina,
  );

  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <h1>Configuração de SLA</h1>
      </div>

      <div className={styles.filters}>
        <input
          type="text"
          placeholder="Pesquisar prioridade..."
          value={filtro}
          onChange={(e) => {
            setFiltro(e.target.value);
            setPagina(1);
          }}
        />

        <select
          value={filtroStatus}
          onChange={(e) => {
            setFiltroStatus(e.target.value);
            setPagina(1);
          }}
        >
          <option value="todos">Todos</option>

          <option value="ativo">Ativos</option>

          <option value="inativo">Inativos</option>
        </select>
      </div>

      <div className={styles.tableContainer}>
        <table className={styles.table}>
          <thead>
            <tr>
              <th>Prioridade</th>
              <th>Tempo de Atendimento</th>
              <th>Tempo de Resolução</th>
              <th>Status</th>
              {podeConfigurarSla && <th>Ações</th>}
            </tr>
          </thead>

          <tbody>
            {slasPaginados.length > 0 ? (
              slasPaginados.map((sla) => {
                const prioridadeNome = getPrioridadeNome(sla.prioridade);

                return (
                  <tr key={sla.id_Sla}>
                    <td>
                      <span
                        className={`${styles.prioridade} ${
                          Number(sla.prioridade) === 4
                            ? styles.critica
                            : Number(sla.prioridade) === 3
                              ? styles.alta
                              : Number(sla.prioridade) === 2
                                ? styles.media
                                : styles.baixa
                        }`}
                      >
                        {prioridadeNome}
                      </span>
                    </td>

                    <td>{formatarHoras(sla.tempoAtendimentoHoras)}</td>

                    <td>{formatarHoras(sla.tempoResolucaoHoras)}</td>

                    <td>
                      <span
                        className={
                          sla.ativo ? styles.statusAtivo : styles.statusInativo
                        }
                      >
                        {sla.ativo ? "Ativo" : "Inativo"}
                      </span>
                    </td>

                    {podeConfigurarSla && (
                      <td>
                        <div className={styles.actionButtons}>
                          <button
                            className={styles.editButton}
                            onClick={() => abrirModalEditar(sla)}
                          >
                            Editar
                          </button>

                          <button
                            className={
                              sla.ativo
                                ? styles.desativarButton
                                : styles.ativarButton
                            }
                            onClick={() => alterarStatus(sla)}
                          >
                            {sla.ativo ? "Desativar" : "Ativar"}
                          </button>
                        </div>
                      </td>
                    )}
                  </tr>
                );
              })
            ) : (
              <tr>
                <td
                  colSpan={podeConfigurarSla ? 5 : 4}
                  className={styles.emptyState}
                >
                  Nenhum SLA encontrado.
                </td>
              </tr>
            )}
          </tbody>
        </table>

        <div className={styles.pagination}>
          <button
            disabled={paginaAtual === 1}
            onClick={() => setPagina((paginaAnterior) => paginaAnterior - 1)}
          >
            Anterior
          </button>

          <span>
            Página {paginaAtual} de {totalPaginas}
          </span>

          <button
            disabled={paginaAtual >= totalPaginas}
            onClick={() => setPagina((paginaAnterior) => paginaAnterior + 1)}
          >
            Próxima
          </button>
        </div>
      </div>

      {openModal && slaEmEdicao && (
        <div className={styles.overlay}>
          <div className={styles.modal}>
            <h2>Editar SLA</h2>

            <p className={styles.subtitle}>
              Configure os tempos máximos para atendimento e resolução dos
              chamados.
            </p>

            <form onSubmit={salvarSla}>
              <div className={styles.field}>
                <label>Prioridade</label>

                <input
                  type="text"
                  value={getPrioridadeNome(slaEmEdicao.prioridade)}
                  disabled
                />
              </div>

              <div className={styles.field}>
                <label>Tempo de Atendimento (horas)</label>

                <input
                  type="number"
                  min="1"
                  value={tempoAtendimento}
                  onChange={(e) => setTempoAtendimento(e.target.value)}
                  required
                />
              </div>

              <div className={styles.field}>
                <label>Tempo de Resolução (horas)</label>

                <input
                  type="number"
                  min="1"
                  value={tempoResolucao}
                  onChange={(e) => setTempoResolucao(e.target.value)}
                  required
                />
              </div>

              <div className={styles.field}>
                <label>Status</label>

                <select
                  value={ativo.toString()}
                  onChange={(e) => setAtivo(e.target.value === "true")}
                >
                  <option value="true">Ativo</option>

                  <option value="false">Inativo</option>
                </select>
              </div>

              <div className={styles.actions}>
                <button type="button" onClick={fecharModal}>
                  Cancelar
                </button>

                <button type="submit">Salvar Alterações</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
