export const STATUS_CHAMADO = {
  ABERTO: 1,
  EM_ATENDIMENTO: 2,
  FECHADO: 3,
  AGUARDANDO_RETORNO: 5,
  DISTRIBUIDO: 6,
  AGUARDANDO_EQUIPAMENTO: 7,
};

export const TELAS_CENTRAL = [
  "fila-prioritaria",
  "fila-regular",
  "hoje",
  "resumo",
];
// Tempo que cada painel permanece visível antes da rotação da Central.
export const INTERVALO_ROTACAO_MS = 15_000;

// Tempo de exibição do alerta visual gerado quando um novo chamado é identificado.
export const DURACAO_ALERTA_MS = 6_000;
export const LIMITE_CHAMADOS_PRIORIDADE = 6;

export const PRIORIDADES = [
  { id: 4, nome: "Crítica", classe: "critica" },
  { id: 3, nome: "Alta", classe: "alta" },
  { id: 2, nome: "Média", classe: "media" },
  { id: 1, nome: "Baixa", classe: "baixa" },
];

export function normalizarLista(valor) {
  return Array.isArray(valor) ? valor : [];
}

export function obterIdChamado(chamado) {
  return (
    chamado?.chamadoId ??
    chamado?.id_Chamado ??
    chamado?.idChamado ??
    chamado?.id
  );
}

export function obterNumeroChamado(chamado) {
  return chamado?.numero || obterIdChamado(chamado) || "-";
}

export function obterAssuntoChamado(chamado) {
  return (
    chamado?.equipamentoAfetado ||
    chamado?.assunto ||
    chamado?.descricao ||
    "Assunto não informado"
  );
}

export function obterStatusNumerico(status) {
  if (typeof status === "number") return status;

  const statusPorNome = {
    Aberto: STATUS_CHAMADO.ABERTO,
    EmAtendimento: STATUS_CHAMADO.EM_ATENDIMENTO,
    "Em Atendimento": STATUS_CHAMADO.EM_ATENDIMENTO,
    Fechado: STATUS_CHAMADO.FECHADO,
    AguardandoRetorno: STATUS_CHAMADO.AGUARDANDO_RETORNO,
    "Aguardando Retorno": STATUS_CHAMADO.AGUARDANDO_RETORNO,
    Distribuido: STATUS_CHAMADO.DISTRIBUIDO,
    "Distribuído": STATUS_CHAMADO.DISTRIBUIDO,
    AguardandoEquipamento: STATUS_CHAMADO.AGUARDANDO_EQUIPAMENTO,
    "Aguardando Equipamento": STATUS_CHAMADO.AGUARDANDO_EQUIPAMENTO,
  };

  return statusPorNome[status] ?? Number(status);
}

export function agruparFilaPorPrioridade(chamados) {
  const ordenados = [...normalizarLista(chamados)].sort(
    (a, b) => new Date(a.dataAbertura) - new Date(b.dataAbertura),
  );

  return PRIORIDADES.map((prioridade) => {
    const itens = ordenados.filter(
      (chamado) => Number(chamado.prioridade) === prioridade.id,
    );

    return {
      ...prioridade,
      total: itens.length,
      itens: itens.slice(0, LIMITE_CHAMADOS_PRIORIDADE),
      restantes: Math.max(0, itens.length - LIMITE_CHAMADOS_PRIORIDADE),
    };
  });
}

export function formatarTempoEmFila(dataAbertura, agora = Date.now()) {
  const abertura = new Date(dataAbertura);

  if (Number.isNaN(abertura.getTime())) return "Tempo não informado";

  const minutos = Math.max(
    0,
    Math.floor((Number(agora) - abertura.getTime()) / 60_000),
  );

  if (minutos < 60) return `${minutos} min`;

  const horas = Math.floor(minutos / 60);
  if (horas < 24) return `${horas}h ${minutos % 60}min`;

  const dias = Math.floor(horas / 24);
  return `${dias}d ${horas % 24}h`;
}

export function detectarNovosAlertas(chamados, fotografiaAnterior) {
  const alertas = [];

  normalizarLista(chamados).forEach((chamado) => {
    const id = obterIdChamado(chamado);
    if (!id) return;

    const statusAtual = obterStatusNumerico(chamado.status);
    const statusAnterior = fotografiaAnterior.get(id);
    const novoChamado = statusAnterior === undefined;
    const passouParaDistribuido =
      statusAnterior !== undefined &&
      statusAnterior !== STATUS_CHAMADO.DISTRIBUIDO &&
      statusAtual === STATUS_CHAMADO.DISTRIBUIDO;

    if (
      (novoChamado && statusAtual === STATUS_CHAMADO.ABERTO) ||
      (novoChamado && statusAtual === STATUS_CHAMADO.DISTRIBUIDO) ||
      passouParaDistribuido
    ) {
      alertas.push(chamado);
    }
  });

  return alertas;
}

export function criarFotografiaChamados(chamados) {
  return new Map(
    normalizarLista(chamados)
      .map((chamado) => [
        obterIdChamado(chamado),
        obterStatusNumerico(chamado.status),
      ])
      .filter(([id]) => Boolean(id)),
  );
}
