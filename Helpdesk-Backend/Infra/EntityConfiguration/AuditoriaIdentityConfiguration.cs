using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infra.EntityConfiguration
{
    public class AuditoriaIdentityConfiguration
        : IEntityTypeConfiguration<AuditoriaIdentity>
    {
        public void Configure(EntityTypeBuilder<AuditoriaIdentity> builder)
        {
            builder.ToTable("auditoria_identity");

            builder.HasKey(a => a.Id_AuditoriaIdentity);

            builder.Property(a => a.Id_AuditoriaIdentity)
                .HasColumnName("Id_AuditoriaIdentity")
                .ValueGeneratedOnAdd();

            builder.Property(a => a.UsuarioId)
                .HasColumnName("UsuarioId")
                .HasMaxLength(450);

            builder.Property(a => a.UsuarioNome)
                .HasColumnName("UsuarioNome")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(a => a.Evento)
                .HasColumnName("Evento")
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

            builder.Property(a => a.Sucesso)
                .HasColumnName("Sucesso")
                .HasColumnType("NUMBER(1)")
                .IsRequired();

            builder.Property(a => a.Motivo)
                .HasColumnName("Motivo")
                .HasMaxLength(255)
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

            builder.Property(a => a.UserAgent)
                .HasColumnName("UserAgent")
                .HasMaxLength(1000);

            builder.Property(a => a.TraceId)
                .HasColumnName("TraceId")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(a => a.DataCriacao)
                .HasColumnName("DataCriacao")
                .IsRequired();

            // Índice principal para a consulta por período
            builder.HasIndex(a => a.DataCriacao)
                .HasDatabaseName(
                    "IX_auditoria_identity_DataCriacao");

            // Índices para os filtros opcionais
            builder.HasIndex(a => new
            {
                a.UsuarioId,
                a.DataCriacao
            })
                .HasDatabaseName(
                    "IX_auditoria_identity_UsuarioId_DataCriacao");

            builder.HasIndex(a => new
            {
                a.Evento,
                a.DataCriacao
            })
                .HasDatabaseName(
                    "IX_auditoria_identity_Evento_DataCriacao");

            builder.HasIndex(a => new
            {
                a.Sucesso,
                a.DataCriacao
            })
                .HasDatabaseName(
                    "IX_auditoria_identity_Sucesso_DataCriacao");
        }
    }
}