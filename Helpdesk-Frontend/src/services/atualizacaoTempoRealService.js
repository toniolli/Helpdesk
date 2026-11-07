import {
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from "@microsoft/signalr";
import { API_URL } from "../api/apiConfig";
import {
  obterAccessTokenValido,
  possuiSessaoNormal,
  registrarMudancaSessao,
} from "./sessaoAutenticacaoService";

// O Hub fica na raiz da API, fora do prefixo /api usado pelos controllers.
const hubUrl = `${API_URL.replace(/\/api\/?$/, "")}/hubs/atualizacoes`;

// Intervalo usado somente quando a primeira conexão ainda não conseguiu ser estabelecida.
const TEMPO_NOVA_TENTATIVA = 5_000;

export const ESTADO_CONEXAO_TEMPO_REAL = Object.freeze({
  DESCONECTADO: "desconectado",
  CONECTANDO: "conectando",
  CONECTADO: "conectado",
  RECONECTANDO: "reconectando",
});

// A conexão é compartilhada por todos os componentes montados na mesma aba do navegador.
let connection;

// Impede que vários componentes iniciem a mesma conexão simultaneamente.
let startPromise;

// Permite interromper o ciclo de tentativas quando o usuário faz logout.
let manterConexao = false;

// Cada tela registra aqui a consulta que recompõe seu estado após uma reconexão.
const callbacksAposReconexao = new Set();

// Permite que as telas mostrem o estado real da única conexão compartilhada.
const callbacksEstadoConexao = new Set();
let estadoConexao = ESTADO_CONEXAO_TEMPO_REAL.DESCONECTADO;

const aguardar = (tempo) =>
  new Promise((resolve) => window.setTimeout(resolve, tempo));

function atualizarEstadoConexao(novoEstado) {
  if (estadoConexao === novoEstado) return;

  estadoConexao = novoEstado;

  [...callbacksEstadoConexao].forEach((callback) => {
    try {
      callback(novoEstado);
    } catch (error) {
      console.error("Erro ao informar o estado da conexão SignalR.", error);
    }
  });
}

export function registrarEstadoConexao(callback) {
  if (typeof callback !== "function") {
    return () => {};
  }

  callbacksEstadoConexao.add(callback);
  callback(estadoConexao);

  let registroAtivo = true;

  return () => {
    if (!registroAtivo) return;

    registroAtivo = false;
    callbacksEstadoConexao.delete(callback);
  };
}

export function registrarAtualizacaoAposReconexao(callback) {
  if (typeof callback !== "function") {
    return () => {};
  }

  callbacksAposReconexao.add(callback);

  let registroAtivo = true;

  return () => {
    if (!registroAtivo) return;

    registroAtivo = false;
    callbacksAposReconexao.delete(callback);
  };
}

async function recuperarEstadoAposReconexao() {
  // Após logout, a reconexão não pode disparar consultas autenticadas.
  if (!manterConexao || !possuiSessaoNormal()) return;

  // A fotografia evita que inscrições ou cancelamentos durante a execução
  // alterem o conjunto de callbacks deste ciclo de recuperação.
  const callbacksAtuais = [...callbacksAposReconexao];

  await Promise.all(
    callbacksAtuais.map(async (callback) => {
      try {
        await callback();
      } catch (error) {
        // A falha de uma tela não pode impedir a recuperação das demais.
        console.error(
          "Erro ao atualizar dados após a reconexão do SignalR.",
          error,
        );
      }
    }),
  );
}

async function conectarComTentativas() {
  // Enquanto existir sessão, tenta abrir a conexão novamente a cada cinco segundos.
  while (manterConexao && possuiSessaoNormal()) {
    try {
      atualizarEstadoConexao(ESTADO_CONEXAO_TEMPO_REAL.CONECTANDO);
      const accessToken = await obterAccessTokenValido();

      if (!accessToken || !manterConexao || !possuiSessaoNormal()) break;

      await connection.start();
      atualizarEstadoConexao(ESTADO_CONEXAO_TEMPO_REAL.CONECTADO);
      return connection;
    } catch (error) {
      if (!manterConexao || !possuiSessaoNormal()) break;

      atualizarEstadoConexao(ESTADO_CONEXAO_TEMPO_REAL.RECONECTANDO);
      console.warn(
        "Não foi possível conectar ao SignalR. Tentando novamente...",
        error,
      );

      await aguardar(TEMPO_NOVA_TENTATIVA);
    }
  }

  atualizarEstadoConexao(ESTADO_CONEXAO_TEMPO_REAL.DESCONECTADO);
  return null;
}

async function reconectarAposEncerramento() {
  atualizarEstadoConexao(ESTADO_CONEXAO_TEMPO_REAL.DESCONECTADO);

  if (!manterConexao || !possuiSessaoNormal()) return;

  startPromise ??= conectarComTentativas().finally(() => {
    startPromise = undefined;
  });

  const conexaoRestabelecida = await startPromise;

  if (conexaoRestabelecida) {
    await recuperarEstadoAposReconexao();
  }
}

export async function iniciarAtualizacoesTempoReal() {
  // Sem autenticação não existe conexão autorizada com o Hub.
  if (!possuiSessaoNormal()) {
    atualizarEstadoConexao(ESTADO_CONEXAO_TEMPO_REAL.DESCONECTADO);
    return null;
  }

  manterConexao = true;

  if (!connection) {
    // A conexão é criada uma única vez. O token é lido novamente sempre que o SignalR precisar conectar.
    connection = new HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: async () =>
          (await obterAccessTokenValido()) || "",
      })
      // Após uma queda de uma conexão já ativa, o próprio cliente tenta restabelecê-la.
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    // É registrado uma única vez na conexão compartilhada e não é executado
    // durante a conexão inicial nem enquanto a reconexão ainda está ocorrendo.
    connection.onreconnecting(() => {
      atualizarEstadoConexao(ESTADO_CONEXAO_TEMPO_REAL.RECONECTANDO);
    });

    connection.onreconnected(async () => {
      atualizarEstadoConexao(ESTADO_CONEXAO_TEMPO_REAL.CONECTADO);
      await recuperarEstadoAposReconexao();
    });

    // Se a política automática esgotar as tentativas, mantém novas tentativas
    // enquanto a sessão continuar ativa e recupera os dados ao reconectar.
    connection.onclose(reconectarAposEncerramento);
  }

  if (connection.state === HubConnectionState.Disconnected) {
    // Reutiliza a tentativa em andamento caso mais de um componente solicite a conexão.
    startPromise ??= conectarComTentativas().finally(() => {
      startPromise = undefined;
    });

    await startPromise;
  } else if (
    connection.state === HubConnectionState.Connecting &&
    startPromise
  ) {
    await startPromise;
  }

  return connection;
}

export async function pararAtualizacoesTempoReal(limparCallbacks = false) {
  // O logout interrompe novas tentativas e fecha a conexão compartilhada da aba.
  manterConexao = false;
  startPromise = undefined;

  if (connection && connection.state !== HubConnectionState.Disconnected) {
    await connection.stop();
  }

  if (limparCallbacks) {
    callbacksAposReconexao.clear();
    callbacksEstadoConexao.clear();
  }

  atualizarEstadoConexao(ESTADO_CONEXAO_TEMPO_REAL.DESCONECTADO);
}

registrarMudancaSessao(({ tipo }) => {
  if (tipo === "logout") {
    pararAtualizacoesTempoReal(true).catch((error) => {
      console.warn("Erro ao encerrar conexão SignalR.", error);
    });
  }
});
