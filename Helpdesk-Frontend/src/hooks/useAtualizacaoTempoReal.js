import { useEffect, useRef } from "react";
import {
  iniciarAtualizacoesTempoReal,
  registrarAtualizacaoAposReconexao,
} from "../services/atualizacaoTempoRealService";

export default function useAtualizacaoTempoReal(
  nomeEvento,
  aoReceber,
  ativo = true,
  aoReconectar = null,
) {
  // Mantém sempre a versão mais recente do callback sem registrar novamente o evento a cada renderização.
  const callbackRef = useRef(aoReceber);
  const callbackReconexaoRef = useRef(aoReconectar);

  useEffect(() => {
    callbackRef.current = aoReceber;
  }, [aoReceber]);

  useEffect(() => {
    callbackReconexaoRef.current = aoReconectar;
  }, [aoReconectar]);

  const possuiCallbackReconexao = typeof aoReconectar === "function";

  useEffect(() => {
    if (!ativo || !possuiCallbackReconexao) return undefined;

    const tratarReconexao = () => callbackReconexaoRef.current?.();

    return registrarAtualizacaoAposReconexao(tratarReconexao);
  }, [ativo, possuiCallbackReconexao]);

  useEffect(() => {
    // Permite que uma tela mantenha o hook montado, mas desative temporariamente sua escuta.
    if (!ativo) return undefined;

    let connection;
    let componenteAtivo = true;

    const tratarEvento = (...args) => {
      // Repassa ao componente o payload recebido do backend.
      callbackRef.current?.(...args);
    };

    const conectar = async () => {
      try {
        connection = await iniciarAtualizacoesTempoReal();

        if (!connection || !componenteAtivo) return;

        // Cada uso do hook adiciona seu próprio listener à conexão compartilhada.
        connection.on(nomeEvento, tratarEvento);
      } catch (error) {
        console.error(`Erro ao conectar ao evento SignalR ${nomeEvento}`, error);
      }
    };

    conectar();

    return () => {
      componenteAtivo = false;
      // Remove apenas o listener deste componente; a conexão continua disponível para os demais.
      connection?.off(nomeEvento, tratarEvento);
    };
  }, [nomeEvento, ativo]);
}
