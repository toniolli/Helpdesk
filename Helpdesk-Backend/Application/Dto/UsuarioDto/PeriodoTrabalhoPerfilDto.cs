namespace Application.Dto.UsuarioDto
{
    public class PeriodoTrabalhoPerfilDto
    {
        public int? Id_PeriodoTrabalhoPerfil { get; set; }
        public int? Versao { get; set; }
        public DateTime? DataInicioVigencia { get; set; }
        public DateTime? DataFimVigencia { get; set; }
        public bool SegundaAtivo { get; set; }
        public bool TercaAtivo { get; set; }
        public bool QuartaAtivo { get; set; }
        public bool QuintaAtivo { get; set; }
        public bool SextaAtivo { get; set; }
        public TimeSpan? HoraInicioSemana { get; set; }
        public TimeSpan? HoraFimSemana { get; set; }
        public bool SabadoAtivo { get; set; }
        public TimeSpan? HoraInicioSabado { get; set; }
        public TimeSpan? HoraFimSabado { get; set; }
        public bool DomingoAtivo { get; set; }
        public TimeSpan? HoraInicioDomingo { get; set; }
        public TimeSpan? HoraFimDomingo { get; set; }
    }
}
