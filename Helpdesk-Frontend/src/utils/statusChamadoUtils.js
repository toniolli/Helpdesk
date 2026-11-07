const statusChamado = {
  1: "Aberto",
  2: "Em Atendimento",
  3: "Fechado",
  4: "Cancelado",
  5: "Aguardando Retorno",
  6: "Distribuído",
  7: "Aguardando Equipamento",
  Aberto: "Aberto",
  EmAtendimento: "Em Atendimento",
  Fechado: "Fechado",
  Cancelado: "Cancelado",
  AguardandoRetorno: "Aguardando Retorno",
  Distribuido: "Distribuído",
  AguardandoEquipamento: "Aguardando Equipamento",
};

export function getStatusChamado(status) {
  return statusChamado[status] || "-";
}
