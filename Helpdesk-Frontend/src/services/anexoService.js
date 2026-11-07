import api from "../api/axios";

const anexoService = {
    getById: async (id) => {
        const response = await api.get(
            `/Anexo/${id}`
        );

        return response.data;
    },

    download: async (id) => {
        const response = await api.get(
            `/Anexo/download/${id}`,
            {
                responseType: "blob"
            }
        );

        return response;
    },

    getByComentario: async (
        comentarioId
    ) => {
        const response = await api.get(
            `/Anexo/comentario/${comentarioId}`
        );

        return response.data;
    },

    delete: async (id) => {
        const response =
            await api.delete(
                `/Anexo/${id}`
            );

        return response.data;
    }
};

export default anexoService;
