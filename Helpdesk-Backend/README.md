# Helpdesk API

API REST para gerenciamento de chamados de suporte técnico. O sistema reúne abertura e acompanhamento de chamados, organização da central de atendimento, controle de acesso, auditoria, notificações e atualizações em tempo real.

O backend foi desenvolvido em .NET 9, utiliza Oracle como banco de dados externo e pode ser executado localmente ou em um container Docker.

## Recursos principais

- Abertura, atribuição, acompanhamento e encerramento de chamados.
- Central de atendimento com filas e organização do trabalho.
- Categorias, prioridades e controle de SLA.
- Comentários e anexos vinculados aos chamados.
- Perfis, permissões e associação de usuários.
- Regras e critérios para distribuição automática de chamados.
- Autenticação com JWT e gerenciamento de sessões de atualização.
- Recuperação e alteração de senha.
- Notificações em tempo real com SignalR.
- Processamento de eventos por outbox.
- Notificações por e-mail via SMTP.
- Auditoria de ações, autenticação e histórico dos chamados.
- Dashboard e monitoramento de requisições e erros.
- Serviços em segundo plano para distribuição, atendimento e limpeza de registros.

## Tecnologias

- .NET 9
- ASP.NET Core Web API
- Entity Framework Core 9
- Oracle Database 19c ou compatível
- Oracle Entity Framework Core
- ASP.NET Core Identity
- JWT Bearer Authentication
- SignalR
- AutoMapper
- FluentValidation
- MailKit
- Swagger / OpenAPI
- Docker e Docker Compose

## Arquitetura

O projeto está organizado em camadas, mantendo as regras de negócio separadas da API e da infraestrutura:

```text
Helpdesk-Backend/
├── Domain/        Entidades, enums, regras de negócio e interfaces
├── Application/   Serviços, DTOs, validações, mapeamentos e contratos
├── Infra/         Entity Framework, Identity, repositórios e integrações
├── InfraIOC/      Configuração da injeção de dependência
└── Helpdesk/      API, controllers, middlewares, hubs e serviços em segundo plano
```

### Domain

Contém o núcleo do sistema: chamados, comentários, anexos, perfis, permissões, SLAs, notificações, auditorias e regras de distribuição.

### Application

Coordena os casos de uso do sistema. Essa camada contém os serviços de aplicação, DTOs, validações e mapeamentos entre as entidades e os dados expostos pela API.

### Infra

Implementa o acesso ao Oracle, os repositórios, o ASP.NET Core Identity, o envio de e-mails e os demais serviços externos utilizados pela aplicação.

### InfraIOC

Centraliza o registro das dependências e conecta as abstrações das camadas superiores às implementações da infraestrutura.

### Helpdesk

É a camada de entrada da aplicação. Contém os controllers, autenticação, autorização, middlewares, Swagger, SignalR, health check e serviços executados em segundo plano.

## Banco de dados

O banco de dados não faz parte do container. A API se conecta a uma instância Oracle externa por meio de uma string de conexão fornecida por variável de ambiente.

O modelo utiliza o schema genérico:

```text
HELPDESK
```

Existem dois contextos do Entity Framework:

- `AppDbContext`: dados funcionais do helpdesk.
- `IdentityContext`: usuários, roles e sessões de autenticação.

As migrations iniciais dos dois contextos estão em:

```text
Infra/Migrations/App/
Infra/Migrations/Identity/
```

> O usuário/schema Oracle deve ser criado previamente e possuir as permissões necessárias. Nunca aponte a aplicação para um banco ou schema que não possa ser alterado.

## Inicialização protegida do banco

Por padrão, a API não aplica migrations nem executa o seed de identidade:

```text
DatabaseInitialization__Enabled=false
```

Para preparar um banco novo, defina temporariamente a variável como `true`. Durante essa inicialização, a aplicação:

1. Aplica as migrations do `AppDbContext`.
2. Aplica as migrations do `IdentityContext`.
3. Cria as roles iniciais.
4. Cria o administrador informado nas variáveis de seed.

Depois da primeira inicialização bem-sucedida, volte a variável para `false` e reinicie a API.

> Essa opção altera o banco de dados. Use somente em uma instância nova e controlada.

## Variáveis de ambiente

O arquivo `.env.example` contém o modelo público de configuração. Ele possui apenas valores fictícios e pode permanecer no repositório.

Crie uma cópia chamada `.env`:

PowerShell:

```powershell
Copy-Item .env.example .env
```

Linux ou macOS:

```bash
cp .env.example .env
```

Edite o novo arquivo e substitua os valores necessários:

| Variável | Finalidade |
| --- | --- |
| `API_PORT` | Porta publicada pelo Docker no computador. |
| `ASPNETCORE_ENVIRONMENT` | Ambiente de execução da API. |
| `ConnectionStrings__ORACLE` | String de conexão com o Oracle externo. |
| `DatabaseInitialization__Enabled` | Autoriza migrations e seed durante a inicialização. |
| `Jwt__Key` | Chave secreta do JWT com pelo menos 32 caracteres. |
| `Jwt__Issuer` | Emissor dos tokens. |
| `Jwt__Audience` | Público dos tokens. |
| `Jwt__AccessTokenExpirationMinutes` | Duração do access token. |
| `Jwt__NormalInactivityMinutes` | Tempo de inatividade da sessão. |
| `Cors__AllowedOrigins__0` | Primeira origem autorizada a acessar a API. |
| `SeedIdentity__AdminUserName` | Nome do administrador inicial. |
| `SeedIdentity__AdminEmail` | E-mail do administrador inicial. |
| `SeedIdentity__AdminPassword` | Senha do administrador inicial. |
| `EmailService__Servidor` | Servidor SMTP. |
| `EmailService__Porta` | Porta do servidor SMTP. |
| `EmailService__Email` | Conta utilizada para autenticação SMTP. |
| `EmailService__Senha` | Senha ou senha de aplicativo do SMTP. |
| `EmailService__Remetente` | Nome exibido como remetente. |

