import { createContext, useContext } from "react";

export const AutenticacaoContext = createContext(null);

export function useAutenticacao() {
  const contexto = useContext(AutenticacaoContext);

  if (!contexto) {
    throw new Error(
      "useAutenticacao deve ser usado dentro de AutenticacaoProvider.",
    );
  }

  return contexto;
}
