import { Navigate } from "react-router-dom";
import { deveTrocarSenha } from "../utils/authUtils";
import { hasPermission } from "../utils/permissoesUtils";
import { useAutenticacao } from "../contexts/AutenticacaoContext";

export default function ProtectedRoute({
    children,
    permission,
    permitirTrocaSenha = false,
}) {
    const { autenticado, inicializando } = useAutenticacao();

    if (inicializando) return null;

    if (!autenticado) {
        return <Navigate to="/login" replace />;
    }

    if (deveTrocarSenha() && !permitirTrocaSenha) {
        return <Navigate to="/trocar-senha" replace />;
    }

    const permissoesNecessarias = Array.isArray(permission)
        ? permission
        : [permission];

    const possuiAlgumaPermissaoNecessaria = permissoesNecessarias
        .filter(Boolean)
        .some((permissaoNecessaria) => hasPermission(permissaoNecessaria));

    if (permission && !possuiAlgumaPermissaoNecessaria) {
        return <Navigate to="/acesso-negado" replace />;
    }

    return children;
}
