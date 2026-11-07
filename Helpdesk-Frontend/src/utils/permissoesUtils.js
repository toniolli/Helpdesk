import { getUserData } from "./authUtils";

export function hasPermission(permission) {
    const user = getUserData();

    if (!user) return false;

    return user.permissoes.includes(permission);
}
