using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infra.EntityConfiguration
{
    public class AuditoriaConfiguration
        : IEntityTypeConfiguration<Auditoria>
    {
        public void Configure(EntityTypeBuilder<Auditoria> builder)
        {
            builder.ToTable("auditoria");

            builder.HasKey(a => a.Id_Auditoria);

            builder.Property(a => a.Id_Auditoria)
                .HasColumnName("Id_Auditoria")
                .ValueGeneratedOnAdd();

            builder.Property(a => a.UsuarioId)
                .HasColumnName("UsuarioId")
                .HasMaxLength(450);

            builder.Property(a => a.UsuarioNome)
                .HasColumnName("UsuarioNome")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(a => a.Acao)
                .HasColumnName("Acao")
                .IsRequired();

            builder.Property(a => a.Origem)
                .HasColumnName("Origem")
                .IsRequired();

            builder.Property(a => a.Entidade)
                .HasColumnName("Entidade")
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(a => a.RegistroId)
                .HasColumnName("RegistroId")
                .HasMaxLength(450)
                .IsRequired();

            builder.Property(a => a.ValoresAnteriores)
                .HasColumnName("ValoresAnteriores")
                .HasMaxLength(4000);

            builder.Property(a => a.ValoresAtuais)
                .HasColumnName("ValoresAtuais")
                .HasMaxLength(4000);

            builder.Property(a => a.EnderecoIp)
                .HasColumnName("EnderecoIp")
                .HasMaxLength(45);

            builder.Property(a => a.MetodoHttp)
                .HasColumnName("MetodoHttp")
                .HasMaxLength(10);

            builder.Property(a => a.Rota)
                .HasColumnName("Rota")
                .HasMaxLength(500);

            builder.Property(a => a.TraceId)
                .HasColumnName("TraceId")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(a => a.DataCriacao)
                .HasColumnName("DataCriacao")
                .IsRequired();

            // Índice principal: toda consulta exige período
            builder.HasIndex(a => a.DataCriacao)
                .HasDatabaseName("IX_auditoria_DataCriacao");

            // Índices para os filtros opcionais
            builder.HasIndex(a => new
            {
                a.UsuarioId,
                a.DataCriacao
            })
                .HasDatabaseName(
                    "IX_auditoria_UsuarioId_DataCriacao");

            builder.HasIndex(a => new
            {
                a.Acao,
                a.DataCriacao
            })
                .HasDatabaseName(
                    "IX_auditoria_Acao_DataCriacao");

            builder.HasIndex(a => new
            {
                a.Entidade,
                a.DataCriacao
            })
                .HasDatabaseName(
                    "IX_auditoria_Entidade_DataCriacao");
        }
    }
}