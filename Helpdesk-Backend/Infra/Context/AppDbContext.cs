using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infra.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        // DbSets

        // Permissões e autenticação
        public DbSet<Perfil> Perfis { get; set; }
        public DbSet<Permissao> Permissoes { get; set; }
        public DbSet<PerfilPermissao> PerfilPermissoes { get; set; }
        public DbSet<UsuarioPerfil> UsuariosPerfis { get; set; }
        public DbSet<PeriodoTrabalhoPerfil> PeriodosTrabalhoPerfis { get; set; }
        public DbSet<CategoriaAtendimento> CategoriasAtendimento { get; set; }
        public DbSet<Chamado> Chamados { get; set; }
        public DbSet<Comentario> Comentarios { get; set; }
        public DbSet<Anexo> Anexos { get; set; }
        public DbSet<Notificacao> Notificacoes { get; set; }
        public DbSet<EmailNotification> EmailNotifications { get; set; }
        public DbSet<EventoSignalROutbox> EventosSignalROutbox { get; set; }
        public DbSet<SLA> Slas { get; set; }
        public DbSet<CriterioDistribuicao> CriterioDistribuicoes { get; set; }
        public DbSet<RegraDistribuicao> RegrasDistribuicoes { get; set; }
        public DbSet<Auditoria> Auditorias { get; set; }
        public DbSet<AuditoriaIdentity> AuditoriaIdentities { get; set; }
        public DbSet<AuditoriaChamado> AuditoriasChamados { get; set; }
       


        // Incrementa Version++ toda vez que atualizar o chamado
        private void AtualizarVersoesDosChamados()
        {
            foreach (var entry in ChangeTracker.Entries<Chamado>()
                .Where(entry => entry.State == EntityState.Modified))
            {
                entry.Property(chamado => chamado.Version)
                    .CurrentValue++;
            }
        }


        public override int SaveChanges()
        {
            AtualizarVersoesDosChamados();

            return base.SaveChanges();
        }


        public override Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            AtualizarVersoesDosChamados();

            return base.SaveChangesAsync(cancellationToken);
        }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            // =========================================================
            // SCHEMA PADRÃO - ORACLE
            // =========================================================

            builder.HasDefaultSchema("HELPDESK");


            // =========================================================
            // ENTITY CONFIGURATIONS
            // =========================================================

            builder.ApplyConfigurationsFromAssembly(
             typeof(AppDbContext).Assembly,
             type => type.Namespace == "Infra.EntityConfiguration"); // somente configuração do appcontext


            // =========================================================
            // STRING
            // Oracle 19c -> VARCHAR2
            // =========================================================

            foreach (var property in builder.Model
                .GetEntityTypes()
                .SelectMany(entity => entity.GetProperties())
                .Where(property => property.ClrType == typeof(string)))
            {
                if (string.Equals(
                    property.GetColumnType(),
                    "CLOB",
                    StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var maxLength = property.GetMaxLength() ?? 255;

                if (maxLength > 4000)
                {
                    throw new InvalidOperationException(
                        $"A propriedade {property.DeclaringType.Name}.{property.Name} " +
                        $"possui tamanho {maxLength}. O limite definido para VARCHAR2 é 4000.");
                }

                property.SetIsUnicode(false);
                property.SetMaxLength(maxLength);
                property.SetColumnType($"VARCHAR2({maxLength})");
            }


            // =========================================================
            // DATETIME
            // Oracle 19c -> TIMESTAMP(7)
            // =========================================================

            foreach (var property in builder.Model
                .GetEntityTypes()
                .SelectMany(entity => entity.GetProperties())
                .Where(property =>
                    property.ClrType == typeof(DateTime) ||
                    property.ClrType == typeof(DateTime?)))
            {
                property.SetColumnType("TIMESTAMP(7)");
            }


            // =========================================================
            // DECIMAL
            // Oracle 19c -> NUMBER(18,2)
            // =========================================================

            foreach (var property in builder.Model
                .GetEntityTypes()
                .SelectMany(entity => entity.GetProperties())
                .Where(property =>
                    property.ClrType == typeof(decimal) ||
                    property.ClrType == typeof(decimal?)))
            {
                property.SetPrecision(18);
                property.SetScale(2);
                property.SetColumnType("NUMBER(18,2)");
            }


            // =========================================================
            // IDs INT
            // Geração automática apenas para chave primária INT
            // =========================================================

            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                var primaryKey = entityType.FindPrimaryKey();

                if (primaryKey == null ||
                    primaryKey.Properties.Count != 1)
                {
                    continue;
                }

                var keyProperty = primaryKey.Properties[0];

                if (keyProperty.ClrType == typeof(int))
                {
                    keyProperty.ValueGenerated =
                        Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.OnAdd;
                }
            }
        }
    }
}
