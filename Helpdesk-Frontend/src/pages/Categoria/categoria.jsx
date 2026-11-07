import { useEffect, useState } from "react";
import categoriaService from "../../services/categoriaService";
import { hasPermission } from "../../utils/permissoesUtils";
import styles from "./categoria.module.css";

export default function Categoria() {
  const podeGerenciarCategorias = hasPermission("GERENCIAR_CATEGORIAS");
  const [categorias, setCategorias] = useState([]);
  const [openModal, setOpenModal] = useState(false);
  const [categoriaEmEdicao, setCategoriaEmEdicao] = useState(null);
  const [nome, setNome] = useState("");
  const [descricao, setDescricao] = useState("");
  const [prioridade, setPrioridade] = useState(1);
  const [filtro, setFiltro] = useState("");
  const [filtroStatus, setFiltroStatus] = useState("todos");
  const [pagina, setPagina] = useState(1);

  const tamanhoPagina = 10;

  const carregarDados = async () => {
    try {
      const data = await categoriaService.getAll();
      setCategorias(data);
    } catch (error) {
      console.error(error);
    }
  };

  useEffect(() => {
    carregarDados();
  }, []);

  const limparFormulario = () => {
    setNome("");
    setDescricao("");
    setPrioridade(1);

    setCategoriaEmEdicao(null);
  };

  const abrirModalNovo = () => {
    limparFormulario();

    setOpenModal(true);
  };

  const abrirModalEditar = (categoria) => {
    setCategoriaEmEdicao(categoria);

    setNome(categoria.nome);
    setDescricao(categoria.descricao ?? "");
    setPrioridade(categoria.prioridade);

    setOpenModal(true);
  };

  const fecharModal = () => {
    setOpenModal(false);

    limparFormulario();
  };

  const salvarCategoria = async (e) => {
    e.preventDefault();

    try {
      const payload = {
        nome,
        descricao,
        prioridade: Number(prioridade),
      };

      if (categoriaEmEdicao) {
        await categoriaService.update(
          categoriaEmEdicao.id_CategoriaAtendimento,
          payload,
        );

        alert("Categoria atualizada com sucesso!");
      } else {
        await categoriaService.create(payload);

        alert("Categoria cadastrada com sucesso!");
      }

      await carregarDados();

      fecharModal();
    } catch (error) {
      console.error(error);

      alert("Erro ao salvar categoria.");
    }
  };

  const alterarStatus = async (categoria) => {
    try {
      if (categoria.ativo) {
        await categoriaService.desativar(categoria.id_CategoriaAtendimento);
      } else {
        await categoriaService.ativar(categoria.id_CategoriaAtendimento);
      }

      await carregarDados();
    } catch (error) {
      console.error(error);

      alert("Erro ao alterar status.");
    }
  };

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

  const categoriasFiltrados = categorias.filter((categoria) => {
    const termo = filtro.toLowerCase();

    const correspondeTexto =
      categoria.nome?.toLowerCase().includes(termo) ||
      categoria.descricao?.toLowerCase().includes(termo) ||
      obterPrioridade(categoria.prioridade)?.toLowerCase().includes(termo);

    const correspondeStatus =
      filtroStatus === "todos" ||
      (filtroStatus === "ativo" && categoria.ativo) ||
      (filtroStatus === "inativo" && !categoria.ativo);

    return correspondeTexto && correspondeStatus;
  });

  const totalPaginas = Math.ceil(categoriasFiltrados.length / tamanhoPagina);

  const indiceInicial = (pagina - 1) * tamanhoPagina;

  const categoriasPaginados = categoriasFiltrados.slice(
    indiceInicial,
    indiceInicial + tamanhoPagina,
  );

  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <h1>Categorias</h1>

        {podeGerenciarCategorias && (
          <button className={styles.novoButton} onClick={abrirModalNovo}>
            Nova Categoria
          </button>
        )}
      </div>
      <div className={styles.filters}>
        <input
          type="text"
          placeholder="Pesquisar..."
          value={filtro}
          onChange={(e) => setFiltro(e.target.value)}
        />

        <select
          value={filtroStatus}
          onChange={(e) => setFiltroStatus(e.target.value)}
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
              <th>Nome</th>
              <th>Descrição</th>
              <th>Prioridade</th>
              <th>Status</th>
              {podeGerenciarCategorias && <th>Ações</th>}
            </tr>
          </thead>

          <tbody>
            {categoriasPaginados.map((categoria) => (
              <tr key={categoria.id_CategoriaAtendimento}>
                <td>{categoria.nome}</td>
                <td>{categoria.descricao || "-"}</td>
                <td>{obterPrioridade(categoria.prioridade)}</td>
                <td>
                  <span
                    className={
                      categoria.ativo
                        ? styles.statusAtivo
                        : styles.statusInativo
                    }
                  >
                    {categoria.ativo ? "Ativo" : "Inativo"}
                  </span>
                </td>

                {podeGerenciarCategorias && (
                  <td>
                    <div className={styles.actionButtons}>
                      <button
                        className={styles.editButton}
                        onClick={() => abrirModalEditar(categoria)}
                      >
                        Editar
                      </button>

                      <button
                        className={
                          categoria.ativo
                            ? styles.deleteButton
                            : styles.permissionButton
                        }
                        onClick={() => alterarStatus(categoria)}
                      >
                        {categoria.ativo ? "Desativar" : "Ativar"}
                      </button>
                    </div>
                  </td>
                )}
              </tr>
            ))}
          </tbody>
        </table>
        <div className={styles.pagination}>
          <button disabled={pagina === 1} onClick={() => setPagina(pagina - 1)}>
            Anterior
          </button>

          <span>
            Página {pagina} de {totalPaginas}
          </span>

          <button
            disabled={pagina >= totalPaginas}
            onClick={() => setPagina(pagina + 1)}
          >
            Próxima
          </button>
        </div>
      </div>

      {openModal && (
        <div className={styles.overlay}>
          <div className={styles.modal}>
            <h2>{categoriaEmEdicao ? "Editar Categoria" : "Nova Categoria"}</h2>

            <form onSubmit={salvarCategoria}>
              <div className={styles.field}>
                <label>Nome</label>

                <input
                  type="text"
                  value={nome}
                  onChange={(e) => setNome(e.target.value)}
                  required
                />
              </div>

              <div className={styles.field}>
                <label>Descrição</label>

                <textarea
                  value={descricao}
                  onChange={(e) => setDescricao(e.target.value)}
                />
              </div>

              <div className={styles.field}>
                <label>Prioridade</label>

                <select
                  value={prioridade}
                  onChange={(e) => setPrioridade(e.target.value)}
                >
                  <option value="1">Baixa</option>
                  <option value="2">Média</option>
                  <option value="3">Alta</option>
                  <option value="4">Crítica</option>
                </select>
              </div>

              <div className={styles.actions}>
                <button type="button" onClick={fecharModal}>
                  Cancelar
                </button>

                <button type="submit">Salvar</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
