export const acoesAuditoria = [
  { valor: 1, texto: "Criação" },
  { valor: 2, texto: "Alteração" },
  { valor: 3, texto: "Exclusão" },
  { valor: 4, texto: "Ativação" },
  { valor: 5, texto: "Desativação" },
  { valor: 6, texto: "Redefinição de senha" },
  { valor: 7, texto: "Vínculo" },
  { valor: 8, texto: "Desvínculo" },
];

export const eventosAuditoriaIdentity = [
  { valor: 1, texto: "Login realizado" },
  { valor: 2, texto: "Login inválido" },
  { valor: 3, texto: "Login bloqueado" },
  { valor: 4, texto: "Criação de usuário" },
  { valor: 5, texto: "Alteração de usuário" },
  { valor: 6, texto: "Ativação de usuário" },
  { valor: 7, texto: "Desativação de usuário" },
  { valor: 8, texto: "Exclusão de usuário" },
  { valor: 9, texto: "Alteração de senha" },
  { valor: 10, texto: "Redefinição de senha" },
];

export const eventosAuditoriaChamado = [
  { valor: 1, texto: "Criação" },
  { valor: 2, texto: "Distribuição" },
  { valor: 3, texto: "Início do atendimento" },
  { valor: 4, texto: "Transferência" },
  { valor: 5, texto: "Alteração" },
  { valor: 6, texto: "Equipamento enviado" },
  { valor: 7, texto: "Equipamento recebido" },
  { valor: 8, texto: "Comentário criado" },
  { valor: 9, texto: "Comentário editado" },
  { valor: 10, texto: "Comentário removido" },
  { valor: 11, texto: "Anexo adicionado" },
  { valor: 12, texto: "Anexo removido" },
  { valor: 13, texto: "Encerramento" },
  { valor: 14, texto: "Cancelamento" },
];

const encontrarTexto = (opcoes, valor, fallback) =>
  opcoes.find((opcao) => opcao.valor === Number(valor))?.texto ?? fallback;

export const acaoAuditoriaTexto = (valor) =>
  encontrarTexto(acoesAuditoria, valor, "Ação desconhecida");

export const eventoAuditoriaTexto = (valor) =>
  encontrarTexto(eventosAuditoriaIdentity, valor, "Evento desconhecido");

export const eventoAuditoriaChamadoTexto = (valor) =>
  encontrarTexto(
    eventosAuditoriaChamado,
    valor,
    "Evento desconhecido",
  );

export const origemAuditoriaTexto = (valor) =>
  encontrarTexto(
    [
      { valor: 1, texto: "Usuário" },
      { valor: 2, texto: "Sistema" },
      { valor: 3, texto: "Anônimo" },
    ],
    valor,
    "Origem desconhecida",
  );

export function formatarDataHora(valor) {
  if (!valor) return "-";

  const data = new Date(valor);
  if (Number.isNaN(data.getTime())) return "-";

  return data.toLocaleString("pt-BR");
}

export function lerValoresAuditoria(valor) {
  if (!valor) return "Nenhum valor registrado.";

  try {
    return JSON.stringify(JSON.parse(valor), null, 2);
  } catch {
    return String(valor);
  }
}
