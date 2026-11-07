using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IusuarioPerfilRepository
    {
            Task<UsuarioPerfil> Vincular(UsuarioPerfil usuarioPerfil);
            Task<UsuarioPerfil?> Desvincular(string usuarioId,int perfilId);
            Task<IEnumerable<Perfil>> BuscarPerfisUsuario(string usuarioId);
            Task<IEnumerable<UsuarioPerfil>> BuscarUsuariosPerfil(int perfilId);
            Task<bool> ExisteVinculo(string usuarioId, int perfilId);
            Task<IEnumerable<string>> BuscarUsuarioComPermissao(string nomePermissao);
    }
}
