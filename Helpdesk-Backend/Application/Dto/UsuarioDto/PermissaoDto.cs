using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Application.Dto.UsuarioDto
{
    public class PermissaoDto
    {
        public int? Id_Permissao { get; set; }
        public string Nome { get; set; }
        public string? Descricao { get; set; }
        public bool Ativo { get; set; }
    }
}
