# Helpdesk Frontend

Interface web de uma plataforma de atendimento e suporte técnico. O sistema permite registrar, acompanhar, distribuir e analisar chamados, além de administrar usuários, permissões, categorias e regras operacionais.

Este repositório contém somente o frontend. Para utilizar todas as funcionalidades, a API do Helpdesk precisa estar disponível e configurada por variável de ambiente.

## Sumário

- [Funcionalidades](#funcionalidades)
- [Tecnologias](#tecnologias)
- [Pré-requisitos](#pré-requisitos)
- [Configuração](#configuração)
- [Execução local](#execução-local)
- [Scripts disponíveis](#scripts-disponíveis)
- [Integração com o backend](#integração-com-o-backend)
- [Autenticação e segurança](#autenticação-e-segurança)
- [Atualizações em tempo real](#atualizações-em-tempo-real)
- [Controle de acesso](#controle-de-acesso)
- [Estrutura do projeto](#estrutura-do-projeto)
- [Build de produção](#build-de-produção)
- [Publicação](#publicação)
- [Checklist para produção](#checklist-para-produção)
- [Licença](#licença)

## Funcionalidades

### Acesso e sessão

- Login com usuário e senha.
- Recuperação e alteração de senha.
- Troca obrigatória de senha quando solicitada pela API.
- Renovação automática da sessão.
- Sincronização de login e logout entre abas do navegador.
- Encerramento da sessão por inatividade.
- Rotas protegidas por autenticação e permissões.

### Chamados

- Abertura de chamados com categoria, prioridade, descrição e equipamento.
- Fila geral de atendimento.
- Listas separadas para solicitantes e técnicos.
- Histórico de chamados com filtros.
- Exportação do histórico para Excel.
- Comentários em formato de conversa.
- Envio e download de anexos.
- Assunção, transferência, atualização e encerramento de chamados conforme as permissões do usuário.
- Consulta de prioridades e acompanhamento dos prazos de SLA.

### Dashboard e análise

- Indicadores operacionais e gráficos.
- Filtros por período, usuário, perfil, categoria, prioridade e status.
- Análise paginada de chamados.
- Visualização detalhada de tempos de atendimento e SLA.
- Consulta somente leitura de comentários, anexos e auditoria.
- Estados de carregamento, erro e ausência de resultados.

### Central de Atendimento

- Tela dedicada para exibição em monitor ou TV.
- Rota independente do layout administrativo.
- Fila organizada por prioridade.
- Totais de atendimentos iniciados e finalizados no dia.
- Resumo operacional dos chamados.
- Alertas visuais e sonoros em tempo real.
- Rotação automática entre os painéis.
- Suporte a tela cheia.
- Preservação da última informação válida durante reconexões.

### Administração

- Gerenciamento de usuários.
- Gerenciamento de perfis e permissões.
- Associação de usuários a perfis.
- Configuração de categorias de atendimento.
- Configuração de SLA e períodos de trabalho.
- Regras e critérios de distribuição de chamados.
- Aplicação manual de regras de distribuição.

### Auditoria e monitoramento

- Auditoria geral do sistema.
- Auditoria de autenticação.
- Histórico de alterações dos chamados.
- Monitoramento de erros e detalhes técnicos.
- Controle de acesso às áreas administrativas.

## Tecnologias

- [React 19](https://react.dev/)
- [Vite 8](https://vite.dev/)
- [React Router 7](https://reactrouter.com/)
- [Axios](https://axios-http.com/)
- [Microsoft SignalR](https://learn.microsoft.com/aspnet/core/signalr/)
- [Recharts](https://recharts.org/)
- [Lucide React](https://lucide.dev/)
- [SheetJS](https://sheetjs.com/) para exportação de arquivos Excel
- CSS Modules
- ESLint

## Pré-requisitos

- Node.js `20.19.0` ou superior. Também são aceitas versões a partir da `22.12.0`.
- npm.
- API do Helpdesk em execução e acessível pelo navegador.
- Origem do frontend liberada na configuração de CORS do backend.

## Configuração

Copie o arquivo de exemplo:

```bash
cp .env.example .env
```

No Windows PowerShell:

```powershell
Copy-Item .env.example .env
```

Configure a URL base da API no arquivo `.env`:

```env
VITE_API_URL=https://localhost:7008/api
```

Para um ambiente publicado:

```env
VITE_API_URL=https://api.seu-dominio.com/api
```

Se `VITE_API_URL` não for definida, a aplicação utilizará `https://localhost:7008/api`.

Variáveis do Vite são incorporadas durante o build. Sempre gere um novo build depois de alterar a URL da API.

## Execução local

1. Instale as dependências:

```bash
npm install
```

2. Crie e configure o arquivo `.env`.

3. Inicie o servidor de desenvolvimento:

```bash
npm run dev
```

O endereço local será informado pelo Vite no terminal, normalmente `http://localhost:5173`.

## Scripts disponíveis

| Comando | Descrição |
| --- | --- |
| `npm run dev` | Inicia o servidor de desenvolvimento. |
| `npm run build` | Gera os arquivos de produção em `dist`. |
| `npm run preview` | Executa uma visualização local do build. |
| `npm run lint` | Verifica a qualidade e a padronização do código. |

## Integração com o backend

Todas as requisições HTTP utilizam a URL definida em `VITE_API_URL`. O cliente Axios está configurado com envio de credenciais, inclusão automática do access token e tentativa controlada de renovação quando uma requisição autenticada recebe `401`.

O backend precisa fornecer, entre outros, recursos para:

- autenticação e renovação de sessão;
- usuários, perfis e permissões;
- chamados, comentários e anexos;
- categorias, SLA e distribuição;
- dashboard, auditoria e monitoramento;
- comunicação em tempo real por SignalR.

O frontend espera respostas e permissões compatíveis com a API correspondente. Mudanças nos contratos, nomes de eventos ou DTOs devem ser aplicadas de forma coordenada nos dois projetos.

## Autenticação e segurança

- O access token da sessão atual é mantido no `sessionStorage`.
- A renovação utiliza as credenciais enviadas pelo navegador para a API.
- O Axios inclui o token automaticamente nas rotas protegidas.
- Requisições concorrentes compartilham o fluxo de renovação para evitar múltiplas tentativas simultâneas.
- Eventos de sessão são sincronizados entre abas.
- O frontend remove dados antigos de autenticação mantidos por versões anteriores.
- A interface esconde áreas sem permissão, mas a autorização definitiva deve ser validada pelo backend.

Não armazene senhas, tokens fixos, chaves privadas ou URLs sensíveis diretamente no código-fonte.

## Atualizações em tempo real

O projeto mantém uma conexão compartilhada com o hub SignalR do backend. Ela é utilizada para atualizar notificações, filas, chamados, comentários, dashboard e Central de Atendimento sem exigir recarregamento manual da página.

A conexão possui reconexão automática e recuperação do estado das telas. A Central de Atendimento entra em um grupo específico, recebe fotografias completas do estado operacional e preserva os últimos dados válidos enquanto a conexão está indisponível.

Para o funcionamento correto:

- o navegador precisa alcançar o backend;
- WebSockets devem estar liberados na hospedagem;
- o proxy reverso deve permitir conexões persistentes;
- o CORS deve aceitar a origem do frontend;
- HTTPS deve ser usado em produção.

## Controle de acesso

As páginas são protegidas por autenticação e, quando aplicável, por permissões recebidas da API.

| Área | Permissão principal |
| --- | --- |
| Dashboard | `VISUALIZAR_DASHBOARD` |
| Central de Atendimento | `VISUALIZAR_CENTRAL_ATENDIMENTO` |
| Usuários | `GERENCIAR_USUARIOS` |
| Perfis | `GERENCIAR_PERFIS` |
| Permissões | `GERENCIAR_PERMISSOES` |
| Categorias | `VISUALIZAR_CATEGORIAS` ou `GERENCIAR_CATEGORIAS` |
| SLA | `VISUALIZAR_SLA` |
| Distribuição | `GERENCIAR_DISTRIBUICAO` |
| Auditoria geral e de login | `VISUALIZAR_AUDITORIA` |
| Auditoria de chamados | `VISUALIZAR_AUDITORIA_CHAMADO` |
| Monitoramento | `VISUALIZAR_AUDITORIA` |

Além das proteções de rota, componentes e ações específicas podem exigir permissões adicionais.

## Estrutura do projeto

```text
Helpdesk-Frontend/
├── public/                 # Recursos estáticos e identidade visual genérica
├── src/
│   ├── api/                # Cliente HTTP e URL da API
│   ├── assets/             # Recursos importados pelo código
│   ├── components/         # Componentes compartilhados
│   ├── contexts/           # Contexto e provedor de autenticação
│   ├── hooks/              # Hooks de atualização em tempo real
│   ├── layouts/            # Layout principal da aplicação
│   ├── pages/              # Páginas e módulos funcionais
│   ├── routes/             # Rotas públicas e protegidas
│   ├── services/           # Comunicação com API e SignalR
│   ├── utils/              # Formatação, validação e regras auxiliares
│   ├── App.jsx             # Componente raiz
│   └── main.jsx            # Inicialização da aplicação
├── .env.example            # Exemplo de configuração
├── eslint.config.js        # Configuração do ESLint
├── index.html              # Documento HTML principal
├── package.json            # Dependências e scripts
└── vite.config.js          # Configuração do Vite
```

## Build de produção

Execute:

```bash
npm run lint
npm run build
```

Os arquivos serão gerados em `dist`.

Para conferir o resultado localmente:

```bash
npm run preview
```

## Publicação

O frontend pode ser publicado em serviços de hospedagem estática, como Vercel, Netlify, Cloudflare Pages ou um servidor web próprio.

Configuração básica:

- comando de instalação: `npm install`;
- comando de build: `npm run build`;
- diretório de saída: `dist`;
- variável de ambiente: `VITE_API_URL`;
- fallback de navegação SPA direcionado para `index.html`.

Exemplo de arquitetura:

```text
Navegador
   │
   ├── Frontend React em hospedagem estática
   │
   └── API Helpdesk em ambiente compatível com Docker
          ├── API HTTP
          ├── SignalR
          ├── serviços em segundo plano
          └── banco de dados
```

No backend, libere a URL pública do frontend na configuração de CORS. Caso exista um proxy reverso, configure também o encaminhamento das conexões SignalR.

## Checklist para produção

- [ ] Configurar `VITE_API_URL` com a URL pública da API.
- [ ] Liberar a origem do frontend no CORS do backend.
- [ ] Ativar HTTPS no frontend e no backend.
- [ ] Confirmar o funcionamento de WebSockets e SignalR.
- [ ] Configurar o fallback de rotas para `index.html`.
- [ ] Executar `npm run lint`.
- [ ] Executar `npm run build`.
- [ ] Validar login, renovação e encerramento da sessão.
- [ ] Validar permissões com diferentes perfis de acesso.
- [ ] Validar abertura, atualização e encerramento de chamados.
- [ ] Validar notificações e reconexão em tempo real.
- [ ] Revisar se arquivos `.env` e dados sensíveis não foram enviados ao repositório.

## Licença

Este projeto ainda não possui uma licença pública definida. Antes de redistribuir ou aceitar contribuições externas, adicione um arquivo `LICENSE` compatível com o uso pretendido.
