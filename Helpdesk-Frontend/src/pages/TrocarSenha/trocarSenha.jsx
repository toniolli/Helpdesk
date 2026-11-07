import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Eye, EyeOff } from "lucide-react";
import authService from "../../services/authService";
import { deveTrocarSenha } from "../../utils/authUtils";
import { apiErrorMessage } from "../../utils/apiError";
import styles from "./trocarSenha.module.css";
import { useAutenticacao } from "../../contexts/AutenticacaoContext";

export default function TrocarSenha() {
  const navigate = useNavigate();
  const { encerrarSessao } = useAutenticacao();
  const trocaObrigatoria = deveTrocarSenha();
  const [passwordAtual, setPasswordAtual] = useState("");
  const [novaSenha, setNovaSenha] = useState("");
  const [confirmacaoNovaSenha, setConfirmacaoNovaSenha] = useState("");
  const [senhasVisiveis, setSenhasVisiveis] = useState({
    atual: false,
    nova: false,
    confirmacao: false,
  });
  const [enviando, setEnviando] = useState(false);
  const [erro, setErro] = useState("");

  const sair = async () => {
    await encerrarSessao();
    navigate("/login", { replace: true });
  };

  const alternarVisibilidadeSenha = (campo) => {
    setSenhasVisiveis((atual) => ({
      ...atual,
      [campo]: !atual[campo],
    }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setErro("");

    try {
      setEnviando(true);

      await authService.alterarSenha({
        passwordAtual,
        novaSenha,
        confirmacaoNovaSenha,
      });

      alert("Senha alterada com sucesso.");

      await sair();
    } catch (error) {
      console.error(error);

      setErro(
        apiErrorMessage(
          error,
          "Não foi possível alterar a senha.",
        ),
      );
    } finally {
      setEnviando(false);
    }
  };

  return (
    <div className={styles.container}>
      <div className={styles.backgroundGlow}></div>
      <div className={styles.backgroundGlowSecondary}></div>
      <div className={styles.backgroundGrid}></div>
      <div className={styles.backgroundCircle}></div>

      <div className={styles.card}>
        <div className={styles.securityHeader}>
          <img
            src="/branding/helpdesk-icon.svg"
            alt=""
            className={styles.companyIcon}
          />

          <span className={styles.eyebrow}>SEGURANÇA DE ACESSO</span>
        </div>

        <h2>
          {trocaObrigatoria
            ? "Crie uma senha para continuar"
            : "Alterar senha"}
        </h2>

        <p className={styles.description}>
          {trocaObrigatoria
            ? "Sua senha foi restaurada para o padrão. Para continuar, cadastre uma nova senha pessoal."
            : "Informe sua senha atual e escolha uma nova senha para sua conta."}
        </p>

        {erro && (
          <div className={styles.errorMessage} role="alert">
            {erro}
          </div>
        )}

        <form className={styles.form} onSubmit={handleSubmit}>
          <div className={styles.passwordInput}>
            <input
              type={senhasVisiveis.atual ? "text" : "password"}
              placeholder="Senha Atual"
              value={passwordAtual}
              onChange={(e) => setPasswordAtual(e.target.value)}
              autoComplete="current-password"
              disabled={enviando}
              required
            />
            <button
              type="button"
              className={styles.passwordToggle}
              onClick={() => alternarVisibilidadeSenha("atual")}
              aria-label={senhasVisiveis.atual ? "Ocultar senha atual" : "Mostrar senha atual"}
              disabled={enviando}
            >
              {senhasVisiveis.atual ? <EyeOff size={18} /> : <Eye size={18} />}
            </button>
          </div>

          <div className={styles.passwordInput}>
            <input
              type={senhasVisiveis.nova ? "text" : "password"}
              placeholder="Nova Senha"
              value={novaSenha}
              onChange={(e) => setNovaSenha(e.target.value)}
              autoComplete="new-password"
              disabled={enviando}
              required
            />
            <button
              type="button"
              className={styles.passwordToggle}
              onClick={() => alternarVisibilidadeSenha("nova")}
              aria-label={senhasVisiveis.nova ? "Ocultar nova senha" : "Mostrar nova senha"}
              disabled={enviando}
            >
              {senhasVisiveis.nova ? <EyeOff size={18} /> : <Eye size={18} />}
            </button>
          </div>

          <div className={styles.passwordInput}>
            <input
              type={senhasVisiveis.confirmacao ? "text" : "password"}
              placeholder="Confirmar Nova Senha"
              value={confirmacaoNovaSenha}
              onChange={(e) => setConfirmacaoNovaSenha(e.target.value)}
              autoComplete="new-password"
              disabled={enviando}
              required
            />
            <button
              type="button"
              className={styles.passwordToggle}
              onClick={() => alternarVisibilidadeSenha("confirmacao")}
              aria-label={senhasVisiveis.confirmacao ? "Ocultar confirmação de senha" : "Mostrar confirmação de senha"}
              disabled={enviando}
            >
              {senhasVisiveis.confirmacao ? <EyeOff size={18} /> : <Eye size={18} />}
            </button>
          </div>

          <button type="submit" disabled={enviando}>
            {enviando ? "Alterando..." : "Alterar Senha"}
          </button>

          <button
            type="button"
            className={styles.secondaryButton}
            onClick={sair}
            disabled={enviando}
          >
            Sair e voltar ao login
          </button>
        </form>

        <p className={styles.companyFooter}>
          Help Desk <span>•</span> Suporte para sua operação
        </p>
      </div>
    </div>
  );
}
