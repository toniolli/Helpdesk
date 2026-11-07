namespace Application.Dto.CentralAtendimento
{
    // Reúne os indicadores de produção do dia exibidos
    // na Central de Atendimento.
    public class CentralAtendimentoHojeDto
    {
        public int Iniciados { get; set; }
        public int Finalizados { get; set; }
    }
}