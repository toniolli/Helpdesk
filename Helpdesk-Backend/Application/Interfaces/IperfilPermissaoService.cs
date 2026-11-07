using Application.Dto.UsuarioDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IperfilPermissaoService
    {
        Task<PerfilPermissaoDto> Vincular( PerfilPermissaoDto perfilPermissaoDto);
        Task<PerfilPermissaoDto?> Desvincular( int perfilId,int permissaoId);
        Task<IEnumerable<PermissaoDto>> BuscarPermissoesPerfil(int perfilId);
        Task<IEnumerable<PerfilDto>>BuscarPerfisPermissao(int permissaoId);

      
    }
}
