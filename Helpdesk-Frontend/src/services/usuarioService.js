import api from "../api/axios";

const usuarioService = {
  getAll: async () => {
    const response = await api.get("/Usuario");
    return response.data;
  },

  create: async (payload) => {
    const response = await api.post("/Usuario/registrar", payload);

    return response.data;
  },

  update: async (id, categoria) => {
    const response = await api.put(`/Usuario/${id}`, categoria);

    return response.data;
  },

  remove: async (id) => {
    const response = await api.delete(`/Usuario/${id}`);

    return response.data;
  },

  getById: async (id) => {
    const response = await api.get(`/Usuario/usuario/${id}`);

    return response.data;
  },

  ativar: async (id) => {
    const response = await api.put(`/Usuario/${id}/ativar`);

    return response.data;
  },
  buscarUsuariosTransferencia: async () => {
    const response = await api.get("/Usuario/usuarios-transferencia");

    return response.data;
  },

  buscarUsuariosDistribuicao: async () => {
    const response = await api.get("/Usuario/usuarios-distribuicao");

    return response.data;
  },

  buscarUsuariosHistorico: async () => {
    const response = await api.get("/Usuario/usuarios-historico");

    return response.data;
  },

  desativar: async (id) => {
    const response = await api.put(`/Usuario/${id}/desativar`);

    return response.data;
  },

  restaurarSenhaPadrao: async (id) => {
    const response = await api.put(`/Usuario/${id}/restaurar-senha`);

    return response.data;
  },
};

export default usuarioService;
