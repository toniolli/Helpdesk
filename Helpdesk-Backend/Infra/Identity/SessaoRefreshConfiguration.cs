using Infra.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infra.EntityConfiguration.Identity
{
    public class SessaoRefreshConfiguration
        : IEntityTypeConfiguration<SessaoRefresh>
    {
        public void Configure(
            EntityTypeBuilder<SessaoRefresh> builder)
        {
            builder.ToTable("SessoesRefresh");

            builder.HasKey(sessao => sessao.Id);

            builder.Property(sessao => sessao.Id)
                .HasColumnName("Id")
                .HasColumnType("RAW(16)")
                .ValueGeneratedNever();

            builder.Property(sessao => sessao.UsuarioId)
                .HasColumnName("UsuarioId")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(sessao => sessao.TokenHash)
                .HasColumnName("TokenHash")
                .HasMaxLength(64)
                .IsRequired();

            builder.Property(sessao => sessao.Tipo)
                .HasColumnName("Tipo")
                .IsRequired();

            builder.Property(sessao => sessao.CriadaEmUtc)
                .HasColumnName("CriadaEmUtc")
                .IsRequired();

            builder.Property(sessao => sessao.UltimaRenovacaoUtc)
                .HasColumnName("UltimaRenovacaoUtc")
                .IsRequired();

            builder.Property(sessao => sessao.ExpiraEmUtc)
                .HasColumnName("ExpiraEmUtc")
                .IsRequired();

            builder.Property(sessao => sessao.RevogadaEmUtc)
                .HasColumnName("RevogadaEmUtc");

            builder.Property(sessao => sessao.MotivoRevogacao)
                .HasColumnName("MotivoRevogacao")
                .HasMaxLength(255);

            builder.Property(sessao => sessao.SubstituidaPorId)
                .HasColumnName("SubstituidaPorId")
                .HasColumnType("RAW(16)");

            builder.HasOne(sessao => sessao.Usuario)
                .WithMany(usuario => usuario.SessoesRefresh)
                .HasForeignKey(sessao => sessao.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(sessao => sessao.TokenHash)
                .IsUnique()
                .HasDatabaseName("IX_SessaoRefresh_TokenHash");

            builder.HasIndex(sessao => sessao.UsuarioId)
                .HasDatabaseName("IX_SessaoRefresh_Usuario");

            builder.HasIndex(sessao => sessao.ExpiraEmUtc)
                .HasDatabaseName("IX_SessaoRefresh_Expiracao");

            builder.HasIndex(sessao => sessao.RevogadaEmUtc)
                .HasDatabaseName("IX_SessaoRefresh_Revogacao");
        }
    }
}