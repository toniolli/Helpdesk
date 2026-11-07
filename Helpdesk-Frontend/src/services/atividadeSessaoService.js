const CHAVE_ULTIMA_ATIVIDADE = "helpdesk:ultima-atividade-normal";
const NOME_CANAL = "helpdesk:atividade-sessao-normal";
const TEMPO_INATIVIDADE_MS = 30 * 60 * 1000;
const INTERVALO_REGISTRO_MS = 15 * 1000;
const INTERVALO_VERIFICACAO_MS = 15 * 1000;

const canalAtividade =
  typeof BroadcastChannel === "function"
    ? new BroadcastChannel(NOME_CANAL)
    : null;

let ultimaAtividadeRegistrada = 0;

function obterUltimaAtividade() {
  const valor = Number(localStorage.getItem(CHAVE_ULTIMA_ATIVIDADE));
  return Number.isFinite(valor) ? valor : 0;
}

export function registrarAtividade(forcar = false) {
  const agora = Date.now();

  if (!forcar && agora - ultimaAtividadeRegistrada < INTERVALO_REGISTRO_MS) {
    return;
  }

  ultimaAtividadeRegistrada = agora;
  localStorage.setItem(CHAVE_ULTIMA_ATIVIDADE, String(agora));
  canalAtividade?.postMessage(agora);
}

export function sessaoEstaInativa() {
  const ultimaAtividade = obterUltimaAtividade();
  return !ultimaAtividade || Date.now() - ultimaAtividade >= TEMPO_INATIVIDADE_MS;
}

export function limparAtividadeSessao() {
  ultimaAtividadeRegistrada = 0;
  localStorage.removeItem(CHAVE_ULTIMA_ATIVIDADE);
}

export function iniciarMonitoramentoAtividade(onInatividade) {
  const eventosDiscretos = ["pointerdown", "keydown", "touchstart"];
  const aoInteragir = () => registrarAtividade(true);
  const aoMoverPonteiro = () => registrarAtividade();
  const verificarInatividade = () => {
    if (sessaoEstaInativa()) {
      onInatividade();
      return true;
    }

    return false;
  };
  const aoRetomar = () => {
    if (!verificarInatividade()) registrarAtividade(true);
  };
  const aoMudarVisibilidade = () => {
    if (document.visibilityState === "visible") aoRetomar();
  };

  if (!obterUltimaAtividade()) {
    registrarAtividade(true);
  }

  if (verificarInatividade()) return () => {};

  eventosDiscretos.forEach((evento) =>
    window.addEventListener(evento, aoInteragir, { passive: true }),
  );
  window.addEventListener("pointermove", aoMoverPonteiro, { passive: true });
  window.addEventListener("focus", aoRetomar);
  document.addEventListener("visibilitychange", aoMudarVisibilidade);

  const intervalo = window.setInterval(
    verificarInatividade,
    INTERVALO_VERIFICACAO_MS,
  );

  return () => {
    eventosDiscretos.forEach((evento) =>
      window.removeEventListener(evento, aoInteragir),
    );
    window.removeEventListener("pointermove", aoMoverPonteiro);
    window.removeEventListener("focus", aoRetomar);
    document.removeEventListener("visibilitychange", aoMudarVisibilidade);
    window.clearInterval(intervalo);
  };
}

canalAtividade?.addEventListener("message", ({ data }) => {
  if (typeof data === "number" && data > obterUltimaAtividade()) {
    localStorage.setItem(CHAVE_ULTIMA_ATIVIDADE, String(data));
  }
});
