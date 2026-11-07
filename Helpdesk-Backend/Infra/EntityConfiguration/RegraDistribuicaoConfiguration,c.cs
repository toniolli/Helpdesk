using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infra.EntityConfiguration
{
    public class RegraDistribuicaoConfiguration : IEntityTypeConfiguration<RegraDistribuicao>
    {
        public void Configure(EntityTypeBuilder<RegraDistribuicao> builder)
        {
            builder.ToTable("regra_distribuicao");

            // PK
            builder.HasKey(r => r.Id_Regras);

            builder.Property(r => r.Id_Regras)
                .HasColumnName("Id_Regras")
                .ValueGeneratedOnAdd();

            builder.Property(r => r.Nome)
                .HasColumnName("Nome")
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(r => r.TipoRegra)
                .HasColumnName("TipoRegra")
                .IsRequired();

            builder.Property(r => r.TipoDestino)
                .HasColumnName("TipoDestino")
                .IsRequired();

            builder.Property(r => r.Ativo)
                .HasColumnName("Ativo")
                .HasColumnType("NUMBER(1)")
                .IsRequired();

            builder.Property(r => r.PeriodoIndefinido)
                .HasColumnName("PeriodoIndefinido")
                .HasColumnType("NUMBER(1)")
                .IsRequired();

            builder.Property(r => r.DataInicial)
                .HasColumnName("DataInicial");

            builder.Property(r => r.DataFinal)
                .HasColumnName("DataFinal");

            builder.Property(r => r.UsuarioDestinoId)
                .HasColumnName("UsuarioDestinoId")
                .HasMaxLength(450);

            builder.Property(r => r.PerfilDestinoId)
                .HasColumnName("PerfilDestinoId");

            // Relação RegraDistribuicao 1:N CriterioDistribuicao
            builder.HasMany(r => r.Criterios)
                .WithOne(c => c.RegraDistribuicao)
                .HasForeignKey(c => c.RegraDistribuicaoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}