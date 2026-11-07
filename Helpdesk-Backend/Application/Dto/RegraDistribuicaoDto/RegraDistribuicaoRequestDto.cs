using Domain.Enum;

namespace Application.DTOs
{
    public class RegraDistribuicaoRequestDto
    {
        public string Nome { get; set; }
        public TipoRegraEnum TipoRegra { get; set; }
        public TipoDestinoEnum TipoDestino { get; set; }
        public bool Ativo { get; set; }
        public bool PeriodoIndefinido { get; set; }
        public DateTime? DataInicial { get; set; }
        public DateTime? DataFinal { get; set; }
        public string? UsuarioDestinoId { get; set; }
        public int? PerfilDestinoId { get; set; }
    }
}