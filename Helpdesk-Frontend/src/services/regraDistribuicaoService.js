import api from "../api/axios";

const regraDistribuicaoService = {
  getAll: async () => {
    try {
      const response = await api.get("/RegraDistribuicao");

      return response.data;
    } catch (error) {
      if (error.response?.status === 404) return [];
      throw error;
    }
  },

  getById: async (id) => {
    const response = await api.get(`/RegraDistribuicao/${id}`);

    return response.data;
  },

  create: async (payload) => {
    const response = await api.post("/RegraDistribuicao", payload);

    return response.data;
  },

  update: async (id, payload) => {
    const response = await api.put(`/RegraDistribuicao/${id}`, payload);

    return response.data;
  },

  ativar: async (id) => {
    const response = await api.put(`/RegraDistribuicao/${id}/ativar`);

    return response.data;
  },

  desativar: async (id) => {
    const response = await api.put(`/RegraDistribuicao/${id}/desativar`);

    return response.data;
  },

  remover: async (id) => {
    const response = await api.delete(`/RegraDistribuicao/${id}`);

    return response.data;
  },

  buscarChamadosDisponiveis: async (id) => {
    try {
      const response = await api.get(
        `/RegraDistribuicao/${id}/chamados-disponiveis`,
      );

      return response.data;
    } catch (error) {
      if (error.response?.status === 404) return [];
      throw error;
    }
  },

  aplicarChamados: async (id, chamadosId) => {
    const response = await api.post(
      `/RegraDistribuicao/${id}/aplicar-chamados`,
      { chamadosId },
    );

    return response.data;
  },
};

export default regraDistribuicaoService;
