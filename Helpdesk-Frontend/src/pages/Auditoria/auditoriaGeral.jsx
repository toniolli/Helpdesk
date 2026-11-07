import { useEffect, useState } from "react";
import { Eye, Search, Trash2 } from "lucide-react";
import auditoriaService from "../../services/auditoriaService";
import usuarioService from "../../services/usuarioService";
import { apiErrorMessage } from "../../utils/apiError";
import { hasPermission } from "../../utils/permissoesUtils";
import {
  acaoAuditoriaTexto,
  acoesAuditoria,
  formatarDataHora,
  origemAuditoriaTexto,
} from "../../utils/auditoriaUtils";
import AuditoriaDetalhesModal from "./AuditoriaDetalhesModal";
import styles from "./auditoria.module.css";

const filtrosIniciais = {
  dataInicial: "",
  dataFinal: "",
  usuarioId: "",
  acao: "",
  entidade: "",
};

const tamanhoPagina = 20;

export default function AuditoriaGeral() {
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
      const resultado = await auditoriaService.buscarPaginado({
        dataInicial: filtrosConsulta.dataInicial,
        dataFinal: filtrosConsulta.dataFinal,
        usuarioId: filtrosConsulta.usuarioId || undefined,
        acao: filtrosConsulta.acao ? Number(filtrosConsulta.acao) : undefined,
        entidade: filtrosConsulta.entidade.trim() || undefined,
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
      setErro(apiErrorMessage(error, "Não foi possível consultar a auditoria geral."));
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
          <h1>Auditoria geral</h1>
          <p>Alterações registradas automaticamente pelo backend.</p>
        </div>
      </header>

      <section className={styles.filtersCard} aria-label="Filtros da auditoria geral">
        <div className={styles.filtersGrid}>
          <div className={styles.field}>
            <label htmlFor="geral-data-inicial">Data inicial</label>
            <input id="geral-data-inicial" type="date" value={filtros.dataInicial} onChange={(event) => atualizarFiltro("dataInicial", event.target.value)} />
          </div>
          <div className={styles.field}>
            <label htmlFor="geral-data-final">Data final</label>
            <input id="geral-data-final" type="date" min={filtros.dataInicial || undefined} value={filtros.dataFinal} onChange={(event) => atualizarFiltro("dataFinal", event.target.value)} />
          </div>
          <div className={styles.field}>
            <label htmlFor="geral-usuario">Usuário</label>
            {podeListarUsuarios ? (
              <select id="geral-usuario" value={filtros.usuarioId} onChange={(event) => atualizarFiltro("usuarioId", event.target.value)}>
                <option value="">Todos</option>
                {usuarios.map((usuario) => <option key={usuario.id} value={usuario.id}>{usuario.username}</option>)}
              </select>
            ) : (
              <input id="geral-usuario" type="text" placeholder="ID do usuário" value={filtros.usuarioId} onChange={(event) => atualizarFiltro("usuarioId", event.target.value)} />
            )}
          </div>
          <div className={styles.field}>
            <label htmlFor="geral-acao">Ação</label>
            <select id="geral-acao" value={filtros.acao} onChange={(event) => atualizarFiltro("acao", event.target.value)}>
              <option value="">Todas</option>
              {acoesAuditoria.map((acao) => <option key={acao.valor} value={acao.valor}>{acao.texto}</option>)}
            </select>
          </div>
          <div className={styles.field}>
            <label htmlFor="geral-entidade">Entidade</label>
            <input id="geral-entidade" type="text" placeholder="Ex.: Chamado" value={filtros.entidade} onChange={(event) => atualizarFiltro("entidade", event.target.value)} />
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
              <thead><tr><th>Data</th><th>Usuário</th><th>Ação</th><th>Entidade</th><th>Registro</th><th>Origem</th><th></th></tr></thead>
              <tbody>
                {auditorias.map((auditoria) => (
                  <tr key={auditoria.id_Auditoria}>
                    <td>{formatarDataHora(auditoria.dataCriacao)}</td>
                    <td>{auditoria.usuarioNome || "-"}</td>
                    <td><span className={styles.neutralBadge}>{acaoAuditoriaTexto(auditoria.acao)}</span></td>
                    <td>{auditoria.entidade}</td>
                    <td className={styles.identifier}>{auditoria.registroId}</td>
                    <td>{origemAuditoriaTexto(auditoria.origem)}</td>
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

      {selecionada && <AuditoriaDetalhesModal auditoria={selecionada} tipo="geral" onClose={() => setSelecionada(null)} />}
    </div>
  );
}

