using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dto.SlaDto
{
    public class SlaRequestDto
    {
      
        public PrioridadeEnum Prioridade { get;  set; }
        public int TempoAtendimentoHoras { get;  set; }
        public int TempoResolucaoHoras { get;  set; }
        public bool Ativo { get;  set; }
        
    }
}
