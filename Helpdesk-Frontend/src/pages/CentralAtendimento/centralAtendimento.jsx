import { useCallback, useEffect, useRef, useState } from "react";
import { LoaderCircle } from "lucide-react";
import useCentralAtendimentoTempoReal from "../../hooks/useCentralAtendimentoTempoReal";
import CentralHeader from "./CentralHeader";
import FilaCentral from "./FilaCentral";
import AtendimentosHoje from "./AtendimentosHoje";
import ResumoOperacional from "./ResumoOperacional";
import AlertaChamado from "./AlertaChamado";
import {
  prepararSomAlerta,
  reproduzirSomAlerta,
} from "./centralAtendimentoSom";
import {
  criarFotografiaChamados,
  detectarNovosAlertas,
  DURACAO_ALERTA_MS,
  INTERVALO_ROTACAO_MS,
  normalizarLista,
  TELAS_CENTRAL,
} from "../../utils/centralAtendimentoUtils";
import styles from "./centralAtendimento.module.css";

export default function CentralAtendimento() {
  const [fila, setFila] = useState([]);
  const [atendimentosHoje, setAtendimentosHoje] = useState({
    iniciados: 0,
    finalizados: 0,
  });
  const [resumoOperacional, setResumoOperacional] = useState({});
  const [telaAtual, setTelaAtual] = useState(0);
  const [agora, setAgora] = useState(new Date());
  const [ultimaAtualizacao, setUltimaAtualizacao] = useState(null);
  const [recebeuFotografia, setRecebeuFotografia] = useState(false);
  const [filaAlertas, setFilaAlertas] = useState([]);
  const [alertaAtual, setAlertaAtual] = useState(null);
  const [exibirBotaoTelaCheia, setExibirBotaoTelaCheia] = useState(false);
  const [somAtivo, setSomAtivo] = useState(
    () => localStorage.getItem("central-atendimento-som") === "ativo",
  );

  const fotografiaRef = useRef(new Map());
  const fotografiaInicializadaRef = useRef(false);
  const instanciaIdRef = useRef(null);
  const versaoRef = useRef(null);

  const aplicarFotografia = useCallback((fotografia) => {
    const instanciaId = String(fotografia?.instanciaId ?? "").trim();
    const versao = Number(fotografia?.versao);

    if (!instanciaId || !Number.isFinite(versao)) return;

    const primeiraFotografiaDaInstancia =
      instanciaIdRef.current !== instanciaId;

    if (
      !primeiraFotografiaDaInstancia &&
      versaoRef.current !== null &&
      versao <= versaoRef.current
    ) {
      return;
    }

    const chamadosMonitorados = normalizarLista(
      fotografia.chamadosMonitorados,
    );

    if (
      fotografiaInicializadaRef.current &&
      !primeiraFotografiaDaInstancia
    ) {
      const novosAlertas = detectarNovosAlertas(
        chamadosMonitorados,
        fotografiaRef.current,
      );

      if (novosAlertas.length) {
        setFilaAlertas((atuais) => [...atuais, ...novosAlertas]);
      }
    } else {
      fotografiaInicializadaRef.current = true;

      if (instanciaIdRef.current !== null) {
        setFilaAlertas([]);
        setAlertaAtual(null);
      }
    }

    fotografiaRef.current = criarFotografiaChamados(chamadosMonitorados);
    instanciaIdRef.current = instanciaId;
    versaoRef.current = versao;

    setFila(normalizarLista(fotografia.fila));
    setAtendimentosHoje(
      fotografia.atendimentosHoje ?? { iniciados: 0, finalizados: 0 },
    );
    setResumoOperacional(fotografia.resumoOperacional ?? {});

    const dataAtualizacao = new Date(fotografia.dataAtualizacaoUtc);
    setUltimaAtualizacao(
      Number.isNaN(dataAtualizacao.getTime()) ? new Date() : dataAtualizacao,
    );
    setRecebeuFotografia(true);
  }, []);

  const { conectado } =
    useCentralAtendimentoTempoReal(aplicarFotografia);

  useEffect(() => {
    const intervalo = window.setInterval(() => setAgora(new Date()), 1_000);
    return () => window.clearInterval(intervalo);
  }, []);

  useEffect(() => {
    if (alertaAtual || filaAlertas.length === 0) return;

    setAlertaAtual(filaAlertas[0]);
    setFilaAlertas((atuais) => atuais.slice(1));
  }, [alertaAtual, filaAlertas]);

  useEffect(() => {
    if (!alertaAtual) return undefined;

    const temporizador = window.setTimeout(
      () => setAlertaAtual(null),
      DURACAO_ALERTA_MS,
    );

    return () => window.clearTimeout(temporizador);
  }, [alertaAtual]);

  useEffect(() => {
    if (!alertaAtual || !somAtivo) return;

    reproduzirSomAlerta().catch(() => undefined);
  }, [alertaAtual, somAtivo]);

  useEffect(() => {
    if (!somAtivo) return undefined;

    const liberarAudio = () => {
      prepararSomAlerta().catch(() => undefined);
    };

    window.addEventListener("pointerdown", liberarAudio, { once: true });
    window.addEventListener("keydown", liberarAudio, { once: true });

    return () => {
      window.removeEventListener("pointerdown", liberarAudio);
      window.removeEventListener("keydown", liberarAudio);
    };
  }, [somAtivo]);

  useEffect(() => {
    if (alertaAtual || filaAlertas.length) return undefined;

    const intervalo = window.setInterval(() => {
      setTelaAtual((atual) => (atual + 1) % TELAS_CENTRAL.length);
    }, INTERVALO_ROTACAO_MS);

    return () => window.clearInterval(intervalo);
  }, [alertaAtual, filaAlertas.length]);

  const ativarTelaCheia = useCallback(async () => {
    try {
      await document.documentElement.requestFullscreen?.();
      setExibirBotaoTelaCheia(!document.fullscreenElement);
    } catch {
      setExibirBotaoTelaCheia(true);
    }
  }, []);

  const alternarSom = useCallback(async () => {
    if (somAtivo) {
      localStorage.setItem("central-atendimento-som", "silencioso");
      setSomAtivo(false);
      return;
    }

    const liberado = await prepararSomAlerta();
    if (!liberado) return;

    localStorage.setItem("central-atendimento-som", "ativo");
    setSomAtivo(true);
  }, [somAtivo]);

  useEffect(() => {
    const atualizarEstadoTelaCheia = () => {
      setExibirBotaoTelaCheia(!document.fullscreenElement);
    };

    document.addEventListener("fullscreenchange", atualizarEstadoTelaCheia);
    ativarTelaCheia();

    return () => {
      document.removeEventListener("fullscreenchange", atualizarEstadoTelaCheia);
    };
  }, [ativarTelaCheia]);

  const renderizarTela = () => {
    if (telaAtual === 0) {
      return (
        <FilaCentral
          chamados={fila}
          agora={agora.getTime()}
          prioridades={[4, 3]}
          titulo="Chamados críticos e de alta prioridade"
          identificador="fila-prioritaria-titulo"
        />
      );
    }

    if (telaAtual === 1) {
      return (
        <FilaCentral
          chamados={fila}
          agora={agora.getTime()}
          prioridades={[2, 1]}
          titulo="Chamados de média e baixa prioridade"
          identificador="fila-regular-titulo"
        />
      );
    }

    if (telaAtual === 2) {
      return (
        <AtendimentosHoje
          iniciados={atendimentosHoje.iniciados ?? 0}
          finalizados={atendimentosHoje.finalizados ?? 0}
        />
      );
    }

    return <ResumoOperacional resumo={resumoOperacional} />;
  };

  return (
    <main className={styles.central}>
      <CentralHeader
        agora={agora}
        ultimaAtualizacao={ultimaAtualizacao}
        conectado={conectado}
        somAtivo={somAtivo}
        exibirBotaoTelaCheia={exibirBotaoTelaCheia}
        onAlternarSom={alternarSom}
        onAtivarTelaCheia={ativarTelaCheia}
      />

      <div className={styles.content}>
        {!recebeuFotografia ? (
          <div className={styles.loading}>
            <LoaderCircle size={38} />
            <strong>
              {conectado
                ? "Preparando a Central de Atendimento..."
                : "Conectando à Central de Atendimento..."}
            </strong>
          </div>
        ) : null}

        {recebeuFotografia ? renderizarTela() : null}
      </div>

      <footer className={styles.footer} aria-label="Tela exibida">
        {TELAS_CENTRAL.map((tela, indice) => (
          <span
            key={tela}
            className={indice === telaAtual ? styles.activeDot : ""}
          />
        ))}
      </footer>

      <AlertaChamado alerta={alertaAtual} />
    </main>
  );
}
