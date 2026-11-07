import { useEffect, useRef, useState } from "react";
import { HubConnectionState } from "@microsoft/signalr";
import {
  ESTADO_CONEXAO_TEMPO_REAL,
  iniciarAtualizacoesTempoReal,
  registrarAtualizacaoAposReconexao,
  registrarEstadoConexao,
} from "../services/atualizacaoTempoRealService";

const EVENTO_CENTRAL_ATUALIZADA = "centralAtendimentoAtualizada";
const METODO_ENTRAR_CENTRAL = "EntrarCentralAtendimento";
const METODO_SAIR_CENTRAL = "SairCentralAtendimento";

export default function useCentralAtendimentoTempoReal(aoReceber) {
  const callbackRef = useRef(aoReceber);
  const [estadoConexao, setEstadoConexao] = useState(
    ESTADO_CONEXAO_TEMPO_REAL.DESCONECTADO,
  );

  useEffect(() => {
    callbackRef.current = aoReceber;
  }, [aoReceber]);

  useEffect(() => {
    let connection;
    let componenteAtivo = true;
    let entradaSolicitada = false;
    let saidaSolicitada = false;

    const cancelarEstadoConexao = registrarEstadoConexao((estado) => {
      if (componenteAtivo) {
        setEstadoConexao(estado);
      }
    });

    const tratarFotografia = (fotografia) => {
      if (componenteAtivo) {
        callbackRef.current?.(fotografia);
      }
    };

    const sairDaCentral = async () => {
      if (
        saidaSolicitada ||
        !connection ||
        connection.state !== HubConnectionState.Connected
      ) {
        return;
      }

      saidaSolicitada = true;

      try {
        await connection.invoke(METODO_SAIR_CENTRAL);
      } catch (error) {
        console.warn("Não foi possível sair do grupo da Central.", error);
      }
    };

    const entrarNaCentral = async () => {
      if (
        !componenteAtivo ||
        !connection ||
        connection.state !== HubConnectionState.Connected
      ) {
        return;
      }

      entradaSolicitada = true;
      saidaSolicitada = false;
      await connection.invoke(METODO_ENTRAR_CENTRAL);

      // Se a tela foi desmontada durante a entrada, desfaz a participação no grupo.
      if (!componenteAtivo) {
        await sairDaCentral();
      }
    };

    const cancelarRecuperacao = registrarAtualizacaoAposReconexao(
      entrarNaCentral,
    );

    const conectar = async () => {
      try {
        connection = await iniciarAtualizacoesTempoReal();

        if (!connection || !componenteAtivo) return;

        connection.on(EVENTO_CENTRAL_ATUALIZADA, tratarFotografia);
        await entrarNaCentral();
      } catch (error) {
        console.error("Erro ao conectar a Central ao SignalR.", error);
      }
    };

    conectar();

    return () => {
      componenteAtivo = false;
      cancelarEstadoConexao();
      cancelarRecuperacao();
      connection?.off(EVENTO_CENTRAL_ATUALIZADA, tratarFotografia);

      if (entradaSolicitada) {
        sairDaCentral();
      }
    };
  }, []);

  return {
    estadoConexao,
    conectado: estadoConexao === ESTADO_CONEXAO_TEMPO_REAL.CONECTADO,
  };
}
