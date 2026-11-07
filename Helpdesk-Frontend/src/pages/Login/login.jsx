import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import authService from "../../services/authService";
import styles from "./login.module.css";
import {
  deveTrocarSenha,
} from "../../utils/authUtils";
import { useAutenticacao } from "../../contexts/AutenticacaoContext";
import { definirSessao } from "../../services/sessaoAutenticacaoService";
import { registrarAtividade } from "../../services/atividadeSessaoService";

const MENSAGEM_RECUPERACAO =
  "Se houver uma conta ativa com e-mail cadastrado, as instruções de recuperação foram enviadas.";

export default function Login() {
  const navigate = useNavigate();
  const { autenticado, inicializando } = useAutenticacao();

  const [userName, setUserName] = useState("");
  const [password, setPassword] = useState("");
  const [loading, setLoading] = useState(false);
  const [openForgotModal, setOpenForgotModal] = useState(false);
  const [recoveryUser, setRecoveryUser] = useState("");
  const [enviandoRecuperacao, setEnviandoRecuperacao] = useState(false);
  const [recuperacaoConcluida, setRecuperacaoConcluida] = useState(false);
  const [erroRecuperacao, setErroRecuperacao] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [erroLogin, setErroLogin] = useState("");

  useEffect(() => {
    if (!inicializando && autenticado) {
      navigate(
        deveTrocarSenha()
          ? "/trocar-senha"
          : "/transition",
        { replace: true },
      );
    }
  }, [autenticado, inicializando, navigate]);

  const handleLogin = async (e) => {
    setErroLogin("");
    e.preventDefault();

    try {
      setLoading(true);

      const respostaAutenticacao = await authService.login(userName, password);
      registrarAtividade(true);
      definirSessao(respostaAutenticacao);

      navigate(
        deveTrocarSenha()
          ? "/trocar-senha"
          : "/transition",
      );
    } catch (error) {
      const status = error.response?.status;
      const mensagem = error.response?.data?.error;

      if (status === 429) {
        setErroLogin(
          mensagem || "Muitas tentativas. Aguarde um minuto e tente novamente.",
        );
      } else if (mensagem?.includes("Muitas tentativas")) {
        setErroLogin(mensagem);
      } else {
        setErroLogin("Usuário ou senha inválidos.");
      }
    } finally {
      setLoading(false);
    }
  };

  const abrirRecuperacaoSenha = () => {
    setRecoveryUser("");
    setErroRecuperacao("");
    setRecuperacaoConcluida(false);
    setOpenForgotModal(true);
  };

  const fecharRecuperacaoSenha = () => {
    if (!enviandoRecuperacao) {
      setOpenForgotModal(false);
    }
  };

  const handleRecuperarSenha = async (event) => {
    event.preventDefault();
    setErroRecuperacao("");

    try {
      setEnviandoRecuperacao(true);
      await authService.recuperarSenha(recoveryUser);
      setRecuperacaoConcluida(true);
    } catch (error) {
      setErroRecuperacao(
        error.response?.status === 429
          ? "Muitas tentativas. Aguarde um minuto e tente novamente."
          : "Não foi possível solicitar a recuperação agora. Tente novamente mais tarde.",
      );
    } finally {
      setEnviandoRecuperacao(false);
    }
  };

  return (
    <>
      <div className={styles.container}>
        <div className={styles.backgroundGlow}></div>

        <main className={styles.loginLayout}>
          <section className={styles.brandSection}>
            <div className={styles.brandOverlay}></div>

            <img
              src="/Login.png"
              alt="Ambiente de tecnologia"
              className={styles.loginImage}
            />

            <div className={styles.brandContent}>
              <div className={styles.brandBadge}>
                <span className={styles.statusDot}></span>
                SISTEMA ONLINE
              </div>

              <div className={styles.brandText}>
                <span className={styles.brandLabel}>
                  CENTRAL DE SUPORTE
                </span>

                <h1>
                  Suporte que mantém
                  <br />
                  <strong>sua operação sempre em movimento.</strong>
                </h1>

                <p>
                  Gerencie chamados, acompanhe solicitações e conecte sua equipe
                  de forma simples e eficiente.
                </p>
              </div>

              <div className={styles.techInfo}>
                <div className={styles.techLine}></div>

                <div className={styles.techStatus}>
                  <span>HELP DESK</span>
                  <span>•</span>
                  <span>SUPORTE PARA SUA OPERAÇÃO</span>
                </div>
              </div>
            </div>
          </section>

          <section className={styles.formSection}>
            <div className={styles.formPanel}>
              <div className={styles.logoArea}>
                <div className={styles.portalIdentity}>
                  <img
                    src="/branding/helpdesk-icon.svg"
                    alt=""
                    className={styles.companyIcon}
                  />

                  <div>
                    <span className={styles.systemLabel}>PORTAL DE ACESSO</span>

                    <h2>Help Desk</h2>
                  </div>
                </div>
              </div>

              <div className={styles.header}>
                <h3>Bem-vindo de volta</h3>
                <p>Informe suas credenciais para acessar o sistema.</p>
              </div>

              <form className={styles.form} onSubmit={handleLogin}>
                <div className={styles.field}>
                  <label>Usuário</label>

                  <div className={styles.inputWrapper}>
                    <span className={styles.inputIcon}>
                      <svg
                        viewBox="0 0 24 24"
                        fill="none"
                        stroke="currentColor"
                        strokeWidth="1.8"
                      >
                        <path d="M20 21a8 8 0 0 0-16 0" />
                        <circle cx="12" cy="7" r="4" />
                      </svg>
                    </span>

                    <input
                      type="text"
                      placeholder="Digite seu usuário"
                      value={userName}
                      onChange={(e) => setUserName(e.target.value)}
                      autoComplete="username"
                      required
                    />
                  </div>
                </div>

                <div className={styles.field}>
                  <label>Senha</label>

                  <div className={styles.inputWrapper}>
                    <span className={styles.inputIcon}>
                      <svg
                        viewBox="0 0 24 24"
                        fill="none"
                        stroke="currentColor"
                        strokeWidth="1.8"
                      >
                        <rect x="4" y="10" width="16" height="10" rx="2" />
                        <path d="M8 10V7a4 4 0 0 1 8 0v3" />
                      </svg>
                    </span>

                    <input
                      type={showPassword ? "text" : "password"}
                      placeholder="Digite sua senha"
                      value={password}
                      onChange={(e) => setPassword(e.target.value)}
                      autoComplete="current-password"
                      required
                    />
                  </div>
                </div>

                <div className={styles.formOptions}>
                  <div className={styles.showPassword}>
                    <label>
                      <input
                        type="checkbox"
                        checked={showPassword}
                        onChange={() => setShowPassword(!showPassword)}
                      />
                      <span>Mostrar senha</span>
                    </label>
                  </div>

                  <div className={styles.forgotPassword}>
                    <button
                      type="button"
                      onClick={abrirRecuperacaoSenha}
                    >
                      Esqueci minha senha
                    </button>
                  </div>
                </div>
                {erroLogin && (
                  <p className={styles.errorMessage}>{erroLogin}</p>
                )}
                <button
                  type="submit"
                  className={styles.button}
                  disabled={loading || inicializando}
                >
                  <span>{loading ? "Entrando..." : "Entrar no sistema"}</span>

                  {!loading && (
                    <svg
                      viewBox="0 0 24 24"
                      fill="none"
                      stroke="currentColor"
                      strokeWidth="2"
                    >
                      <path d="M5 12h14" />
                      <path d="m13 6 6 6-6 6" />
                    </svg>
                  )}
                </button>
              </form>

              <div className={styles.footer}>
                <span>Helpdesk Corporativo</span>

                <div className={styles.version}>
                  <span className={styles.versionDot}></span>
                  v1.0.0
                </div>
              </div>
            </div>
          </section>
        </main>
      </div>

      {openForgotModal && (
        <div className={styles.overlay}>
          <div className={styles.modal}>
            <div className={styles.modalHeader}>
              <div className={styles.modalIcon}>?</div>

              <div>
                <span>SUPORTE</span>
                <h2>Recuperação de Senha</h2>
              </div>
            </div>

            {recuperacaoConcluida ? (
              <div className={styles.recoverySuccess} role="status">
                <p>{MENSAGEM_RECUPERACAO}</p>

                <button
                  type="button"
                  className={styles.requestButton}
                  onClick={fecharRecuperacaoSenha}
                >
                  Voltar ao login
                </button>
              </div>
            ) : (
              <form onSubmit={handleRecuperarSenha}>
                <p className={styles.modalDescription}>
                  Informe seu usuário para solicitar a recuperação de acesso.
                </p>

                <div className={styles.field}>
                  <label>Usuário</label>

                  <div className={styles.inputWrapper}>
                    <span className={styles.inputIcon}>
                      <svg
                        viewBox="0 0 24 24"
                        fill="none"
                        stroke="currentColor"
                        strokeWidth="1.8"
                      >
                        <path d="M20 21a8 8 0 0 0-16 0" />
                        <circle cx="12" cy="7" r="4" />
                      </svg>
                    </span>

                    <input
                      type="text"
                      placeholder="Digite seu usuário"
                      value={recoveryUser}
                      onChange={(e) => setRecoveryUser(e.target.value)}
                      autoComplete="username"
                      disabled={enviandoRecuperacao}
                      required
                    />
                  </div>
                </div>

                {erroRecuperacao && (
                  <p className={styles.modalErrorMessage} role="alert">
                    {erroRecuperacao}
                  </p>
                )}

                <div className={styles.actions}>
                  <button
                    type="button"
                    className={styles.cancelButton}
                    onClick={fecharRecuperacaoSenha}
                    disabled={enviandoRecuperacao}
                  >
                    Cancelar
                  </button>

                  <button
                    type="submit"
                    className={styles.requestButton}
                    disabled={enviandoRecuperacao}
                  >
                    {enviandoRecuperacao
                      ? "Enviando..."
                      : "Solicitar recuperação"}
                  </button>
                </div>
              </form>
            )}
          </div>
        </div>
      )}
    </>
  );
}
