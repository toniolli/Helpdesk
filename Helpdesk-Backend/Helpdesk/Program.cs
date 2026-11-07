using Application.Mapping;
using Helpdesk.Authorization;
using Helpdesk.BackgroundServices;
using Helpdesk.Extensions;
using Helpdesk.Hubs;
using Helpdesk.Middleware;
using Infra.Context;
using Infra.Identity;
using Infra.Services;
using InfraIOC;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Oracle.EntityFrameworkCore;
using System.Text;

// Configura automaticamente a variavel de ambiente
var builder = WebApplication.CreateBuilder(args);

// Origem do cors
var origensPermitidas = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>();

if (origensPermitidas is null || origensPermitidas.Length == 0)
{
    throw new Exception(
        "Cors deve possuir pelo menos uma origem.");
}

// Permitindo cors
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(origensPermitidas)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Configuração Rate Limit
builder.Services.AddLoginRateLimiting();

// Dependências gerais
builder.Services.AddHttpContextAccessor();

builder.Services.Configure<
    Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
    {
        options.MultipartBodyLengthLimit =
            10 * 1024 * 1024;
    });

// Authorization
builder.Services.AddAuthorization();


builder.Services.AddSingleton<
    IAuthorizationPolicyProvider,
    PermissaoPolicyProvider>();

builder.Services.AddScoped<
    IAuthorizationHandler,
    PermissaoHandler>();

// Conexão Oracle
var connectionString =
    builder.Configuration.GetConnectionString("ORACLE");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new Exception(
        "ConnectionStrings deve ser configurada por segredo externo.");
}

// IdentityContext - Runtime
builder.Services.AddDbContext<IdentityContext>(options =>
{
    options.UseOracle(
        connectionString,
        oracleOptions =>
        {
            oracleOptions.UseOracleSQLCompatibility(
                OracleSQLCompatibility.DatabaseVersion19);
        });
});

// Identity
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Configurações básicas
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;

        options.User.RequireUniqueEmail = true;

        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(10);

        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddEntityFrameworkStores<IdentityContext>()
    .AddDefaultTokenProviders();

// Configurando o JWT
var jwtSettings =
    builder.Configuration.GetSection("Jwt");

var jwtKey =
    jwtSettings["Key"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new Exception("Jwt não configurada.");
}

if (jwtKey.Contains(
        "ALTERE",
        StringComparison.OrdinalIgnoreCase) ||
    jwtKey.Length < 32)
{
    throw new Exception(
        "Jwt deve ser configurada por segredo externo e ter ao menos 32 caracteres.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters =
        new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],

            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtKey))
        };

    // O cliente SignalR envia o JWT na query string durante a negociação da conexão.
    // Por segurança, o token da URL é aceito somente no endpoint do Hub.
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken =
                context.Request.Query["access_token"];

            var path =
                context.HttpContext.Request.Path;

            if (!string.IsNullOrWhiteSpace(accessToken) &&
                path.StartsWithSegments(
                    "/hubs/atualizacoes"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

// Registra a infraestrutura principal do SignalR.
builder.Services.AddSignalR();

// Registra o serviço da aplicação responsável por publicar os eventos do sistema.
builder.Services.AddSignalRServices();

// Injeção de dependência
builder.Services.AddInfrastructure(
    builder.Configuration);

// Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "Helpdesk API",
            Version = "v1"
        });

    // Definição do JWT
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Description =
                "Digite: Bearer {seu token}",

            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });

    // Exigir token nos endpoints
    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference =
                        new OpenApiReference
                        {
                            Type =
                                ReferenceType.SecurityScheme,

                            Id = "Bearer"
                        }
                },
                Array.Empty<string>()
            }
        });
});

// Cache
builder.Services.AddMemoryCache();

// Serviço de Email
builder.Services.Configure<EmailService>(
    builder.Configuration.GetSection(
        "EmailService"));

// Configuração dos BackgroundServices
builder.Services.AddHostedService<
    DistribuicaoEscalonamentoBackgroundService>();

//BackgroundServices de evento e limpeza
builder.Services.AddHostedService<
    EventoSignalROutboxProcessamentoBackgroundService>();

builder.Services.AddHostedService<
    AuditoriaLimpezaBackgroundService>();

//BackgroundServices do central de atendimento
builder.Services.AddHostedService<
    CentralAtendimentoBackgroundService>();

//BackgroundServices Refresh hash
builder.Services.AddHostedService<
    SessaoRefreshLimpezaBackgroundService>();

var app = builder.Build();

// Migrações e seed são executados somente quando habilitados explicitamente.
// O padrão desativado evita alterações automáticas no banco ao iniciar a API.
if (builder.Configuration.GetValue<bool>("DatabaseInitialization:Enabled"))
{
    using var scope = app.Services.CreateScope();

    var services = scope.ServiceProvider;

    var appDbContext =
        services.GetRequiredService<AppDbContext>();

    var identityContext =
        services.GetRequiredService<IdentityContext>();

    await appDbContext.Database.MigrateAsync();

    await identityContext.Database.MigrateAsync();

    var roleManager =
        services.GetRequiredService<
            RoleManager<IdentityRole>>();

    var userManager =
        services.GetRequiredService<
            UserManager<ApplicationUser>>();

    await SeedIdentity.SeedRolesAsync(
        roleManager);

    await SeedIdentity.SeedAdminAsync(
        userManager,
        builder.Configuration,
        appDbContext);
}

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Implementando ExceptionMiddleware
app.UseMiddleware<
    MonitoramentoRequisicaoMiddleware>();

app.UseMiddleware<
    ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("Frontend");

app.UseRateLimiter();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

// Verifica somente se a API está em execução, sem consultar o banco de dados.
app.MapHealthChecks("/health");

// Endpoint usado pelo frontend para manter a conexão de atualizações em tempo real.
app.MapHub<AtualizacoesHub>(
    "/hubs/atualizacoes",
    options =>
    {
        // Encerra a conexão quando o JWT utilizado na autenticação expira.
        // O frontend poderá renovar o token e criar uma nova conexão válida.
        options.CloseOnAuthenticationExpiration = true;
    });

app.Run();

public partial class Program
{
}
