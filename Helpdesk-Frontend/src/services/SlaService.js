import api from "../api/axios";

const slaService = {
  getAll: async () => {
    const response = await api.get("/Sla");
    return response.data;
  },

  getById: async (id) => {
    const response = await api.get(`/Sla/${id}`);
    return response.data;
  },

  getByPrioridade: async (prioridade) => {
    const response = await api.get(`/Sla/prioridade/${prioridade}`);
    return response.data;
  },

  create: async (sla) => {
    const response = await api.post("/Sla", sla);
    return response.data;
  },

  update: async (id, sla) => {
    const response = await api.put(`/Sla/${id}`, sla);
    return response.data;
  },

  ativar: async (id) => {
    const response = await api.put(`/Sla/${id}/ativar`);
    return response.data;
  },

  desativar: async (id) => {
    const response = await api.put(`/Sla/${id}/desativar`);
    return response.data;
  },

  remove: async (id) => {
    const response = await api.delete(`/Sla/${id}`);
    return response.data;
  },

  calcularTempoAtendimento: async (chamadoId, slaId) => {
    const response = await api.get(
      `/Sla/chamado/${chamadoId}/tempo-atendimento/${slaId}`,
    );

    return response.data;
  },

  calcularTempoResolucao: async (chamadoId, slaId) => {
    const response = await api.get(
      `/Sla/chamado/${chamadoId}/tempo-resolucao/${slaId}`,
    );

    return response.data;
  },
};

export default slaService;
