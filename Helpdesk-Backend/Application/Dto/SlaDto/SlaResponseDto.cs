using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dto.SlaDto
{
    public class SlaResponseDto
    {
        public int Id_Sla { get; set; }
        public PrioridadeEnum Prioridade { get; set; }
        public int TempoAtendimentoHoras { get; set; }
        public int TempoResolucaoHoras { get; set; }
        public bool Ativo { get; set; }
        public double TempoAtendimentoRealHoras { get; set; }
        public double TempoResolucaoRealHoras { get; set; }
        public bool AtendimentoDentroPrazo { get; set; }
        public bool ResolucaoDentroPrazo { get; set; }
       

    }
}
