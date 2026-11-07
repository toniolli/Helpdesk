using Domain.Entities;
using Domain.Enum;

namespace Application.Dto.CentralAtendimento
{
    // Representa somente os dados de um chamado necessários
    // para a fila e os alertas da Central de Atendimento.
    public class CentralAtendimentoChamadoDto
    {
        public int ChamadoId { get; set; }
        public string Numero { get; set; } = string.Empty;
        public string SolicitanteNome { get; set; } = string.Empty;
        public string Assunto { get; set; } = string.Empty;
        public PrioridadeEnum Prioridade { get; set; }
        public StatusChamadoEnum Status { get; set; }
        public DateTime DataAbertura { get; set; }
        public string Destino { get; set; } = string.Empty;
    }
}