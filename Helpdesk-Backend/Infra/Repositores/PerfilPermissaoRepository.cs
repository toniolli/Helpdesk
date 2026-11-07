using Domain.Entities;
using Domain.Interfaces;
using Infra.Context;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositores
{
    // Centraliza consultas e alterações dos vínculos entre perfis e permissões.
    public class PerfilPermissaoRepository : IperfilPermissaoRepository
    {
        private readonly AppDbContext _permissaoContext;

        public PerfilPermissaoRepository(AppDbContext permissaoContext)
        {
            _permissaoContext = permissaoContext;
        }

        // CONSULTAS

        // Retorna os perfis que possuem a permissão informada.
        public async Task<IEnumerable<Perfil>> BuscarPerfisPermissao(
            int permissaoId)
        {
            return await _permissaoContext
                .PerfilPermissoes
                .Where(p =>
                    p.PermissaoId == permissaoId)
                .Select(p =>
                    p.Perfil)
                .ToListAsync();
        }

        // Retorna as permissões vinculadas ao perfil informado.
        public async Task<IEnumerable<Permissao>> BuscarPermissoesPerfil(
            int perfilId)
        {
            return await _permissaoContext
                .PerfilPermissoes
                .Where(p =>
                    p.PerfilId == perfilId)
                .Select(p =>
                    p.Permissao)
                .ToListAsync();
        }

        // ALTERAÇÃO DE VÍNCULOS

        // Retorna nulo quando o vínculo solicitado não existe.
        public async Task<PerfilPermissao?> Desvincular(
            int perfilId,
            int permissaoId)
        {
            var perfilPermissao = await _permissaoContext
                .PerfilPermissoes
                .FirstOrDefaultAsync(p =>
                    p.PerfilId == perfilId &&
                    p.PermissaoId == permissaoId);

            if (perfilPermissao == null)
                return null;

            _permissaoContext.Remove(perfilPermissao);

            await _permissaoContext.SaveChangesAsync();

            return perfilPermissao;
        }

        // CONSULTA DE EXISTÊNCIA

        public async Task<bool> ExisteVinculo(
            int perfilId,
            int permissaoId)
        {
            return await _permissaoContext
                .PerfilPermissoes
                .AnyAsync(p =>
                    p.PerfilId == perfilId &&
                    p.PermissaoId == permissaoId);
        }

        // ALTERAÇÃO DE VÍNCULOS

        public async Task<PerfilPermissao> Vincular(
            PerfilPermissao perfilPermissao)
        {
            await _permissaoContext.AddAsync(perfilPermissao);
            await _permissaoContext.SaveChangesAsync();

            return perfilPermissao;
        }
    }
}
