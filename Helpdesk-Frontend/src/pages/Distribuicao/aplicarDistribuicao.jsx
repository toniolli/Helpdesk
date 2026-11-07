import { useEffect, useMemo, useState } from "react";
import {
  Check,
  CheckCircle2,
  ListFilter,
  ListChecks,
  Pencil,
  Plus,
  Power,
  RefreshCw,
  Search,
  X,
} from "lucide-react";
import categoriaService from "../../services/categoriaService";
import criterioDistribuicaoService from "../../services/criterioDistribuicaoService";
import perfilService from "../../services/perfilService";
import regraDistribuicaoService from "../../services/regraDistribuicaoService";
import usuarioService from "../../services/usuarioService";
import { apiErrorMessage } from "../../utils/apiError";
import styles from "./aplicarDistribuicao.module.css";

const LIMITE_CHAMADOS = 10;

const regraId = (regra) =>
  regra.id_Regras ?? regra.id_RegraDistribuicao ?? regra.id;
const chamadoId = (chamado) => chamado.id_Chamado ?? chamado.id;
const categoriaId = (categoria) =>
  categoria.id_CategoriaAtendimento ?? categoria.id;
const criterioId = (criterio) => criterio.id_Criterio ?? criterio.id;
const criterioRegraId = (criterio) =>
  criterio.regraDistribuicaoId ?? criterio.id_RegraDistribuicao;
const criterioCategoriaId = (criterio) =>
  criterio.categoriaAtendimentoId ?? criterio.id_CategoriaAtendimento;
const usuarioId = (usuario) => usuario.id_Usuario ?? usuario.id;
const perfilId = (perfil) => perfil.id_Perfil ?? perfil.id;

const regraEstaVigente = (regra) => {
  if (regra.periodoIndefinido) return true;

  const inicio = regra.dataInicial ? new Date(regra.dataInicial) : null;
  const fim = regra.dataFinal ? new Date(regra.dataFinal) : null;
  const agora = new Date();

  return (
    inicio &&
    fim &&
    !Number.isNaN(inicio.getTime()) &&
    !Number.isNaN(fim.getTime()) &&
    agora >= inicio &&
    agora <= fim
  );
};

const formatarData = (valor) => {
  const data = String(valor ?? "").slice(0, 10);
  if (!data) return "-";
  const [ano, mes, dia] = data.split("-");
  return ano && mes && dia ? `${dia}/${mes}/${ano}` : "-";
};

const estadoOperacional = (regra) => {
  if (!regra.ativo) return { texto: "Inativa", classe: styles.inactiveStatus };
  if (regra.periodoIndefinido) {
    return { texto: "Em vigor", classe: styles.activeStatus };
  }

  const agora = new Date();
  const inicio = regra.dataInicial ? new Date(regra.dataInicial) : null;
  const fim = regra.dataFinal ? new Date(regra.dataFinal) : null;
  if (inicio && !Number.isNaN(inicio.getTime()) && agora < inicio) {
    return { texto: "Agendada", classe: styles.scheduledStatus };
  }
  if (fim && !Number.isNaN(fim.getTime()) && agora > fim) {
    return { texto: "Expirada", classe: styles.expiredStatus };
  }
  return { texto: "Em vigor", classe: styles.activeStatus };
};

const filtrarRegrasAplicaveis = (regras) =>
  Array.isArray(regras)
    ? regras.filter(
        (regra) =>
          regra.ativo &&
          Number(regra.tipoRegra) === 2 &&
          regraEstaVigente(regra),
      )
    : [];

const formInicial = {
  nome: "",
  tipoDestino: 1,
  usuarioDestinoId: "",
  perfilDestinoId: "",
  periodoIndefinido: true,
  dataInicial: "",
  dataFinal: "",
  categorias: [],
};

const prioridadeClasse = (prioridade) => {
  if (Number(prioridade) === 4) return styles.critical;
  if (Number(prioridade) === 3) return styles.high;
  if (Number(prioridade) === 2) return styles.medium;
  return styles.low;
};

