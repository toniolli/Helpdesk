import { useCallback, useEffect, useState } from "react";
import chamadoService from "../../services/chamadoService";
import styles from "./ConsultasPrioridadesModal.module.css";

export default function ConsultasPrioridadesModal({
  isOpen,
  onClose,
  prioridade,
  prioridadeId,
  onAssumir,
}) {
  const [pagina, setPagina] = useState(1);
  const [chamados, setChamados] = useState([]);
  const [totalPaginas, setTotalPaginas] = useState(0);
  const [filtro, setFiltro] = useState("");
  const [dataInicial, setDataInicial] = useState("");
  const [dataFinal, setDataFinal] = useState("");

  console.log("prioridadeId", prioridadeId);

  const tamanhoPagina = 10;

  const carregarChamados = useCallback(async () => {
    try {
      const resultado = await chamadoService.buscarFilaPaginada({
        prioridade: prioridadeId,
        pagina,
        tamanhoPagina,
        dataInicial,
        dataFinal,
      });

      setChamados(resultado.dados);
      setTotalPaginas(resultado.totalPaginas);
    } catch (error) {
      console.error(error);
    }
  }, [dataFinal, dataInicial, pagina, prioridadeId]);

  const chamadosFiltrados = chamados.filter((chamado) => {
    const pesquisa = filtro.toLowerCase().trim();

    return (
      chamado.numero?.toString().includes(pesquisa) ||
      (chamado.solicitanteNome || "").toLowerCase().includes(pesquisa) ||
      (chamado.equipamentoAfetado || "").toLowerCase().includes(pesquisa) ||
      (chamado.prioridadeNome || "").toLowerCase().includes(pesquisa)
    );
  });

  useEffect(() => {
    if (!isOpen) return;

    carregarChamados();
  }, [isOpen, carregarChamados]);

  useEffect(() => {
    if (isOpen) {
      setPagina(1);
    }
  }, [prioridadeId, isOpen]);

  useEffect(() => {
    if (!isOpen) {
      setFiltro("");
      setDataInicial("");
      setDataFinal("");
    }
  }, [isOpen]);

  const atualizarDataInicial = (event) => {
    setDataInicial(event.target.value);
    setPagina(1);
  };

  const atualizarDataFinal = (event) => {
    setDataFinal(event.target.value);
    setPagina(1);
  };

  if (!isOpen) return null;

  return (
    <div className={styles.overlay}>
      <div className={styles.modal}>
        <div className={styles.header}>
          <h2>Chamados {prioridade}</h2>

          <button className={styles.closeButton} onClick={onClose}>
            ×
          </button>
        </div>
        <div className={styles.filters}>
          <input
            type="text"
            placeholder="Pesquisar chamado..."
            value={filtro}
            onChange={(e) => setFiltro(e.target.value)}
          />

          <label className={styles.dateFilter}>
            <span>De</span>
            <input
              type="date"
              value={dataInicial}
              onChange={atualizarDataInicial}
            />
          </label>

          <label className={styles.dateFilter}>
            <span>Até</span>
            <input
              type="date"
              min={dataInicial || undefined}
              value={dataFinal}
              onChange={atualizarDataFinal}
            />
          </label>
        </div>
        <div className={styles.tableContainer}>
          <table className={styles.table}>
            <thead>
              <tr>
                <th>Número</th>
                <th>Solicitante</th>
                <th> Equipamento</th>
                <th>Prioridade</th>
                <th>Data Abertura</th>
                <th>Ações</th>
              </tr>
            </thead>

            <tbody>
              {chamadosFiltrados.length > 0 ? (
                chamadosFiltrados.map((chamado) => (
                  <tr key={chamado.id_Chamado}>
                    <td>{chamado.numero}</td>

                    <td>{chamado.solicitanteNome || "-"}</td>

                    <td>{chamado.equipamentoAfetado}</td>

                    <td>{chamado.prioridadeNome}</td>

                    <td>
                      {new Date(chamado.dataAbertura).toLocaleDateString(
                        "pt-BR",
                      )}
                    </td>

                    <td>
                      <button
                        className={styles.assumirButton}
                        onClick={() => onAssumir(chamado)}
                      >
                        Iniciar chamado
                      </button>
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={5} className={styles.empty}>
                    Nenhum chamado encontrado.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
        <div className={styles.pagination}>
          <button
            disabled={pagina === 1}
            onClick={() => setPagina((prev) => prev - 1)}
          >
            Anterior
          </button>

          <span>
            Página {pagina} de {totalPaginas || 1}
          </span>

          <button
            disabled={totalPaginas === 0 || pagina >= totalPaginas}
            onClick={() => setPagina((prev) => prev + 1)}
          >
            Próxima
          </button>
        </div>

        <div className={styles.actions}>
          <button onClick={onClose}>Fechar</button>
        </div>
      </div>
    </div>
  );
}
