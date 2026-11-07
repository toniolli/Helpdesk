using Domain.Entities;
using Domain.Interfaces;
using Infra.Context;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositores
{
    // Centraliza a persistência e as consultas das permissões.
    public class PermissaoRepository : IpermissaoRepository
    {
        private readonly AppDbContext _permissaoContext;

        public PermissaoRepository(AppDbContext permissaoContext)
        {
            _permissaoContext = permissaoContext;
        }

        // PERSISTÊNCIA

        public async Task<Permissao> Adicionar(Permissao permissao)
        {
            _permissaoContext
                .Permissoes
                .AddAsync(permissao);

            await _permissaoContext.SaveChangesAsync();

            return permissao;
        }

        // CONSULTAS

        public async Task<Permissao?> BuscarPorId(int id)
        {
            return await _permissaoContext
                .Permissoes
                .FindAsync(id);
        }

        public async Task<IEnumerable<Permissao>> BuscarTodos()
        {
            return await _permissaoContext
                .Permissoes
                .ToListAsync();
        }

        // ALTERAÇÃO E REMOÇÃO

        public async Task<Permissao> Atualizar(Permissao permissao)
        {
            _permissaoContext
                .Permissoes
                .Update(permissao);

            await _permissaoContext.SaveChangesAsync();

            return permissao;
        }

        public async Task Remover(int id)
        {
            var permissao = await BuscarPorId(id);

            _permissaoContext.Remove(permissao);

            await _permissaoContext.SaveChangesAsync();
        }
    }
}