export default function AplicarDistribuicao() {
  const [regras, setRegras] = useState([]);
  const [regraSelecionada, setRegraSelecionada] = useState("");
  const [chamados, setChamados] = useState([]);
  const [selecionados, setSelecionados] = useState([]);
  const [filtro, setFiltro] = useState("");
  const [carregandoRegras, setCarregandoRegras] = useState(true);
  const [carregandoChamados, setCarregandoChamados] = useState(false);
  const [consultaRealizada, setConsultaRealizada] = useState(false);
  const [aplicando, setAplicando] = useState(false);
  const [erro, setErro] = useState("");
  const [sucesso, setSucesso] = useState("");
  const [modalAberto, setModalAberto] = useState(false);
  const [modalGestaoAberto, setModalGestaoAberto] = useState(false);
  const [regraEmEdicao, setRegraEmEdicao] = useState(null);
  const [form, setForm] = useState(formInicial);
  const [criterios, setCriterios] = useState([]);
  const [categorias, setCategorias] = useState([]);
  const [usuarios, setUsuarios] = useState([]);
  const [perfis, setPerfis] = useState([]);
  const [salvando, setSalvando] = useState(false);

  useEffect(() => {
    const carregarRegras = async () => {
      try {
        const [dados, criteriosData] = await Promise.all([
          regraDistribuicaoService.getAll(),
          criterioDistribuicaoService.getAll(),
        ]);
        setRegras(
          Array.isArray(dados)
            ? dados.filter((regra) => Number(regra.tipoRegra) === 2)
            : [],
        );
        setCriterios(Array.isArray(criteriosData) ? criteriosData : []);
      } catch (error) {
        console.error("Erro ao carregar regras de distribuição:", error);
        setErro(
          apiErrorMessage(
            error,
            "Não foi possível carregar as regras disponíveis.",
          ),
        );
      } finally {
        setCarregandoRegras(false);
      }
    };

    carregarRegras();
  }, []);

  const carregarDadosFormulario = async () => {
    try {
      const [categoriasData, usuariosData, perfisData] = await Promise.all([
        categoriaService.getAll(),
        usuarioService.buscarUsuariosDistribuicao(),
        perfilService.getAll(),
      ]);
      setCategorias(Array.isArray(categoriasData) ? categoriasData : []);
      setUsuarios(Array.isArray(usuariosData) ? usuariosData : []);
      setPerfis(Array.isArray(perfisData) ? perfisData : []);
    } catch (error) {
      console.error("Erro ao carregar dados do formulário:", error);
      setErro(
        apiErrorMessage(
          error,
          "Não foi possível carregar todos os campos para criar a regra.",
        ),
      );
    }
  };

  const abrirNovaRegra = () => {
    setForm(formInicial);
    setRegraEmEdicao(null);
    setErro("");
    setSucesso("");
    setModalAberto(true);
    carregarDadosFormulario();
  };

  const fecharModal = () => {
    setModalAberto(false);
    setForm(formInicial);
    setRegraEmEdicao(null);
    setErro("");
  };

  const alternarCategoria = (id) => {
    setForm((atual) => ({
      ...atual,
      categorias: atual.categorias.includes(id)
        ? atual.categorias.filter((categoria) => categoria !== id)
        : [...atual.categorias, id],
    }));
  };

  const criteriosPorRegra = useMemo(() => {
    const mapa = new Map();
    criterios.forEach((criterio) => {
      const id = criterioRegraId(criterio);
      mapa.set(id, [...(mapa.get(id) ?? []), criterio]);
    });
    return mapa;
  }, [criterios]);

  const regrasAplicaveis = useMemo(
    () => filtrarRegrasAplicaveis(regras),
    [regras],
  );

  const recarregarRegras = async () => {
    const [dados, criteriosData] = await Promise.all([
      regraDistribuicaoService.getAll(),
      criterioDistribuicaoService.getAll(),
    ]);
    setRegras(
      Array.isArray(dados)
        ? dados.filter((regra) => Number(regra.tipoRegra) === 2)
        : [],
    );
    setCriterios(Array.isArray(criteriosData) ? criteriosData : []);
  };

  const abrirGestao = async () => {
    setErro("");
    setSucesso("");
    setModalGestaoAberto(true);
    try {
      await Promise.all([recarregarRegras(), carregarDadosFormulario()]);
    } catch (error) {
      console.error("Erro ao carregar regras aplicadas:", error);
      setErro(
        apiErrorMessage(
          error,
          "Não foi possível carregar as regras aplicadas.",
        ),
      );
    }
  };

  const abrirEdicao = async (regra) => {
    const id = regraId(regra);
    setRegraEmEdicao(regra);
    setForm({
      nome: regra.nome ?? "",
      tipoDestino: Number(regra.tipoDestino ?? 1),
      usuarioDestinoId: regra.usuarioDestinoId ?? "",
      perfilDestinoId: regra.perfilDestinoId ?? "",
      periodoIndefinido: regra.periodoIndefinido ?? true,
      dataInicial: String(regra.dataInicial ?? "").slice(0, 10),
      dataFinal: String(regra.dataFinal ?? "").slice(0, 10),
      categorias: (criteriosPorRegra.get(id) ?? []).map(criterioCategoriaId),
    });
    setModalGestaoAberto(false);
    setModalAberto(true);
    await carregarDadosFormulario();
  };

  const alterarStatus = async (regra) => {
    try {
      setErro("");
      setSucesso("");
      const id = regraId(regra);
      if (regra.ativo) await regraDistribuicaoService.desativar(id);
      else await regraDistribuicaoService.ativar(id);
      await recarregarRegras();
      if (String(regraSelecionada) === String(id) && regra.ativo) {
        setRegraSelecionada("");
        setChamados([]);
        setSelecionados([]);
      }
      setSucesso(
        regra.ativo ? "Regra aplicada desativada." : "Regra aplicada ativada.",
      );
    } catch (error) {
      console.error("Erro ao alterar status da regra aplicada:", error);
      setErro(
        apiErrorMessage(error, "Não foi possível alterar o status da regra."),
      );
    }
  };

  const salvarRegra = async (event) => {
    event.preventDefault();
    if (!form.categorias.length) {
      setErro("Selecione ao menos uma categoria para a regra.");
      return;
    }

    setSalvando(true);
    setErro("");
    setSucesso("");
    try {
      const payload = {
        nome: form.nome.trim(),
        tipoRegra: 2,
        tipoDestino: Number(form.tipoDestino),
        ativo: regraEmEdicao?.ativo ?? true,
        periodoIndefinido: form.periodoIndefinido,
        dataInicial: form.periodoIndefinido
          ? null
          : `${form.dataInicial}T00:00:00`,
        dataFinal: form.periodoIndefinido ? null : `${form.dataFinal}T23:59:59`,
        usuarioDestinoId: form.tipoDestino === 1 ? form.usuarioDestinoId : null,
        perfilDestinoId:
          form.tipoDestino === 2 ? Number(form.perfilDestinoId) : null,
      };

      let id = regraEmEdicao ? regraId(regraEmEdicao) : null;
      if (id) {
        await regraDistribuicaoService.update(id, payload);
      } else {
        const regraCriada = await regraDistribuicaoService.create(payload);
        id = regraId(regraCriada);
      }
      if (!id) {
        const regrasAtualizadas = await regraDistribuicaoService.getAll();
        id = regraId(
          [...(regrasAtualizadas ?? [])]
            .reverse()
            .find(
              (regra) =>
                regra.nome === form.nome.trim() &&
                Number(regra.tipoRegra) === 2,
            ),
        );
      }
      if (!id) throw new Error("A regra foi criada sem identificador.");

      const criteriosAtuais = criteriosPorRegra.get(id) ?? [];
      const idsAtuais = criteriosAtuais.map(criterioCategoriaId);
      await Promise.all([
        ...form.categorias
          .filter(
            (categoriaAtendimentoId) =>
              !idsAtuais.includes(categoriaAtendimentoId),
          )
          .map((categoriaAtendimentoId) =>
            criterioDistribuicaoService.create({
              regraDistribuicaoId: id,
              categoriaAtendimentoId,
            }),
          ),
        ...criteriosAtuais
          .filter(
            (criterio) =>
              !form.categorias.includes(criterioCategoriaId(criterio)),
          )
          .map((criterio) =>
            criterioDistribuicaoService.remover(criterioId(criterio)),
          ),
      ]);

      await recarregarRegras();
      const regrasAtualizadas = await regraDistribuicaoService.getAll();
      const regrasAplicaveisAtualizadas =
        filtrarRegrasAplicaveis(regrasAtualizadas);
      const regraDisponivel = regrasAplicaveisAtualizadas.some(
        (regra) => String(regraId(regra)) === String(id),
      );

      setRegraSelecionada(regraDisponivel ? String(id) : "");
      setChamados([]);
      setSelecionados([]);
      setConsultaRealizada(false);
      setModalAberto(false);
      setSucesso(
        regraDisponivel
          ? `Regra aplicada ${regraEmEdicao ? "atualizada" : "criada"} e selecionada. Agora busque os chamados compatíveis.`
          : `Regra aplicada ${regraEmEdicao ? "atualizada" : "criada"}. Ela ficará disponível durante o período de vigência configurado.`,
      );
    } catch (error) {
      console.error("Erro ao criar regra aplicada:", error);
      setErro(
        apiErrorMessage(
          error,
          "Não foi possível criar a regra aplicada. Revise os dados e tente novamente.",
        ),
      );
    } finally {
      setSalvando(false);
    }
  };

  const buscarChamados = async () => {
    if (!regraSelecionada) return;

    setCarregandoChamados(true);
    setErro("");
    setSucesso("");
    setSelecionados([]);
    setConsultaRealizada(false);
    try {
      const dados =
        await regraDistribuicaoService.buscarChamadosDisponiveis(
          regraSelecionada,
        );
      setChamados(Array.isArray(dados) ? dados : []);
      setConsultaRealizada(true);
    } catch (error) {
      console.error("Erro ao buscar chamados compatíveis:", error);
      setChamados([]);
      setErro(
        apiErrorMessage(
          error,
          "Não foi possível buscar os chamados compatíveis com a regra.",
        ),
      );
    } finally {
      setCarregandoChamados(false);
    }
  };

  const chamadosFiltrados = useMemo(() => {
    const termo = filtro.trim().toLowerCase();
    if (!termo) return chamados;

    return chamados.filter((chamado) =>
      [
        chamado.numero,
        chamado.equipamentoAfetado,
        chamado.solicitanteNome,
        chamado.prioridadeNome,
      ]
        .filter(Boolean)
        .some((valor) => String(valor).toLowerCase().includes(termo)),
    );
  }, [chamados, filtro]);

  const alternarChamado = (id) => {
    setSelecionados((atual) => {
      if (atual.includes(id)) return atual.filter((item) => item !== id);
      if (atual.length >= LIMITE_CHAMADOS) return atual;
      return [...atual, id];
    });
  };

  const alternarTodos = () => {
    const idsVisiveis = chamadosFiltrados.map(chamadoId);

    setSelecionados((atual) => {
      const foraDoFiltro = atual.filter((id) => !idsVisiveis.includes(id));
      const selecionadosVisiveis = atual.filter((id) =>
        idsVisiveis.includes(id),
      );
      const limiteVisivel = Math.min(
        idsVisiveis.length,
        LIMITE_CHAMADOS - foraDoFiltro.length,
      );

      if (limiteVisivel > 0 && selecionadosVisiveis.length === limiteVisivel) {
        return foraDoFiltro;
      }

      return [...foraDoFiltro, ...idsVisiveis.slice(0, limiteVisivel)];
    });
  };

  const aplicarRegra = async () => {
    if (!regraSelecionada || !selecionados.length) return;

    setAplicando(true);
    setErro("");
    setSucesso("");

    try {
      const quantidade = selecionados.length;
      await regraDistribuicaoService.aplicarChamados(
        regraSelecionada,
        selecionados,
      );

      const dados =
        await regraDistribuicaoService.buscarChamadosDisponiveis(
          regraSelecionada,
        );
      setChamados(Array.isArray(dados) ? dados : []);
      setSelecionados([]);
      setConsultaRealizada(true);
      setSucesso(`Regra aplicada com sucesso em ${quantidade} chamado(s).`);
    } catch (error) {
      console.error("Erro ao aplicar regra aos chamados:", error);
      setErro(
        apiErrorMessage(
          error,
          "Não foi possível aplicar a regra aos chamados selecionados.",
        ),
      );
    } finally {
      setAplicando(false);
    }
  };

  const regraAtual = regras.find(
    (regra) => String(regraId(regra)) === String(regraSelecionada),
  );
  const idsVisiveis = chamadosFiltrados.map(chamadoId);
  const selecionadosForaDoFiltro = selecionados.filter(
    (id) => !idsVisiveis.includes(id),
  ).length;
  const limiteSelecionavelVisivel = Math.min(
    idsVisiveis.length,
    LIMITE_CHAMADOS - selecionadosForaDoFiltro,
  );
  const selecionadosVisiveis = idsVisiveis.filter((id) =>
    selecionados.includes(id),
  ).length;
  const todosVisiveisSelecionados =
    limiteSelecionavelVisivel > 0 &&
    selecionadosVisiveis === limiteSelecionavelVisivel;

  return (
    <div className={styles.container}>
      <header className={styles.header}>
        <div>
          <h1>Aplicar regras em chamados</h1>
          <p>
            Encontre os chamados compatíveis e selecione quais devem receber uma
            regra de distribuição já criada.
          </p>
        </div>
        <div className={styles.headerActions}>
          <button
            type="button"
            className={styles.newRuleButton}
            onClick={abrirNovaRegra}
          >
            <Plus size={17} /> Criar regra aplicada
          </button>
          <button
            type="button"
            className={styles.manageRulesButton}
            onClick={abrirGestao}
          >
            <ListChecks size={16} /> Ver regras
          </button>
        </div>
      </header>

      <section className={styles.selectionPanel} aria-label="Seleção da regra">
        <div className={styles.stepNumber}>1</div>
        <div className={styles.selectionContent}>
          <label htmlFor="regra">Escolha uma regra ativa</label>
          <select
            id="regra"
            value={regraSelecionada}
            onChange={(event) => {
              setRegraSelecionada(event.target.value);
              setChamados([]);
              setSelecionados([]);
              setFiltro("");
              setConsultaRealizada(false);
              setErro("");
              setSucesso("");
            }}
            disabled={carregandoRegras}
          >
            <option value="">
              {carregandoRegras
                ? "Carregando regras..."
                : regrasAplicaveis.length
                  ? "Selecione uma regra"
                  : "Nenhuma regra aplicada vigente"}
            </option>
            {regrasAplicaveis.map((regra) => (
              <option key={regraId(regra)} value={regraId(regra)}>
                {regra.nome}
              </option>
            ))}
          </select>
        </div>
        <button
          type="button"
          className={styles.searchButton}
          onClick={buscarChamados}
          disabled={!regraSelecionada || carregandoChamados}
        >
          {carregandoChamados ? (
            <RefreshCw className={styles.spinning} size={17} />
          ) : (
            <Search size={17} />
          )}
          Buscar chamados
        </button>
      </section>

      {erro && <div className={styles.feedback}>{erro}</div>}
      {sucesso && (
        <div className={styles.successFeedback} role="status">
          <CheckCircle2 size={17} /> {sucesso}
        </div>
      )}

      <section className={styles.results}>
        <div className={styles.resultsHeader}>
          <div>
            <span className={styles.sectionEyebrow}>
              2. Selecione os chamados
            </span>
            <h2>Chamados compatíveis</h2>
            <p>
              {regraAtual
                ? `Resultados compatíveis com a regra “${regraAtual.nome}”.`
                : "Escolha uma regra para consultar os chamados compatíveis."}
            </p>
          </div>
          {chamados.length > 0 && (
            <label className={styles.filter}>
              <Search size={16} />
              <input
                value={filtro}
                onChange={(event) => setFiltro(event.target.value)}
                placeholder="Filtrar chamados"
              />
            </label>
          )}
        </div>

        {chamados.length > 0 ? (
          <>
            <div className={styles.tableWrap}>
              <table className={styles.table}>
                <thead>
                  <tr>
                    <th className={styles.checkColumn}>
                      <input
                        type="checkbox"
                        aria-label="Selecionar o máximo permitido entre os chamados visíveis"
                        checked={todosVisiveisSelecionados}
                        onChange={alternarTodos}
                      />
                    </th>
                    <th>Chamado</th>
                    <th>Solicitante</th>
                    <th>Equipamento</th>
                    <th>Prioridade</th>
                    <th>Aberto em</th>
                  </tr>
                </thead>
                <tbody>
                  {chamadosFiltrados.map((chamado) => {
                    const id = chamadoId(chamado);
                    return (
                      <tr
                        key={id}
                        className={
                          selecionados.includes(id) ? styles.selectedRow : ""
                        }
                      >
                        <td className={styles.checkColumn}>
                          <input
                            type="checkbox"
                            checked={selecionados.includes(id)}
                            onChange={() => alternarChamado(id)}
                            disabled={
                              selecionados.length >= LIMITE_CHAMADOS &&
                              !selecionados.includes(id)
                            }
                            aria-label={`Selecionar chamado ${chamado.numero ?? id}`}
                          />
                        </td>
                        <td>
                          <strong>{chamado.numero ?? `#${id}`}</strong>
                          <span className={styles.description}>
                            {chamado.descricao ?? "Sem descrição"}
                          </span>
                        </td>
                        <td>{chamado.solicitanteNome ?? "Não informado"}</td>
                        <td>{chamado.equipamentoAfetado ?? "Não informado"}</td>
                        <td>
                          <span
                            className={`${styles.priority} ${prioridadeClasse(chamado.prioridade)}`}
                          >
                            {chamado.prioridadeNome ?? "Não definida"}
                          </span>
                        </td>
                        <td>
                          {chamado.dataAbertura
                            ? new Date(chamado.dataAbertura).toLocaleDateString(
                                "pt-BR",
                              )
                            : "-"}
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
            {!chamadosFiltrados.length && (
              <div className={styles.empty}>
                Nenhum chamado corresponde ao filtro.
              </div>
            )}
          </>
        ) : (
          <div className={styles.emptyState}>
            <ListFilter size={26} />
            <h3>
              {consultaRealizada
                ? "Nenhum chamado disponível"
                : regraSelecionada
                  ? "Pronto para consultar"
                  : "Selecione uma regra"}
            </h3>
            <p>
              {consultaRealizada
                ? "Não há chamados abertos e sem responsável nas categorias desta regra."
                : regraSelecionada
                  ? "Use “Buscar chamados” para listar somente os chamados que atendem aos critérios da regra."
                  : "A busca considera as categorias configuradas na regra escolhida."}
            </p>
          </div>
        )}
      </section>

      <footer className={styles.actionBar}>
        <div>
          <span className={styles.stepNumber}>3</span>
          <span>
            {selecionados.length
              ? `${selecionados.length} de ${LIMITE_CHAMADOS} chamado(s) selecionado(s)`
              : `Selecione até ${LIMITE_CHAMADOS} chamados`}
          </span>
        </div>
        <button
          type="button"
          onClick={aplicarRegra}
          disabled={!selecionados.length || aplicando}
        >
          {aplicando ? (
            <RefreshCw className={styles.spinning} size={17} />
          ) : (
            <Check size={17} />
          )}
          {aplicando ? "Aplicando..." : "Aplicar regra selecionada"}
        </button>
      </footer>

      {modalAberto && (
        <div
          className={styles.overlay}
          role="dialog"
          aria-modal="true"
          aria-labelledby="nova-regra-aplicada"
        >
          <div className={styles.modal}>
            <div className={styles.modalHeader}>
              <div>
                <h2 id="nova-regra-aplicada">
                  {regraEmEdicao
                    ? "Editar regra aplicada"
                    : "Criar regra aplicada"}
                </h2>
                <p>
                  Defina os critérios e o destino para usar em chamados já
                  existentes.
                </p>
              </div>
              <button type="button" onClick={fecharModal} aria-label="Fechar">
                <X size={20} />
              </button>
            </div>
            <form onSubmit={salvarRegra}>
              {erro && <div className={styles.modalFeedback}>{erro}</div>}
              <div className={styles.field}>
                <label htmlFor="nome-regra-aplicada">Nome da regra</label>
                <input
                  id="nome-regra-aplicada"
                  value={form.nome}
                  onChange={(event) =>
                    setForm({ ...form, nome: event.target.value })
                  }
                  required
                />
              </div>
              <div className={styles.twoColumns}>
                <div className={styles.field}>
                  <label>Destino</label>
                  <select
                    value={form.tipoDestino}
                    onChange={(event) =>
                      setForm({
                        ...form,
                        tipoDestino: Number(event.target.value),
                        usuarioDestinoId: "",
                        perfilDestinoId: "",
                      })
                    }
                  >
                    <option value={1}>Usuário</option>
                    <option value={2}>Perfil</option>
                  </select>
                </div>
                <div className={styles.field}>
                  <label>
                    {form.tipoDestino === 1
                      ? "Usuário responsável"
                      : "Perfil responsável"}
                  </label>
                  <select
                    value={
                      form.tipoDestino === 1
                        ? form.usuarioDestinoId
                        : form.perfilDestinoId
                    }
                    onChange={(event) =>
                      setForm({
                        ...form,
                        [form.tipoDestino === 1
                          ? "usuarioDestinoId"
                          : "perfilDestinoId"]: event.target.value,
                      })
                    }
                    required
                  >
                    <option value="">Selecione</option>
                    {form.tipoDestino === 1
                      ? usuarios
                          .filter((usuario) => usuario.ativo !== false)
                          .map((usuario) => (
                            <option
                              key={usuarioId(usuario)}
                              value={usuarioId(usuario)}
                            >
                              {usuario.username ?? usuario.nome}
                            </option>
                          ))
                      : perfis
                          .filter((perfil) => perfil.ativo !== false)
                          .map((perfil) => (
                            <option
                              key={perfilId(perfil)}
                              value={perfilId(perfil)}
                            >
                              {perfil.nome}
                            </option>
                          ))}
                  </select>
                </div>
              </div>
              <label className={styles.checkLabel}>
                <input
                  type="checkbox"
                  checked={form.periodoIndefinido}
                  onChange={(event) =>
                    setForm({
                      ...form,
                      periodoIndefinido: event.target.checked,
                    })
                  }
                />{" "}
                Vigência indeterminada
              </label>
              {!form.periodoIndefinido && (
                <div className={styles.twoColumns}>
                  <div className={styles.field}>
                    <label>Data inicial</label>
                    <input
                      type="date"
                      value={form.dataInicial}
                      onChange={(event) =>
                        setForm({ ...form, dataInicial: event.target.value })
                      }
                      required
                    />
                  </div>
                  <div className={styles.field}>
                    <label>Data final</label>
                    <input
                      type="date"
                      min={form.dataInicial}
                      value={form.dataFinal}
                      onChange={(event) =>
                        setForm({ ...form, dataFinal: event.target.value })
                      }
                      required
                    />
                  </div>
                </div>
              )}
              <div className={styles.field}>
                <label>Categorias atendidas</label>
                <div className={styles.categoryPicker}>
                  {categorias
                    .filter((categoria) => categoria.ativo !== false)
                    .map((categoria) => {
                      const id = categoriaId(categoria);
                      return (
                        <label key={id}>
                          <input
                            type="checkbox"
                            checked={form.categorias.includes(id)}
                            onChange={() => alternarCategoria(id)}
                          />{" "}
                          {categoria.nome}
                        </label>
                      );
                    })}
                </div>
              </div>
              <div className={styles.modalActions}>
                <button
                  type="button"
                  className={styles.cancelButton}
                  onClick={fecharModal}
                >
                  Cancelar
                </button>
                <button
                  type="submit"
                  className={styles.saveButton}
                  disabled={salvando}
                >
                  {salvando
                    ? "Salvando..."
                    : regraEmEdicao
                      ? "Salvar alterações"
                      : "Criar regra aplicada"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {modalGestaoAberto && (
        <div
          className={styles.overlay}
          role="dialog"
          aria-modal="true"
          aria-labelledby="gestao-regras-aplicadas"
        >
          <div className={`${styles.modal} ${styles.managementModal}`}>
            <div className={styles.modalHeader}>
              <div>
                <h2 id="gestao-regras-aplicadas">Gerenciar regras aplicadas</h2>
                <p>
                  Edite, ative ou desative as regras usadas em chamados já
                  existentes.
                </p>
              </div>
              <button
                type="button"
                onClick={() => setModalGestaoAberto(false)}
                aria-label="Fechar"
              >
                <X size={20} />
              </button>
            </div>

            <div className={styles.tableWrap}>
              <table className={`${styles.table} ${styles.managementTable}`}>
                <thead>
                  <tr>
                    <th>Regra</th>
                    <th>Categorias</th>
                    <th>Destino</th>
                    <th>Vigência</th>
                    <th>Status</th>
                    <th aria-label="Ações" />
                  </tr>
                </thead>
                <tbody>
                  {regras.length ? (
                    regras.map((regra) => {
                      const categoriasDaRegra =
                        criteriosPorRegra.get(regraId(regra)) ?? [];
                      const status = estadoOperacional(regra);
                      return (
                        <tr key={regraId(regra)}>
                          <td>
                            <strong>{regra.nome}</strong>
                          </td>
                          <td>
                            <div className={styles.categoryNames}>
                              {categoriasDaRegra.length ? (
                                categoriasDaRegra.map((criterio) => {
                                  const categoria = categorias.find(
                                    (item) =>
                                      String(categoriaId(item)) ===
                                      String(criterioCategoriaId(criterio)),
                                  );
                                  return (
                                    <span key={criterioId(criterio)}>
                                      {categoria?.nome ?? "Categoria"}
                                    </span>
                                  );
                                })
                              ) : (
                                <span>Sem categorias</span>
                              )}
                            </div>
                          </td>
                          <td>
                            {Number(regra.tipoDestino) === 2
                              ? (perfis.find(
                                  (perfil) =>
                                    String(perfilId(perfil)) ===
                                    String(regra.perfilDestinoId),
                                )?.nome ?? "Perfil não encontrado")
                              : (usuarios.find(
                                  (usuario) =>
                                    String(usuarioId(usuario)) ===
                                    String(regra.usuarioDestinoId),
                                )?.username ?? "Usuário não encontrado")}
                          </td>
                          <td>
                            {regra.periodoIndefinido
                              ? "Indeterminada"
                              : `${formatarData(regra.dataInicial)} até ${formatarData(regra.dataFinal)}`}
                          </td>
                          <td>
                            <span className={status.classe}>
                              {status.texto}
                            </span>
                          </td>
                          <td>
                            <div className={styles.rowActions}>
                              <button
                                type="button"
                                title="Editar regra"
                                onClick={() => abrirEdicao(regra)}
                              >
                                <Pencil size={16} />
                              </button>
                              <button
                                type="button"
                                title={
                                  regra.ativo
                                    ? "Desativar regra"
                                    : "Ativar regra"
                                }
                                onClick={() => alterarStatus(regra)}
                              >
                                <Power size={16} />
                              </button>
                            </div>
                          </td>
                        </tr>
                      );
                    })
                  ) : (
                    <tr>
                      <td colSpan="6" className={styles.empty}>
                        Nenhuma regra aplicada encontrada.
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
