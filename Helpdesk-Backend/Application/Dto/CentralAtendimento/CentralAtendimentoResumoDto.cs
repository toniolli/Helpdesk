namespace Application.Dto.CentralAtendimento
{
    // Reúne as quantidades do panorama operacional
    // e do acompanhamento de SLA.
    public class CentralAtendimentoResumoDto
    {
        public int Fila { get; set; }
        public int EmAtendimento { get; set; }
        public int AguardandoRetorno { get; set; }
        public int AguardandoEquipamento { get; set; }
        public int ProximosVencimento { get; set; }
        public int ForaSla { get; set; }
    }
}