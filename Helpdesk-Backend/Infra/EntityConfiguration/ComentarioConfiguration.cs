using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infra.EntityConfiguration
{
    public class ComentarioConfiguration : IEntityTypeConfiguration<Comentario>
    {
        public void Configure(EntityTypeBuilder<Comentario> builder)
        {
            builder.ToTable("comentario");

            builder.HasKey(c => c.Id_Comentario);

            builder.Property(c => c.Id_Comentario)
                .HasColumnName("Id_Comentario")
                .ValueGeneratedOnAdd();

            builder.Property(c => c.ChamadoId)
                .HasColumnName("ChamadoId")
                .IsRequired();

            builder.Property(c => c.UsuarioId)
                .HasColumnName("UsuarioId")
                .HasMaxLength(450)
                .IsRequired();

            builder.Property(c => c.Mensagem)
                .HasColumnName("Mensagem")
                .HasMaxLength(4000)
                .IsRequired(false);

            builder.Property(c => c.DataCriacao)
                .HasColumnName("DataCriacao")
                .IsRequired();

            builder.HasOne(c => c.Chamado)
                .WithMany(c => c.Comentarios)
                .HasForeignKey(c => c.ChamadoId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.Anexos)
                .WithOne(a => a.Comentario)
                .HasForeignKey(a => a.ComentarioId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}