import { useEffect, useRef, useState } from "react";
import usuarioService from "../../services/usuarioService";
import perfilService from "../../services/perfilService";
import usuarioPerfilService from "../../services/usuarioPerfilService";

import styles from "./usuarios.module.css";

export default function Usuarios() {
  const [usuarios, setUsuarios] = useState([]);
  const [perfis, setPerfis] = useState([]);
  const [username, setUsername] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [passwordConfirmation, setPasswordConfirmation] = useState("");
  const [openModal, setOpenModal] = useState(false);
  const [usuarioEmEdicao, setUsuarioEmEdicao] = useState(null);
  const [ativo, setAtivo] = useState(true);
  const [perfilId, setPerfilId] = useState("");
  const [filtro, setFiltro] = useState("");
  const [filtroStatus, setFiltroStatus] = useState("todos");
  const [pagina, setPagina] = useState(1);
  const [usuarioParaRedefinir, setUsuarioParaRedefinir] = useState(null);
  const [redefinindoSenha, setRedefinindoSenha] = useState(false);
  const redefinicaoEmAndamentoRef = useRef(false);

  const tamanhoPagina = 10;

  const carregarDados = async () => {
    try {
      const [usuariosData, perfisData] = await Promise.all([
        usuarioService.getAll(),
        perfilService.getAll(),
      ]);

      console.log(JSON.stringify(usuariosData[0], null, 2));

      setUsuarios(usuariosData);
      setPerfis(perfisData.filter((perfil) => perfil.ativo));

      if (perfisData.length > 0) {
        setPerfilId(perfisData[0].id_Perfil);
      }
    } catch (error) {
      console.error(error);
    }
  };

  useEffect(() => {
    carregarDados();
  }, []);

  const limparFormulario = () => {
    setUsername("");
    setEmail("");
    setPassword("");
    setPasswordConfirmation("");
    setAtivo(true);
    setUsuarioEmEdicao(null);

    if (perfis.length > 0) {
      setPerfilId(perfis[0].id_Perfil);
    }
  };

  const abrirModalEditar = async (usuario) => {
    try {
      const dados = await usuarioService.getById(usuario.id);

      setUsuarioEmEdicao(usuario);
      setUsername(dados.username);
      setEmail(dados.email);
      setAtivo(dados.ativo);
      setPerfilId(dados.id_Perfil);
      setPassword("");
      setPasswordConfirmation("");
      setOpenModal(true);
    } catch (error) {
      console.error(error);
    }
  };

  const abrirModal = () => {
    limparFormulario();
    setOpenModal(true);
  };

  const fecharModal = () => {
    setOpenModal(false);
    limparFormulario();
  };
  const salvarUsuario = async (e) => {
    e.preventDefault();
    try {
      if (usuarioEmEdicao) {
        await usuarioService.update(usuarioEmEdicao.id, {
          username,
          email,
          ativo,
        });
        alert("Usuário atualizado com sucesso!");
      } else {
        const usuarioCriado = await usuarioService.create({
          username,
          email,
          password,
          passwordConfirmation,
        });

        await usuarioPerfilService.vincular({
          usuarioId: usuarioCriado.id,
          perfilId: Number(perfilId),
        });
        alert("Usuário cadastrado com sucesso!");
      }
      await carregarDados();
      fecharModal();
    } catch (error) {
      console.error(error);
      alert("Erro ao salvar usuário.");
    }
  };
  const alterarStatus = async (usuario) => {
    try {
      if (usuario.ativo) {
        await usuarioService.desativar(usuario.id);
      } else {
        await usuarioService.ativar(usuario.id);
      }
      await carregarDados();
    } catch (error) {
      console.error(error);
      alert("Erro ao alterar status.");
    }
  };

  const abrirConfirmacaoRedefinicao = (usuario) => {
    setUsuarioParaRedefinir(usuario);
  };

  const fecharConfirmacaoRedefinicao = () => {
    if (redefinicaoEmAndamentoRef.current) return;

    setUsuarioParaRedefinir(null);
  };

  const confirmarRedefinicaoSenha = async () => {
    if (!usuarioParaRedefinir || redefinicaoEmAndamentoRef.current) return;

    redefinicaoEmAndamentoRef.current = true;
    setRedefinindoSenha(true);

    try {
      const mensagem = await usuarioService.restaurarSenhaPadrao(
        usuarioParaRedefinir.id,
      );

      setUsuarioParaRedefinir(null);
      alert(mensagem || "Senha redefinida com sucesso.");
    } catch (error) {
      console.error(error);

      const dadosErro = error.response?.data;
      const mensagemErro =
        (typeof dadosErro === "string" && dadosErro) ||
        dadosErro?.message ||
        dadosErro?.error ||
        "Não foi possível redefinir a senha.";

      alert(mensagemErro);
    } finally {
      redefinicaoEmAndamentoRef.current = false;
      setRedefinindoSenha(false);
    }
  };

  const usuariosFiltrados = usuarios.filter((usuario) => {
    const termo = filtro.toLowerCase();

    const correspondeTexto =
      usuario.username?.toLowerCase().includes(termo) ||
      usuario.email?.toLowerCase().includes(termo);

    const correspondeStatus =
      filtroStatus === "todos" ||
      (filtroStatus === "ativo" && usuario.ativo) ||
      (filtroStatus === "inativo" && !usuario.ativo);

    return correspondeTexto && correspondeStatus;
  });

  const totalPaginas = Math.ceil(usuariosFiltrados.length / tamanhoPagina);

  const indiceInicial = (pagina - 1) * tamanhoPagina;

  const usuariosPaginados = usuariosFiltrados.slice(
    indiceInicial,
    indiceInicial + tamanhoPagina,
  );

  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <h1>Usuários</h1>

        <button className={styles.novoButton} onClick={abrirModal}>
          Novo Usuário
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
              <th>Usuário</th>
              <th>E-mail</th>
              <th>Status</th>
              <th>Ações</th>
            </tr>
          </thead>

          <tbody>
            {usuariosPaginados.map((usuario) => (
              <tr key={usuario.id}>
                <td>{usuario.username}</td>

                <td>{usuario.email}</td>

                <td>
                  <span
                    className={
                      usuario.ativo ? styles.statusAtivo : styles.statusInativo
                    }
                  >
                    {usuario.ativo ? "Ativo" : "Inativo"}
                  </span>
                </td>

                <td>
                  <div className={styles.actionButtons}>
                    <button
                      className={styles.editButton}
                      onClick={() => abrirModalEditar(usuario)}
                    >
                      Editar
                    </button>

                    <button
                      className={
                        usuario.ativo
                          ? styles.deleteButton
                          : styles.permissionButton
                      }
                      onClick={() => alterarStatus(usuario)}
                    >
                      {usuario.ativo ? "Desativar" : "Ativar"}
                    </button>

                    <button
                      type="button"
                      className={styles.resetPasswordButton}
                      onClick={() => abrirConfirmacaoRedefinicao(usuario)}
                    >
                      Redefinir senha
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
            <h2>{usuarioEmEdicao ? "Editar Usuário" : "Novo Usuário"}</h2>
            <form onSubmit={salvarUsuario}>
              <div className={styles.field}>
                <label>Usuário</label>

                <input
                  type="text"
                  value={username}
                  onChange={(e) => setUsername(e.target.value)}
                  required
                />
              </div>

              <div className={styles.field}>
                <label>E-mail</label>

                <input
                  type="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  required
                />
              </div>

              {!usuarioEmEdicao && (
                <>
                  <div className={styles.field}>
                    <label>Senha</label>

                    <input
                      type="password"
                      value={password}
                      onChange={(e) => setPassword(e.target.value)}
                      required
                    />
                  </div>

                  <div className={styles.field}>
                    <label>Confirmar Senha</label>

                    <input
                      type="password"
                      value={passwordConfirmation}
                      onChange={(e) => setPasswordConfirmation(e.target.value)}
                      required
                    />
                  </div>
                </>
              )}

              <div className={styles.field}>
                <label>Perfil</label>

                <select
                  value={perfilId}
                  onChange={(e) => setPerfilId(e.target.value)}
                >
                  {perfis.map((perfil) => (
                    <option key={perfil.id_Perfil} value={perfil.id_Perfil}>
                      {perfil.nome}
                    </option>
                  ))}
                </select>
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

      {usuarioParaRedefinir && (
        <div className={styles.overlay}>
          <div
            className={`${styles.modal} ${styles.confirmationModal}`}
            role="dialog"
            aria-modal="true"
            aria-labelledby="titulo-redefinir-senha"
          >
            <h2 id="titulo-redefinir-senha">Redefinir senha</h2>

            <p className={styles.resetDescription}>
              Deseja restaurar a senha de
              {" "}
              <strong>{usuarioParaRedefinir.username}</strong>
              {" "}
              para a senha padrão?
            </p>

            <p className={styles.resetWarning}>
              O usuário deverá trocar a senha no próximo login.
            </p>

            <div className={styles.actions}>
              <button
                type="button"
                onClick={fecharConfirmacaoRedefinicao}
                disabled={redefinindoSenha}
              >
                Cancelar
              </button>

              <button
                type="button"
                className={styles.confirmResetButton}
                onClick={confirmarRedefinicaoSenha}
                disabled={redefinindoSenha}
                aria-label="Confirmar redefinição"
              >
                {redefinindoSenha ? "Redefinindo..." : "Redefinir senha"}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
