# Helpdesk API

REST API for managing technical support tickets. The system supports ticket creation and tracking, service desk operations, access control, auditing, notifications, and real-time updates.

The backend uses .NET 9 and connects to an external Oracle database. It can run locally or in a Docker container.

## Main features

- Create, assign, track, and close tickets.
- Organize service desk work with queues.
- Manage categories, priorities, and SLAs.
- Add comments and attachments to tickets.
- Manage roles and permissions and assign roles to users.
- Configure rules and criteria for automatic ticket assignment.
- Authenticate with JWT and manage refresh sessions.
- Reset and change passwords.
- Send real-time notifications through SignalR.
- Process events through an outbox.
- Send email notifications through SMTP.
- Audit user actions and authentication and track ticket history.
- View dashboard metrics and monitor requests and errors.
- Run background services for ticket assignment, support operations, and record cleanup.

## Technologies

- .NET 9
- ASP.NET Core Web API
- Entity Framework Core 9
- Oracle Database 19c or a compatible version
- Oracle Entity Framework Core
- ASP.NET Core Identity
- JWT Bearer Authentication
- SignalR
- AutoMapper
- FluentValidation
- MailKit
- Swagger / OpenAPI
- Docker and Docker Compose

## Architecture

The project uses layers to separate business rules from the API and infrastructure.

```text
Helpdesk-Backend/
├── Domain/        Entities, enums, business rules, and interfaces
├── Application/   Services, DTOs, validation, mapping, and contracts
├── Infra/         Entity Framework, Identity, repositories, and integrations
├── InfraIOC/      Dependency injection configuration
└── Helpdesk/      API, controllers, middleware, hubs, and background services
```

### Domain

Contains the core business model for tickets, comments, attachments, roles, permissions, SLAs, notifications, audit records, and assignment rules.

### Application

Coordinates the application's use cases. This layer contains application services, DTOs, validation, and mappings between entities and the data exposed by the API.

### Infra

Implements Oracle access, repositories, ASP.NET Core Identity, email delivery, and other external services used by the application.

### InfraIOC

Registers dependencies and connects higher-layer abstractions to infrastructure implementations.

### Helpdesk

Serves as the application entry point. It contains controllers, authentication, authorization, middleware, Swagger, SignalR, the health check, and background services.

## Database

The API connects to an external Oracle instance using a connection string supplied through an environment variable. Run the database separately from the API container.

The model uses the following schema name.

```text
HELPDESK
```

The application uses two Entity Framework contexts.

- `AppDbContext` stores helpdesk data.
- `IdentityContext` stores users, roles, and authentication sessions.

The initial migrations are in the following directories.

```text
Infra/Migrations/App/
Infra/Migrations/Identity/
```

> Create the Oracle user and schema beforehand and grant the required permissions. Point the application only at a database or schema you are allowed to modify.

## Controlled database initialization

Database migrations and identity seeding are disabled by default.

```text
DatabaseInitialization__Enabled=false
```

To set up a new database, temporarily set the variable to `true`. During startup, the application performs these steps.

1. Applies the `AppDbContext` migrations.
2. Applies the `IdentityContext` migrations.
3. Creates the initial roles.
4. Creates the administrator specified by the seed variables.

After the first successful initialization, set the variable back to `false` and restart the API.

> This option modifies the database. Use it only on a new instance you control.

## Environment variables

The `.env.example` file contains sample configuration values and can be kept in the repository.

Copy it to `.env` using PowerShell.

```powershell
Copy-Item .env.example .env
```

On Linux or macOS, use the following command.

```bash
cp .env.example .env
```

Edit the new file and replace the values as needed.

| Variable | Purpose |
| --- | --- |
| `API_PORT` | Port Docker exposes on the host. |
| `ASPNETCORE_ENVIRONMENT` | API runtime environment. |
| `ConnectionStrings__ORACLE` | Connection string for the external Oracle database. |
| `DatabaseInitialization__Enabled` | Enables migrations and seeding at startup. |
| `Jwt__Key` | JWT signing key with at least 32 characters. |
| `Jwt__Issuer` | Token issuer. |
| `Jwt__Audience` | Token audience. |
| `Jwt__AccessTokenExpirationMinutes` | Access token lifetime. |
| `Jwt__NormalInactivityMinutes` | Session inactivity timeout. |
| `Cors__AllowedOrigins__0` | First origin allowed to access the API. |
| `SeedIdentity__AdminUserName` | Initial administrator username. |
| `SeedIdentity__AdminEmail` | Initial administrator email address. |
| `SeedIdentity__AdminPassword` | Initial administrator password. |
| `EmailService__Servidor` | SMTP server. |
| `EmailService__Porta` | SMTP server port. |
| `EmailService__Email` | Account used for SMTP authentication. |
| `EmailService__Senha` | SMTP password or app password. |
| `EmailService__Remetente` | Sender display name. |

