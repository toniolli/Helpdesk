import axios from "axios";
import { API_URL } from "./apiConfig";
import {
    obterAccessTokenAtual,
    obterAccessTokenValido,
    possuiSessaoNormal,
    renovarSessao,
} from "../services/sessaoAutenticacaoService";

const api = axios.create({
    baseURL: API_URL,
    withCredentials: true,
});

const rotasSemAutenticacao = [
    "/Usuario/login",
    "/Usuario/renovar",
    "/Usuario/logout",
    "/Usuario/esqueci-senha",
];

function rotaDispensaAutenticacao(url = "") {
    return rotasSemAutenticacao.some((rota) => url.includes(rota));
}

api.interceptors.request.use(
    async (config) => {
        if (rotaDispensaAutenticacao(config.url)) return config;

        const token = await obterAccessTokenValido();

        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }

        return config;
    },
    (error) => Promise.reject(error)
);

api.interceptors.response.use(
    (response) => response,
    async (error) => {
        const requisicaoOriginal = error.config;

        if (
            error.response?.status !== 401 ||
            !requisicaoOriginal ||
            requisicaoOriginal._retryAutenticacao ||
            rotaDispensaAutenticacao(requisicaoOriginal.url) ||
            !possuiSessaoNormal()
        ) {
            return Promise.reject(error);
        }

        requisicaoOriginal._retryAutenticacao = true;

        try {
            const tokenUsado = requisicaoOriginal.headers?.Authorization
                ?.replace(/^Bearer\s+/i, "") || obterAccessTokenAtual();
            const novoToken = await renovarSessao(tokenUsado);

            if (!novoToken) return Promise.reject(error);

            requisicaoOriginal.headers.Authorization = `Bearer ${novoToken}`;
            return api(requisicaoOriginal);
        } catch (erroRenovacao) {
            return Promise.reject(erroRenovacao);
        }
    },
);

export default api;
