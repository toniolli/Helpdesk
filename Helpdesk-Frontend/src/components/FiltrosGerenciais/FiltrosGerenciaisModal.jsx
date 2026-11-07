import { useEffect } from "react";
import {
  RefreshCw,
  RotateCcw,
  SlidersHorizontal,
  X,
} from "lucide-react";
import styles from "./filtrosGerenciaisModal.module.css";

export default function FiltrosGerenciaisModal({
  titulo,
  descricao,
  mostrarNumeroChamado = false,
  filtros,
  usuarios,
  perfis,
  categorias,
  erro,
  carregando,
  carregandoOpcoes,
  onChange,
  onApply,
  onClear,
  onClose,
}) {
  useEffect(() => {
    const overflowAnterior = document.body.style.overflow;
    const fecharComEscape = (event) => {
      if (event.key === "Escape") onClose();
    };

    document.body.style.overflow = "hidden";
    window.addEventListener("keydown", fecharComEscape);

    return () => {
      document.body.style.overflow = overflowAnterior;
      window.removeEventListener("keydown", fecharComEscape);
    };
  }, [onClose]);

  const aplicar = () => {
    if (onApply()) onClose();
  };

  const limpar = () => {
    onClear();
    onClose();
  };

  return (
    <div
      className={styles.overlay}
      onMouseDown={(event) => {
        if (event.target === event.currentTarget) onClose();
      }}
    >
      <section
        className={styles.modal}
        role="dialog"
        aria-modal="true"
        aria-labelledby="filtros-gerenciais-titulo"
      >
        <div className={styles.header}>
          <div>
            <span className={styles.eyebrow}>FILTROS GERENCIAIS</span>
            <h2 id="filtros-gerenciais-titulo">{titulo}</h2>
            <p>{descricao}</p>
          </div>

          <button
            type="button"
            className={styles.closeButton}
            onClick={onClose}
            aria-label="Fechar filtros"
          >
            <X size={20} />
          </button>
        </div>

        <div className={styles.body}>
          {erro ? <div className={styles.errorMessage}>{erro}</div> : null}

          {carregandoOpcoes ? (
            <span className={styles.optionsLoading}>
              <RefreshCw size={14} className={styles.loadingIcon} />
              Carregando opções
            </span>
          ) : null}

          <div className={styles.grid}>
            {mostrarNumeroChamado ? (
              <div className={styles.field}>
                <label htmlFor="filtro-numero-chamado">Número do chamado</label>
                <input
                  id="filtro-numero-chamado"
                  type="text"
                  value={filtros.numeroChamado}
                  placeholder="Ex.: CH-2026-000123"
                  onChange={(event) =>
                    onChange("numeroChamado", event.target.value)
                  }
                />
              </div>
            ) : null}

            <div className={styles.field}>
              <label htmlFor="filtro-data-inicial">Data inicial</label>
              <input
                id="filtro-data-inicial"
                type="date"
                value={filtros.dataInicial}
                onChange={(event) =>
                  onChange("dataInicial", event.target.value)
                }
              />
            </div>

            <div className={styles.field}>
              <label htmlFor="filtro-data-final">Data final</label>
              <input
                id="filtro-data-final"
                type="date"
                min={filtros.dataInicial || undefined}
                value={filtros.dataFinal}
                onChange={(event) =>
                  onChange("dataFinal", event.target.value)
                }
              />
            </div>

            <div className={styles.field}>
              <label htmlFor="filtro-tipo-usuario">Analisar como</label>
              <select
                id="filtro-tipo-usuario"
                value={filtros.tipoUsuario}
                onChange={(event) =>
                  onChange("tipoUsuario", event.target.value)
                }
              >
                <option value="1">Técnico responsável</option>
                <option value="2">Solicitante</option>
              </select>
            </div>

            <div className={styles.field}>
              <label htmlFor="filtro-usuario">Usuário</label>
              <select
                id="filtro-usuario"
                value={filtros.usuarioId}
                disabled={carregandoOpcoes}
                onChange={(event) =>
                  onChange("usuarioId", event.target.value)
                }
              >
                <option value="">Todos</option>
                {usuarios.map((usuario) => (
                  <option key={usuario.id} value={usuario.id}>
                    {usuario.username}
                  </option>
                ))}
              </select>
            </div>

            <div className={styles.field}>
              <label htmlFor="filtro-perfil">Perfil</label>
              <select
                id="filtro-perfil"
                value={filtros.perfilId}
                disabled={carregandoOpcoes}
                onChange={(event) =>
                  onChange("perfilId", event.target.value)
                }
              >
                <option value="">Todos</option>
                {perfis.map((perfil) => (
                  <option key={perfil.id_Perfil} value={perfil.id_Perfil}>
                    {perfil.nome}
                  </option>
                ))}
              </select>
            </div>

            <div className={styles.field}>
              <label htmlFor="filtro-categoria">Categoria</label>
              <select
                id="filtro-categoria"
                value={filtros.categoriaAtendimentoId}
                disabled={carregandoOpcoes}
                onChange={(event) =>
                  onChange("categoriaAtendimentoId", event.target.value)
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
            </div>

            <div className={styles.field}>
              <label htmlFor="filtro-prioridade">Prioridade</label>
              <select
                id="filtro-prioridade"
                value={filtros.prioridade}
                onChange={(event) =>
                  onChange("prioridade", event.target.value)
                }
              >
                <option value="">Todas</option>
                <option value="1">Baixa</option>
                <option value="2">Média</option>
                <option value="3">Alta</option>
                <option value="4">Crítica</option>
              </select>
            </div>

            <div className={styles.field}>
              <label htmlFor="filtro-status">Status</label>
              <select
                id="filtro-status"
                value={filtros.status}
                onChange={(event) => onChange("status", event.target.value)}
              >
                <option value="">Todos</option>
                <option value="1">Aberto</option>
                <option value="2">Em Atendimento</option>
                <option value="3">Fechado</option>
                <option value="4">Cancelado</option>
                <option value="5">Aguardando Retorno</option>
                <option value="6">Distribuído</option>
                <option value="7">Aguardando Equipamento</option>
              </select>
            </div>
          </div>
        </div>

        <div className={styles.actions}>
          <button
            type="button"
            className={styles.clearButton}
            disabled={carregando}
            onClick={limpar}
          >
            <RotateCcw size={17} />
            Limpar filtros
          </button>
          <button
            type="button"
            className={styles.applyButton}
            disabled={carregando || carregandoOpcoes}
            onClick={aplicar}
          >
            <SlidersHorizontal size={17} />
            Aplicar filtros
          </button>
        </div>
      </section>
    </div>
  );
}
