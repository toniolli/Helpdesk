using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infra.EntityConfiguration
{
    public class AnexoConfiguration : IEntityTypeConfiguration<Anexo>
    {
        public void Configure(EntityTypeBuilder<Anexo> builder)
        {
            builder.ToTable("anexo");

            builder.HasKey(a => a.Id_Anexo);

            builder.Property(a => a.Id_Anexo)
                .HasColumnName("Id_Anexo")
                .ValueGeneratedOnAdd();

            builder.Property(a => a.ComentarioId)
                .HasColumnName("ComentarioId")
                .IsRequired();

            builder.Property(a => a.NomeArquivo)
                .HasColumnName("NomeArquivo")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(a => a.NomeOriginal)
                .HasColumnName("NomeOriginal")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(a => a.ContentType)
                .HasColumnName("ContentType")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(a => a.Tamanho)
                .HasColumnName("Tamanho")
                .IsRequired();

            builder.Property(a => a.CaminhoArquivo)
                .HasColumnName("CaminhoArquivo")
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(a => a.DataUpload)
                .HasColumnName("DataUpload")
                .IsRequired();

            builder.HasOne(a => a.Comentario)
                .WithMany(c => c.Anexos)
                .HasForeignKey(a => a.ComentarioId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}