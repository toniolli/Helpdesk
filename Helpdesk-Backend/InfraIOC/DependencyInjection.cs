using Application.Interfaces;
using Application.Mapping;
using Application.Services;
using Application.Validators.UsuarioValidator;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infra.Context;
using Infra.Identity;
using Infra.Repositores;
using Infra.Services;
using Infra.Interceptors;
using Infra.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Oracle.EntityFrameworkCore;

namespace InfraIOC
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services,IConfiguration configuration)
        {
            var connectionString =
                configuration.GetConnectionString("ORACLE");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new Exception(
                    "ConnectionStrings deve ser configurada por segredo externo.");
            }

            // DATABASE E PASSA PELO INTERCEPTOR
            services.AddScoped<AuditoriaSaveChangesInterceptor>();

            services.AddDbContext<AppDbContext>(
                (serviceProvider, options) =>
                {
                    options
                        .UseOracle(
                            connectionString,
                            oracleOptions =>
                            {
                                oracleOptions.UseOracleSQLCompatibility(
                                    OracleSQLCompatibility.DatabaseVersion19);
                            })
                        .AddInterceptors(
                            serviceProvider.GetRequiredService<
                                AuditoriaSaveChangesInterceptor>());
                });

            // REPOSITORIES
            services.AddScoped<IperfilRepository, PerfilRepository>();
            services.AddScoped<IpermissaoRepository, PermissaoRepository>();
            services.AddScoped<IperfilPermissaoRepository, PerfilPermissaoRepository>();
            services.AddScoped<IusuarioPerfilRepository, UsuarioPerfilRepository>();
            services.AddScoped<ICategoriaAtendimentoRepository, CategoriaAtendimentoRepository>();
            services.AddScoped<IChamadoRepository, ChamadoRepository>();
            services.AddScoped<IComentarioRepository, ComentarioRepository>();
            services.AddScoped<IAnexoRepository, AnexoRepository>();
            services.AddScoped<INotificacaoRepository, NotificacaoRepository>();
            services.AddScoped<IEmailNotificationRepository, EmailNotificationRepository>();
            services.AddScoped<ISlaRepository, SlaRepository>();
            services.AddScoped<IRegraDistribuicaoRepository, RegraDistribuicaoRepository>();
            services.AddScoped<ICriterioDistribuicaoRepository, CriterioDistribuicaoRepository>();
            services.AddScoped<IAuditoriaRepository, AuditoriaRepository>();
            services.AddScoped<IAuditoriaIdentityRepository, AuditoriaIdentityRepository>();
            services.AddScoped<IAuditoriaChamadoRepository, AuditoriaChamadoRepository>();
            services.AddScoped<ICacheRepository, CacheRepository>();
            services.AddScoped<IEventoSignalROutboxRepository,EventoSignalROutboxRepository>();

            // SERVICES
            services.AddScoped<IusuarioService, UsuarioService>();
            services.AddScoped<IPerfilService, PerfilService>();
            services.AddScoped<IPerfilSlaService, PerfilSlaService>();
            services.AddScoped<IPermissaoService, PermissaoService>();
            services.AddScoped<IperfilPermissaoService, PerfilPermissaoService>();
            services.AddScoped<IUsuarioPerfilService, UsuarioPerfilService>();
            services.AddScoped<ICategoriaAtendimentoService, CategoriaAtendimentoService>();
            services.AddScoped<IChamadoService, ChamadoService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IComentarioService, ComentarioService>();
            services.AddScoped<IAnexoService, AnexoService>();
            services.AddScoped<IFileStorageService, FileStorageService>();
            services.AddScoped<INotificacaoService, NotificacaoService>();
            services.AddScoped<IEmailNotificationService, EmailNotificationService>();
            services.AddScoped<IEnviarEmail, EnviarEmailService>();
            services.AddScoped<ISlaService, SlaService>();
            services.AddScoped<IRegraDistribuicaoService, RegraDistribuicaoService>();
            services.AddScoped<ICriterioDistribuicaoService, CriterioDistribuicaoService>();
            services.AddScoped<IDistribuicaoEscalonamentoService, DistribuicaoEscalonamentoService>();
            services.AddScoped<IAuditoriaService, AuditoriaService>();
            services.AddScoped<IAuditoriaIdentityService, AuditoriaIdentityService>();
            services.AddScoped<IAuditoriaChamadoService, AuditoriaChamadoService>();
            services.AddScoped<ICacheService, CacheService>();
            services.AddScoped<ITransacaoService,TransacaoService>();
            services.AddScoped<IAtualizacaoTempoRealService,AtualizacaoTempoRealOutboxService>();
            services.AddScoped<IProcessadorEventoSignalROutboxService, ProcessadorEventoSignalROutboxService>();
            services.AddScoped<ICentralAtendimentoService,CentralAtendimentoService>();
            services.AddScoped<ISessaoRefreshService, SessaoRefreshService>();

            // MONITORAMENTO EM MEMÓRIA
            services.AddSingleton<IMonitoramentoService, MonitoramentoService>();
            services.AddSingleton<ILoggerProvider, MonitoramentoLoggerProvider>();

            // FLUENTVALIDATION
            services.AddValidatorsFromAssemblyContaining<CreateUserValidator>();
            services.AddFluentValidationAutoValidation();

            // SERVICO EMAIL
            services.Configure<EmailService>(
                configuration.GetSection("EmailService"));

            // MONITORAMENTO EM MEMÓRIA CENTRAL DE ATENDIMENTO
            services.AddSingleton<
                ICentralAtendimentoTempoRealService,
                CentralAtendimentoTempoRealService>();

            services.AddSingleton<
                IMonitoramentoService,
                MonitoramentoService>();

            services.AddSingleton<
                ILoggerProvider,
                MonitoramentoLoggerProvider>();


            // AUTOMAPPER
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<DomainToDTOMappingProfile>();
            });

            return services;
        }
    }
}
