import api from "../api/axios";

const auditoriaService = {
  buscarPaginado: async (filtros) => {
    const { data } = await api.get("/Auditoria/paginado", {
      params: filtros,
    });

    return data;
  },

  buscarPorId: async (id) => {
    const { data } = await api.get(`/Auditoria/${id}`);
    return data;
  },
};

export default auditoriaService;

