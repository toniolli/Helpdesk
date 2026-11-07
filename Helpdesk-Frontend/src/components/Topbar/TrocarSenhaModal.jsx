import { useState } from "react";
import { createPortal } from "react-dom";
import { useNavigate } from "react-router-dom";
import { Eye, EyeOff, KeyRound, X } from "lucide-react";
import authService from "../../services/authService";
import { apiErrorMessage } from "../../utils/apiError";
import styles from "./trocarSenhaModal.module.css";
import { useAutenticacao } from "../../contexts/AutenticacaoContext";

export default function TrocarSenhaModal({ onClose }) {
  const navigate = useNavigate();
  const { encerrarSessao } = useAutenticacao();
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

  const fecharModal = () => {
    if (!enviando) onClose();
  };

  const alternarVisibilidadeSenha = (campo) => {
    setSenhasVisiveis((atual) => ({
      ...atual,
      [campo]: !atual[campo],
    }));
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    setErro("");

    try {
      setEnviando(true);

      await authService.alterarSenha({
        passwordAtual,
        novaSenha,
        confirmacaoNovaSenha,
      });

      alert("Senha alterada com sucesso.");
      await encerrarSessao();
      navigate("/login", { replace: true });
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

  return createPortal(
    <div className={styles.overlay}>
      <section
        className={styles.modal}
        role="dialog"
        aria-modal="true"
        aria-labelledby="trocar-senha-titulo"
      >
        <header className={styles.header}>
          <div className={styles.icon}>
            <KeyRound size={20} />
          </div>

          <div className={styles.titleArea}>
            <span>SEGURANÇA DA CONTA</span>
            <h2 id="trocar-senha-titulo">Alterar senha</h2>
          </div>

          <button
            type="button"
            className={styles.closeButton}
            onClick={fecharModal}
            disabled={enviando}
            aria-label="Fechar"
          >
            <X size={19} />
          </button>
        </header>

        <p className={styles.description}>
          Informe sua senha atual e escolha uma nova senha para sua conta.
        </p>

        {erro && (
          <div className={styles.errorMessage} role="alert">
            {erro}
          </div>
        )}

        <form className={styles.form} onSubmit={handleSubmit}>
          <label>
            Senha atual
            <div className={styles.passwordInput}>
              <input
                type={senhasVisiveis.atual ? "text" : "password"}
                value={passwordAtual}
                onChange={(event) => setPasswordAtual(event.target.value)}
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
          </label>

          <label>
            Nova senha
            <div className={styles.passwordInput}>
              <input
                type={senhasVisiveis.nova ? "text" : "password"}
                value={novaSenha}
                onChange={(event) => setNovaSenha(event.target.value)}
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
          </label>

          <label>
            Confirmar nova senha
            <div className={styles.passwordInput}>
              <input
                type={senhasVisiveis.confirmacao ? "text" : "password"}
                value={confirmacaoNovaSenha}
                onChange={(event) => setConfirmacaoNovaSenha(event.target.value)}
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
          </label>

          <div className={styles.actions}>
            <button
              type="button"
              className={styles.cancelButton}
              onClick={fecharModal}
              disabled={enviando}
            >
              Cancelar
            </button>

            <button type="submit" className={styles.submitButton} disabled={enviando}>
              {enviando ? "Alterando..." : "Alterar senha"}
            </button>
          </div>
        </form>
      </section>
    </div>,
    document.body,
  );
}
