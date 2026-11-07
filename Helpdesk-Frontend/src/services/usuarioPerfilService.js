import api from "../api/axios";

const usuarioPerfilService = {
    buscarPerfisUsuario: async (usuarioId) => {
        const response = await api.get(
            `/UsuarioPerfil/usuario/${usuarioId}`
        );

        return response.data;
    },

    buscarUsuariosPerfil: async (perfilId) => {
        const response = await api.get(
            `/UsuarioPerfil/perfil/${perfilId}`
        );

        return response.data;
    },

    vincular: async (payload) => {
        const response = await api.post(
            "/UsuarioPerfil",
            payload
        );

        return response.data;
    },

    desvincular: async (usuarioId, perfilId) => {
        const response = await api.delete(
            `/UsuarioPerfil/${usuarioId}/${perfilId}`
        );

        return response.data;
    }
};

export default usuarioPerfilService;