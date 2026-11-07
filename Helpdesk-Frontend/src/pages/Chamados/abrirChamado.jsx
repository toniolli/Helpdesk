import { useEffect, useState } from "react";
import categoriaService from "../../services/categoriaService";
import chamadoService from "../../services/chamadoService";
import styles from "./abrirChamado.module.css";

export default function AbrirChamado() {
  const [categorias, setCategorias] = useState([]);
  const [categoriaAtendimentoId, setCategoriaAtendimentoId] = useState("");
  const [modalCategoriasAberto, setModalCategoriasAberto] = useState(false);
  const [buscaCategoria, setBuscaCategoria] = useState("");
  const [equipamentoAfetado, setEquipamentoAfetado] = useState("");
  const [descricao, setDescricao] = useState("");
  const [loading, setLoading] = useState(false);

  const carregarCategorias = async () => {
    try {
      const data = await categoriaService.getAll();

      setCategorias(data.filter((categoria) => categoria.ativo));
    } catch (error) {
      console.error(error);
    }
  };

  const limparFormulario = () => {
    setCategoriaAtendimentoId("");
    setBuscaCategoria("");
    setEquipamentoAfetado("");
    setDescricao("");
  };

  useEffect(() => {
    carregarCategorias();
  }, []);

  const abrirChamado = async (e) => {
    e.preventDefault();

    if (!categoriaAtendimentoId) {
      alert("Selecione uma categoria para abrir o chamado.");
      return;
    }

    try {
      setLoading(true);

      await chamadoService.create({
        categoriaAtendimentoId: Number(categoriaAtendimentoId),
        equipamentoAfetado,
        descricao,
      });

      alert("Chamado aberto com sucesso.");
      limparFormulario();
    } catch (error) {
      console.error(error);

      alert("Erro ao abrir chamado.");
    } finally {
      setLoading(false);
    }
  };

  const categoriaSelecionada = categorias.find(
    (categoria) =>
      categoria.id_CategoriaAtendimento === Number(categoriaAtendimentoId),
  );

  const categoriasFiltradas = categorias.filter((categoria) => {
    const termo = buscaCategoria.trim().toLowerCase();

    if (!termo) return true;

    return (
      categoria.nome.toLowerCase().includes(termo) ||
      categoria.descricao?.toLowerCase().includes(termo)
    );
  });

  const selecionarCategoria = (categoria) => {
    setCategoriaAtendimentoId(categoria.id_CategoriaAtendimento);
    setBuscaCategoria("");
    setModalCategoriasAberto(false);
  };

  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <h1>Abrir Chamado</h1>

        <p>
          Informe o problema para que a equipe técnica possa iniciar o
          atendimento.
        </p>
      </div>

      <form className={styles.form} onSubmit={abrirChamado}>
        <div className={styles.field}>
          <label>Categoria</label>

          <button
            type="button"
            className={styles.categorySelector}
            onClick={() => setModalCategoriasAberto(true)}
          >
            <span
              className={
                categoriaSelecionada
                  ? styles.categorySelected
                  : styles.categoryPlaceholder
              }
            >
              {categoriaSelecionada?.nome || "Selecione uma categoria"}
            </span>

            <span className={styles.categorySelectorAction}>Selecionar</span>
          </button>
        </div>

        <div className={styles.field}>
          <label>Assunto / Equipamento afetado</label>

          <input
            type="text"
            value={equipamentoAfetado}
            onChange={(e) => setEquipamentoAfetado(e.target.value)}
            placeholder="Ex: Computador não liga"
            required
          />
        </div>

        <div className={styles.field}>
          <label>Descrição do Problema</label>

          <textarea
            value={descricao}
            onChange={(e) => setDescricao(e.target.value)}
            placeholder="Descreva detalhadamente o problema encontrado..."
            required
          />
        </div>

        <div className={styles.actions}>
          <button type="submit" disabled={loading}>
            {loading ? "Abrindo Chamado..." : "Abrir Chamado"}
          </button>
        </div>
      </form>

      {modalCategoriasAberto && (
        <div
          className={styles.categoryModalOverlay}
          onMouseDown={() => setModalCategoriasAberto(false)}
        >
          <section
            className={styles.categoryModal}
            role="dialog"
            aria-modal="true"
            aria-labelledby="titulo-selecionar-categoria"
            onMouseDown={(event) => event.stopPropagation()}
          >
            <div className={styles.categoryModalHeader}>
              <div>
                <span className={styles.categoryModalLabel}>ABRIR CHAMADO</span>
                <h2 id="titulo-selecionar-categoria">Selecione uma categoria</h2>
                <p>Escolha a opção que melhor descreve sua solicitação.</p>
              </div>

              <button
                type="button"
                className={styles.categoryModalClose}
                onClick={() => setModalCategoriasAberto(false)}
                aria-label="Fechar seleção de categorias"
              >
                ×
              </button>
            </div>

            <input
              type="search"
              className={styles.categorySearch}
              value={buscaCategoria}
              onChange={(event) => setBuscaCategoria(event.target.value)}
              placeholder="Pesquisar categoria..."
              autoFocus
            />

            <div className={styles.categoryList}>
              {categoriasFiltradas.length ? (
                categoriasFiltradas.map((categoria) => (
                  <button
                    type="button"
                    key={categoria.id_CategoriaAtendimento}
                    className={`${styles.categoryCard} ${
                      categoria.id_CategoriaAtendimento ===
                      Number(categoriaAtendimentoId)
                        ? styles.categoryCardSelected
                        : ""
                    }`}
                    onClick={() => selecionarCategoria(categoria)}
                  >
                    <strong>{categoria.nome}</strong>
                    <span>
                      {categoria.descricao || "Sem descrição cadastrada."}
                    </span>
                  </button>
                ))
              ) : (
                <p className={styles.emptyCategories}>
                  Nenhuma categoria encontrada.
                </p>
              )}
            </div>
          </section>
        </div>
      )}
    </div>
  );
}
