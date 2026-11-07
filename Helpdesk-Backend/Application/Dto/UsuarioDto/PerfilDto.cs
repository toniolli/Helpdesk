using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dto.UsuarioDto
{
    public class PerfilDto
    {
        public int? Id_Perfil { get; set; }
        public string Nome { get; set; }
        public string? Descricao { get; set; }
        public bool Ativo { get; set; }
        public bool? PossuiPeriodoTrabalho { get; set; }
        public PeriodoTrabalhoPerfilDto? PeriodoTrabalho { get; set; }
    }
}
