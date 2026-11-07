import { useEffect, useMemo, useState } from "react";
import { useSearchParams } from "react-router-dom";
import { Plus, Pencil, Power, Search, X } from "lucide-react";
import categoriaService from "../../services/categoriaService";
import criterioDistribuicaoService from "../../services/criterioDistribuicaoService";
import perfilService from "../../services/perfilService";
import regraDistribuicaoService from "../../services/regraDistribuicaoService";
import usuarioService from "../../services/usuarioService";
import { apiErrorMessage } from "../../utils/apiError";
import styles from "./distribuicao.module.css";

const formInicial = {
  nome: "",
  tipoRegra: 1,
  tipoDestino: 1,
  periodoIndefinido: true,
  dataInicial: "",
  dataFinal: "",
  usuarioDestinoId: "",
  perfilDestinoId: "",
  categorias: [],
};

const regraId = (regra) =>
  regra.id_Regras ?? regra.id_RegraDistribuicao ?? regra.id;
const criterioId = (criterio) => criterio.id_Criterio ?? criterio.id;
const categoriaId = (categoria) =>
  categoria.id_CategoriaAtendimento ?? categoria.id;
const perfilId = (perfil) => perfil.id_Perfil ?? perfil.id;
const usuarioId = (usuario) => usuario.id_Usuario ?? usuario.id;
const criterioRegraId = (criterio) =>
  criterio.regraDistribuicaoId ?? criterio.id_RegraDistribuicao;
const criterioCategoriaId = (criterio) =>
  criterio.categoriaAtendimentoId ?? criterio.id_CategoriaAtendimento;

function paraDataInput(valor) {
  if (!valor) return "";

  const dataIso = String(valor).match(/^\d{4}-\d{2}-\d{2}/)?.[0];
  if (dataIso) return dataIso;

  const data = new Date(valor);
  return Number.isNaN(data.getTime()) ? "" : data.toISOString().slice(0, 10);
}

function formatarData(valor) {
  const data = paraDataInput(valor);
  if (!data) return "-";

  const [ano, mes, dia] = data.split("-");
  return `${dia}/${mes}/${ano}`;
}

function estadoOperacional(regra) {
  if (!regra.ativo) return { texto: "Inativa", classe: styles.inactive };
  if (regra.periodoIndefinido) {
    return { texto: "Em vigor", classe: styles.active };
  }

  const agora = new Date();
  const inicio = regra.dataInicial ? new Date(regra.dataInicial) : null;
  const fim = regra.dataFinal ? new Date(regra.dataFinal) : null;

  if (inicio && !Number.isNaN(inicio.getTime()) && agora < inicio) {
    return { texto: "Agendada", classe: styles.scheduled };
  }
  if (fim && !Number.isNaN(fim.getTime()) && agora > fim) {
    return { texto: "Expirada", classe: styles.expired };
  }

  return { texto: "Em vigor", classe: styles.active };
}

