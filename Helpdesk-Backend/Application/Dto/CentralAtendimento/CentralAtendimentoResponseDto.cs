namespace Application.Dto.CentralAtendimento
{
    // Representa uma fotografia completa da Central de Atendimento.
    // Cada nova fotografia substitui integralmente a anterior no frontend.
    public class CentralAtendimentoResponseDto
    {
        public Guid InstanciaId { get; set; }
        public long Versao { get; set; }
        public DateTime DataAtualizacaoUtc { get; set; }
        public IReadOnlyCollection<CentralAtendimentoChamadoDto> Fila { get; set; } = Array.Empty<CentralAtendimentoChamadoDto>();
        public IReadOnlyCollection<CentralAtendimentoChamadoDto>ChamadosMonitorados{ get; set; } = Array.Empty<CentralAtendimentoChamadoDto>();
        public CentralAtendimentoHojeDto AtendimentosHoje{ get; set; } = new();
        public CentralAtendimentoResumoDto ResumoOperacional{ get; set; } = new();
    }
}