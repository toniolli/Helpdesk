import api from "../api/axios";

const criterioDistribuicaoService = {
  getAll: async () => {
    try {
      const response = await api.get("/CriterioDistribuicao");

      return response.data;
    } catch (error) {
      if (error.response?.status === 404) return [];
      throw error;
    }
  },

  create: async (payload) => {
    const response = await api.post("/CriterioDistribuicao", payload);

    return response.data;
  },

  remover: async (id) => {
    const response = await api.delete(`/CriterioDistribuicao/${id}`);

    return response.data;
  },
};

export default criterioDistribuicaoService;
