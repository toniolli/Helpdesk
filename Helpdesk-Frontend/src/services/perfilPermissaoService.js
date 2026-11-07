import api from "../api/axios";

const perfilPermissaoService = {
    buscarPermissoesPerfil: async (perfilId) => {
        const response = await api.get(
            `/PerfilPermissao/perfil/${perfilId}`
        );

        return response.data;
    },

    buscarPerfisPermissao: async (permissaoId) => {
        const response = await api.get(
            `/PerfilPermissao/permissao/${permissaoId}`
        );

        return response.data;
    },

    vincular: async (payload) => {
        const response = await api.post(
            "/PerfilPermissao",
            payload
        );

        return response.data;
    },

    desvincular: async (perfilId, permissaoId) => {
        const response = await api.delete(
            `/PerfilPermissao/${perfilId}/${permissaoId}`
        );

        return response.data;
    }
};

export default perfilPermissaoService;