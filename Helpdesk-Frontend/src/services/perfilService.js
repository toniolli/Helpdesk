import api from "../api/axios";

const perfilService = {
    getAll: async () => {
        const response = await api.get("/Perfil");
        return response.data;
    },

    buscarOpcoesFiltro: async () => {
        const response = await api.get("/Perfil/opcoes-filtro");
        return response.data;
    },

    getById: async (id) => {
        const response = await api.get(`/Perfil/${id}`);
        return response.data;
    },

    create: async (perfil) => {
        const response = await api.post("/Perfil", perfil);
        return response.data;
    },

    update: async (id, perfil) => {
        const response = await api.put(`/Perfil/${id}`, perfil);
        return response.data;
    },

    remove: async (id) => {
        const response = await api.delete(`/Perfil/${id}`);
        return response.data;
    },
    
    ativar: async (id) => {
        const response = await api.put(
            `/Perfil/${id}/ativar`
        );

        return response.data;
    },

    desativar: async (id) => {
        const response = await api.put(
            `/Perfil/${id}/desativar`
        );

        return response.data;
    }

};

export default perfilService;
