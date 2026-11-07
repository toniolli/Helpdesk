import { useEffect, useState } from "react";
import { NavLink, useLocation, useNavigate } from "react-router-dom";
import { hasPermission } from "../../utils/permissoesUtils";
import { useAutenticacao } from "../../contexts/AutenticacaoContext";
import {
  Menu,
  Home,
  Ticket,
  Users,
  Shield,
  Tags,
  LogOut,
  KeyRound,
  ChevronDown,
  ChevronRight,
  Clock,
  GitBranch,
  ListChecks,
  ScrollText,
  Activity,
  MonitorUp,
} from "lucide-react";

import styles from "./sidebar.module.css";

export default function Sidebar() {
  const { encerrarSessao } = useAutenticacao();
  const [open, setOpen] = useState(() =>
    typeof window === "undefined" ||
    !window.matchMedia("(max-width: 768px)").matches,
  );
  const [chamadosOpen, setChamadosOpen] = useState(false);
  const [distribuicaoOpen, setDistribuicaoOpen] = useState(false);
  const [auditoriaOpen, setAuditoriaOpen] = useState(false);

  const navigate = useNavigate();
  const location = useLocation();
  const distribuicaoAtiva = location.pathname.startsWith("/distribuicao");
  const auditoriaAtiva = location.pathname.startsWith("/auditoria");
  const podeVisualizarCategorias =
    hasPermission("VISUALIZAR_CATEGORIAS") ||
    hasPermission("GERENCIAR_CATEGORIAS");
  const podeVisualizarAuditoriaGeral = hasPermission(
    "VISUALIZAR_AUDITORIA",
  );
  const podeVisualizarAuditoriaChamado = hasPermission(
    "VISUALIZAR_AUDITORIA_CHAMADO",
  );
  const podeVisualizarAuditoria =
    podeVisualizarAuditoriaGeral || podeVisualizarAuditoriaChamado;

  useEffect(() => {
    if (auditoriaAtiva) {
      setOpen(true);
      setAuditoriaOpen(true);
    }
  }, [auditoriaAtiva]);

  const logout = async () => {
    await encerrarSessao();
    navigate("/login", { replace: true });
  };

  return (
    <aside
      className={`${styles.sidebar} ${open ? styles.open : styles.closed}`}
    >
      {/* Fundo decorativo */}
      <div className={styles.sidebarGlow}></div>
      <div className={styles.sidebarPattern}></div>

      {/* =========================
          TOPO
      ========================== */}

      <div className={styles.top}>
        <div className={styles.brand}>
          <img
            src="/branding/helpdesk-icon.svg"
            alt="Help Desk"
            className={styles.logoIcon}
          />

          {open && (
            <div className={styles.logoContent}>
              <span className={styles.logoSmall}>CENTRAL DE SUPORTE</span>
              <h2 className={styles.logo}>Help Desk</h2>
            </div>
          )}
        </div>

        <button
          type="button"
          className={styles.menuButton}
          onClick={() => setOpen(!open)}
          aria-label={open ? "Fechar menu" : "Abrir menu"}
        >
          <Menu size={19} />
        </button>
      </div>

      {/* =========================
          NAVEGAÇÃO
      ========================== */}

      <nav className={styles.menu}>
        {open && <span className={styles.menuLabel}>NAVEGAÇÃO</span>}

        {hasPermission("VISUALIZAR_DASHBOARD") && (
          <NavLink
            to="/dashboard"
            title="Dashboard"
            className={({ isActive }) =>
              `${styles.menuItem} ${isActive ? styles.active : ""}`
            }
          >
            <Home size={19} />

            {open && <span>Dashboard</span>}
          </NavLink>
        )}

        {hasPermission("VISUALIZAR_CENTRAL_ATENDIMENTO") && (
          <a
            href="/central-atendimento"
            target="_blank"
            rel="noopener noreferrer"
            title="Central de Atendimento"
            className={styles.menuItem}
          >
            <MonitorUp size={19} />
            {open && <span>Central de Atendimento</span>}
          </a>
        )}

        {/* =========================
            CHAMADOS
        ========================== */}

        <div className={styles.menuGroup}>
          <div
            className={`${styles.menuItem} ${
              chamadosOpen ? styles.menuItemOpen : ""
            }`}
            onClick={() => setChamadosOpen(!chamadosOpen)}
            title="Chamados"
          >
            <Ticket size={19} />

            {open && (
              <>
                <span>Chamados</span>

                <div className={styles.chevron}>
                  {chamadosOpen ? (
                    <ChevronDown size={16} />
                  ) : (
                    <ChevronRight size={16} />
                  )}
                </div>
              </>
            )}
          </div>

          {open && chamadosOpen && (
            <div className={styles.subMenu}>
              {hasPermission("ABRIR_CHAMADO") && (
                <NavLink
                  to="/chamados/novo"
                  className={({ isActive }) =>
                    `${styles.subItem} ${isActive ? styles.active : ""}`
                  }
                >
                  <span className={styles.subItemDot}></span>
                  Abrir Chamado
                </NavLink>
              )}

              {hasPermission("VISUALIZAR_FILA") && (
                <NavLink
                  to="/chamados/fila"
                  className={({ isActive }) =>
                    `${styles.subItem} ${isActive ? styles.active : ""}`
                  }
                >
                  <span className={styles.subItemDot}></span>
                  Fila de Atendimento
                </NavLink>
              )}

              {hasPermission("ASSUMIR_CHAMADO") && (
                <NavLink
                  to="/chamados/meus"
                  className={({ isActive }) =>
                    `${styles.subItem} ${isActive ? styles.active : ""}`
                  }
                >
                  <span className={styles.subItemDot}></span>
                  Chamados Técnico
                </NavLink>
              )}

              {hasPermission("ABRIR_CHAMADO") && (
                <NavLink
                  to="/chamados/meusSolicitante"
                  className={({ isActive }) =>
                    `${styles.subItem} ${isActive ? styles.active : ""}`
                  }
                >
                  <span className={styles.subItemDot}></span>
                  Chamados
                </NavLink>
              )}

              {hasPermission("VISUALIZAR_HISTORICO") && (
                <NavLink
                  to="/chamados/historicoChamados"
                  className={({ isActive }) =>
                    `${styles.subItem} ${isActive ? styles.active : ""}`
                  }
                >
                  <span className={styles.subItemDot}></span>
                  Histórico Chamados
                </NavLink>
              )}
            </div>
          )}
        </div>

        {podeVisualizarAuditoriaGeral && (
          <NavLink
            to="/monitoramento"
            title="Monitoramento"
            className={({ isActive }) =>
              `${styles.menuItem} ${isActive ? styles.active : ""}`
            }
          >
            <Activity size={19} />
            {open && <span>Monitoramento</span>}
          </NavLink>
        )}

        {podeVisualizarAuditoria && (
          <div className={styles.menuGroup}>
            <button
              type="button"
              className={`${styles.menuItem} ${
                auditoriaOpen || auditoriaAtiva ? styles.menuItemOpen : ""
              }`}
              onClick={() => {
                setOpen(true);
                setAuditoriaOpen((atual) => !atual);
              }}
              title="Auditoria"
              aria-expanded={auditoriaOpen}
            >
              <ScrollText size={19} />
              {open && (
                <>
                  <span>Auditoria</span>
                  <div className={styles.chevron}>
                    {auditoriaOpen ? (
                      <ChevronDown size={16} />
                    ) : (
                      <ChevronRight size={16} />
                    )}
                  </div>
                </>
              )}
            </button>

            {open && auditoriaOpen && (
              <div className={styles.subMenu}>
                {podeVisualizarAuditoriaGeral && (
                  <>
                    <NavLink
                      to="/auditoria/geral"
                      className={({ isActive }) =>
                        `${styles.subItem} ${isActive ? styles.active : ""}`
                      }
                    >
                      <span className={styles.subItemDot} />
                      Auditoria geral
                    </NavLink>

                    <NavLink
                      to="/auditoria/login"
                      className={({ isActive }) =>
                        `${styles.subItem} ${isActive ? styles.active : ""}`
                      }
                    >
                      <span className={styles.subItemDot} />
                      Auditoria de login
                    </NavLink>
                  </>
                )}

                {podeVisualizarAuditoriaChamado && (
                  <NavLink
                    to="/auditoria/chamados"
                    className={({ isActive }) =>
                      `${styles.subItem} ${isActive ? styles.active : ""}`
                    }
                  >
                    <span className={styles.subItemDot} />
                    Auditoria de chamados
                  </NavLink>
                )}
              </div>
            )}
          </div>
        )}

        {hasPermission("GERENCIAR_USUARIOS") && (
          <NavLink
            to="/usuarios"
            title="Usuários"
            className={({ isActive }) =>
              `${styles.menuItem} ${isActive ? styles.active : ""}`
            }
          >
            <Users size={19} />

            {open && <span>Usuários</span>}
          </NavLink>
        )}

        {hasPermission("GERENCIAR_PERFIS") && (
          <NavLink
            to="/perfis"
            title="Perfis"
            className={({ isActive }) =>
              `${styles.menuItem} ${isActive ? styles.active : ""}`
            }
          >
            <Shield size={19} />

            {open && <span>Perfis</span>}
          </NavLink>
        )}

        {hasPermission("GERENCIAR_PERMISSOES") && (
          <NavLink
            to="/permissoes"
            title="Permissões"
            className={({ isActive }) =>
              `${styles.menuItem} ${isActive ? styles.active : ""}`
            }
          >
            <KeyRound size={19} />

            {open && <span>Permissões</span>}
          </NavLink>
        )}

        {podeVisualizarCategorias && (
          <NavLink
            to="/categorias"
            title="Categorias"
            className={({ isActive }) =>
              `${styles.menuItem} ${isActive ? styles.active : ""}`
            }
          >
            <Tags size={19} />

            {open && <span>Categorias</span>}
          </NavLink>
        )}

        {hasPermission("VISUALIZAR_SLA") && (
          <NavLink
            to="/sla"
            title="SLA"
            className={({ isActive }) =>
              `${styles.menuItem} ${isActive ? styles.active : ""}`
            }
          >
            <Clock size={19} />

            {open && <span>SLA</span>}
          </NavLink>
        )}

        {hasPermission("GERENCIAR_DISTRIBUICAO") && (
          <div className={styles.menuGroup}>
            <button
              type="button"
              className={`${styles.menuItem} ${
                distribuicaoOpen || distribuicaoAtiva ? styles.menuItemOpen : ""
              }`}
              onClick={() => {
                setOpen(true);
                setDistribuicaoOpen((atual) => !atual);
              }}
              title="Distribuição"
              aria-expanded={distribuicaoOpen}
            >
              <GitBranch size={19} />
              {open && (
                <>
                  <span>Distribuição</span>
                  <div className={styles.chevron}>
                    {distribuicaoOpen ? (
                      <ChevronDown size={16} />
                    ) : (
                      <ChevronRight size={16} />
                    )}
                  </div>
                </>
              )}
            </button>

            {open && distribuicaoOpen && (
              <div className={styles.subMenu}>
                <NavLink
                  to="/distribuicao"
                  end
                  className={({ isActive }) =>
                    `${styles.subItem} ${isActive ? styles.active : ""}`
                  }
                >
                  <span className={styles.subItemDot} />
                  Regras automáticas
                </NavLink>
                <NavLink
                  to="/distribuicao/aplicar"
                  className={({ isActive }) =>
                    `${styles.subItem} ${isActive ? styles.active : ""}`
                  }
                >
                  <ListChecks size={15} />
                  Regras aplicadas
                </NavLink>
              </div>
            )}
          </div>
        )}
      </nav>

      {/* =========================
          FOOTER / LOGOUT
      ========================== */}

      <div className={styles.bottom}>
        {open && (
          <div className={styles.systemStatus}>
            <span className={styles.systemStatusDot}></span>

            <div>
              <span className={styles.systemStatusLabel}>AMBIENTE</span>

              <span className={styles.systemStatusText}>Online</span>
            </div>
          </div>
        )}

        <div className={styles.logout} onClick={logout} title="Sair">
          <div className={styles.logoutIcon}>
            <LogOut size={18} />
          </div>

          {open && <span>Sair do sistema</span>}
        </div>
      </div>
    </aside>
  );
}
