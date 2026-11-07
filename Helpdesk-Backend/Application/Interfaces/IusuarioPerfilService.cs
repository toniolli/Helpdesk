using Application.Dto.UsuarioDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUsuarioPerfilService
    {
        Task<UsuarioPerfilDto> Vincular(UsuarioPerfilDto usuarioPerfilDto);
        Task<UsuarioPerfilDto?> Desvincular(string usuarioId,int perfilId);
        Task<IEnumerable<PerfilDto>>BuscarPerfisUsuario(string usuarioId);
        Task<IEnumerable<UsuarioPerfilDto>> BuscarUsuariosPerfil(int perfilId);
        Task<IEnumerable<string>> BuscarUsuariosComPermissao(string nomePermissao);
    }
}
