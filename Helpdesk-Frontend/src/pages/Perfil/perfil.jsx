import { useEffect, useState } from "react";
import perfilService from "../../services/perfilService";
import permissaoService from "../../services/permissaoService";
import perfilPermissaoService from "../../services/perfilPermissaoService";
import { hasPermission } from "../../utils/permissoesUtils";
import { apiErrorMessage } from "../../utils/apiError";
import PeriodoTrabalhoForm from "./PeriodoTrabalhoForm";
import {
  criarPeriodoFormulario,
  criarPeriodoPayload,
  PERIODO_TRABALHO_INICIAL,
} from "../../utils/periodoTrabalhoUtils";
import styles from "./perfil.module.css";

export default function Perfis() {
  const [perfis, setPerfis] = useState([]);
  const [permissoes, setPermissoes] = useState([]);
  const [openModal, setOpenModal] = useState(false);
  const [openPermissoesModal, setOpenPermissoesModal] = useState(false);
  const [perfilEmEdicao, setPerfilEmEdicao] = useState(null);
  const [perfilSelecionado, setPerfilSelecionado] = useState(null);
  const [nome, setNome] = useState("");
  const [descricao, setDescricao] = useState("");
  const [ativo, setAtivo] = useState(true);
  const [possuiPeriodoTrabalho, setPossuiPeriodoTrabalho] = useState(true);
  const [periodoTrabalho, setPeriodoTrabalho] = useState(() => ({
    ...PERIODO_TRABALHO_INICIAL,
  }));
  const [salvandoPerfil, setSalvandoPerfil] = useState(false);
  const [erroFormulario, setErroFormulario] = useState("");
  const [permissoesSelecionadas, setPermissoesSelecionadas] = useState([]);
  const [filtro, setFiltro] = useState("");
  const [filtroStatus, setFiltroStatus] = useState("todos");
  const [pagina, setPagina] = useState(1);

  const tamanhoPagina = 10;
  const podeGerenciarPermissoes = hasPermission("GERENCIAR_PERMISSOES");

  const carregarDados = async () => {
    try {
      const perfisData = await perfilService.getAll();

      setPerfis(perfisData);
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
    setPossuiPeriodoTrabalho(true);
    setPeriodoTrabalho({ ...PERIODO_TRABALHO_INICIAL });
    setErroFormulario("");
    setPerfilEmEdicao(null);
  };

  const abrirModalNovo = () => {
    limparFormulario();
    setOpenModal(true);
  };

  const abrirModalEditar = async (perfil) => {
    try {
      const perfilAtualizado = await perfilService.getById(perfil.id_Perfil);

      setPerfilEmEdicao(perfilAtualizado);
      setNome(perfilAtualizado.nome);
      setDescricao(perfilAtualizado.descricao ?? "");
      setAtivo(perfilAtualizado.ativo);
      setPossuiPeriodoTrabalho(
        perfilAtualizado.possuiPeriodoTrabalho ??
          Boolean(perfilAtualizado.periodoTrabalho),
      );
      setPeriodoTrabalho(
        criarPeriodoFormulario(perfilAtualizado.periodoTrabalho),
      );
      setErroFormulario("");
      setOpenModal(true);
    } catch (error) {
      console.error(error);
      alert(apiErrorMessage(error, "Erro ao carregar o perfil."));
    }
  };
  const fecharModal = () => {
    setOpenModal(false);
    limparFormulario();
  };

  const salvarPerfil = async (e) => {
    e.preventDefault();
    setErroFormulario("");
    setSalvandoPerfil(true);

    try {
      const periodoPayload = possuiPeriodoTrabalho
        ? criarPeriodoPayload(periodoTrabalho)
        : null;
      const payload = {
        nome,
        descricao,
        ativo,
        possuiPeriodoTrabalho,
        periodoTrabalho: periodoPayload,
      };

      if (perfilEmEdicao) {
        await perfilService.update(perfilEmEdicao.id_Perfil, payload);
      } else {
        await perfilService.create(payload);
      }

      await carregarDados();
      fecharModal();
    } catch (error) {
      console.error(error);
      setErroFormulario(
        error?.response
          ? apiErrorMessage(error, "Erro ao salvar perfil.")
          : error.message || "Erro ao salvar perfil.",
      );
    } finally {
      setSalvandoPerfil(false);
    }
  };

  const alterarStatus = async (perfil) => {
    try {
      if (perfil.ativo) {
        await perfilService.desativar(perfil.id_Perfil);
      } else {
        await perfilService.ativar(perfil.id_Perfil);
      }

      await carregarDados();
    } catch (error) {
      console.error(error);

      alert("Erro ao alterar status.");
    }
  };

  const abrirPermissoes = async (perfil) => {
    try {
      setPerfilSelecionado(perfil);

      const todasPermissoes = await permissaoService.getAll();

      const permissoesPerfil =
        await perfilPermissaoService.buscarPermissoesPerfil(perfil.id_Perfil);

      console.log(
        "TODAS AS PERMISSOES:",
        JSON.stringify(todasPermissoes, null, 2),
      );

      console.log(
        "PERMISSOES DO PERFIL:",
        JSON.stringify(permissoesPerfil, null, 2),
      );

      setPermissoes(todasPermissoes);

      setPermissoesSelecionadas(permissoesPerfil.map((x) => x.id_Permissao));

      setOpenPermissoesModal(true);
    } catch (error) {
      console.error("Erro ao carregar permissões:", error);
    }
  };

  const fecharPermissoes = () => {
    setOpenPermissoesModal(false);

    setPermissoes([]);
    setPermissoesSelecionadas([]);
    setPerfilSelecionado(null);
  };

  const alterarPermissao = (permissaoId) => {
    setPermissoesSelecionadas((atual) => {
      if (atual.includes(permissaoId)) {
        return atual.filter((x) => x !== permissaoId);
      }

      return [...atual, permissaoId];
    });
  };

  const salvarPermissoes = async () => {
    try {
      const permissoesAtuais =
        await perfilPermissaoService.buscarPermissoesPerfil(
          perfilSelecionado.id_Perfil,
        );

      const idsAtuais = permissoesAtuais.map((x) => x.id_Permissao);

      const paraAdicionar = permissoesSelecionadas.filter(
        (id) => !idsAtuais.includes(id),
      );

      const paraRemover = idsAtuais.filter(
        (id) => !permissoesSelecionadas.includes(id),
      );

      for (const permissaoId of paraAdicionar) {
        await perfilPermissaoService.vincular({
          perfilId: perfilSelecionado.id_Perfil,
          permissaoId,
        });
      }

      for (const permissaoId of paraRemover) {
        await perfilPermissaoService.desvincular(
          perfilSelecionado.id_Perfil,
          permissaoId,
        );
      }

      fecharPermissoes();
    } catch (error) {
      console.error(error);

      alert("Erro ao salvar permissões.");
    }
  };

  const perfisFiltrados = perfis.filter((perfil) => {
    const termo = filtro.toLowerCase();

    const correspondeTexto =
      perfil.nome?.toLowerCase().includes(termo) ||
      perfil.descricao?.toLowerCase().includes(termo);

    const correspondeStatus =
      filtroStatus === "todos" ||
      (filtroStatus === "ativo" && perfil.ativo) ||
      (filtroStatus === "inativo" && !perfil.ativo);

    return correspondeTexto && correspondeStatus;
  });

  const totalPaginas = Math.ceil(perfisFiltrados.length / tamanhoPagina);

  const indiceInicial = (pagina - 1) * tamanhoPagina;

  const perfilPaginados = perfisFiltrados.slice(
    indiceInicial,
    indiceInicial + tamanhoPagina,
  );

  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <h1>Perfis</h1>

        <button className={styles.novoButton} onClick={abrirModalNovo}>
          Novo Perfil
        </button>
      </div>
      <div className={styles.filters}>
        <input
          type="text"
          placeholder="Pesquisar..."
          value={filtro}
          onChange={(e) => {
            setFiltro(e.target.value);
            setPagina(1);
          }}
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
            {perfilPaginados.map((perfil) => (
              <tr key={perfil.id_Perfil}>
                <td>{perfil.nome}</td>

                <td>{perfil.descricao || "-"}</td>

                <td>
                  <span
                    className={
                      perfil.ativo ? styles.statusAtivo : styles.statusInativo
                    }
                  >
                    {perfil.ativo ? "Ativo" : "Inativo"}
                  </span>
                </td>
                <td>
                  <div className={styles.actionButtons}>
                    <button
                      className={styles.editButton}
                      onClick={() => abrirModalEditar(perfil)}
                    >
                      Editar
                    </button>

                    {podeGerenciarPermissoes && (
                      <button
                        className={styles.permissionButton}
                        onClick={() => abrirPermissoes(perfil)}
                      >
                        Permissões
                      </button>
                    )}

                    <button
                      className={
                        perfil.ativo
                          ? styles.deleteButton
                          : styles.permissionButton
                      }
                      onClick={() => alterarStatus(perfil)}
                    >
                      {perfil.ativo ? "Desativar" : "Ativar"}
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
          <div className={`${styles.modal} ${styles.profileModal}`}>
            <h2>{perfilEmEdicao ? "Editar Perfil" : "Novo Perfil"}</h2>

            <p className={styles.subtitle}>
              Informe os dados do perfil e o período utilizado nos prazos de
              atendimento.
            </p>

            <form onSubmit={salvarPerfil}>
              <div className={styles.profileFieldsGrid}>
                <div className={styles.field}>
                  <label htmlFor="perfil-nome">Nome</label>

                  <input
                    id="perfil-nome"
                    type="text"
                    value={nome}
                    onChange={(e) => setNome(e.target.value)}
                    required
                  />
                </div>

                <div className={styles.field}>
                  <label htmlFor="perfil-status">Ativo</label>

                  <select
                    id="perfil-status"
                    value={ativo.toString()}
                    onChange={(e) => setAtivo(e.target.value === "true")}
                  >
                    <option value="true">Sim</option>

                    <option value="false">Não</option>
                  </select>
                </div>

                <div className={`${styles.field} ${styles.descriptionField}`}>
                  <label htmlFor="perfil-descricao">Descrição</label>

                  <input
                    id="perfil-descricao"
                    type="text"
                    value={descricao}
                    onChange={(e) => setDescricao(e.target.value)}
                  />
                </div>
              </div>

              <label className={styles.scheduleFlag}>
                <input
                  type="checkbox"
                  checked={possuiPeriodoTrabalho}
                  onChange={(event) =>
                    setPossuiPeriodoTrabalho(event.target.checked)
                  }
                />
                <span>
                  <strong>Perfil tem período de trabalho?</strong>
                  <small>
                    Marque para configurar os horários utilizados nos cálculos
                    de SLA.
                  </small>
                </span>
              </label>

              {possuiPeriodoTrabalho ? (
                <PeriodoTrabalhoForm
                  value={periodoTrabalho}
                  onChange={setPeriodoTrabalho}
                />
              ) : (
                <div className={styles.noScheduleNotice}>
                  Este perfil não terá período de trabalho e não será usado
                  como referência de horário para o SLA.
                </div>
              )}

              {erroFormulario && (
                <div className={styles.formError} role="alert">
                  {erroFormulario}
                </div>
              )}

              <div className={styles.actions}>
                <button
                  type="button"
                  onClick={fecharModal}
                  disabled={salvandoPerfil}
                >
                  Cancelar
                </button>

                <button type="submit" disabled={salvandoPerfil}>
                  {salvandoPerfil ? "Salvando..." : "Salvar"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {openPermissoesModal && (
        <div className={styles.overlay}>
          <div className={styles.permissionsModal}>
            <h2>Permissões</h2>

            <p className={styles.subtitle}>Perfil: {perfilSelecionado?.nome}</p>

            <div className={styles.permissionList}>
              {permissoes.map((permissao) => (
                <label key={permissao.nome} className={styles.permissionItem}>
                  <input
                    type="checkbox"
                    checked={permissoesSelecionadas.includes(
                      permissao.id_Permissao,
                    )}
                    onChange={() => alterarPermissao(permissao.id_Permissao)}
                  />

                  {permissao.nome}
                </label>
              ))}
            </div>

            <div className={styles.actions}>
              <button type="button" onClick={fecharPermissoes}>
                Cancelar
              </button>

              <button type="button" onClick={salvarPermissoes}>
                Salvar
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
