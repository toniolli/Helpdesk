using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infra.EntityConfiguration
{
    public class CategoriaAtendimentoConfiguration
        : IEntityTypeConfiguration<CategoriaAtendimento>
    {
        public void Configure(EntityTypeBuilder<CategoriaAtendimento> builder)
        {
            builder.ToTable("categoria_atendimento");

            builder.HasKey(c => c.Id_CategoriaAtendimento);

            builder.Property(c => c.Id_CategoriaAtendimento)
                .HasColumnName("Id_CategoriaAtendimento")
                .ValueGeneratedOnAdd();

            builder.Property(c => c.Nome)
                .HasColumnName("Nome")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(c => c.Descricao)
                .HasColumnName("Descricao")
                .HasMaxLength(255);

            builder.Property(c => c.Prioridade)
                .HasColumnName("Prioridade")
                .IsRequired();

            builder.Property(c => c.Ativo)
                .HasColumnName("Ativo")
                .HasColumnType("NUMBER(1)")
                .IsRequired();
        }
    }
}