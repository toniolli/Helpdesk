using Domain.Entities;
using Domain.Enum;

namespace Application.Dto.Chamado
{
    public class FecharChamadoDto
    {
        public TipoAtendimentoEnum TipoAtendimento { get; set; }
        public TipoEncerramentoEnum TipoEncerramento { get; set; }
        public string? Solucao { get; set; }
    }
}