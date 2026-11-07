using Domain.Entities;
using Domain.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infra.EntityConfiguration
{
    // Define como os eventos SignalR do Outbox serão armazenados no Oracle.
    public class EventoSignalROutboxConfiguration
        : IEntityTypeConfiguration<EventoSignalROutbox>
    {
        public void Configure(
            EntityTypeBuilder<EventoSignalROutbox> builder)
        {
            builder.ToTable("eventosignalroutbox");

            builder.HasKey(
                evento => evento.Id_EventoSignalROutbox);

            builder.Property(
                    evento => evento.Id_EventoSignalROutbox)
                .HasColumnName("Id_EventoSignalROutbox")
                .ValueGeneratedOnAdd();

            builder.Property(
                    evento => evento.NomeEvento)
                .HasColumnName("NomeEvento")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(
                    evento => evento.TipoDestino)
                .HasColumnName("TipoDestino")
                .IsRequired();

            builder.Property(
                    evento => evento.DestinatariosJson)
                .HasColumnName("DestinatariosJson")
                .HasColumnType("CLOB");

            builder.Property(
                    evento => evento.GrupoDestino)
                .HasColumnName("GrupoDestino")
                .HasMaxLength(100);

            builder.Property(
                    evento => evento.PayloadJson)
                .HasColumnName("PayloadJson")
                .HasColumnType("CLOB");

            builder.Property(
                    evento => evento.Status)
                .HasColumnName("Status")
                .HasDefaultValue(
                    StatusEventoSignalROutboxEnum.Pendente)
                .HasSentinel(
                    (StatusEventoSignalROutboxEnum)0)
                .IsRequired();

            builder.Property(
                    evento => evento.QuantidadeTentativas)
                .HasColumnName("QuantidadeTentativas")
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(
                    evento => evento.DataCriacaoUtc)
                .HasColumnName("DataCriacaoUtc")
                .IsRequired();

            builder.Property(
                    evento => evento.ProximaTentativaUtc)
                .HasColumnName("ProximaTentativaUtc")
                .IsRequired();

            builder.Property(
                    evento => evento.ProcessamentoIniciadoUtc)
                .HasColumnName("ProcessamentoIniciadoUtc");

            builder.Property(
                    evento => evento.DataEnvioUtc)
                .HasColumnName("DataEnvioUtc");

            builder.Property(
                    evento => evento.UltimoErro)
                .HasColumnName("UltimoErro")
                .HasMaxLength(4000);

            // Otimiza a busca do BackgroundService por eventos pendentes.
            builder.HasIndex(
                    evento => new
                    {
                        evento.Status,
                        evento.ProximaTentativaUtc,
                        evento.DataCriacaoUtc
                    })
                .HasDatabaseName(
                    "IX_Outbox_Status_ProximaTentativa");

            // Otimiza a limpeza dos eventos enviados há mais de cinco dias.
            builder.HasIndex(
                    evento => new
                    {
                        evento.Status,
                        evento.DataEnvioUtc
                    })
                .HasDatabaseName(
                    "IX_Outbox_Status_DataEnvio");
        }
    }
}
