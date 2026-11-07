using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IperfilPermissaoRepository
    {
            Task<PerfilPermissao> Vincular(PerfilPermissao perfilPermissao);
            Task<PerfilPermissao?> Desvincular(int perfilId,int permissaoId);
            Task<IEnumerable<Permissao>>BuscarPermissoesPerfil(int perfilId);
            Task<IEnumerable<Perfil>>BuscarPerfisPermissao(int permissaoId);
            Task<bool> ExisteVinculo(int perfilId,int permissaoId);
    }
}
