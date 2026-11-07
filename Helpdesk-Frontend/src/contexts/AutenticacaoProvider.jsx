import { useCallback, useEffect, useMemo, useState } from "react";
import { useLocation } from "react-router-dom";
import { AutenticacaoContext } from "./AutenticacaoContext";
import {
  encerrarSessao as encerrarSessaoNoServico,
  inicializarSessao,
  obterAccessTokenValido,
  possuiSessaoNormal,
  registrarMudancaSessao,
} from "../services/sessaoAutenticacaoService";
import {
  iniciarMonitoramentoAtividade,
  limparAtividadeSessao,
  registrarAtividade,
  sessaoEstaInativa,
} from "../services/atividadeSessaoService";

const INTERVALO_VERIFICACAO_TOKEN_MS = 60 * 1000;

export default function AutenticacaoProvider({ children }) {
  const location = useLocation();
  const [inicializando, setInicializando] = useState(true);
  const [autenticado, setAutenticado] = useState(possuiSessaoNormal());
  const [modoPainel, setModoPainel] = useState(false);

  const encerrarSessao = useCallback(async () => {
    limparAtividadeSessao();
    await encerrarSessaoNoServico();
  }, []);

  useEffect(() => {
    let ativo = true;

    const cancelarRegistro = registrarMudancaSessao(({ tipo, sessao }) => {
      if (!ativo) return;

      const sessaoAtiva = tipo === "autenticado";
      setAutenticado(sessaoAtiva);
      setModoPainel(sessaoAtiva && Boolean(sessao?.modoPainel));
    });

    inicializarSessao().finally(() => {
      if (ativo) {
        setAutenticado(possuiSessaoNormal());
        setInicializando(false);
      }
    });

    return () => {
      ativo = false;
      cancelarRegistro();
    };
  }, []);

  useEffect(() => {
    if (!autenticado) return undefined;

    const pararMonitoramento = iniciarMonitoramentoAtividade(encerrarSessao);
    const intervaloToken = window.setInterval(() => {
      if (!sessaoEstaInativa()) {
        obterAccessTokenValido().catch(() => {});
      }
    }, INTERVALO_VERIFICACAO_TOKEN_MS);

    return () => {
      pararMonitoramento();
      window.clearInterval(intervaloToken);
    };
  }, [autenticado, encerrarSessao]);

  useEffect(() => {
    if (autenticado) registrarAtividade(true);
  }, [autenticado, location.pathname]);

  const valor = useMemo(
    () => ({
      autenticado,
      inicializando,
      modoPainel,
      encerrarSessao,
    }),
    [autenticado, inicializando, modoPainel, encerrarSessao],
  );

  return (
    <AutenticacaoContext.Provider value={valor}>
      {children}
    </AutenticacaoContext.Provider>
  );
}
