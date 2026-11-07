import axios from "axios";
import { jwtDecode } from "jwt-decode";
import { API_URL } from "../api/apiConfig";
import { sessaoEstaInativa } from "./atividadeSessaoService";

const CHAVE_ACCESS_TOKEN = "helpdesk:access-token:normal";
const CHAVE_SESSAO = "helpdesk:sessao-normal";
const CHAVE_EVENTO_SESSAO = "helpdesk:evento-sessao-normal";
const CHAVE_ULTIMO_LOGOUT = "helpdesk:ultimo-logout-normal";
const CHAVE_LOCK_RENOVACAO = "helpdesk:lock-renovacao-normal";
const NOME_CANAL = "helpdesk:autenticacao-normal";
const MARGEM_RENOVACAO_MS = 2 * 60 * 1000;
const TEMPO_ESPERA_SESSAO_OUTRA_ABA_MS = 1_500;
const TEMPO_LOCK_RENOVACAO_MS = 20 * 1000;
const IDENTIFICADOR_ABA = crypto.randomUUID();

const callbacksMudancaSessao = new Set();
const canalSessao =
  typeof BroadcastChannel === "function"
    ? new BroadcastChannel(NOME_CANAL)
    : null;

const clienteSessao = axios.create({
  baseURL: API_URL,
  withCredentials: true,
  timeout: 15_000,
});

let renovacaoEmAndamento;
let versaoSessao = 0;

const aguardar = (tempo) =>
  new Promise((resolve) => window.setTimeout(resolve, tempo));

function obterUltimoLogout() {
  return Number(localStorage.getItem(CHAVE_ULTIMO_LOGOUT)) || 0;
}

function obterExpiracaoDoToken(accessToken) {
  try {
    const payload = jwtDecode(accessToken);
    return typeof payload.exp === "number" ? payload.exp * 1000 : 0;
  } catch {
    return 0;
  }
}

function normalizarRespostaAutenticacao(resposta) {
  const accessToken = resposta?.accessToken;

  if (!accessToken || !obterExpiracaoDoToken(accessToken)) {
    throw new Error("Resposta de autenticação inválida.");
  }

  return {
    accessToken,
    accessTokenExpiraEmUtc:
      resposta.accessTokenExpiraEmUtc ??
      new Date(obterExpiracaoDoToken(accessToken)).toISOString(),
    modoPainel: Boolean(resposta.modoPainel),
  };
}

function notificarMudancaSessao(
  tipo,
  sessao = null,
  publicar = true,
  emitidoEm = Date.now(),
) {
  callbacksMudancaSessao.forEach((callback) => callback({ tipo, sessao }));

  if (publicar) {
    canalSessao?.postMessage({ tipo, sessao, emitidoEm });
    localStorage.setItem(CHAVE_EVENTO_SESSAO, `${tipo}:${emitidoEm}`);
  }
}

function limparDadosLocais(publicar = true) {
  versaoSessao += 1;

  const logoutEm = Date.now();
  if (publicar) localStorage.setItem(CHAVE_ULTIMO_LOGOUT, String(logoutEm));

  sessionStorage.removeItem(CHAVE_ACCESS_TOKEN);
  sessionStorage.removeItem(CHAVE_SESSAO);

  // Remove somente as chaves usadas pela autenticação antiga.
  localStorage.removeItem("token");
  localStorage.removeItem("user");

  notificarMudancaSessao("logout", null, publicar, logoutEm);
}

function salvarSessao(resposta, publicar = true, autenticadoEm = Date.now()) {
  const sessao = {
    ...normalizarRespostaAutenticacao(resposta),
    autenticadoEm,
  };

  sessionStorage.setItem(CHAVE_ACCESS_TOKEN, sessao.accessToken);
  sessionStorage.setItem(
    CHAVE_SESSAO,
    JSON.stringify({
      accessTokenExpiraEmUtc: sessao.accessTokenExpiraEmUtc,
      modoPainel: sessao.modoPainel,
      autenticadoEm: sessao.autenticadoEm,
    }),
  );

  localStorage.removeItem("token");
  localStorage.removeItem("user");
  notificarMudancaSessao(
    "autenticado",
    sessao,
    publicar,
    sessao.autenticadoEm,
  );

  return sessao;
}

function obterDadosSessao() {
  try {
    return JSON.parse(sessionStorage.getItem(CHAVE_SESSAO));
  } catch {
    return null;
  }
}

