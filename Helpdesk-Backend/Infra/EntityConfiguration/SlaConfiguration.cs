using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infra.EntityConfiguration
{
    public class SLAConfiguration : IEntityTypeConfiguration<SLA>
    {
        public void Configure(EntityTypeBuilder<SLA> builder)
        {
            builder.ToTable("sla");

            builder.HasKey(x => x.Id_SLA);

            builder.Property(x => x.Id_SLA)
                .HasColumnName("Id_SLA")
                .ValueGeneratedOnAdd();

            builder.Property(x => x.Prioridade)
                .HasColumnName("Prioridade")
                .IsRequired();

            builder.Property(x => x.TempoAtendimentoHoras)
                .HasColumnName("TempoAtendimentoHoras")
                .IsRequired();

            builder.Property(x => x.TempoResolucaoHoras)
                .HasColumnName("TempoResolucaoHoras")
                .IsRequired();

            builder.Property(x => x.Ativo)
                .HasColumnName("Ativo")
                .HasColumnType("NUMBER(1)")
                .IsRequired();

            builder.HasData(
                new
                {
                    Id_SLA = 1,
                    Prioridade = PrioridadeEnum.Baixa,
                    TempoAtendimentoHoras = 8,
                    TempoResolucaoHoras = 72,
                    Ativo = true
                },
                new
                {
                    Id_SLA = 2,
                    Prioridade = PrioridadeEnum.Media,
                    TempoAtendimentoHoras = 4,
                    TempoResolucaoHoras = 48,
                    Ativo = true
                },
                new
                {
                    Id_SLA = 3,
                    Prioridade = PrioridadeEnum.Alta,
                    TempoAtendimentoHoras = 2,
                    TempoResolucaoHoras = 24,
                    Ativo = true
                },
                new
                {
                    Id_SLA = 4,
                    Prioridade = PrioridadeEnum.Critica,
                    TempoAtendimentoHoras = 1,
                    TempoResolucaoHoras = 4,
                    Ativo = true
                });
        }
    }
}