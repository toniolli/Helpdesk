using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infra.EntityConfiguration
{
    public class PerfilPermissaoConfiguration :
        IEntityTypeConfiguration<PerfilPermissao>
    {
        public void Configure(EntityTypeBuilder<PerfilPermissao> builder)
        {
            builder.ToTable("perfil_permissao");

            // PK composta
            builder.HasKey(pp => new
            {
                pp.PerfilId,
                pp.PermissaoId
            });

            builder.Property(pp => pp.PerfilId)
                .HasColumnName("PerfilId");

            builder.Property(pp => pp.PermissaoId)
                .HasColumnName("PermissaoId");

            // Perfil 1:N PerfilPermissao
            builder.HasOne(pp => pp.Perfil)
                .WithMany(p => p.PerfisPermissoes)
                .HasForeignKey(pp => pp.PerfilId)
                .OnDelete(DeleteBehavior.Cascade);

            // Permissao 1:N PerfilPermissao
            builder.HasOne(pp => pp.Permissao)
                .WithMany(p => p.PerfisPermissoes)
                .HasForeignKey(pp => pp.PermissaoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}