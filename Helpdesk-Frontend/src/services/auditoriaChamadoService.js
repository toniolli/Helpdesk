import api from "../api/axios";

const auditoriaChamadoService = {
  buscarPaginado: async (filtros) => {
    const { data } = await api.get("/AuditoriaChamado/paginado", {
      params: filtros,
    });

    return data;
  },

  buscarPorId: async (id) => {
    const { data } = await api.get(`/AuditoriaChamado/${id}`);
    return data;
  },

  buscarPorChamado: async (chamadoId) => {
    const { data } = await api.get(
      `/AuditoriaChamado/chamado/${chamadoId}`,
    );

    return data;
  },
};

export default auditoriaChamadoService;
