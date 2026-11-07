using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infra.EntityConfiguration
{
    public class AuditoriaChamadoConfiguration
        : IEntityTypeConfiguration<AuditoriaChamado>
    {
        public void Configure(
            EntityTypeBuilder<AuditoriaChamado> builder)
        {
            builder.ToTable("auditoria_chamado");

            builder.HasKey(a => a.Id_AuditoriaChamado);

            builder.Property(a => a.Id_AuditoriaChamado)
                .HasColumnName("Id_AuditoriaChamado")
                .ValueGeneratedOnAdd();

            builder.Property(a => a.ChamadoId)
                .HasColumnName("ChamadoId")
                .IsRequired();

            builder.Property(a => a.NumeroChamado)
                .HasColumnName("NumeroChamado")
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(a => a.Evento)
                .HasColumnName("Evento")
                .IsRequired();

            builder.Property(a => a.DataEvento)
                .HasColumnName("DataEvento")
                .IsRequired();

            builder.Property(a => a.UsuarioId)
                .HasColumnName("UsuarioId")
                .HasMaxLength(450);

            builder.Property(a => a.UsuarioNome)
                .HasColumnName("UsuarioNome")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(a => a.PerfilUsuario)
                .HasColumnName("PerfilUsuario")
                .HasMaxLength(100);

            builder.Property(a => a.StatusAnterior)
                .HasColumnName("StatusAnterior");

            builder.Property(a => a.StatusNovo)
                .HasColumnName("StatusNovo");

            builder.Property(a => a.Descricao)
                .HasColumnName("Descricao")
                .HasMaxLength(1000)
                .IsRequired();

            builder.Property(a => a.DetalhesAnteriores)
                .HasColumnName("DetalhesAnteriores")
                .HasColumnType("CLOB");

            builder.Property(a => a.DetalhesNovos)
                .HasColumnName("DetalhesNovos")
                .HasColumnType("CLOB");

            builder.HasOne(a => a.Chamado)
                .WithMany(c => c.Auditorias)
                .HasForeignKey(a => a.ChamadoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(a => a.DataEvento)
                .HasDatabaseName(
                    "IX_auditoria_chamado_DataEvento");

            builder.HasIndex(a => new
            {
                a.ChamadoId,
                a.DataEvento
            })
                .HasDatabaseName(
                    "IX_auditoria_chamado_ChamadoId_DataEvento");

            builder.HasIndex(a => new
            {
                a.UsuarioId,
                a.DataEvento
            })
                .HasDatabaseName(
                    "IX_auditoria_chamado_UsuarioId_DataEvento");

            builder.HasIndex(a => new
            {
                a.Evento,
                a.DataEvento
            })
                .HasDatabaseName(
                    "IX_auditoria_chamado_Evento_DataEvento");
        }
    }
}
