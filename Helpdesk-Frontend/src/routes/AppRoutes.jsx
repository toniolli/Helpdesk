import { Routes, Route, Navigate } from "react-router-dom";
import Login from "../pages/Login/login";
import MainLayout from "../layouts/mainLayout";
import TrocarSenha from "../pages/TrocarSenha/trocarSenha";
import Usuarios from "../pages/Usuarios/usuarios";
import Perfil from "../pages/Perfil/perfil";
import Permissao from "../pages/Permissao/permissao";
import Categoria from "../pages/Categoria/categoria";
import Sla from "../pages/Sla/sla";
import AbrirChamado from "../pages/Chamados/abrirChamado";
import FilaAtendimento from "../pages/Chamados/filaAtendimento";
import MeusChamadosTecnico from "../pages/Chamados/meusChamadosTecnico";
import MeusChamadosSolicitante from "../pages/Chamados/meusChamadosSolicitante";
import ProtectedRoute from "./ProtectedRoute";
import Dashboard from "../pages/Dashboard/dashboard";
import HistoricoChamados from "../pages/Chamados/historicoChamados";
import Transition from "../pages/Transition/transition";
import Distribuicao from "../pages/Distribuicao/distribuicao";
import AplicarDistribuicao from "../pages/Distribuicao/aplicarDistribuicao";
import AuditoriaGeral from "../pages/Auditoria/auditoriaGeral";
import AuditoriaLogin from "../pages/Auditoria/auditoriaLogin";
import AuditoriaChamados from "../pages/Auditoria/auditoriaChamados";
import Monitoramento from "../pages/Monitoramento/monitoramento";
import AcessoNegado from "../pages/AcessoNegado/acessoNegado";
import CentralAtendimento from "../pages/CentralAtendimento/centralAtendimento";

export default function AppRoutes() {
  return (
    <Routes>
        {/* Login */}
        <Route path="/login" element={<Login />} />
        <Route path="/transition" element={<Transition />} />
        <Route
          path="/trocar-senha"
          element={
            <ProtectedRoute permitirTrocaSenha>
              <TrocarSenha />
            </ProtectedRoute>
          }
        />
        <Route path="/" element={<Navigate to="/dashboard" replace />} />
        <Route
          path="/central-atendimento"
          element={
            <ProtectedRoute permission="VISUALIZAR_CENTRAL_ATENDIMENTO">
              <CentralAtendimento />
            </ProtectedRoute>
          }
        />
        {/* Área protegida */}
        <Route
          element={
            <ProtectedRoute>
              <MainLayout />
            </ProtectedRoute>
          }
        >
          <Route
            path="/dashboard"
            element={
              <ProtectedRoute permission="VISUALIZAR_DASHBOARD">
                <Dashboard />
              </ProtectedRoute>
            }
          />

          <Route
            path="/usuarios"
            element={
              <ProtectedRoute permission="GERENCIAR_USUARIOS">
                <Usuarios />
              </ProtectedRoute>
            }
          />
          <Route
            path="/perfis"
            element={
              <ProtectedRoute permission="GERENCIAR_PERFIS">
                <Perfil />
              </ProtectedRoute>
            }
          />
          <Route
            path="/permissoes"
            element={
              <ProtectedRoute permission="GERENCIAR_PERMISSOES">
                <Permissao />
              </ProtectedRoute>
            }
          />
          <Route
            path="/categorias"
            element={
              <ProtectedRoute
                permission={[
                  "VISUALIZAR_CATEGORIAS",
                  "GERENCIAR_CATEGORIAS",
                ]}
              >
                <Categoria />
              </ProtectedRoute>
            }
          />
          <Route
            path="/sla"
            element={
              <ProtectedRoute permission="VISUALIZAR_SLA">
                <Sla />
              </ProtectedRoute>
            }
          />
          <Route
            path="/distribuicao"
            element={
              <ProtectedRoute permission="GERENCIAR_DISTRIBUICAO">
                <Distribuicao />
              </ProtectedRoute>
            }
          />
          <Route
            path="/distribuicao/aplicar"
            element={
              <ProtectedRoute permission="GERENCIAR_DISTRIBUICAO">
                <AplicarDistribuicao />
              </ProtectedRoute>
            }
          />
          <Route path="/acesso-negado" element={<AcessoNegado />} />
          <Route
            path="/auditoria/geral"
            element={
              <ProtectedRoute permission="VISUALIZAR_AUDITORIA">
                <AuditoriaGeral />
              </ProtectedRoute>
            }
          />
          <Route
            path="/auditoria/login"
            element={
              <ProtectedRoute permission="VISUALIZAR_AUDITORIA">
                <AuditoriaLogin />
              </ProtectedRoute>
            }
          />
          <Route
            path="/auditoria/chamados"
            element={
              <ProtectedRoute permission="VISUALIZAR_AUDITORIA_CHAMADO">
                <AuditoriaChamados />
              </ProtectedRoute>
            }
          />
          <Route
            path="/monitoramento"
            element={
              <ProtectedRoute permission="VISUALIZAR_AUDITORIA">
                <Monitoramento />
              </ProtectedRoute>
            }
          />
          <Route path="/chamados/novo" element={<AbrirChamado />} />
          <Route path="/chamados/fila" element={<FilaAtendimento />} />
          <Route path="/chamados/meus" element={<MeusChamadosTecnico />} />
          <Route
            path="/chamados/meusSolicitante"
            element={<MeusChamadosSolicitante />}
          />
          <Route
            path="/chamados/historicoChamados"
            element={<HistoricoChamados />}
          />
        </Route>
    </Routes>
  );
}