Use this format for the Oracle connection string.

```text
User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;Data Source=YOUR_HOST:1521/YOUR_SERVICE
```

To allow multiple CORS origins, use consecutive indexes.

```text
Cors__AllowedOrigins__0=http://localhost:5173
Cors__AllowedOrigins__1=https://frontend.example.com
```

> The `.env` file contains secrets and is excluded by `.gitignore`. Keep it out of the repository.

## Running with Docker

### Prerequisites

- Docker Desktop or Docker Engine with Docker Compose.
- An Oracle instance reachable from the machine running the container.
- A configured `.env` file based on `.env.example`.

### Start the API

Run the following command from the backend root directory.

```bash
docker compose up --build -d
```

Docker performs these steps.

1. Restores the project dependencies.
2. Publishes the API using the `Release` configuration.
3. Builds the backend image.
4. Starts the container on the port configured in `API_PORT`.
5. Mounts a persistent volume for attachments.
6. Checks the `/health` endpoint periodically.

The default port provides the following addresses.

```text
API:     http://localhost:8080
Swagger: http://localhost:8080/swagger
Health:  http://localhost:8080/health
```

The health check verifies that the API is running. Database connectivity is outside the scope of this check.

### Check container status

```bash
docker compose ps
```

### Follow the logs

```bash
docker compose logs -f api
```

### Stop the application

```bash
docker compose down
```

This command preserves the `helpdesk_uploads` volume. Use the following command to remove the volume as well.

```bash
docker compose down --volumes
```

> Removing the volume deletes the attachments stored by the container.

## Running without Docker

### Prerequisites

- .NET SDK 9.
- A reachable external Oracle instance.
- Environment variables configured in the operating system.

Run the following commands from the backend root directory.

```bash
dotnet restore Helpdesk.slnx
dotnet run --project Helpdesk/Helpdesk.csproj
```

The local HTTP profile uses the following default addresses.

```text
API:     http://localhost:5024
Swagger: http://localhost:5024/swagger
Health:  http://localhost:5024/health
```

You can also set environment variables in the terminal or use the development environment's secret management tools. The API stops during startup if the Oracle connection string, CORS origin, or JWT signing key is missing or invalid.

## Swagger and authentication

Swagger documents the API endpoints and lets you test authenticated routes.

After obtaining a token, click **Authorize** and enter it in the following format.

```text
Bearer YOUR_TOKEN
```

The application uses ASP.NET Core Identity for users and roles and custom policies for permission-based authorization.

## Real-time updates

SignalR exposes the following hub.

```text
/hubs/atualizacoes
```

The client must send a valid JWT during negotiation. When the token used for authentication expires, the server closes the connection so the frontend can refresh the session and reconnect.

## Attachments

The container stores uploaded files in `/app/uploads`. Docker Compose uses the named volume `helpdesk_uploads` to preserve those files across container restarts and recreation.

The configured size limit for multipart requests is 10 MB.

## Background services

The API runs background services for the following tasks.

- Assigning and escalating tickets.
- Processing SignalR events from the outbox.
- Processing service desk operations.
- Removing old audit records.
- Removing expired refresh sessions.

Deploy the API to a host that can keep the container running continuously. Fully serverless platforms may be unsuitable for this setup.

## Security

- Keep `.env`, passwords, tokens, and real connection strings out of the repository.
- Generate a strong, unique JWT signing key for each environment.
- Keep `DatabaseInitialization__Enabled=false` during normal operation.
- Restrict CORS to known frontend origins.
- Use an Oracle account with only the required permissions.
- Use HTTPS in public environments.
- Protect access to Swagger as appropriate for the environment.
- Back up the database and the attachment volume.
- Review dependency security alerts regularly.

## Deployment

1. Push the code, `Dockerfile`, `docker-compose.yml`, and `.env.example` to the repository.
2. Keep the `.env` file local.
3. Configure secrets directly in the hosting platform.
4. Ensure the hosting platform can reach the external Oracle database.
5. Mount a persistent volume for `/app/uploads`.
6. Expose internal port `8080`.
7. Configure HTTPS through the hosting platform, a reverse proxy, or a gateway.

The repository provides the files needed to build the API image. Supply the Oracle database and its data separately.

## License

This project has no distribution license. Before distributing or reusing it, add a `LICENSE` file with the intended terms.
