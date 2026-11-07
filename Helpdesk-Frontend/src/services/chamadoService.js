import api from "../api/axios";

const chamadoService = {
  getAll: async () => {
    const response = await api.get("/Chamado");

    return response.data;
  },

  getById: async (id) => {
    const response = await api.get(`/Chamado/${id}`);

    return response.data;
  },

  create: async (chamado) => {
    const response = await api.post("/Chamado", chamado);

    return response.data;
  },

  assumir: async (id) => {
    const response = await api.put(`/Chamado/${id}/assumir`);

    return response.data;
  },

  transferir: async (id, destinoId) => {
    const response = await api.put(
      `/Chamado/${id}/transferir?destinoId=${destinoId}`,
    );

    return response.data;
  },

  aceitarTransferencia: async (chamadoId, notificacaoId) => {
    const response = await api.put(
      `/Chamado/${chamadoId}/aceitar-transferencia?notificacaoId=${notificacaoId}`,
    );

    return response.data;
  },

  recusarTransferencia: async (notificacaoId) => {
    const response = await api.put(
      `/Chamado/recusar-transferencia?notificacaoId=${notificacaoId}`,
    );

    return response.data;
  },

  fechar: async (id, payload) => {
    const response = await api.put(`/Chamado/${id}/fechar`, payload);

    return response.data;
  },

  cancelar: async (id) => {
    const response = await api.put(`/Chamado/${id}/cancelar`);

    return response.data;
  },

  buscarFila: async () => {
    const response = await api.get("/Chamado/fila");

    return response.data;
  },

  buscarMeusChamados: async (tecnicoId) => {
    const response = await api.get(`/Chamado/meus-chamados/${tecnicoId}`);

    return response.data;
  },
  buscarMeusChamadosSolicitante: async (solicitanteId) => {
    const response = await api.get(
      `Chamado/meus-chamados-solicitante/${solicitanteId}`,
    );

    return response.data;
  },
  remove: async (id) => {
    const response = await api.delete(`/Chamado/${id}`);

    return response.data;
  },

  buscarHistoricoPaginado: async ({
    pagina = 1,
    tamanhoPagina = 10,
    dataInicial,
    dataFinal,
  }) => {
    const { data } = await api.get("/Chamado/historico-paginado", {
      params: {
        pagina,
        tamanhoPagina,
        dataInicial,
        dataFinal,
      },
    });

    return data;
  },

  buscarAnalisePaginada: async ({
    numeroChamado,
    dataInicial,
    dataFinal,
    tipoUsuario = 1,
    usuarioId,
    perfilId,
    categoriaAtendimentoId,
    prioridade,
    status,
    pagina = 1,
  }) => {
    const { data } = await api.get("/Chamado/analise-paginada", {
      params: {
        numeroChamado: numeroChamado?.trim() || undefined,
        dataInicial: dataInicial || undefined,
        dataFinal: dataFinal || undefined,
        tipoUsuario,
        usuarioId: usuarioId || undefined,
        perfilId: perfilId || undefined,
        categoriaAtendimentoId: categoriaAtendimentoId || undefined,
        prioridade: prioridade || undefined,
        status: status || undefined,
        pagina,
        tamanhoPagina: 20,
      },
    });

    return data;
  },

  buscarFilaPaginada: async ({
    prioridade,
    pagina = 1,
    tamanhoPagina = 10,
    dataInicial,
    dataFinal,
  }) => {
    const { data } = await api.get("/Chamado/fila-paginado", {
      params: {
        prioridade,
        pagina,
        tamanhoPagina,
        dataInicial: dataInicial || undefined,
        dataFinal: dataFinal || undefined,
      },
    });

    return data;
  },

  buscarMeusChamadosPaginado: async (
    tecnicoId,
    { pagina = 1, tamanhoPagina = 10 },
  ) => {
    const { data } = await api.get(
      `/Chamado/meus-chamados-paginado/${tecnicoId}`,
      {
        params: {
          pagina,
          tamanhoPagina,
        },
      },
    );

    return data;
  },

  buscarMeusChamadosSolicitantePaginado: async (
    solicitanteId,
    { pagina = 1, tamanhoPagina = 10 },
  ) => {
    const { data } = await api.get(
      `/Chamado/meus-chamados-solicitante-paginado/${solicitanteId}`,
      {
        params: {
          pagina,
          tamanhoPagina,
        },
      },
    );

    return data;
  },

  enviarEquipamento: async (id) => {
    const response = await api.put(`/Chamado/${id}/enviar-equipamento`);

    return response.data;
  },

  receberEquipamento: async (id) => {
    const response = await api.put(`/Chamado/${id}/receber-equipamento`);

    return response.data;
  },
};

export default chamadoService;
