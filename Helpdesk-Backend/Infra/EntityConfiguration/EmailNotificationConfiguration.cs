using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infra.EntityConfiguration
{
    public class EmailNotificationConfiguration
        : IEntityTypeConfiguration<EmailNotification>
    {
        public void Configure(EntityTypeBuilder<EmailNotification> builder)
        {
            builder.ToTable("emailnotification");

            builder.HasKey(e => e.Id_EmailNotfication);

            builder.Property(e => e.Id_EmailNotfication)
                .HasColumnName("Id_EmailNotfication")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.ChamadoId)
                .HasColumnName("ChamadoId")
                .IsRequired();

            builder.Property(e => e.Tipo)
                .HasColumnName("Tipo")
                .IsRequired();

            builder.Property(e => e.Destinatario)
                .HasColumnName("Destinatario")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(e => e.Assunto)
                .HasColumnName("Assunto")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(e => e.Status)
                .HasColumnName("Status")
                .IsRequired();

            builder.Property(e => e.QuantidadeTentativas)
                .HasColumnName("QuantidadeTentativas")
                .IsRequired();

            builder.Property(e => e.DataCriacao)
                .HasColumnName("DataCriacao")
                .IsRequired();

            builder.Property(e => e.DataEnvio)
                .HasColumnName("DataEnvio");

            builder.Property(e => e.MensagemErro)
                .HasColumnName("MensagemErro")
                .HasMaxLength(4000);

            builder.HasOne(e => e.Chamado)
                .WithMany()
                .HasForeignKey(e => e.ChamadoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}