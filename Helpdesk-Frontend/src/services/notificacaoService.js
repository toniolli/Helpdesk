import api from "../api/axios";

const notificacaoService = {
  getAll: async () => {
    const response = await api.get("/Notificacao");

    return response.data;
  },

  getById: async (id) => {
    const response = await api.get(`/Notificacao/${id}`);

    return response.data;
  },

  getByUsuario: async (usuarioId) => {
    const response = await api.get(`/Notificacao/usuario/${usuarioId}`);

    return response.data;
  },

  getNaoLidas: async (lida = false) => {
    const response = await api.get(`/Notificacao/nao-lidas?lida=${lida}`);

    return response.data;
  },

  getTotalNaoLidas: async (usuarioId) => {
    const response = await api.get(
      `/Notificacao/usuario/${usuarioId}/total-nao-lidas`,
    );

    return response.data;
  },

  create: async (notificacao) => {
    const response = await api.post("/Notificacao", notificacao);

    return response.data;
  },

  marcarComoLida: async (id) => {
    const response = await api.put(`/Notificacao/${id}/marcar-como-lida`);

    return response.data;
  },

  aceitar: async (id) => {
    const response = await api.put(`/Notificacao/${id}/aceitar`);

    return response.data;
  },

  recusar: async (id) => {
    const response = await api.put(`/Notificacao/${id}/recusar`);

    return response.data;
  },

  expirar: async (id) => {
    const response = await api.put(`/Notificacao/${id}/expirar`);

    return response.data;
  },

  pendente: async (id) => {
    const response = await api.put(`/Notificacao/${id}/pendente`);

    return response.data;
  },

  buscarNotificacaoPaginado: async ({ pagina = 1, tamanhoPagina = 10 }) => {
    const { data } = await api.get("/Notificacao/buscar-notificacao-paginado", {
      params: {
        pagina,
        tamanhoPagina,
      },
    });

    return data;
  },
};

export default notificacaoService;
