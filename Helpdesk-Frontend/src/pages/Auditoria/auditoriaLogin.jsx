import { useEffect, useState } from "react";
import { Eye, Search, Trash2 } from "lucide-react";
import auditoriaIdentityService from "../../services/auditoriaIdentityService";
import usuarioService from "../../services/usuarioService";
import { apiErrorMessage } from "../../utils/apiError";
import { hasPermission } from "../../utils/permissoesUtils";
import {
  eventoAuditoriaTexto,
  eventosAuditoriaIdentity,
  formatarDataHora,
} from "../../utils/auditoriaUtils";
import AuditoriaDetalhesModal from "./AuditoriaDetalhesModal";
import styles from "./auditoria.module.css";

const filtrosIniciais = {
  dataInicial: "",
  dataFinal: "",
  usuarioId: "",
  evento: "",
  sucesso: "",
};

const tamanhoPagina = 20;

export default function AuditoriaLogin() {
  const [filtros, setFiltros] = useState(filtrosIniciais);
  const [filtrosAplicados, setFiltrosAplicados] = useState(null);
  const [auditorias, setAuditorias] = useState([]);
  const [usuarios, setUsuarios] = useState([]);
  const [pagina, setPagina] = useState(1);
  const [totalPaginas, setTotalPaginas] = useState(0);
  const [totalRegistros, setTotalRegistros] = useState(0);
  const [pesquisado, setPesquisado] = useState(false);
  const [carregando, setCarregando] = useState(false);
  const [erro, setErro] = useState("");
  const [selecionada, setSelecionada] = useState(null);
  const podeListarUsuarios = hasPermission("GERENCIAR_USUARIOS");

  useEffect(() => {
    if (!podeListarUsuarios) return;

    usuarioService
      .getAll()
      .then(setUsuarios)
      .catch(() => setUsuarios([]));
  }, [podeListarUsuarios]);

  const executarPesquisa = async (paginaAtual, filtrosConsulta) => {
    setCarregando(true);
    setErro("");

    try {
      const resultado = await auditoriaIdentityService.buscarPaginado({
        dataInicial: filtrosConsulta.dataInicial,
        dataFinal: filtrosConsulta.dataFinal,
        usuarioId: filtrosConsulta.usuarioId || undefined,
        evento: filtrosConsulta.evento ? Number(filtrosConsulta.evento) : undefined,
        sucesso: filtrosConsulta.sucesso === "" ? undefined : filtrosConsulta.sucesso === "true",
        pagina: paginaAtual,
        tamanhoPagina,
      });

      setAuditorias(resultado.dados ?? []);
      setPagina(resultado.pagina ?? paginaAtual);
      setTotalPaginas(resultado.totalPaginas ?? 0);
      setTotalRegistros(resultado.totalRegistros ?? 0);
      setPesquisado(true);
    } catch (error) {
      setAuditorias([]);
      setPesquisado(true);
      setErro(apiErrorMessage(error, "Não foi possível consultar a auditoria de login."));
    } finally {
      setCarregando(false);
    }
  };

  const pesquisar = () => {
    if (!filtros.dataInicial || !filtros.dataFinal) {
      setErro("Informe a data inicial e a data final.");
      return;
    }

    if (filtros.dataInicial > filtros.dataFinal) {
      setErro("A data inicial não pode ser maior que a data final.");
      return;
    }

    const novosFiltros = { ...filtros };
    setFiltrosAplicados(novosFiltros);
    executarPesquisa(1, novosFiltros);
  };

  const limpar = () => {
    setFiltros(filtrosIniciais);
    setFiltrosAplicados(null);
    setAuditorias([]);
    setPagina(1);
    setTotalPaginas(0);
    setTotalRegistros(0);
    setPesquisado(false);
    setErro("");
  };

  const atualizarFiltro = (campo, valor) => {
    setFiltros((atuais) => ({ ...atuais, [campo]: valor }));
  };

  return (
    <div className={styles.container}>
      <header className={styles.pageHeader}>
        <div>
          <span className={styles.eyebrow}>AUDITORIA</span>
          <h1>Auditoria de login</h1>
          <p>Entradas, falhas e ações de segurança registradas pelo Identity.</p>
        </div>
      </header>

      <section className={styles.filtersCard} aria-label="Filtros da auditoria de login">
        <div className={styles.filtersGrid}>
          <div className={styles.field}>
            <label htmlFor="login-data-inicial">Data inicial</label>
            <input id="login-data-inicial" type="date" value={filtros.dataInicial} onChange={(event) => atualizarFiltro("dataInicial", event.target.value)} />
          </div>
          <div className={styles.field}>
            <label htmlFor="login-data-final">Data final</label>
            <input id="login-data-final" type="date" min={filtros.dataInicial || undefined} value={filtros.dataFinal} onChange={(event) => atualizarFiltro("dataFinal", event.target.value)} />
          </div>
          <div className={styles.field}>
            <label htmlFor="login-usuario">Usuário</label>
            {podeListarUsuarios ? (
              <select id="login-usuario" value={filtros.usuarioId} onChange={(event) => atualizarFiltro("usuarioId", event.target.value)}>
                <option value="">Todos</option>
                {usuarios.map((usuario) => <option key={usuario.id} value={usuario.id}>{usuario.username}</option>)}
              </select>
            ) : (
              <input id="login-usuario" type="text" placeholder="ID do usuário" value={filtros.usuarioId} onChange={(event) => atualizarFiltro("usuarioId", event.target.value)} />
            )}
          </div>
          <div className={styles.field}>
            <label htmlFor="login-evento">Evento</label>
            <select id="login-evento" value={filtros.evento} onChange={(event) => atualizarFiltro("evento", event.target.value)}>
              <option value="">Todos</option>
              {eventosAuditoriaIdentity.map((evento) => <option key={evento.valor} value={evento.valor}>{evento.texto}</option>)}
            </select>
          </div>
          <div className={styles.field}>
            <label htmlFor="login-resultado">Resultado</label>
            <select id="login-resultado" value={filtros.sucesso} onChange={(event) => atualizarFiltro("sucesso", event.target.value)}>
              <option value="">Todos</option>
              <option value="true">Sucesso</option>
              <option value="false">Falha</option>
            </select>
          </div>
        </div>

        {erro && <div className={styles.errorMessage} role="alert">{erro}</div>}

        <div className={styles.actionsRow}>
          <button type="button" className={styles.clearButton} onClick={limpar}><Trash2 size={17} />Limpar</button>
          <button type="button" className={styles.searchButton} onClick={pesquisar} disabled={carregando}><Search size={17} />{carregando ? "Pesquisando..." : "Pesquisar"}</button>
        </div>
      </section>

      {!pesquisado && !carregando && <div className={styles.emptyState}>Informe o período e clique em Pesquisar.</div>}
      {carregando && <div className={styles.emptyState}>Carregando registros...</div>}
      {pesquisado && !carregando && auditorias.length === 0 && <div className={styles.emptyState}>Nenhum registro encontrado para os filtros informados.</div>}

      {pesquisado && !carregando && auditorias.length > 0 && (
        <section className={styles.tableCard}>
          <div className={styles.resultSummary}>{totalRegistros} registro(s) encontrado(s)</div>
          <div className={styles.tableScroll}>
            <table className={styles.table}>
              <thead><tr><th>Data</th><th>Usuário</th><th>Evento</th><th>Resultado</th><th>Motivo</th><th>IP</th><th></th></tr></thead>
              <tbody>
                {auditorias.map((auditoria) => (
                  <tr key={auditoria.id_AuditoriaIdentity}>
                    <td>{formatarDataHora(auditoria.dataCriacao)}</td>
                    <td>{auditoria.usuarioNome || "-"}</td>
                    <td>{eventoAuditoriaTexto(auditoria.evento)}</td>
                    <td><span className={auditoria.sucesso ? styles.successBadge : styles.failureBadge}>{auditoria.sucesso ? "Sucesso" : "Falha"}</span></td>
                    <td>{auditoria.motivo || "-"}</td>
                    <td>{auditoria.enderecoIp || "-"}</td>
                    <td><button type="button" className={styles.detailButton} onClick={() => setSelecionada(auditoria)}><Eye size={16} />Detalhes</button></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <div className={styles.pagination}>
            <button type="button" disabled={pagina <= 1} onClick={() => executarPesquisa(pagina - 1, filtrosAplicados)}>Anterior</button>
            <span>Página {pagina} de {Math.max(totalPaginas, 1)}</span>
            <button type="button" disabled={pagina >= totalPaginas} onClick={() => executarPesquisa(pagina + 1, filtrosAplicados)}>Próxima</button>
          </div>
        </section>
      )}

      {selecionada && <AuditoriaDetalhesModal auditoria={selecionada} tipo="login" onClose={() => setSelecionada(null)} />}
    </div>
  );
}

