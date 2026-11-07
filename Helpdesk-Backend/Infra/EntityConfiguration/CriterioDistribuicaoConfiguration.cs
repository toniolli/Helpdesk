using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infra.EntityConfiguration
{
    public class CriterioDistribuicaoConfiguration
        : IEntityTypeConfiguration<CriterioDistribuicao>
    {
        public void Configure(EntityTypeBuilder<CriterioDistribuicao> builder)
        {
            builder.ToTable("criterio_distribuicao");

            // PK
            builder.HasKey(c => c.Id_Criterio);

            builder.Property(c => c.Id_Criterio)
                .HasColumnName("Id")
                .ValueGeneratedOnAdd();

            builder.Property(c => c.RegraDistribuicaoId)
                .HasColumnName("RegraDistribuicaoId")
                .IsRequired();

            builder.Property(c => c.CategoriaAtendimentoId)
                .HasColumnName("CategoriaAtendimentoId")
                .IsRequired();

            // Relação N:1 RegraDistribuicao
            builder.HasOne(c => c.RegraDistribuicao)
                .WithMany(r => r.Criterios)
                .HasForeignKey(c => c.RegraDistribuicaoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relação N:1 CategoriaAtendimento
            builder.HasOne(c => c.CategoriaAtendimento)
                .WithMany()
                .HasForeignKey(c => c.CategoriaAtendimentoId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}