using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infra.EntityConfiguration
{
    public class PerfilConfiguration : IEntityTypeConfiguration<Perfil>
    {
        public void Configure(EntityTypeBuilder<Perfil> builder)
        {
            builder.ToTable("perfil");

            // PK
            builder.HasKey(p => p.Id_Perfil);

            builder.Property(p => p.Id_Perfil)
                .HasColumnName("Id_Perfil")
                .ValueGeneratedOnAdd();

            // Propriedades
            builder.Property(p => p.Nome)
                .HasColumnName("Nome")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(p => p.Descricao)
                .HasColumnName("Descricao")
                .HasMaxLength(255);

            builder.Property(p => p.Ativo)
                .HasColumnName("Ativo")
                .HasColumnType("NUMBER(1)")
                .IsRequired();

            // Relação Perfil 1:N PerfilPermissao
            builder.HasMany(p => p.PerfisPermissoes)
                .WithOne(pp => pp.Perfil)
                .HasForeignKey(pp => pp.PerfilId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relação Perfil 1:N UsuarioPerfil
            builder.HasMany(p => p.UsuariosPerfis)
                .WithOne(up => up.Perfil)
                .HasForeignKey(up => up.PerfilId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relação Perfil 1:N versões do período de trabalho
            builder.HasMany(p => p.PeriodosTrabalho)
                .WithOne(periodo => periodo.Perfil)
                .HasForeignKey(periodo => periodo.PerfilId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