function obterSessaoAtual() {
  const accessToken = obterAccessTokenAtual();
  const dadosSessao = obterDadosSessao();
  const autenticadoEm = Number(dadosSessao?.autenticadoEm);

  if (
    !accessToken ||
    !dadosSessao ||
    !Number.isFinite(autenticadoEm) ||
    autenticadoEm <= obterUltimoLogout()
  ) {
    return null;
  }

  return { accessToken, ...dadosSessao };
}

function tokenExpiraEmBreve(accessToken) {
  return obterExpiracaoDoToken(accessToken) <= Date.now() + MARGEM_RENOVACAO_MS;
}

async function executarRenovacao(
  tokenAnterior,
  versaoEsperada,
  logoutNoInicio,
) {
  if (
    versaoEsperada !== versaoSessao ||
    obterUltimoLogout() !== logoutNoInicio ||
    !possuiSessaoNormal()
  ) {
    return obterAccessTokenAtual();
  }

  const tokenAtual = obterAccessTokenAtual();
  const versaoNoInicio = versaoSessao;

  if (
    tokenAnterior &&
    tokenAtual &&
    tokenAtual !== tokenAnterior &&
    !tokenExpiraEmBreve(tokenAtual)
  ) {
    return tokenAtual;
  }

  const response = await clienteSessao.post(
    "/Usuario/renovar",
    null,
    { headers: { "X-Helpdesk-Request": "1" } },
  );

  // Um logout ocorrido enquanto a requisição estava em andamento
  // não pode restaurar a sessão com uma resposta atrasada.
  if (
    versaoNoInicio !== versaoSessao ||
    obterUltimoLogout() !== logoutNoInicio
  ) {
    limparDadosLocais(false);
    return null;
  }

  return salvarSessao(response.data).accessToken;
}

async function adquirirLockRenovacaoFallback() {
  while (true) {
    const agora = Date.now();
    let lockAtual;

    try {
      lockAtual = JSON.parse(localStorage.getItem(CHAVE_LOCK_RENOVACAO));
    } catch {
      lockAtual = null;
    }

    if (!lockAtual || lockAtual.expiraEm <= agora) {
      localStorage.setItem(
        CHAVE_LOCK_RENOVACAO,
        JSON.stringify({
          proprietario: IDENTIFICADOR_ABA,
          expiraEm: agora + TEMPO_LOCK_RENOVACAO_MS,
        }),
      );

      await aguardar(50);

      try {
        const lockConfirmado = JSON.parse(
          localStorage.getItem(CHAVE_LOCK_RENOVACAO),
        );

        if (lockConfirmado?.proprietario === IDENTIFICADOR_ABA) {
          return () => {
            try {
              const lockFinal = JSON.parse(
                localStorage.getItem(CHAVE_LOCK_RENOVACAO),
              );

              if (lockFinal?.proprietario === IDENTIFICADOR_ABA) {
                localStorage.removeItem(CHAVE_LOCK_RENOVACAO);
              }
            } catch {
              // Um lock inválido expira sozinho e não afeta a sessão.
            }
          };
        }
      } catch {
        // Outra aba alterou o lock; uma nova tentativa será feita.
      }
    }

    await aguardar(100);
  }
}

async function renovarComExclusaoMutua(
  tokenAnterior,
  versaoEsperada,
  logoutNoInicio,
) {
  if (navigator.locks?.request) {
    return navigator.locks.request("helpdesk-renovacao-sessao", () =>
      executarRenovacao(tokenAnterior, versaoEsperada, logoutNoInicio),
    );
  }

  const liberarLock = await adquirirLockRenovacaoFallback();

  try {
    return await executarRenovacao(
      tokenAnterior,
      versaoEsperada,
      logoutNoInicio,
    );
  } finally {
    liberarLock();
  }
}

function solicitarSessaoDeOutraAba() {
  if (!canalSessao) return Promise.resolve(null);

  const solicitacaoId = crypto.randomUUID();

  return new Promise((resolve) => {
    const concluir = (sessao) => {
      window.clearTimeout(timeout);
      canalSessao.removeEventListener("message", aoReceberResposta);
      resolve(sessao);
    };
    const aoReceberResposta = ({ data }) => {
      if (
        data?.tipo === "resposta-sessao" &&
        data.solicitacaoId === solicitacaoId &&
        obterExpiracaoDoToken(data.sessao?.accessToken) > 0 &&
        data.sessao?.autenticadoEm > obterUltimoLogout()
      ) {
        concluir(data.sessao);
      }
    };
    const timeout = window.setTimeout(
      () => concluir(null),
      TEMPO_ESPERA_SESSAO_OUTRA_ABA_MS,
    );

    canalSessao.addEventListener("message", aoReceberResposta);
    canalSessao.postMessage({ tipo: "solicitar-sessao", solicitacaoId });
  });
}

