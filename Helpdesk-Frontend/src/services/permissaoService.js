import api from "../api/axios";

const permissaoService = {
    getAll: async () => {
        const response = await api.get("/Permissao");
        return response.data;
    },

    getById: async (id) => {
        const response = await api.get(`/Permissao/${id}`);
        return response.data;
    },

    create: async (permissao) => {
        const response = await api.post(
            "/Permissao",
            permissao
        );

        return response.data;
    },

    update: async (id, permissao) => {
        const response = await api.put(
            `/Permissao/${id}`,
            permissao
        );

        return response.data;
    },

    remove: async (id) => {
        const response = await api.delete(
            `/Permissao/${id}`
        );

        return response.data;
    },

    ativar: async (id) => {
        const response = await api.put(
            `/Permissao/${id}/ativar`
        );

        return response.data;
    },

    desativar: async (id) => {
        const response = await api.put(
            `/Permissao/${id}/desativar`
        );

        return response.data;
    }

};

export default permissaoService;