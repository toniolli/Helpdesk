import { useCallback, useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import notificacaoService from "../../services/notificacaoService";
import {
  Bell,
  ChevronDown,
  User,
  KeyRound,
  LogOut,
} from "lucide-react";
import chamadoService from "../../services/chamadoService";
import {
  iniciarAtualizacoesTempoReal,
  registrarAtualizacaoAposReconexao,
} from "../../services/atualizacaoTempoRealService";
import { getUserData } from "../../utils/authUtils";
import { apiErrorMessage } from "../../utils/apiError";
import TrocarSenhaModal from "./TrocarSenhaModal";
import { useAutenticacao } from "../../contexts/AutenticacaoContext";

import styles from "./topbar.module.css";

const TIPO_CHAMADO_DISTRIBUIDO_PERFIL = 1;
const TIPO_CHAMADO_DISTRIBUIDO_USUARIO = 2;
const TIPO_CHAMADO_TRANSFERIDO_USUARIO = 5;

export default function Topbar() {
  const user = getUserData();
  const { encerrarSessao } = useAutenticacao();

  const navigate = useNavigate();

  const [openMenu, setOpenMenu] = useState(false);
  const [totalNaoLidas, setTotalNaoLidas] = useState(0);
  const [openNotifications, setOpenNotifications] = useState(false);
  const [notificacoes, setNotificacoes] = useState([]);
  const [notificacaoEmAcao, setNotificacaoEmAcao] = useState(null);
  const [modalTrocaSenhaAberto, setModalTrocaSenhaAberto] = useState(false);

  const handleLogout = async () => {
    await encerrarSessao();
    navigate("/login", { replace: true });
  };

  const atualizarNotificacoes = useCallback(async () => {
    if (!user?.id) return;

    try {
      const [notificacoesAtualizadas, total] = await Promise.all([
        notificacaoService.getByUsuario(user.id),
        notificacaoService.getTotalNaoLidas(user.id),
      ]);

      setNotificacoes(notificacoesAtualizadas);
      setTotalNaoLidas(total);
    } catch (error) {
      console.error("Erro ao carregar notificações", error);
    }
  }, [user?.id]);

  useEffect(() => {
    if (!user?.id) return undefined;

    atualizarNotificacoes();

    const atualizarAoRetornarParaAba = () => {
      if (document.visibilityState === "visible") {
        atualizarNotificacoes();
      }
    };

    document.addEventListener("visibilitychange", atualizarAoRetornarParaAba);

    return () => {
      document.removeEventListener(
        "visibilitychange",
        atualizarAoRetornarParaAba,
      );
    };
  }, [user?.id, atualizarNotificacoes]);

  useEffect(() => {
    let connection;
    let ativo = true;
    const cancelarAtualizacaoAposReconexao =
      registrarAtualizacaoAposReconexao(atualizarNotificacoes);

    // Mantém a Topbar sincronizada quando o backend cria ou altera uma notificação deste usuário.
    const conectarAtualizacoesTempoReal = async () => {
      try {
        connection = await iniciarAtualizacoesTempoReal();

        if (!connection || !ativo) return;

        // Remove um registro anterior antes de adicionar o listener para evitar chamadas duplicadas.
        connection.off("notificacaoAtualizada");
        connection.on("notificacaoAtualizada", atualizarNotificacoes);
      } catch (error) {
        console.error("Erro ao conectar ao SignalR", error);
      }
    };

    conectarAtualizacoesTempoReal();

    return () => {
      ativo = false;
      cancelarAtualizacaoAposReconexao();
      // Ao desmontar a Topbar, remove sua escuta sem encerrar a conexão compartilhada.
      connection?.off("notificacaoAtualizada");
    };
  }, [atualizarNotificacoes]);

  const assumirChamadoDistribuido = async (notificacao) => {
    setNotificacaoEmAcao(notificacao.id_Notificacao);

    try {
      await chamadoService.assumir(notificacao.chamadoId);

      if (notificacao.tipo === TIPO_CHAMADO_DISTRIBUIDO_USUARIO) {
        await notificacaoService.marcarComoLida(
          notificacao.id_Notificacao,
        );
      }

      await atualizarNotificacoes();
      setOpenNotifications(false);
      navigate("/chamados/meus");
    } catch (error) {
      alert(
        apiErrorMessage(
          error,
          "Não foi possível iniciar este chamado. Atualize as notificações e tente novamente.",
        ),
      );
      await atualizarNotificacoes();
    } finally {
      setNotificacaoEmAcao(null);
    }
  };

  return (
    <header className={styles.topbar}>
      {/* =========================
          ESQUERDA
      ========================== */}

      <div className={styles.left}>
        <div className={styles.pageIndicator}>
          <img
            src="/branding/helpdesk-icon.svg"
            alt="Help Desk"
            className={styles.indicatorIcon}
          />

          <div>
            <span className={styles.systemLabel}>
              CENTRAL DE SUPORTE
            </span>

            <h1 className={styles.title}>Help Desk</h1>
          </div>
        </div>
      </div>

      {/* =========================
          DIREITA
      ========================== */}

      <div className={styles.right}>
        {/* STATUS */}

        <div className={styles.onlineStatus}>
          <span className={styles.onlineDot}></span>

          <span>Ambiente online</span>
        </div>

        {/* NOTIFICAÇÕES */}

        <div className={styles.notificationContainer}>
          <button
            className={styles.notificationButton}
            onClick={async () => {
              const deveAbrir = !openNotifications;
              setOpenNotifications(deveAbrir);

              if (deveAbrir) {
                await atualizarNotificacoes();
              }
            }}
            aria-label="Notificações"
          >
            <Bell size={19} />

            {totalNaoLidas > 0 && (
              <span className={styles.badge}>
                {totalNaoLidas > 99 ? "99+" : totalNaoLidas}
              </span>
            )}
          </button>

          {openNotifications && (
            <div className={styles.notificationDropdown}>
              <div className={styles.notificationTop}>
                <div>
                  <span className={styles.dropdownLabel}>ATUALIZAÇÕES</span>

                  <h3>Notificações</h3>
                </div>

                {totalNaoLidas > 0 && (
                  <span className={styles.notificationCount}>
                    {totalNaoLidas} nova
                    {totalNaoLidas > 1 ? "s" : ""}
                  </span>
                )}
              </div>

              <div className={styles.notificationList}>
                {notificacoes.length === 0 ? (
                  <div className={styles.emptyNotification}>
                    <div className={styles.emptyBell}>
                      <Bell size={22} />
                    </div>

                    <strong>Tudo em dia</strong>

                    <span>Você não possui novas notificações.</span>
                  </div>
                ) : (
                  notificacoes.map((notificacao) => (
                    <div
                      key={notificacao.id_Notificacao}
                      className={styles.notificationItem}
                      onClick={async () => {
                        if (
                          notificacao.tipo ===
                            TIPO_CHAMADO_TRANSFERIDO_USUARIO ||
                          notificacao.tipo === TIPO_CHAMADO_DISTRIBUIDO_PERFIL ||
                          notificacao.tipo === TIPO_CHAMADO_DISTRIBUIDO_USUARIO
                        ) {
                          return;
                        }

                        await notificacaoService.marcarComoLida(
                          notificacao.id_Notificacao,
                        );

                        await atualizarNotificacoes();
                      }}
                    >
                      <div className={styles.notificationHeader}>
                        <span className={styles.notificationTitle}>
                          {notificacao.titulo}
                        </span>

                        <span className={styles.notificationDot}></span>
                      </div>

                      <div className={styles.notificationMessage}>
                        {notificacao.mensagem}
                      </div>

                      <div className={styles.notificationDate}>
                        {new Date(notificacao.dataCriacao).toLocaleString(
                          "pt-BR",
                        )}
                      </div>

                      {(notificacao.tipo === TIPO_CHAMADO_DISTRIBUIDO_PERFIL ||
                        notificacao.tipo ===
                          TIPO_CHAMADO_DISTRIBUIDO_USUARIO) && (
                        <div className={styles.notificationActions}>
                          <button
                            className={styles.acceptBtn}
                            disabled={
                              notificacaoEmAcao === notificacao.id_Notificacao
                            }
                            onClick={(event) => {
                              event.stopPropagation();
                              assumirChamadoDistribuido(notificacao);
                            }}
                          >
                            {notificacaoEmAcao === notificacao.id_Notificacao
                              ? "Iniciando..."
                              : "Iniciar chamado"}
                          </button>
                        </div>
                      )}

                      {notificacao.tipo ===
                        TIPO_CHAMADO_TRANSFERIDO_USUARIO && (
                        <div className={styles.notificationActions}>
                          <button
                            className={styles.acceptBtn}
                            onClick={async (e) => {
                              e.stopPropagation();

                              await chamadoService.aceitarTransferencia(
                                notificacao.chamadoId,
                                notificacao.id_Notificacao,
                              );

                              await atualizarNotificacoes();
                            }}
                          >
                            Aceitar
                          </button>

                          <button
                            className={styles.rejectBtn}
                            onClick={async (e) => {
                              e.stopPropagation();

                              await chamadoService.recusarTransferencia(
                                notificacao.id_Notificacao,
                              );

                              await atualizarNotificacoes();
                            }}
                          >
                            Recusar
                          </button>
                        </div>
                      )}
                    </div>
                  ))
                )}
              </div>
            </div>
          )}
        </div>

        <div className={styles.verticalDivider}></div>

        {/* USUÁRIO */}

        <div className={styles.userContainer}>
          <button
            className={`${styles.userButton} ${
              openMenu ? styles.userButtonOpen : ""
            }`}
            onClick={() => setOpenMenu(!openMenu)}
          >
            <div className={styles.avatar}>
              <User size={17} />
            </div>

            <div className={styles.userInfo}>
              <span className={styles.userName}>{user?.nome || "Usuário"}</span>

              <span className={styles.userRole}>
                {user?.role || "Usuário do sistema"}
              </span>
            </div>

            <ChevronDown
              size={16}
              className={`${styles.userChevron} ${
                openMenu ? styles.userChevronOpen : ""
              }`}
            />
          </button>

          {openMenu && (
            <div className={styles.dropdown}>
              <div className={styles.dropdownHeader}>
                <div className={styles.dropdownAvatar}>
                  <User size={17} />
                </div>

                <div>
                  <strong>{user?.nome || "Usuário"}</strong>

                  <span>{user?.role || "Usuário do sistema"}</span>
                </div>
              </div>

              <div className={styles.dropdownDivider}></div>

              <button
                className={styles.dropdownItem}
                onClick={() => {
                  setOpenMenu(false);
                  setModalTrocaSenhaAberto(true);
                }}
              >
                <div className={styles.dropdownIcon}>
                  <KeyRound size={16} />
                </div>

                <span>Trocar senha</span>
              </button>

              <button
                className={`${styles.dropdownItem} ${styles.logoutItem}`}
                onClick={handleLogout}
              >
                <div className={styles.dropdownIcon}>
                  <LogOut size={16} />
                </div>

                <span>Sair do sistema</span>
              </button>
            </div>
          )}
        </div>
      </div>

      {modalTrocaSenhaAberto && (
        <TrocarSenhaModal
          onClose={() => setModalTrocaSenhaAberto(false)}
        />
      )}
    </header>
  );
}
