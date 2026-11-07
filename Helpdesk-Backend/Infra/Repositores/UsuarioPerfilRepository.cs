using Domain.Entities;
using Domain.Interfaces;
using Infra.Context;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositores
{
    // Centraliza as consultas e alterações dos vínculos entre usuários e perfis.
    public class UsuarioPerfilRepository : IusuarioPerfilRepository
    {
        private readonly AppDbContext _usuarioPerfilContext;

        public UsuarioPerfilRepository(AppDbContext usuarioPerfilContext)
        {
            _usuarioPerfilContext = usuarioPerfilContext;
        }

        // CONSULTAS

        // Retorna os perfis vinculados ao usuário informado.
        public async Task<IEnumerable<Perfil>> BuscarPerfisUsuario(
            string usuarioId)
        {
            return await _usuarioPerfilContext
                .UsuariosPerfis
                .Where(p =>
                    p.UsuarioId == usuarioId)
                .Select(p =>
                    p.Perfil)
                .ToListAsync();
        }

        // Retorna os usuários cujos perfis possuem a permissão informada.
        public async Task<IEnumerable<string>> BuscarUsuarioComPermissao(
            string nomePermissao)
        {
            return await _usuarioPerfilContext
                .UsuariosPerfis
                .Where(usuarioPerfil =>
                    _usuarioPerfilContext.PerfilPermissoes.Any(
                        perfilPermissao =>
                            perfilPermissao.PerfilId ==
                                usuarioPerfil.PerfilId &&
                            perfilPermissao.Permissao.Nome ==
                                nomePermissao))
                .Select(usuarioPerfil =>
                    usuarioPerfil.UsuarioId)
                .Distinct()
                .ToListAsync();
        }

        // Retorna todos os vínculos associados ao perfil informado.
        public async Task<IEnumerable<UsuarioPerfil>> BuscarUsuariosPerfil(
            int perfilId)
        {
            return await _usuarioPerfilContext
                .UsuariosPerfis
                .Where(p =>
                    p.PerfilId == perfilId)
                .ToListAsync();
        }

        // ALTERAÇÃO DE VÍNCULOS

        // Retorna nulo quando o vínculo solicitado não existe.
        public async Task<UsuarioPerfil?> Desvincular(
            string usuarioId,
            int perfilId)
        {
            var usuarioPerfil = await _usuarioPerfilContext
                .UsuariosPerfis
                .FirstOrDefaultAsync(p =>
                    p.UsuarioId == usuarioId &&
                    p.PerfilId == perfilId);

            if (usuarioPerfil == null)
                return null;

            _usuarioPerfilContext.Remove(usuarioPerfil);

            await _usuarioPerfilContext.SaveChangesAsync();

            return usuarioPerfil;
        }

        public async Task<bool> ExisteVinculo(
            string usuarioId,
            int perfilId)
        {
            return await _usuarioPerfilContext
                .UsuariosPerfis
                .AnyAsync(p =>
                    p.UsuarioId == usuarioId &&
                    p.PerfilId == perfilId);
        }

        public async Task<UsuarioPerfil> Vincular(
            UsuarioPerfil usuarioPerfil)
        {
            await _usuarioPerfilContext.AddAsync(usuarioPerfil);
            await _usuarioPerfilContext.SaveChangesAsync();

            return usuarioPerfil;
        }
    }
}