Exemplo de conexão Oracle:

```text
User Id=SEU_USUARIO;Password=SUA_SENHA;Data Source=SEU_SERVIDOR:1521/SEU_SERVICO
```

Para liberar mais de uma origem no CORS, adicione índices sequenciais:

```text
Cors__AllowedOrigins__0=http://localhost:5173
Cors__AllowedOrigins__1=https://seu-frontend.exemplo.com
```

> O arquivo `.env` contém segredos e está no `.gitignore`. Nunca publique esse arquivo.

## Executando com Docker

### Pré-requisitos

- Docker Desktop ou Docker Engine com Docker Compose.
- Uma instância Oracle acessível pela máquina que executará o container.
- Um arquivo `.env` preenchido a partir do `.env.example`.

### Iniciar a API

Na raiz do backend, execute:

```bash
docker compose up --build -d
```

O Docker irá:

1. Restaurar as dependências do projeto.
2. Publicar a API em modo `Release`.
3. Criar a imagem do backend.
4. Iniciar o container na porta definida em `API_PORT`.
5. Montar um volume persistente para os anexos.
6. Verificar periodicamente o endpoint `/health`.

Com a porta padrão, os endereços são:

```text
API:     http://localhost:8080
Swagger: http://localhost:8080/swagger
Health:  http://localhost:8080/health
```

O endpoint de health check confirma apenas que a API está em execução. Ele não consulta o Oracle.

### Consultar o container

```bash
docker compose ps
```

### Acompanhar os logs

```bash
docker compose logs -f api
```

### Parar a aplicação

```bash
docker compose down
```

O volume `helpdesk_uploads` permanece armazenado depois desse comando. Para remover também o volume e seus anexos, use conscientemente:

```bash
docker compose down --volumes
```

> A remoção do volume apaga os anexos armazenados pelo container.

## Executando sem Docker

### Pré-requisitos

- .NET SDK 9.
- Oracle externo acessível.
- Variáveis de ambiente configuradas no sistema operacional.

Na raiz do backend, execute:

```bash
dotnet restore Helpdesk.slnx
dotnet run --project Helpdesk/Helpdesk.csproj
```

O perfil HTTP local utiliza, por padrão:

```text
API:     http://localhost:5024
Swagger: http://localhost:5024/swagger
Health:  http://localhost:5024/health
```

As variáveis de ambiente também podem ser configuradas pelo terminal ou pelas ferramentas de segredo do ambiente de desenvolvimento. A API interrompe a inicialização quando a conexão Oracle, a origem do CORS ou a chave JWT não estão configuradas corretamente.

## Swagger e autenticação

O Swagger documenta os endpoints da API e permite testar rotas autenticadas.

Depois de obter um token, utilize o botão **Authorize** e informe:

```text
Bearer SEU_TOKEN
```

O sistema utiliza ASP.NET Core Identity para usuários e roles, além de políticas próprias para autorização por permissão.

## Atualizações em tempo real

O SignalR disponibiliza o hub:

```text
/hubs/atualizacoes
```

O cliente deve enviar um JWT válido durante a negociação. A conexão é encerrada quando o token utilizado na autenticação expira, permitindo que o frontend renove a sessão e estabeleça uma nova conexão.

## Anexos

Os arquivos enviados são armazenados no diretório `/app/uploads` dentro do container. O Docker Compose utiliza o volume nomeado `helpdesk_uploads` para preservar esses arquivos entre reinicializações e recriações do container.

O limite configurado para requisições multipart é de 10 MB.

## Serviços em segundo plano

A API executa serviços internos para:

- Distribuição e escalonamento de chamados.
- Processamento da outbox de eventos do SignalR.
- Processamento da central de atendimento.
- Limpeza de auditorias antigas.
- Limpeza de sessões de atualização expiradas.

Por esse motivo, a aplicação deve ser hospedada em um serviço que mantenha o container em execução contínua. Plataformas exclusivamente serverless podem não ser adequadas para esse modelo.

## Segurança

- Nunca publique `.env`, senhas, tokens ou strings de conexão reais.
- Gere uma chave JWT exclusiva e forte para cada ambiente.
- Mantenha `DatabaseInitialization__Enabled=false` durante a operação normal.
- Restrinja o CORS somente aos endereços conhecidos do frontend.
- Utilize uma conta Oracle com apenas as permissões necessárias.
- Use HTTPS em ambientes públicos.
- Proteja o acesso ao Swagger conforme a necessidade do ambiente.
- Faça backup do banco e do volume de anexos.
- Revise periodicamente os alertas de segurança das dependências.

## Publicação

Para publicar o projeto:

1. Envie o código, o `Dockerfile`, o `docker-compose.yml` e o `.env.example` para o repositório.
2. Não envie o arquivo `.env`.
3. Configure os segredos diretamente na plataforma de hospedagem.
4. Garanta que a hospedagem consiga acessar o Oracle externo.
5. Mantenha um volume persistente para `/app/uploads`.
6. Exponha a porta interna `8080`.
7. Configure HTTPS por meio da plataforma, proxy reverso ou gateway.

O repositório fornece a imagem da API, mas não inclui uma imagem do Oracle nem qualquer dado de banco de dados.

## Licença

Nenhuma licença de distribuição foi definida. Antes de distribuir ou reutilizar o projeto, adicione um arquivo `LICENSE` com os termos desejados.
