export function formatarTempoChamado(horas) {
  const valor = Number(horas);

  if (!Number.isFinite(valor) || valor <= 0) return "0 min";

  const totalMinutos = Math.round(valor * 60);
  const dias = Math.floor(totalMinutos / 1440);
  const horasRestantes = Math.floor((totalMinutos % 1440) / 60);
  const minutos = totalMinutos % 60;
  const partes = [];

  if (dias > 0) partes.push(`${dias}d`);
  if (horasRestantes > 0) partes.push(`${horasRestantes}h`);
  if (minutos > 0 || partes.length === 0) partes.push(`${minutos}min`);

  return partes.join(" ");
}

export function formatarDataChamado(valor) {
  if (!valor) return "-";

  const data = new Date(valor);
  return Number.isNaN(data.getTime()) ? "-" : data.toLocaleString("pt-BR");
}

export function formatarPrioridadeChamado(prioridade, prioridadeNome) {
  const prioridades = {
    1: "Baixa",
    2: "Média",
    3: "Alta",
    4: "Crítica",
    Baixa: "Baixa",
    Media: "Média",
    Alta: "Alta",
    Critica: "Crítica",
  };

  return prioridadeNome || prioridades[prioridade] || "-";
}

export function classePrioridade(prioridade) {
  const valor = Number(prioridade);
  if (valor === 4 || prioridade === "Critica") return "priorityCritical";
  if (valor === 3 || prioridade === "Alta") return "priorityHigh";
  if (valor === 2 || prioridade === "Media") return "priorityMedium";
  return "priorityLow";
}

export function classeStatus(status) {
  const valor = Number(status);
  const mapa = {
    1: "statusOpen",
    2: "statusService",
    3: "statusClosed",
    4: "statusCanceled",
    5: "statusWaiting",
    6: "statusDistributed",
    7: "statusEquipment",
  };

  const nomes = {
    Aberto: "statusOpen",
    EmAtendimento: "statusService",
    Fechado: "statusClosed",
    Cancelado: "statusCanceled",
    AguardandoRetorno: "statusWaiting",
    Distribuido: "statusDistributed",
    AguardandoEquipamento: "statusEquipment",
  };

  return mapa[valor] || nomes[status] || "statusOpen";
}
