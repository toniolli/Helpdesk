import { useEffect, useState } from "react";
import { Eye, Search, Trash2 } from "lucide-react";
import auditoriaChamadoService from "../../services/auditoriaChamadoService";
import categoriaService from "../../services/categoriaService";
import perfilService from "../../services/perfilService";
import usuarioService from "../../services/usuarioService";
import { apiErrorMessage } from "../../utils/apiError";
import {
  eventoAuditoriaChamadoTexto,
  eventosAuditoriaChamado,
  formatarDataHora,
} from "../../utils/auditoriaUtils";
import { hasPermission } from "../../utils/permissoesUtils";
import { getStatusChamado } from "../../utils/statusChamadoUtils";
import AuditoriaDetalhesModal from "./AuditoriaDetalhesModal";
import styles from "./auditoria.module.css";

const filtrosIniciais = {
  dataInicial: "",
  dataFinal: "",
  numeroChamado: "",
  usuarioId: "",
  perfilUsuario: "",
  evento: "",
  status: "",
  categoriaAtendimentoId: "",
};

const statusChamado = [
  { valor: 1, texto: "Aberto" },
  { valor: 2, texto: "Em atendimento" },
  { valor: 3, texto: "Fechado" },
  { valor: 4, texto: "Cancelado" },
  { valor: 5, texto: "Aguardando retorno" },
  { valor: 6, texto: "Distribuído" },
  { valor: 7, texto: "Aguardando equipamento" },
];

const tamanhoPagina = 20;

