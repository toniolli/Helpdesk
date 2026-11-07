using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dto.CategoriaAtendimentoDto
{
    public class CategoriaAtendimentoRequestDto
    {
        public string Nome { get; set; }
        public string? Descricao { get; set; }
        public PrioridadeEnum Prioridade { get; set; }
    }
}
