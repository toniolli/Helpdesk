import { useCallback, useEffect, useState } from "react";
import chamadoService from "../../services/chamadoService";
import usuarioService from "../../services/usuarioService";
import categoriaService from "../../services/categoriaService";
import * as XLSX from "xlsx";
import styles from "./historicoChamados.module.css";
import { hasPermission } from "../../utils/permissoesUtils";
import { getStatusChamado } from "../../utils/statusChamadoUtils";

export default function HistoricoChamados() {
  const [historico, setHistorico] = useState([]);
  const [usuarios, setUsuarios] = useState([]);
  const [categorias, setCategorias] = useState([]);
  const [pesquisado, setPesquisado] = useState(false);
  const podeVerTudo = hasPermission("VISUALIZAR_TODOS_CHAMADOS");
  const [pagina, setPagina] = useState(1);
  const [totalPaginas, setTotalPaginas] = useState(0);

  const tamanhoPagina = 10;

  const [filtros, setFiltros] = useState({
    dataInicial: "",
    dataFinal: "",
    status: "",
    prioridade: "",
    categoriaId: "",
    solicitanteId: "",
    tecnicoId: "",
  });

  const obterPrioridade = (prioridade) => {
    switch (prioridade) {
      case 1:
        return "Baixa";

      case 2:
        return "Média";

      case 3:
        return "Alta";

      case 4:
        return "Crítica";

      default:
        return "-";
    }
  };

  const pesquisar = async (paginaAtual = 1) => {
    try {
      setPagina(paginaAtual);

      if (!filtros.dataInicial || !filtros.dataFinal) {
        alert("Informe Data Inicial e Data Final.");
        return;
      }

      const chamadosPromise = chamadoService.buscarHistoricoPaginado({
        pagina: paginaAtual,
        tamanhoPagina,
        dataInicial: filtros.dataInicial,
        dataFinal: filtros.dataFinal,
      });

      const [chamadosData, categoriasData] = await Promise.all([
        chamadosPromise,
        categoriaService.getAll(),
      ]);

      setCategorias(categoriasData);

      setPagina(chamadosData.pagina);
      setTotalPaginas(chamadosData.totalPaginas);

      const chamadosTratados = chamadosData.dados.map((chamado) => {
        const categoria = categoriasData.find(
          (categoria) =>
            categoria.id_CategoriaAtendimento ===
            chamado.categoriaAtendimentoId,
        );

        return {
          ...chamado,
          solicitante: chamado.solicitanteNome,
          tecnico: chamado.tecnicoNome ?? "-",
          categoria: categoria?.nome ?? "-",
          statusDescricao: getStatusChamado(chamado.status),
          prioridadeDescricao: obterPrioridade(chamado.prioridade),
        };
      });

      const resultado = chamadosTratados.filter((chamado) => {
        const status =
          !filtros.status || chamado.status === Number(filtros.status);

        const prioridade =
          !filtros.prioridade ||
          chamado.prioridade === Number(filtros.prioridade);

        const categoria =
          !filtros.categoriaId ||
          chamado.categoriaAtendimentoId === Number(filtros.categoriaId);

        const solicitante =
          !filtros.solicitanteId ||
          chamado.solicitanteId === filtros.solicitanteId;

        const tecnico =
          !filtros.tecnicoId ||
          chamado.tecnicoResponsavelId === filtros.tecnicoId;

        return status && prioridade && categoria && solicitante && tecnico;
      });

      setHistorico(resultado);
      setPesquisado(true);
    } catch (error) {
      console.error(error);
      alert("Erro ao consultar histórico.");
    }
  };

  const limpar = () => {
    setFiltros({
      dataInicial: "",
      dataFinal: "",
      status: "",
      prioridade: "",
      categoriaId: "",
      solicitanteId: "",
      tecnicoId: "",
    });

    setHistorico([]);

    setPesquisado(false);
  };
  const carregarFiltros = useCallback(async () => {
    try {
      if (podeVerTudo) {
        const [usuariosData, categoriasData] = await Promise.all([
          usuarioService.buscarUsuariosHistorico(),
          categoriaService.getAll(),
        ]);

        setUsuarios(usuariosData);
        setCategorias(categoriasData);
        return;
      }

      const categoriasData = await categoriaService.getAll();
      setCategorias(categoriasData);
    } catch (error) {
      console.error(error);
    }
  }, [podeVerTudo]);

  const exportarExcel = () => {
    const dados = historico.map((chamado) => ({
      Número: chamado.numero,
      Categoria: chamado.categoria,
      Equipamento: chamado.equipamentoAfetado,
      Prioridade: chamado.prioridadeDescricao,
      Status: chamado.statusDescricao,
      Solicitante: chamado.solicitante,
      Técnico: chamado.tecnico,
      Abertura: new Date(chamado.dataAbertura).toLocaleString("pt-BR"),
      Fechamento: chamado.dataFechamento
        ? new Date(chamado.dataFechamento).toLocaleString("pt-BR")
        : "-",
    }));

    const worksheet = XLSX.utils.json_to_sheet(dados);

    const workbook = XLSX.utils.book_new();

    XLSX.utils.book_append_sheet(workbook, worksheet, "Historico");

    XLSX.writeFile(workbook, "HistoricoChamados.xlsx");
  };

  useEffect(() => {
    carregarFiltros();
  }, [carregarFiltros]);

  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <h1>Histórico de Chamados</h1>
      </div>

      <div className={styles.filtersCard}>
        <div className={styles.filtersGrid}>
          <div className={styles.field}>
            <label>Data Inicial *</label>

            <input
              type="date"
              value={filtros.dataInicial}
              onChange={(e) =>
                setFiltros({
                  ...filtros,
                  dataInicial: e.target.value,
                })
              }
            />
          </div>

          <div className={styles.field}>
            <label>Data Final *</label>

            <input
              type="date"
              value={filtros.dataFinal}
              onChange={(e) =>
                setFiltros({
                  ...filtros,
                  dataFinal: e.target.value,
                })
              }
            />
          </div>

          <div className={styles.field}>
            <label>Status</label>

            <select
              value={filtros.status}
              onChange={(e) =>
                setFiltros({
                  ...filtros,
                  status: e.target.value,
                })
              }
            >
              <option value="">Todos</option>
              <option value="1">Aberto</option>
              <option value="2">Em Atendimento</option>
              <option value="3">Fechado</option>
              <option value="4">Cancelado</option>
              <option value="5">Aguardando Retorno</option>
              <option value="6">Distribuído</option>
              <option value="7">Aguardando Equipamento</option>
            </select>
          </div>

          <div className={styles.field}>
            <label>Prioridade</label>

            <select
              value={filtros.prioridade}
              onChange={(e) =>
                setFiltros({
                  ...filtros,
                  prioridade: e.target.value,
                })
              }
            >
              <option value="">Todas</option>

              <option value="1">Baixa</option>

              <option value="2">Média</option>

              <option value="3">Alta</option>

              <option value="4">Crítica</option>
            </select>
          </div>

          <div className={styles.field}>
            <label>Categoria</label>

            <select
              value={filtros.categoriaId}
              onChange={(e) =>
                setFiltros({
                  ...filtros,
                  categoriaId: e.target.value,
                })
              }
            >
              <option value="">Todas</option>

              {categorias.map((categoria) => (
                <option
                  key={categoria.id_CategoriaAtendimento}
                  value={categoria.id_CategoriaAtendimento}
                >
                  {categoria.nome}
                </option>
              ))}
            </select>
          </div>

          {podeVerTudo && (
            <div className={styles.field}>
              <label>Solicitante</label>

              <select
                value={filtros.solicitanteId}
                onChange={(e) =>
                  setFiltros({
                    ...filtros,
                    solicitanteId: e.target.value,
                  })
                }
              >
                <option value="">Todos</option>

                {usuarios.map((usuario) => (
                  <option key={usuario.id} value={usuario.id}>
                    {usuario.username}
                  </option>
                ))}
              </select>
            </div>
          )}
          {podeVerTudo && (
            <div className={styles.field}>
              <label>Técnico</label>

              <select
                value={filtros.tecnicoId}
                onChange={(e) =>
                  setFiltros({
                    ...filtros,
                    tecnicoId: e.target.value,
                  })
                }
              >
                <option value="">Todos</option>

                {usuarios.map((usuario) => (
                  <option key={usuario.id} value={usuario.id}>
                    {usuario.username}
                  </option>
                ))}
              </select>
            </div>
          )}
        </div>
        <div className={styles.actionsRow}>
          <button className={styles.searchButton} onClick={() => pesquisar(1)}>
            Pesquisar
          </button>

          <button className={styles.clearButton} onClick={limpar}>
            Limpar
          </button>

          <button
            className={styles.exportButton}
            disabled={!historico.length}
            onClick={exportarExcel}
          >
            Exportar Excel
          </button>
        </div>
      </div>

      {!pesquisado && (
        <div className={styles.emptyState}>
          Selecione os filtros e clique em Pesquisar.
        </div>
      )}
      {pesquisado && historico.length === 0 && (
        <div className={styles.emptyState}>
          Nenhum chamado encontrado para os filtros informados.
        </div>
      )}

      {pesquisado && historico.length > 0 && (
        <div className={styles.tableContainer}>
          <table className={styles.table}>
            <thead>
              <tr>
                <th>Número</th>
                <th>Categoria</th>
                <th>Equipamento</th>
                <th>Prioridade</th>
                <th>Status</th>
                <th>Solicitante</th>
                <th>Técnico</th>
                <th>Abertura</th>
                <th>Fechamento</th>
              </tr>
            </thead>

            <tbody>
              {historico.map((chamado) => (
                <tr key={chamado.id_Chamado}>
                  <td>{chamado.numero}</td>
                  <td>{chamado.categoria}</td>
                  <td>{chamado.equipamentoAfetado}</td>
                  <td>{chamado.prioridadeDescricao}</td>
                  <td>{chamado.statusDescricao}</td>
                  <td>{chamado.solicitante}</td>
                  <td>{chamado.tecnico}</td>

                  <td>
                    {new Date(chamado.dataAbertura).toLocaleDateString("pt-BR")}
                  </td>

                  <td>
                    {chamado.dataFechamento
                      ? new Date(chamado.dataFechamento).toLocaleDateString(
                          "pt-BR",
                        )
                      : "-"}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          {pesquisado && historico.length > 0 && (
            <>
              <div className={styles.pagination}>
                <button
                  disabled={pagina === 1}
                  onClick={() => pesquisar(pagina - 1)}
                >
                  Anterior
                </button>

                <span>
                  Página {pagina} de {totalPaginas}
                </span>

                <button
                  disabled={pagina >= totalPaginas}
                  onClick={() => pesquisar(pagina + 1)}
                >
                  Próxima
                </button>
              </div>
            </>
          )}
        </div>
      )}
    </div>
  );
}
