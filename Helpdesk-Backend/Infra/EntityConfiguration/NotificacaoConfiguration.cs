using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infra.EntityConfiguration
{
    public class NotificacaoConfiguration : IEntityTypeConfiguration<Notificacao>
    {
        public void Configure(EntityTypeBuilder<Notificacao> builder)
        {
            builder.ToTable("notificacao");

            builder.HasKey(n => n.Id_Notificacao);

            builder.Property(n => n.Id_Notificacao)
                .HasColumnName("Id_Notificacao")
                .ValueGeneratedOnAdd();

            builder.Property(n => n.UsuarioId)
                .HasColumnName("UsuarioId")
                .HasMaxLength(450)
                .IsRequired();

            builder.Property(n => n.UsuarioOrigemId)
                .HasColumnName("UsuarioOrigemId")
                .HasMaxLength(450);

            builder.Property(n => n.ChamadoId)
                .HasColumnName("ChamadoId")
                .IsRequired();

            builder.Property(n => n.Tipo)
                .HasColumnName("Tipo")
                .IsRequired();

            builder.Property(n => n.Titulo)
                .HasColumnName("Titulo")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(n => n.Mensagem)
                .HasColumnName("Mensagem")
                .HasMaxLength(4000)
                .IsRequired();

            builder.Property(n => n.Lida)
                .HasColumnName("Lida")
                .HasColumnType("NUMBER(1)")
                .IsRequired();

            builder.Property(n => n.Status)
                .HasColumnName("Status")
                .IsRequired();

            builder.Property(n => n.DataCriacao)
                .HasColumnName("DataCriacao")
                .IsRequired();

            builder.Property(n => n.DataLida)
                .HasColumnName("DataLida");

            builder.HasOne(n => n.Chamado)
                .WithMany(c => c.Notificacoes)
                .HasForeignKey(n => n.ChamadoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}