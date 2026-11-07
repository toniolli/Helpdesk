let contextoAudio;

function obterContextoAudio() {
  if (contextoAudio) return contextoAudio;

  const AudioContexto = window.AudioContext || window.webkitAudioContext;
  if (!AudioContexto) return null;

  contextoAudio = new AudioContexto();
  return contextoAudio;
}

export async function prepararSomAlerta() {
  try {
    const contexto = obterContextoAudio();
    if (!contexto) return false;

    if (contexto.state === "suspended") {
      await contexto.resume();
    }

    return contexto.state === "running";
  } catch {
    return false;
  }
}

export async function reproduzirSomAlerta() {
  try {
    const contexto = obterContextoAudio();
    if (!contexto) return false;

    if (contexto.state === "suspended") {
      await contexto.resume();
    }

    if (contexto.state !== "running") return false;

    const inicio = contexto.currentTime;
    const ganhoPrincipal = contexto.createGain();

    ganhoPrincipal.gain.setValueAtTime(0.0001, inicio);
    ganhoPrincipal.gain.exponentialRampToValueAtTime(0.18, inicio + 0.04);
    ganhoPrincipal.gain.exponentialRampToValueAtTime(0.0001, inicio + 0.72);
    ganhoPrincipal.connect(contexto.destination);

    [
      { frequencia: 659.25, inicio: 0, duracao: 0.34 },
      { frequencia: 880, inicio: 0.26, duracao: 0.42 },
    ].forEach((nota) => {
      const oscilador = contexto.createOscillator();
      const ganhoNota = contexto.createGain();
      const inicioNota = inicio + nota.inicio;

      oscilador.type = "sine";
      oscilador.frequency.setValueAtTime(nota.frequencia, inicioNota);
      ganhoNota.gain.setValueAtTime(0.0001, inicioNota);
      ganhoNota.gain.exponentialRampToValueAtTime(1, inicioNota + 0.025);
      ganhoNota.gain.exponentialRampToValueAtTime(
        0.0001,
        inicioNota + nota.duracao,
      );

      oscilador.connect(ganhoNota);
      ganhoNota.connect(ganhoPrincipal);
      oscilador.start(inicioNota);
      oscilador.stop(inicioNota + nota.duracao);
    });

    return true;
  } catch {
    return false;
  }
}
