import { jwtDecode } from "jwt-decode";
import { obterAccessTokenAtual } from "../services/sessaoAutenticacaoService";

const CLAIMS = {
  id: "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
  nome: "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",
  email: "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
  role: "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
};

export function getTokenPayload() {
  const token = obterAccessTokenAtual();

  if (!token) return null;

  try {
    return jwtDecode(token);
  } catch {
    return null;
  }
}

export function isTokenValid() {
  const payload = getTokenPayload();

  return Boolean(
    payload &&
      typeof payload.exp === "number" &&
      payload.exp * 1000 > Date.now(),
  );
}

export function deveTrocarSenha() {
  const payload = getTokenPayload();
  const valor = payload?.DeveTrocarSenha;

  return valor === true ||
    String(valor).toLowerCase() === "true";
}

export function getUserData() {
  const payload = getTokenPayload();

  if (!payload) return null;

  return {
    id: payload[CLAIMS.id],
    nome: payload[CLAIMS.nome],
    email: payload[CLAIMS.email],
    role: payload[CLAIMS.role],
    perfil: payload.Perfil,
    permissoes: Array.isArray(payload.Permissao)
      ? payload.Permissao
      : payload.Permissao
        ? [payload.Permissao]
        : [],
  };
}

export function getUserId() {

    const payload =
        getTokenPayload();

    return payload?.[CLAIMS.id];
}

export function getUsername() {

    const payload =
        getTokenPayload();

    return payload?.[CLAIMS.nome];
}

export function getEmail() {

    const payload =
        getTokenPayload();

    return payload?.[CLAIMS.email];
}

export function getPerfis() {

    const payload =
        getTokenPayload();

    const perfis =
        payload?.Perfil;

    if (!perfis) {
        return [];
    }

    return Array.isArray(perfis)
        ? perfis
        : [perfis];
}

export function getPermissoes() {

    const payload =
        getTokenPayload();

    const permissoes =
        payload?.Permissao;

    if (!permissoes) {
        return [];
    }

    return Array.isArray(permissoes)
        ? permissoes
        : [permissoes];
}

export function hasPerfil(
    perfil
) {
    return getPerfis()
        .includes(perfil);
}

export function hasPermissao(
    permissao
) {
    return getPermissoes()
        .includes(permissao);
}
