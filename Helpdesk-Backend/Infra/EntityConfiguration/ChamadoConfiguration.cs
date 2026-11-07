using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infra.EntityConfiguration
{
    public class ChamadoConfiguration : IEntityTypeConfiguration<Chamado>
    {
        public void Configure(EntityTypeBuilder<Chamado> builder)
        {
            builder.ToTable("chamado");

            builder.HasKey(c => c.Id_Chamado);

            builder.Property(c => c.Id_Chamado)
                .HasColumnName("Id_Chamado")
                .ValueGeneratedOnAdd();

            builder.Property(c => c.Numero)
                .HasColumnName("Numero")
                .HasMaxLength(30);

            builder.Property(c => c.SolicitanteId)
                .HasColumnName("SolicitanteId")
                .HasMaxLength(450);

            builder.Property(c => c.CategoriaAtendimentoId)
                .HasColumnName("CategoriaAtendimentoId")
                .IsRequired();

            builder.Property(c => c.EquipamentoAfetado)
                .HasColumnName("EquipamentoAfetado")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(c => c.Descricao)
                .HasColumnName("Descricao")
                .HasMaxLength(4000)
                .IsRequired();

            builder.Property(c => c.Prioridade)
                .HasColumnName("Prioridade")
                .IsRequired();

            builder.Property(c => c.Status)
                .HasColumnName("Status")
                .IsRequired();

            builder.Property(c => c.TecnicoResponsavelId)
                .HasColumnName("TecnicoResponsavelId")
                .HasMaxLength(450);

            builder.Property(c => c.TipoAtendimento)
                .HasColumnName("TipoAtendimento");

            builder.Property(c => c.TipoEncerramento)
                .HasColumnName("TipoEncerramento");

            builder.Property(c => c.Solucao)
                .HasColumnName("Solucao")
                .HasMaxLength(4000);

            builder.Property(c => c.DataAbertura)
                .HasColumnName("DataAbertura")
                .IsRequired();

            builder.Property(c => c.DataPrimeiroAtendimento)
                .HasColumnName("DataPrimeiroAtendimento");

            builder.Property(c => c.DataInicioPausaSla)
                .HasColumnName("DataInicioPausaSla");

            builder.Property(c => c.TempoPausadoHoras)
                .HasColumnName("TempoPausadoHoras")
                .HasDefaultValue(0);

            builder.Property(c => c.TempoPausadoEquipamentoHoras)
                .HasColumnName("TempoPausadoEquipamentoHoras")
                .HasDefaultValue(0);

            builder.Property(c => c.PeriodoTrabalhoPerfilId)
                .HasColumnName("PeriodoTrabalhoPerfilId");

            builder.Property(c => c.TempoPausadoSlaRetornoHoras)
                .HasColumnName("TempoPausadoSlaRetornoHoras")
                .HasColumnType("BINARY_DOUBLE")
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(c => c.TempoPausadoSlaEquipamentoHoras)
                .HasColumnName("TempoPausadoSlaEquipamentoHoras")
                .HasColumnType("BINARY_DOUBLE")
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(c => c.DataFechamento)
                .HasColumnName("DataFechamento");

            builder.Property(c => c.Version)
                .HasColumnName("Version")
                .IsRequired()
                .HasDefaultValue(1)
                .IsConcurrencyToken();

            builder.HasOne(c => c.CategoriaAtendimento)
                .WithMany()
                .HasForeignKey(c => c.CategoriaAtendimentoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.PeriodoTrabalhoPerfil)
                .WithMany(periodo => periodo.Chamados)
                .HasForeignKey(c => c.PeriodoTrabalhoPerfilId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(c => c.Comentarios)
                .WithOne(c => c.Chamado)
                .HasForeignKey(c => c.ChamadoId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.Notificacoes)
                .WithOne(n => n.Chamado)
                .HasForeignKey(n => n.ChamadoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
