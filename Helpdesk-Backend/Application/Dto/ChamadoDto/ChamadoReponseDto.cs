using Domain.Entities;
using Domain.Enum;
using System.Globalization;

namespace Application.Dto.Chamado
{
    public class ChamadoResponseDto
    {
        public int Id_Chamado { get; set; }
        public string Numero { get; set; }
        public string SolicitanteNome { get; set; }
        public string SolicitanteId { get; set; }
        public int CategoriaAtendimentoId { get; set; }
        public string EquipamentoAfetado { get; set; }
        public string Descricao { get; set; }
        public PrioridadeEnum Prioridade { get; set; }
        public string PrioridadeNome { get; set; }
        public StatusChamadoEnum Status { get; set; }
        public string TecnicoNome { get; set; }
        public string? TecnicoResponsavelId { get; set; }
        public TipoAtendimentoEnum? TipoAtendimento { get; set; }
        public TipoEncerramentoEnum? TipoEncerramento { get; set; }
        public string? Solucao { get; set; }
        public DateTime DataAbertura { get; set; }
        public DateTime? DataFechamento { get; set; }
        public DateTime? DataPrimeiroAtendimento { get; set; }
        public double TempoPausadoHoras { get; set; }
        public double TempoPausadoEquipamentoHoras { get; set; }
        public double TempoCorridoHoras { get; set; }
        public double? TempoPrimeiroAtendimentoHoras { get; set; }
        public double? TempoPrimeiroAtendimentoSlaHoras { get; set; }
        public double TempoAguardandoRetornoHoras { get; set; }
        public double TempoAguardandoEquipamentoHoras { get; set; }
        public double TempoTotalPausadoHoras { get; set; }
        public double? TempoResolucaoSlaHoras { get; set; }
        public bool SlaPendenteDefinicaoPerfil { get; set; }
        public double? TempoEfetivoAtendimentoHoras { get; set; }
    }
}
