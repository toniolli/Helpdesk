using Domain.Enum;

namespace Application.Dto.Chamado
{
    public class ChamadoRequestDto
    {
        public int CategoriaAtendimentoId { get; set; }
        public string EquipamentoAfetado { get; set; }
        public string Descricao { get; set; }
    }
}