export default function AuditoriaChamados() {
  const [filtros, setFiltros] = useState(filtrosIniciais);
  const [filtrosAplicados, setFiltrosAplicados] = useState(null);
  const [auditorias, setAuditorias] = useState([]);
  const [usuarios, setUsuarios] = useState([]);
  const [perfis, setPerfis] = useState([]);
  const [categorias, setCategorias] = useState([]);
  const [pagina, setPagina] = useState(1);
  const [totalPaginas, setTotalPaginas] = useState(0);
  const [totalRegistros, setTotalRegistros] = useState(0);
  const [pesquisado, setPesquisado] = useState(false);
  const [carregando, setCarregando] = useState(false);
  const [erro, setErro] = useState("");
  const [selecionada, setSelecionada] = useState(null);

  const podeListarUsuarios = hasPermission("GERENCIAR_USUARIOS");
  const podeListarPerfis = hasPermission("GERENCIAR_PERFIS");
  const podeListarCategorias =
    hasPermission("VISUALIZAR_CATEGORIAS") ||
    hasPermission("GERENCIAR_CATEGORIAS");

  useEffect(() => {
    if (podeListarUsuarios) {
      usuarioService.getAll().then(setUsuarios).catch(() => setUsuarios([]));
    }

    if (podeListarPerfis) {
      perfilService.getAll().then(setPerfis).catch(() => setPerfis([]));
    }

    if (podeListarCategorias) {
      categoriaService
        .getAll()
        .then(setCategorias)
        .catch(() => setCategorias([]));
    }
  }, [podeListarCategorias, podeListarPerfis, podeListarUsuarios]);

  const executarPesquisa = async (paginaAtual, filtrosConsulta) => {
    setCarregando(true);
    setErro("");

    try {
      const resultado = await auditoriaChamadoService.buscarPaginado({
        dataInicial: filtrosConsulta.dataInicial,
        dataFinal: filtrosConsulta.dataFinal,
        numeroChamado: filtrosConsulta.numeroChamado.trim() || undefined,
        usuarioId: filtrosConsulta.usuarioId || undefined,
        perfilUsuario: filtrosConsulta.perfilUsuario || undefined,
        evento: filtrosConsulta.evento
          ? Number(filtrosConsulta.evento)
          : undefined,
        status: filtrosConsulta.status
          ? Number(filtrosConsulta.status)
          : undefined,
        categoriaAtendimentoId: filtrosConsulta.categoriaAtendimentoId
          ? Number(filtrosConsulta.categoriaAtendimentoId)
          : undefined,
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
      setErro(
        apiErrorMessage(
          error,
          "Não foi possível consultar a auditoria de chamados.",
        ),
      );
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
          <h1>Auditoria de chamados</h1>
          <p>Histórico das ações realizadas durante o atendimento.</p>
        </div>
      </header>

      <section
        className={styles.filtersCard}
        aria-label="Filtros da auditoria de chamados"
      >
        <div className={styles.filtersGrid}>
          <div className={styles.field}>
            <label htmlFor="chamado-data-inicial">Data inicial</label>
            <input
              id="chamado-data-inicial"
              type="date"
              value={filtros.dataInicial}
              onChange={(event) =>
                atualizarFiltro("dataInicial", event.target.value)
              }
            />
          </div>

          <div className={styles.field}>
            <label htmlFor="chamado-data-final">Data final</label>
            <input
              id="chamado-data-final"
              type="date"
              min={filtros.dataInicial || undefined}
              value={filtros.dataFinal}
              onChange={(event) =>
                atualizarFiltro("dataFinal", event.target.value)
              }
            />
          </div>

          <div className={styles.field}>
            <label htmlFor="chamado-numero">Número do chamado</label>
            <input
              id="chamado-numero"
              type="text"
              placeholder="Ex.: CH-2026-000001"
              value={filtros.numeroChamado}
              onChange={(event) =>
                atualizarFiltro("numeroChamado", event.target.value)
              }
            />
          </div>

          <div className={styles.field}>
            <label htmlFor="chamado-usuario">Usuário</label>
            {podeListarUsuarios ? (
              <select
                id="chamado-usuario"
                value={filtros.usuarioId}
                onChange={(event) =>
                  atualizarFiltro("usuarioId", event.target.value)
                }
              >
                <option value="">Todos</option>
                {usuarios.map((usuario) => (
                  <option key={usuario.id} value={usuario.id}>
                    {usuario.username}
                  </option>
                ))}
              </select>
            ) : (
              <input
                id="chamado-usuario"
                type="text"
                placeholder="ID do usuário"
                value={filtros.usuarioId}
                onChange={(event) =>
                  atualizarFiltro("usuarioId", event.target.value)
                }
              />
            )}
          </div>

          <div className={styles.field}>
            <label htmlFor="chamado-perfil">Perfil</label>
            {podeListarPerfis ? (
              <select
                id="chamado-perfil"
                value={filtros.perfilUsuario}
                onChange={(event) =>
                  atualizarFiltro("perfilUsuario", event.target.value)
                }
              >
                <option value="">Todos</option>
                {perfis.map((perfil) => (
                  <option key={perfil.id_Perfil} value={perfil.nome}>
                    {perfil.nome}
                  </option>
                ))}
              </select>
            ) : (
              <input
                id="chamado-perfil"
                type="text"
                placeholder="Nome do perfil"
                value={filtros.perfilUsuario}
                onChange={(event) =>
                  atualizarFiltro("perfilUsuario", event.target.value)
                }
              />
            )}
          </div>

          <div className={styles.field}>
            <label htmlFor="chamado-evento">Evento</label>
            <select
              id="chamado-evento"
              value={filtros.evento}
              onChange={(event) =>
                atualizarFiltro("evento", event.target.value)
              }
            >
              <option value="">Todos</option>
              {eventosAuditoriaChamado.map((evento) => (
                <option key={evento.valor} value={evento.valor}>
                  {evento.texto}
                </option>
              ))}
            </select>
          </div>

          <div className={styles.field}>
            <label htmlFor="chamado-status">Status final</label>
            <select
              id="chamado-status"
              value={filtros.status}
              onChange={(event) =>
                atualizarFiltro("status", event.target.value)
              }
            >
              <option value="">Todos</option>
              {statusChamado.map((status) => (
                <option key={status.valor} value={status.valor}>
                  {status.texto}
                </option>
              ))}
            </select>
          </div>

          <div className={styles.field}>
            <label htmlFor="chamado-categoria">Categoria</label>
            {podeListarCategorias ? (
              <select
                id="chamado-categoria"
                value={filtros.categoriaAtendimentoId}
                onChange={(event) =>
                  atualizarFiltro(
                    "categoriaAtendimentoId",
                    event.target.value,
                  )
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
            ) : (
              <input
                id="chamado-categoria"
                type="number"
                min="1"
                placeholder="ID da categoria"
                value={filtros.categoriaAtendimentoId}
                onChange={(event) =>
                  atualizarFiltro(
                    "categoriaAtendimentoId",
                    event.target.value,
                  )
                }
              />
            )}
          </div>
        </div>

        {erro && (
          <div className={styles.errorMessage} role="alert">
            {erro}
          </div>
        )}

        <div className={styles.actionsRow}>
          <button
            type="button"
            className={styles.clearButton}
            onClick={limpar}
          >
            <Trash2 size={17} />
            Limpar
          </button>
          <button
            type="button"
            className={styles.searchButton}
            onClick={pesquisar}
            disabled={carregando}
          >
            <Search size={17} />
            {carregando ? "Pesquisando..." : "Pesquisar"}
          </button>
        </div>
      </section>

      {!pesquisado && !carregando && (
        <div className={styles.emptyState}>
          Informe o período e clique em Pesquisar.
        </div>
      )}
      {carregando && (
        <div className={styles.emptyState}>Carregando registros...</div>
      )}
      {pesquisado && !carregando && auditorias.length === 0 && (
        <div className={styles.emptyState}>
          Nenhum registro encontrado para os filtros informados.
        </div>
      )}

      {pesquisado && !carregando && auditorias.length > 0 && (
        <section className={styles.tableCard}>
          <div className={styles.resultSummary}>
            {totalRegistros} registro(s) encontrado(s)
          </div>
          <div className={styles.tableScroll}>
            <table className={styles.table}>
              <thead>
                <tr>
                  <th>Data</th>
                  <th>Chamado</th>
                  <th>Usuário</th>
                  <th>Perfil</th>
                  <th>Evento</th>
                  <th>Status</th>
                  <th>Descrição</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {auditorias.map((auditoria) => (
                  <tr key={auditoria.id_AuditoriaChamado}>
                    <td>{formatarDataHora(auditoria.dataEvento)}</td>
                    <td className={styles.identifier}>
                      {auditoria.numeroChamado}
                    </td>
                    <td>{auditoria.usuarioNome || "-"}</td>
                    <td>{auditoria.perfilUsuario || "-"}</td>
                    <td>
                      <span className={styles.neutralBadge}>
                        {eventoAuditoriaChamadoTexto(auditoria.evento)}
                      </span>
                    </td>
                    <td>
                      {getStatusChamado(auditoria.statusAnterior)} →{" "}
                      {getStatusChamado(auditoria.statusNovo)}
                    </td>
                    <td>{auditoria.descricao}</td>
                    <td>
                      <button
                        type="button"
                        className={styles.detailButton}
                        onClick={() => setSelecionada(auditoria)}
                      >
                        <Eye size={16} />
                        Detalhes
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <div className={styles.pagination}>
            <button
              type="button"
              disabled={pagina <= 1}
              onClick={() => executarPesquisa(pagina - 1, filtrosAplicados)}
            >
              Anterior
            </button>
            <span>Página {pagina} de {Math.max(totalPaginas, 1)}</span>
            <button
              type="button"
              disabled={pagina >= totalPaginas}
              onClick={() => executarPesquisa(pagina + 1, filtrosAplicados)}
            >
              Próxima
            </button>
          </div>
        </section>
      )}

      {selecionada && (
        <AuditoriaDetalhesModal
          auditoria={selecionada}
          tipo="chamado"
          onClose={() => setSelecionada(null)}
        />
      )}
    </div>
  );
}
