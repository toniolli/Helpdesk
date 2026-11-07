import {
  formatarDataChamado,
  formatarTempoChamado,
} from "./analiseChamadosUtils";
import { getStatusChamado } from "./statusChamadoUtils";

export { formatarDataChamado, formatarTempoChamado };

export function criarMapaCategorias(categorias) {
  return new Map(
    (Array.isArray(categorias) ? categorias : []).map((categoria) => [
      Number(categoria.id_CategoriaAtendimento ?? categoria.id),
      categoria.nome,
    ]),
  );
}

export function obterNomeCategoria(categoriasPorId, categoriaId) {
  return (
    categoriasPorId.get(Number(categoriaId)) || "Categoria não informada"
  );
}

export function formatarTempoDesdeAbertura(tempoCorridoHoras) {
  return `Aberto há ${formatarTempoChamado(tempoCorridoHoras)}`;
}

export function formatarTempoSla(chamado, campo) {
  if (chamado?.slaPendenteDefinicaoPerfil) return "Pendente";

  return formatarTempoChamado(chamado?.[campo]);
}

export function obterClasseStatusCard(status) {
  const classes = {
    1: "statusAberto",
    2: "statusEmAtendimento",
    5: "statusAguardandoRetorno",
    6: "statusDistribuido",
    7: "statusAguardandoEquipamento",
  };

  return classes[Number(status)] || "statusAberto";
}

export function obterStatusSolicitante(status) {
  if (Number(status) === 1) return "Aguardando atendimento";
  if (Number(status) === 5) return "Aguardando sua resposta";
  if (Number(status) === 7) return "Equipamento enviado";

  return getStatusChamado(status);
}

export function obterMensagemStatus(status, visaoSolicitante = false) {
  const valor = Number(status);

  const mensagensSolicitante = {
    1: "Seu chamado está na fila e aguarda o início do atendimento.",
    2: "Seu chamado está sendo analisado pelo responsável.",
    5: "O atendimento aguarda uma nova informação sua.",
    6: "Seu chamado foi direcionado e aguarda o responsável assumir.",
    7: "O atendimento aguarda a chegada do equipamento na matriz.",
  };

  const mensagensTecnico = {
    1: "Chamado aguardando início do atendimento.",
    2: "Chamado em atendimento pelo responsável.",
    5: "Atendimento pausado enquanto aguarda o solicitante.",
    6: "Chamado distribuído e aguardando aceite.",
    7: "Atendimento pausado enquanto aguarda o equipamento.",
  };

  const mensagens = visaoSolicitante
    ? mensagensSolicitante
    : mensagensTecnico;

  return mensagens[valor] || "Acompanhe o status atual deste chamado.";
}
