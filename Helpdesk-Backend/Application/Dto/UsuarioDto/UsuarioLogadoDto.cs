using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dto.UsuarioDto
{
    public class UsuarioLogadoDto
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int PerfilId { get; set; }
        public string Perfil { get; set; } = string.Empty;
        public List<string> Permissoes { get; set; } = new();
    }
}
