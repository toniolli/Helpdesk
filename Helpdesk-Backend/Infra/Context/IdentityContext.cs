using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Infra.Identity;
using System.Linq;
using Infra.EntityConfiguration.Identity;

namespace Infra.Context
{
    public class IdentityContext : IdentityDbContext<ApplicationUser>
    {

        public DbSet<SessaoRefresh> SessoesRefresh { get; set; }


        public IdentityContext(DbContextOptions<IdentityContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Schema padrão do Identity
            builder.HasDefaultSchema("HELPDESK");

            // Limite dos nomes gerados pelo EF/Identity
            builder.HasAnnotation(
                "Relational:MaxIdentifierLength",
                128);


            //Refresh token
            builder.ApplyConfiguration(
                new SessaoRefreshConfiguration());

            // Configurações específicas do usuário
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(u => u.UserName)
                    .HasMaxLength(100);

                entity.Property(u => u.NormalizedUserName)
                    .HasMaxLength(100);

                entity.Property(u => u.Email)
                    .HasMaxLength(150);

                entity.Property(u => u.NormalizedEmail)
                    .HasMaxLength(150);
            });

            // Configurações específicas das Roles
            builder.Entity<IdentityRole>(entity =>
            {
                entity.Property(r => r.Name)
                    .HasMaxLength(100);

                entity.Property(r => r.NormalizedName)
                    .HasMaxLength(100);
            });

            // Strings do Identity -> VARCHAR2
            foreach (var property in builder.Model
                .GetEntityTypes()
                .SelectMany(entity => entity.GetProperties())
                .Where(property => property.ClrType == typeof(string)))
            {
                var maxLength = property.GetMaxLength() ?? 255;

                property.SetIsUnicode(false);
                property.SetMaxLength(maxLength);
                property.SetColumnType($"VARCHAR2({maxLength})");
            }

            // BOOL do Identity -> NUMBER(1)
            foreach (var property in builder.Model
                .GetEntityTypes()
                .SelectMany(entity => entity.GetProperties())
                .Where(property =>
                    property.ClrType == typeof(bool) ||
                    property.ClrType == typeof(bool?)))
            {
                property.SetColumnType("NUMBER(1)");
            }
        }
    }
}
