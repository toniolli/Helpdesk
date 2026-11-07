import api from "../api/axios";

const categoriaService = {
    getAll: async () => {
        const response = await api.get(
            "/CategoriaAtendimento"
        );

        return response.data;
    },

    getById: async (id) => {
        const response = await api.get(
            `/CategoriaAtendimento/${id}`
        );

        return response.data;
    },

    create: async (categoria) => {
        const response = await api.post(
            "/CategoriaAtendimento",
            categoria
        );

        return response.data;
    },

    update: async (id, categoria) => {
        const response = await api.put(
            `/CategoriaAtendimento/${id}`,
            categoria
        );

        return response.data;
    },

    ativar: async (id) => {
        const response = await api.put(
            `/CategoriaAtendimento/${id}/ativar`
        );

        return response.data;
    },

    desativar: async (id) => {
        const response = await api.put(
            `/CategoriaAtendimento/${id}/desativar`
        );

        return response.data;
    }
};

export default categoriaService;