export default function Distribuicao() {
  const [searchParams, setSearchParams] = useSearchParams();
  const [regras, setRegras] = useState([]);
  const [criterios, setCriterios] = useState([]);
  const [categorias, setCategorias] = useState([]);
  const [usuarios, setUsuarios] = useState([]);
  const [perfis, setPerfis] = useState([]);
  const [filtro, setFiltro] = useState("");
  const [modalAberto, setModalAberto] = useState(false);
  const [regraEmEdicao, setRegraEmEdicao] = useState(null);
  const [form, setForm] = useState(formInicial);
  const [salvando, setSalvando] = useState(false);
  const [erro, setErro] = useState("");
  const [sucesso, setSucesso] = useState("");

  const carregarDados = async () => {
    try {
      setErro("");
      const resultados = await Promise.allSettled([
        regraDistribuicaoService.getAll(),
        criterioDistribuicaoService.getAll(),
        categoriaService.getAll(),
        usuarioService.buscarUsuariosDistribuicao(),
        perfilService.getAll(),
      ]);

      const dados = resultados.map((resultado) =>
        resultado.status === "fulfilled" ? (resultado.value ?? []) : [],
      );
      const listaPerfis = Array.isArray(dados[4]) ? dados[4] : [];

      setRegras(Array.isArray(dados[0]) ? dados[0] : []);
      setCriterios(Array.isArray(dados[1]) ? dados[1] : []);
      setCategorias(Array.isArray(dados[2]) ? dados[2] : []);
      setUsuarios(Array.isArray(dados[3]) ? dados[3] : []);
      setPerfis(listaPerfis);

      if (resultados.some((resultado) => resultado.status === "rejected")) {
        setErro(
          "Alguns dados não puderam ser carregados. Os campos disponíveis continuam utilizáveis.",
        );
      }
    } catch (error) {
      console.error("Erro ao carregar distribuição:", error);
      setErro(
        apiErrorMessage(
          error,
          "Não foi possível carregar as regras de distribuição.",
        ),
      );
    }
  };

  useEffect(() => {
    carregarDados();
  }, []);

  useEffect(() => {
    if (searchParams.get("tipo") !== "2") return;

    setRegraEmEdicao(null);
    setForm({ ...formInicial, tipoRegra: 2 });
    setModalAberto(true);
    setSearchParams({}, { replace: true });
  }, [searchParams, setSearchParams]);

  const criteriosPorRegra = useMemo(() => {
    const mapa = new Map();
    criterios.forEach((criterio) => {
      const id = criterioRegraId(criterio);
      mapa.set(id, [...(mapa.get(id) ?? []), criterio]);
    });
    return mapa;
  }, [criterios]);

  const nomeDestino = (regra) => {
    if (Number(regra.tipoDestino) === 2) {
      const perfil = perfis.find(
        (item) => Number(perfilId(item)) === Number(regra.perfilDestinoId),
      );
      return perfil?.nome ? `Perfil: ${perfil.nome}` : "Perfil não encontrado";
    }

    const usuario = usuarios.find(
      (item) => String(usuarioId(item)) === String(regra.usuarioDestinoId),
    );
    return usuario?.username ?? usuario?.nome ?? "Usuário não encontrado";
  };

  const regrasFiltradas = regras.filter((regra) => {
    if (Number(regra.tipoRegra ?? 1) !== 1) return false;

    const termo = filtro.trim().toLowerCase();
    if (!termo) return true;

    const categoriasDaRegra = criteriosPorRegra.get(regraId(regra)) ?? [];
    const nomesCategorias = categoriasDaRegra
      .map(
        (criterio) =>
          categorias.find(
            (categoria) =>
              categoriaId(categoria) === criterioCategoriaId(criterio),
          )?.nome,
      )
      .filter(Boolean)
      .join(" ");

    return [regra.nome, nomeDestino(regra), nomesCategorias]
      .filter(Boolean)
      .some((valor) => valor.toLowerCase().includes(termo));
  });

  const usuariosDestino = useMemo(() => {
    return usuarios.filter((usuario) => usuario.ativo !== false);
  }, [usuarios]);

  const categoriasPrincipaisReservadas = useMemo(() => {
    const idEmEdicao = regraEmEdicao ? regraId(regraEmEdicao) : null;
    const idsRegrasPrincipais = new Set(
      regras
        .filter(
          (regra) =>
            regraId(regra) !== idEmEdicao &&
            regra.ativo &&
            Number(regra.tipoRegra) === 1 &&
            regra.periodoIndefinido,
        )
        .map(regraId),
    );

    return new Set(
      criterios
        .filter((criterio) =>
          idsRegrasPrincipais.has(criterioRegraId(criterio)),
        )
        .map(criterioCategoriaId),
    );
  }, [criterios, regraEmEdicao, regras]);

  const abrirNovo = () => {
    setRegraEmEdicao(null);
    setForm(formInicial);
    setErro("");
    setSucesso("");
    setModalAberto(true);
  };

  const abrirEdicao = (regra) => {
    const id = regraId(regra);
    const categoriasDaRegra = (criteriosPorRegra.get(id) ?? []).map(
      (criterio) => criterioCategoriaId(criterio),
    );
    setRegraEmEdicao(regra);
    setErro("");
    setSucesso("");
    setForm({
      nome: regra.nome ?? "",
      tipoRegra: Number(regra.tipoRegra ?? 1),
      tipoDestino: Number(regra.tipoDestino ?? 1),
      periodoIndefinido: regra.periodoIndefinido ?? true,
      dataInicial: paraDataInput(regra.dataInicial),
      dataFinal: paraDataInput(regra.dataFinal),
      usuarioDestinoId: regra.usuarioDestinoId ?? "",
      perfilDestinoId: regra.perfilDestinoId ?? "",
      categorias: categoriasDaRegra,
    });
    setModalAberto(true);
  };

  const fecharModal = () => {
    setModalAberto(false);
    setRegraEmEdicao(null);
    setForm(formInicial);
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

  const salvar = async (event) => {
    event.preventDefault();
    if (!form.categorias.length) {
      setErro("Selecione ao menos uma categoria para a regra.");
      return;
    }

    const regraPrincipalAtiva =
      Number(form.tipoRegra) === 1 &&
      form.periodoIndefinido &&
      (regraEmEdicao?.ativo ?? true);
    const categoriasEmConflito = regraPrincipalAtiva
      ? form.categorias.filter((id) => categoriasPrincipaisReservadas.has(id))
      : [];

    if (categoriasEmConflito.length) {
      setErro(
        "Uma ou mais categorias já possuem uma regra automática fixa ativa.",
      );
      return;
    }

    setSalvando(true);
    setErro("");
    setSucesso("");
    try {
      const payload = {
        nome: form.nome.trim(),
        tipoRegra: Number(form.tipoRegra),
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

      if (!payload.nome) {
        setErro("Informe um nome para a regra.");
        return;
      }

      let idRegra = regraEmEdicao ? regraId(regraEmEdicao) : null;
      if (idRegra) {
        await regraDistribuicaoService.update(idRegra, payload);
      } else {
        const criada = await regraDistribuicaoService.create(payload);
        idRegra = regraId(criada);

        // O contrato do POST retorna 200 sem um corpo garantido. Quando isso
        // ocorre, buscamos a regra recém-criada para poder incluir critérios.
        if (!idRegra) {
          const regrasAtualizadas = await regraDistribuicaoService.getAll();
          idRegra = regraId(
            [...(regrasAtualizadas ?? [])]
              .reverse()
              .find((regra) => regra.nome === payload.nome),
          );
        }

        if (!idRegra) {
          throw new Error(
            "A regra foi criada, mas seu identificador não retornou.",
          );
        }
      }

      const criteriosAtuais = criteriosPorRegra.get(idRegra) ?? [];
      const idsAtuais = criteriosAtuais.map((item) =>
        criterioCategoriaId(item),
      );
      const paraAdicionar = form.categorias.filter(
        (id) => !idsAtuais.includes(id),
      );
      const paraRemover = criteriosAtuais.filter(
        (item) => !form.categorias.includes(criterioCategoriaId(item)),
      );

      await Promise.all([
        ...paraAdicionar.map((categoriaAtendimentoId) =>
          criterioDistribuicaoService.create({
            regraDistribuicaoId: idRegra,
            categoriaAtendimentoId,
          }),
        ),
        ...paraRemover.map((criterio) =>
          criterioDistribuicaoService.remover(criterioId(criterio)),
        ),
      ]);

      fecharModal();
      await carregarDados();
      const tipoSalvo = Number(form.tipoRegra);
      setSucesso(
        regraEmEdicao
          ? `${tipoSalvo === 2 ? "Regra aplicada" : "Regra automática"} atualizada com sucesso.`
          : `${tipoSalvo === 2 ? "Regra aplicada" : "Regra automática"} criada com sucesso.`,
      );
    } catch (error) {
      console.error("Erro ao salvar regra:", error);
      setErro(
        apiErrorMessage(
          error,
          "Não foi possível salvar a regra. Revise os dados e tente novamente.",
        ),
      );
    } finally {
      setSalvando(false);
    }
  };

  const alterarStatus = async (regra) => {
    try {
      setErro("");
      setSucesso("");
      const id = regraId(regra);
      if (regra.ativo) await regraDistribuicaoService.desativar(id);
      else await regraDistribuicaoService.ativar(id);
      await carregarDados();
      setSucesso(
        regra.ativo
          ? "Regra automática desativada."
          : "Regra automática ativada.",
      );
    } catch (error) {
      console.error("Erro ao alterar status da regra:", error);
      setErro(
        apiErrorMessage(
          error,
          "Não foi possível alterar o status da regra.",
        ),
      );
    }
  };

  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <div>
          <h1>Distribuição automática</h1>
          <p>
            Defina como novos chamados serão encaminhados conforme a categoria.
          </p>
        </div>
        <button className={styles.primaryButton} onClick={abrirNovo}>
          <Plus size={18} /> Nova regra
        </button>
      </div>

      {erro && <div className={styles.feedback}>{erro}</div>}
      {sucesso && (
        <div className={styles.successFeedback} role="status">
          {sucesso}
        </div>
      )}

      <div className={styles.toolbar}>
        <label className={styles.search}>
          <Search size={17} />
          <input
            placeholder="Pesquisar regra..."
            value={filtro}
            onChange={(event) => setFiltro(event.target.value)}
          />
        </label>
        <span>{regrasFiltradas.length} regra(s)</span>
      </div>

      <div className={styles.tableWrap}>
        <table className={styles.table}>
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
            {regrasFiltradas.length ? (
              regrasFiltradas.map((regra) => {
                const categoriasRegra =
                  criteriosPorRegra.get(regraId(regra)) ?? [];
                const estado = estadoOperacional(regra);
                return (
                  <tr key={regraId(regra)}>
                    <td>
                      <strong>{regra.nome}</strong>
                    </td>
                    <td>
                      <div className={styles.categoryList}>
                        {categoriasRegra.length ? (
                          categoriasRegra.map((criterio) => {
                            const categoria = categorias.find(
                              (item) =>
                                categoriaId(item) ===
                                criterioCategoriaId(criterio),
                            );
                            return (
                              <span key={criterioId(criterio)}>
                                {categoria?.nome ?? "Categoria"}
                              </span>
                            );
                          })
                        ) : (
                          <span className={styles.muted}>Sem categorias</span>
                        )}
                      </div>
                    </td>
                    <td>{nomeDestino(regra)}</td>
                    <td>
                      {regra.periodoIndefinido
                        ? "Indeterminada"
                        : `${formatarData(regra.dataInicial)} até ${formatarData(regra.dataFinal)}`}
                    </td>
                    <td>
                      <span
                        className={estado.classe}
                      >
                        {estado.texto}
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
                            regra.ativo ? "Desativar regra" : "Ativar regra"
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
                  Nenhuma regra de distribuição encontrada.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      {modalAberto && (
        <div className={styles.overlay} role="dialog" aria-modal="true">
          <div className={styles.modal}>
            <div className={styles.modalHeader}>
              <div>
                <h2>
                  {regraEmEdicao
                    ? "Editar regra"
                    : form.tipoRegra === 2
                      ? "Nova regra aplicada"
                      : "Nova regra"}
                </h2>
                <p>
                  {form.tipoRegra === 2
                    ? "Configure a regra para usar nos chamados já existentes."
                    : "Configure o destino e as categorias atendidas."}
                </p>
              </div>
              <button type="button" onClick={fecharModal} aria-label="Fechar">
                <X size={20} />
              </button>
            </div>
            <form onSubmit={salvar}>
              {erro && <div className={styles.modalFeedback}>{erro}</div>}
              <div className={styles.field}>
                <label htmlFor="nome">Nome da regra</label>
                <input
                  id="nome"
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
                      ? usuariosDestino.map((usuario) => (
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
              <div className={styles.field}>
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
              </div>
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
                      const indisponivel =
                        Number(form.tipoRegra) === 1 &&
                        form.periodoIndefinido &&
                        (regraEmEdicao?.ativo ?? true) &&
                        categoriasPrincipaisReservadas.has(id);
                      return (
                        <label
                          key={id}
                          className={
                            indisponivel ? styles.unavailableCategory : ""
                          }
                          title={
                            indisponivel
                              ? "Esta categoria já possui uma regra automática fixa ativa."
                              : undefined
                          }
                        >
                          <input
                            type="checkbox"
                            checked={form.categorias.includes(id)}
                            onChange={() => alternarCategoria(id)}
                            disabled={indisponivel}
                          />{" "}
                          {categoria.nome}
                          {indisponivel ? " — já utilizada" : ""}
                        </label>
                      );
                    })}
                </div>
              </div>
              <div className={styles.modalActions}>
                <button
                  type="button"
                  className={styles.secondaryButton}
                  onClick={fecharModal}
                >
                  Cancelar
                </button>
                <button className={styles.primaryButton} disabled={salvando}>
                  {salvando ? "Salvando..." : "Salvar regra"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
