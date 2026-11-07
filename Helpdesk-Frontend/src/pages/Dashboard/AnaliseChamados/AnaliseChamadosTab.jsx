import { useCallback, useEffect, useMemo, useState } from "react";
import {
  ChevronLeft,
  ChevronRight,
  FileSearch,
  LoaderCircle,
  SearchX,
} from "lucide-react";
import chamadoService from "../../../services/chamadoService";
import slaService from "../../../services/SlaService";
import { apiErrorMessage } from "../../../utils/apiError";
import ChamadoAnaliseCard from "./ChamadoAnaliseCard";
import ChamadoAnaliseDetalhe from "./ChamadoAnaliseDetalhe";
import styles from "./analiseChamados.module.css";

function obterSituacaoSla(chamado, sla) {
  const chamadoCancelado =
    Number(chamado.status) === 4 || chamado.status === "Cancelado";

  if (chamadoCancelado) {
    return null;
  }

  if (chamado.slaPendenteDefinicaoPerfil || !sla?.ativo) {
    return "pendente";
  }

  const tempoPrimeiroAtendimento = Number(
    chamado.tempoPrimeiroAtendimentoSlaHoras,
  );
  const tempoResolucao = Number(chamado.tempoResolucaoSlaHoras);

  const primeiroAtendimentoViolado =
    Number.isFinite(tempoPrimeiroAtendimento) &&
    tempoPrimeiroAtendimento > Number(sla.tempoAtendimentoHoras);

  const resolucaoViolada =
    Number.isFinite(tempoResolucao) &&
    tempoResolucao > Number(sla.tempoResolucaoHoras);

  return primeiroAtendimentoViolado || resolucaoViolada
    ? "fora"
    : "dentro";
}

export default function AnaliseChamadosTab({ filtros, categorias }) {
  const [chamados, setChamados] = useState([]);
  const [slas, setSlas] = useState([]);
  const [pagina, setPagina] = useState(1);
  const [totalPaginas, setTotalPaginas] = useState(0);
  const [totalRegistros, setTotalRegistros] = useState(0);
  const [chamadoSelecionado, setChamadoSelecionado] = useState(null);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState("");

  const categoriasPorId = useMemo(
    () =>
      new Map(
        categorias.map((categoria) => [
          categoria.id_CategoriaAtendimento,
          categoria.nome,
        ]),
      ),
    [categorias],
  );

  const slasPorPrioridade = useMemo(
    () =>
      new Map(
        slas
          .filter((sla) => sla.ativo)
          .map((sla) => [Number(sla.prioridade), sla]),
      ),
    [slas],
  );

  const carregar = useCallback(
    async (paginaSolicitada) => {
      try {
        setCarregando(true);
        setErro("");
        const resultado = await chamadoService.buscarAnalisePaginada({
          ...filtros,
          pagina: paginaSolicitada,
        });

        setChamados(resultado.dados ?? []);
        setPagina(resultado.pagina ?? paginaSolicitada);
        setTotalPaginas(resultado.totalPaginas ?? 0);
        setTotalRegistros(resultado.totalRegistros ?? 0);
      } catch (error) {
        setChamados([]);
        setErro(
          apiErrorMessage(
            error,
            "Não foi possível carregar os chamados para análise.",
          ),
        );
      } finally {
        setCarregando(false);
      }
    },
    [filtros],
  );

  useEffect(() => {
    setChamadoSelecionado(null);
    carregar(1);
  }, [carregar]);

  useEffect(() => {
    let ativo = true;

    const carregarSlas = async () => {
      try {
        const resultado = await slaService.getAll();

        if (ativo) {
          setSlas(Array.isArray(resultado) ? resultado : []);
        }
      } catch (error) {
        console.error("Não foi possível carregar os SLAs para análise:", error);

        if (ativo) {
          setSlas([]);
        }
      }
    };

    carregarSlas();

    return () => {
      ativo = false;
    };
  }, []);

  if (chamadoSelecionado) {
    return (
      <ChamadoAnaliseDetalhe
        chamado={chamadoSelecionado}
        categoriaNome={categoriasPorId.get(
          chamadoSelecionado.categoriaAtendimentoId,
        )}
        onBack={() => setChamadoSelecionado(null)}
      />
    );
  }

  return (
    <div className={styles.analysisView}>
      <div className={styles.analysisIntro}>
        <div className={styles.analysisIntroIcon}>
          <FileSearch size={23} />
        </div>
        <div>
          <span>ANÁLISE INDIVIDUAL</span>
          <h2>Atendimentos encontrados</h2>
          <p>
            Selecione um chamado para conferir tempos, participantes,
            comentários e toda a linha do tempo da auditoria.
          </p>
        </div>
        <strong>{totalRegistros} chamado(s)</strong>
      </div>

      {erro ? (
        <div className={styles.errorState}>
          <span>{erro}</span>
          <button type="button" onClick={() => carregar(pagina)}>
            Tentar novamente
          </button>
        </div>
      ) : null}

      {carregando ? (
        <div className={styles.loadingState}>
          <LoaderCircle size={24} />
          Carregando chamados...
        </div>
      ) : null}

      {!carregando && chamados.length === 0 && !erro ? (
        <div className={styles.emptyState}>
          <SearchX size={34} />
          <strong>Nenhum chamado encontrado</strong>
          <span>Ajuste os filtros para ampliar o resultado da consulta.</span>
        </div>
      ) : null}

      {!carregando && chamados.length > 0 ? (
        <div className={styles.ticketGrid}>
          {chamados.map((chamado) => (
            <ChamadoAnaliseCard
              key={chamado.id_Chamado}
              chamado={chamado}
              categoriaNome={categoriasPorId.get(
                chamado.categoriaAtendimentoId,
              )}
              situacaoSla={obterSituacaoSla(
                chamado,
                slasPorPrioridade.get(Number(chamado.prioridade)),
              )}
              onSelect={setChamadoSelecionado}
            />
          ))}
        </div>
      ) : null}

      {!carregando && totalPaginas > 1 ? (
        <nav className={styles.pagination} aria-label="Paginação dos chamados">
          <button
            type="button"
            disabled={pagina <= 1}
            onClick={() => carregar(pagina - 1)}
          >
            <ChevronLeft size={17} />
            Anterior
          </button>
          <span>
            Página <strong>{pagina}</strong> de <strong>{totalPaginas}</strong>
          </span>
          <button
            type="button"
            disabled={pagina >= totalPaginas}
            onClick={() => carregar(pagina + 1)}
          >
            Próxima
            <ChevronRight size={17} />
          </button>
        </nav>
      ) : null}
    </div>
  );
}
