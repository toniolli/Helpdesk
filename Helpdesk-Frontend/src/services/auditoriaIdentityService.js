import api from "../api/axios";

const auditoriaIdentityService = {
  buscarPaginado: async (filtros) => {
    const { data } = await api.get("/AuditoriaIdentity/paginado", {
      params: filtros,
    });

    return data;
  },

  buscarPorId: async (id) => {
    const { data } = await api.get(`/AuditoriaIdentity/${id}`);
    return data;
  },
};

export default auditoriaIdentityService;
