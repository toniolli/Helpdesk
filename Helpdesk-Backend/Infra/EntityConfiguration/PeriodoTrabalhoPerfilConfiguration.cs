using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infra.EntityConfiguration
{
    public class PeriodoTrabalhoPerfilConfiguration :
        IEntityTypeConfiguration<PeriodoTrabalhoPerfil>
    {
        public void Configure(
            EntityTypeBuilder<PeriodoTrabalhoPerfil> builder)
        {
            builder.ToTable("periodo_trabalho_perfil");

            builder.HasKey(periodo =>
                periodo.Id_PeriodoTrabalhoPerfil);

            builder.Property(periodo =>
                    periodo.Id_PeriodoTrabalhoPerfil)
                .HasColumnName("Id_PeriodoTrabalhoPerfil")
                .ValueGeneratedOnAdd();

            builder.Property(periodo => periodo.PerfilId)
                .HasColumnName("PerfilId")
                .IsRequired();

            builder.Property(periodo => periodo.Versao)
                .HasColumnName("Versao")
                .IsRequired();

            builder.Property(periodo => periodo.DataInicioVigencia)
                .HasColumnName("DataInicioVigencia")
                .IsRequired();

            builder.Property(periodo => periodo.DataFimVigencia)
                .HasColumnName("DataFimVigencia");

            ConfigurarBooleano(
                builder.Property(periodo => periodo.SegundaAtivo),
                "SegundaAtivo");
            ConfigurarBooleano(
                builder.Property(periodo => periodo.TercaAtivo),
                "TercaAtivo");
            ConfigurarBooleano(
                builder.Property(periodo => periodo.QuartaAtivo),
                "QuartaAtivo");
            ConfigurarBooleano(
                builder.Property(periodo => periodo.QuintaAtivo),
                "QuintaAtivo");
            ConfigurarBooleano(
                builder.Property(periodo => periodo.SextaAtivo),
                "SextaAtivo");
            ConfigurarBooleano(
                builder.Property(periodo => periodo.SabadoAtivo),
                "SabadoAtivo");
            ConfigurarBooleano(
                builder.Property(periodo => periodo.DomingoAtivo),
                "DomingoAtivo");

            ConfigurarHorario(
                builder.Property(periodo => periodo.HoraInicioSemana),
                "HoraInicioSemana");
            ConfigurarHorario(
                builder.Property(periodo => periodo.HoraFimSemana),
                "HoraFimSemana");
            ConfigurarHorario(
                builder.Property(periodo => periodo.HoraInicioSabado),
                "HoraInicioSabado");
            ConfigurarHorario(
                builder.Property(periodo => periodo.HoraFimSabado),
                "HoraFimSabado");
            ConfigurarHorario(
                builder.Property(periodo => periodo.HoraInicioDomingo),
                "HoraInicioDomingo");
            ConfigurarHorario(
                builder.Property(periodo => periodo.HoraFimDomingo),
                "HoraFimDomingo");

            builder.HasIndex(periodo => new
                {
                    periodo.PerfilId,
                    periodo.Versao
                })
                .IsUnique();
        }

        private static void ConfigurarBooleano(
            PropertyBuilder<bool> property,
            string nomeColuna)
        {
            property
                .HasColumnName(nomeColuna)
                .HasColumnType("NUMBER(1)")
                .IsRequired();
        }

        private static void ConfigurarHorario(
            PropertyBuilder<TimeSpan?> property,
            string nomeColuna)
        {
            property
                .HasColumnName(nomeColuna)
                .HasConversion(
                    horario => horario.HasValue
                        ? (short?)horario.Value.TotalMinutes
                        : null,
                    minutos => minutos.HasValue
                        ? TimeSpan.FromMinutes(minutos.Value)
                        : null)
                .HasColumnType("NUMBER(5)");
        }
    }
}
