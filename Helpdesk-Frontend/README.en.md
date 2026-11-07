# Helpdesk Frontend

Web interface for a technical support platform. The system lets users create, track, assign, and analyze tickets and manage users, permissions, categories, and operational rules.

This directory contains the frontend. To use all features, run the Helpdesk API and configure its URL through an environment variable.

## Table of contents

- [Features](#features)
- [Technologies](#technologies)
- [Prerequisites](#prerequisites)
- [Configuration](#configuration)
- [Running locally](#running-locally)
- [Available scripts](#available-scripts)
- [Backend integration](#backend-integration)
- [Authentication and security](#authentication-and-security)
- [Real-time updates](#real-time-updates)
- [Access control](#access-control)
- [Project structure](#project-structure)
- [Production build](#production-build)
- [Deployment](#deployment)
- [Production checklist](#production-checklist)
- [License](#license)

## Features

### Access and sessions

- Sign in with a username and password.
- Reset and change passwords.
- Require a password change when requested by the API.
- Refresh sessions automatically.
- Synchronize sign-in and sign-out across browser tabs.
- End inactive sessions.
- Protect routes through authentication and permissions.

### Tickets

- Create tickets with a category, priority, description, and equipment details.
- View the general support queue.
- View separate ticket lists for requesters and technicians.
- Filter the ticket history.
- Export the ticket history to Excel.
- Discuss tickets in threaded conversations.
- Upload and download attachments.
- Claim, transfer, update, and close tickets according to user permissions.
- View priorities and track SLA deadlines.

### Dashboard and analytics

- View operational metrics and charts.
- Filter by date range, user, role, category, priority, and status.
- Browse paginated ticket analytics.
- Inspect response and resolution times and SLA details.
- View comments, attachments, and audit records in read-only mode.
- See loading, error, and empty states.

### Service desk

- Display a dedicated view on a monitor or TV.
- Open a route independent of the administration layout.
- View the queue in priority order.
- See daily totals for tickets started and completed.
- Review a summary of ticket activity.
- Receive real-time visual and audio alerts.
- Rotate automatically between panels.
- Use full-screen mode.
- Retain the last valid data during reconnection.

### Administration

- Manage users.
- Manage roles and permissions.
- Assign roles to users.
- Configure ticket categories.
- Configure SLAs and working hours.
- Configure ticket assignment rules and criteria.
- Apply assignment rules manually.

### Auditing and monitoring

- Review system-wide audit records.
- Review authentication audit records.
- Track ticket changes.
- Monitor errors and inspect technical details.
- Control access to administrative areas.

## Technologies

- [React 19](https://react.dev/)
- [Vite 8](https://vite.dev/)
- [React Router 7](https://reactrouter.com/)
- [Axios](https://axios-http.com/)
- [Microsoft SignalR](https://learn.microsoft.com/aspnet/core/signalr/)
- [Recharts](https://recharts.org/)
- [Lucide React](https://lucide.dev/)
- [SheetJS](https://sheetjs.com/) for Excel exports
- CSS Modules
- ESLint

## Prerequisites

- Node.js `20.19.0` or later in the 20.x series, or `22.12.0` or later.
- npm.
- A running Helpdesk API that the browser can reach.
- The frontend origin must be allowed by the backend's CORS configuration.

## Configuration

Copy the example environment file.

```bash
cp .env.example .env
```

Use this command in Windows PowerShell.

```powershell
Copy-Item .env.example .env
```

Set the API base URL in `.env`.

```env
VITE_API_URL=https://localhost:7008/api
```

For a deployed environment, use the public API URL.

```env
VITE_API_URL=https://api.example.com/api
```

The application defaults to `https://localhost:7008/api` if `VITE_API_URL` is unset.

Vite embeds environment variables at build time. Rebuild the application after changing the API URL.

## Running locally

1. Install the dependencies.

   ```bash
   npm install
   ```

2. Create and configure the `.env` file.
3. Start the development server.

   ```bash
   npm run dev
   ```

Vite prints the local address in the terminal. The default is `http://localhost:5173`.

## Available scripts

| Command | Description |
| --- | --- |
| `npm run dev` | Starts the development server. |
| `npm run build` | Writes production files to `dist`. |
| `npm run preview` | Serves the production build locally for preview. |
| `npm run lint` | Checks the code against the project's lint rules. |

## Backend integration

All HTTP requests use the URL set in `VITE_API_URL`. The Axios client sends credentials, attaches the access token automatically, and attempts to refresh the session when an authenticated request receives a `401` response.

The backend provides the following capabilities, among others.

- Authentication and session refresh
- Users, roles, and permissions
- Tickets, comments, and attachments
- Categories, SLAs, and ticket assignment
- Dashboard, auditing, and monitoring
- Real-time communication through SignalR

The frontend expects responses and permissions that match the API contract. Coordinate changes to contracts, event names, or DTOs across both projects.

## Authentication and security

- The current session's access token is stored in `sessionStorage`.
- Session refresh uses the credentials the browser sends to the API.
- Axios automatically attaches the token to requests to protected routes.
- Concurrent requests share the same refresh operation to avoid duplicate attempts.
- Session events are synchronized across tabs.
- The frontend clears legacy authentication data stored by previous versions.
- The interface hides restricted areas. The backend enforces authorization.

Keep passwords, hard-coded tokens, private keys, and sensitive URLs out of the source code.

## Real-time updates

The project maintains a shared connection to the backend's SignalR hub. It updates notifications, queues, tickets, comments, the dashboard, and the service desk without requiring a page reload.

The connection reconnects automatically and restores the state of the affected views. The service desk joins a dedicated group, receives complete snapshots of the current state, and displays the last valid data while the connection is down.

- The browser must be able to reach the backend.
- The hosting service must support WebSockets.
- The reverse proxy must allow persistent connections.
- CORS must allow the frontend origin.
- Production environments must use HTTPS.

## Access control

Pages require authentication and, where applicable, permissions returned by the API.

| Area | Primary permission |
| --- | --- |
| Dashboard | `VISUALIZAR_DASHBOARD` |
| Service desk | `VISUALIZAR_CENTRAL_ATENDIMENTO` |
| Users | `GERENCIAR_USUARIOS` |
| Roles | `GERENCIAR_PERFIS` |
| Permissions | `GERENCIAR_PERMISSOES` |
| Categories | `VISUALIZAR_CATEGORIAS` or `GERENCIAR_CATEGORIAS` |
| SLA | `VISUALIZAR_SLA` |
| Ticket assignment | `GERENCIAR_DISTRIBUICAO` |
| System and sign-in auditing | `VISUALIZAR_AUDITORIA` |
| Ticket auditing | `VISUALIZAR_AUDITORIA_CHAMADO` |
| Monitoring | `VISUALIZAR_AUDITORIA` |

Individual components and actions can require additional permissions beyond those enforced at the route level.

## Project structure

```text
Helpdesk-Frontend/
├── public/                 # Static assets and generic branding
├── src/
│   ├── api/                # HTTP client and API URL
│   ├── assets/             # Assets imported by the application
│   ├── components/         # Shared components
│   ├── contexts/           # Authentication context and provider
│   ├── hooks/              # Hooks for real-time updates
│   ├── layouts/            # Main application layout
│   ├── pages/              # Pages and feature modules
│   ├── routes/             # Public and protected routes
│   ├── services/           # API and SignalR communication
│   ├── utils/              # Formatting, validation, and helper logic
│   ├── App.jsx             # Root component
│   └── main.jsx            # Application entry point
├── .env.example            # Example configuration
├── eslint.config.js        # ESLint configuration
├── index.html              # Main HTML document
├── package.json            # Dependencies and scripts
└── vite.config.js          # Vite configuration
```

## Production build

Run the linter and build the application.

```bash
npm run lint
npm run build
```

The build writes the generated files to `dist`. Preview the result locally with the following command.

```bash
npm run preview
```

## Deployment

The frontend can run on static hosting services such as Vercel, Netlify, or Cloudflare Pages, or on a web server you manage.

| Setting | Value |
| --- | --- |
| Install command | `npm install` |
| Build command | `npm run build` |
| Output directory | `dist` |
| Environment variable | `VITE_API_URL` |
| SPA navigation fallback | `index.html` |

The following diagram shows an example deployment.

```text
Browser
   │
   ├── React frontend on static hosting
   │
   └── Helpdesk API in a Docker-compatible environment
          ├── HTTP API
          ├── SignalR
          ├── Background services
          └── Database
```

Allow the frontend's public URL in the backend's CORS configuration. If you use a reverse proxy, configure it to forward SignalR connections.

## Production checklist

- [ ] Set `VITE_API_URL` to the public API URL.
- [ ] Allow the frontend origin in the backend's CORS configuration.
- [ ] Enable HTTPS for the frontend and backend.
- [ ] Verify that WebSockets and SignalR work.
- [ ] Configure the route fallback to `index.html`.
- [ ] Run `npm run lint`.
- [ ] Run `npm run build`.
- [ ] Verify sign-in, session refresh, and sign-out.
- [ ] Verify permissions for different roles.
- [ ] Verify ticket creation, updates, and closure.
- [ ] Verify real-time notifications and reconnection.
- [ ] Check that `.env` files and sensitive data are absent from the repository.

## License

This project has no public license. Before redistributing it or accepting external contributions, add a `LICENSE` file that fits the intended use.
