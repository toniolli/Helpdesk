import api from "../api/axios";

const authService = {
    login: async (userName, password) => {
        const response = await api.post(
            "/Usuario/login",
            {
                userName,
                password
            }
        );

        return response.data;
    },


    alterarSenha: async (dados) => {
    const response = await api.post(
        "/Usuario/alterar-senha",
        dados
        );

    return response.data;
    },

    recuperarSenha: async (userName) => {
        const response = await api.post(
            "/Usuario/esqueci-senha",
            { userName }
        );

        return response.data;
    }
};

export default authService;