export function obterAccessTokenAtual() {
  return sessionStorage.getItem(CHAVE_ACCESS_TOKEN);
}

export function possuiSessaoNormal() {
  return Boolean(obterSessaoAtual());
}

export function definirSessao(resposta) {
  return salvarSessao(resposta);
}

export function accessTokenEstaValido() {
  const accessToken = obterAccessTokenAtual();
  return Boolean(
    accessToken && obterExpiracaoDoToken(accessToken) > Date.now(),
  );
}

export async function renovarSessao(tokenAnterior) {
  if (!possuiSessaoNormal()) return null;

  if (sessaoEstaInativa()) {
    await encerrarSessao();
    return null;
  }

  const versaoRenovacao = versaoSessao;
  const logoutNoInicio = obterUltimoLogout();

  renovacaoEmAndamento ??= renovarComExclusaoMutua(
    tokenAnterior,
    versaoRenovacao,
    logoutNoInicio,
  )
    .catch((error) => {
      if (versaoRenovacao === versaoSessao) limparDadosLocais();
      throw error;
    })
    .finally(() => {
      renovacaoEmAndamento = undefined;
    });

  return renovacaoEmAndamento;
}

export async function obterAccessTokenValido() {
  const accessToken = obterSessaoAtual()?.accessToken;

  if (!accessToken) {
    if (obterAccessTokenAtual() || obterDadosSessao()) {
      limparDadosLocais(false);
    }

    return null;
  }

  if (sessaoEstaInativa()) {
    await encerrarSessao();
    return null;
  }

  if (!tokenExpiraEmBreve(accessToken)) return accessToken;

  return renovarSessao(accessToken);
}

export async function inicializarSessao() {
  localStorage.removeItem("token");
  localStorage.removeItem("user");

  if (!possuiSessaoNormal()) {
    const sessaoDeOutraAba = await solicitarSessaoDeOutraAba();

    if (
      sessaoDeOutraAba &&
      obterExpiracaoDoToken(sessaoDeOutraAba.accessToken) > 0 &&
      sessaoDeOutraAba.autenticadoEm > obterUltimoLogout()
    ) {
      salvarSessao(
        sessaoDeOutraAba,
        false,
        sessaoDeOutraAba.autenticadoEm,
      );
      return obterAccessTokenValido();
    }

    limparDadosLocais();

    try {
      await clienteSessao.post(
        "/Usuario/logout",
        null,
        { headers: { "X-Helpdesk-Request": "1" } },
      );
    } catch {
      // Sem sessão local, o aplicativo continua desautenticado mesmo offline.
    }

    return null;
  }

  try {
    return await obterAccessTokenValido();
  } catch {
    return null;
  }
}

export async function encerrarSessao() {
  const haviaSessao = possuiSessaoNormal();
  limparDadosLocais();

  if (!haviaSessao) return;

  try {
    await clienteSessao.post(
      "/Usuario/logout",
      null,
      { headers: { "X-Helpdesk-Request": "1" } },
    );
  } catch (error) {
    // O estado local deve ser encerrado mesmo se a API estiver indisponível.
    console.warn("Não foi possível encerrar a sessão no servidor.", error);
  }
}

export function registrarMudancaSessao(callback) {
  callbacksMudancaSessao.add(callback);
  return () => callbacksMudancaSessao.delete(callback);
}

canalSessao?.addEventListener("message", ({ data }) => {
  if (data?.tipo === "solicitar-sessao") {
    const sessao = obterSessaoAtual();

    if (sessao) {
      canalSessao.postMessage({
        tipo: "resposta-sessao",
        solicitacaoId: data.solicitacaoId,
        sessao,
      });
    }

    return;
  }

  if (data?.tipo === "autenticado" && data.sessao) {
    if (data.emitidoEm <= obterUltimoLogout()) return;

    salvarSessao(data.sessao, false, data.emitidoEm);
    return;
  }

  if (data?.tipo === "logout") {
    if (data.emitidoEm) {
      localStorage.setItem(CHAVE_ULTIMO_LOGOUT, String(data.emitidoEm));
    }

    limparDadosLocais(false);
  }
});

window.addEventListener("storage", (event) => {
  if (
    event.key === CHAVE_EVENTO_SESSAO &&
    event.newValue?.startsWith("logout:")
  ) {
    limparDadosLocais(false);
  }
});
