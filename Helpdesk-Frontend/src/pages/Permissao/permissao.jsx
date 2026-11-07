import { useEffect, useState } from "react";
import permissaoService from "../../services/permissaoService";
import styles from "./permissao.module.css";

export default function Permissao() {
  const [permissoes, setPermissoes] = useState([]);
  const [openModal, setOpenModal] = useState(false);
  const [permissaoEmEdicao, setPermissaoEmEdicao] = useState(null);
  const [nome, setNome] = useState("");
  const [descricao, setDescricao] = useState("");
  const [ativo, setAtivo] = useState(true);
  const [filtro, setFiltro] = useState("");
  const [filtroStatus, setFiltroStatus] = useState("todos");
  const [pagina, setPagina] = useState(1);

  const tamanhoPagina = 10;

  const carregarDados = async () => {
    try {
      const data = await permissaoService.getAll();

      setPermissoes(data);
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
    setAtivo(true);
    setPermissaoEmEdicao(null);
  };

  const abrirModalNovo = () => {
    limparFormulario();
    setOpenModal(true);
  };

  const abrirModalEditar = (permissao) => {
    setPermissaoEmEdicao(permissao);
    setNome(permissao.nome);
    setDescricao(permissao.descricao ?? "");
    setAtivo(permissao.ativo);
    setOpenModal(true);
  };

  const fecharModal = () => {
    setOpenModal(false);
    limparFormulario();
  };

  const salvarPermissao = async (e) => {
    e.preventDefault();

    try {
      const payload = {
        nome,
        descricao,
        ativo,
      };

      if (permissaoEmEdicao) {
        await permissaoService.update(permissaoEmEdicao.id_Permissao, {
          ...permissaoEmEdicao,
          ...payload,
        });
      } else {
        await permissaoService.create(payload);
      }

      await carregarDados();

      fecharModal();
    } catch (error) {
      console.error(error);

      alert("Erro ao salvar permissão.");
    }
  };

  const alterarStatus = async (permissao) => {
    console.log("ID:", permissao.id_Permissao);

    try {
      if (permissao.ativo) {
        await permissaoService.desativar(permissao.id_Permissao);
      } else {
        await permissaoService.ativar(permissao.id_Permissao);
      }
      await carregarDados();
    } catch (error) {
      console.error(error);

      alert("Erro ao alterar status.");
    }
  };

  const permissoesFiltradas = permissoes.filter((permissao) => {
    const termo = filtro.toLowerCase();

    const correspondeTexto =
      permissao.nome?.toLowerCase().includes(termo) ||
      permissao.descricao?.toLowerCase().includes(termo);

    const correspondeStatus =
      filtroStatus === "todos" ||
      (filtroStatus === "ativo" && permissao.ativo) ||
      (filtroStatus === "inativo" && !permissao.ativo);

    return correspondeTexto && correspondeStatus;
  });

  const totalPaginas = Math.ceil(permissoesFiltradas.length / tamanhoPagina);

  const indiceInicial = (pagina - 1) * tamanhoPagina;

  const permissoesPaginados = permissoesFiltradas.slice(
    indiceInicial,
    indiceInicial + tamanhoPagina,
  );

  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <h1>Permissões</h1>

        <button className={styles.novoButton} onClick={abrirModalNovo}>
          Nova Permissão
        </button>
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
              <th>Status</th>
              <th>Ações</th>
            </tr>
          </thead>

          <tbody>
            {permissoesPaginados.map((permissao) => (
              <tr key={permissao.id_Permissao}>
                <td>{permissao.nome}</td>

                <td>{permissao.descricao || "-"}</td>

                <td>
                  <span
                    className={
                      permissao.ativo
                        ? styles.statusAtivo
                        : styles.statusInativo
                    }
                  >
                    {permissao.ativo ? "Ativo" : "Inativo"}
                  </span>
                </td>

                <td>
                  <div className={styles.actionButtons}>
                    <button
                      className={styles.editButton}
                      onClick={() => abrirModalEditar(permissao)}
                    >
                      Editar
                    </button>

                    <button
                      className={
                        permissao.ativo
                          ? styles.deleteButton
                          : styles.permissionButton
                      }
                      onClick={() => {
                        console.log("PERMISSAO CLICADA:", permissao);

                        alterarStatus(permissao);
                      }}
                    >
                      {permissao.ativo ? "Desativar" : "Ativar"}
                    </button>
                  </div>
                </td>
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
            <h2>{permissaoEmEdicao ? "Editar Permissão" : "Nova Permissão"}</h2>

            <form onSubmit={salvarPermissao}>
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
                <label>Ativo</label>

                <select
                  value={ativo.toString()}
                  onChange={(e) => setAtivo(e.target.value === "true")}
                >
                  <option value="true">Sim</option>

                  <option value="false">Não</option>
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
