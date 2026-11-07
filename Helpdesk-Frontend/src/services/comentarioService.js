import api from "../api/axios";

const comentarioService = {
  getById: async (id) => {
    const response = await api.get(`/Comentario/${id}`);

    return response.data;
  },

  getByChamado: async (chamadoId) => {
    const response = await api.get(`/Comentario/chamado/${chamadoId}`);

    return response.data;
  },

  create: async ({ chamadoId, mensagem, arquivos = [] }) => {
    const formData = new FormData();

    formData.append("chamadoId", chamadoId);

    if (mensagem?.trim()) {
      formData.append("mensagem", mensagem);
    }

    arquivos.forEach((arquivo) => {
      formData.append("arquivos", arquivo);
    });

    const response = await api.post("/Comentario", formData);

    return response.data;
  },

  update: async (id, comentario) => {
    const response = await api.put(`/Comentario/${id}`, comentario);

    return response.data;
  },

  delete: async (id) => {
    const response = await api.delete(`/Comentario/${id}`);

    return response.data;
  },
  buscarComentariosPorChamadoPaginado: async (
    chamadoId,
    pagina = 1,
    tamanhoPagina = 10,
  ) => {
    const { data } = await api.get(
      `/Comentario/buscar-por-chamado-paginado/${chamadoId}`,
      {
        params: {
          pagina,
          tamanhoPagina,
        },
      },
    );

    return data;
  },
};

export default comentarioService;
