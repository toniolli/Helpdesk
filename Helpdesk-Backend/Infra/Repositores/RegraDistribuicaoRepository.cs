using Domain.Entities;
using Domain.Interfaces;
using Infra.Context;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositores
{
    // Centraliza a persistência e as consultas das regras de distribuição.
    public class RegraDistribuicaoRepository : IRegraDistribuicaoRepository
    {
        private readonly AppDbContext _regrasContext;

        public RegraDistribuicaoRepository(AppDbContext regrasContext)
        {
            _regrasContext = regrasContext;
        }

        // PERSISTÊNCIA

        public async Task<RegraDistribuicao> Adicionar(
            RegraDistribuicao regra)
        {
            await _regrasContext
                .RegrasDistribuicoes
                .AddAsync(regra);

            await _regrasContext.SaveChangesAsync();

            return regra;
        }

        public async Task<RegraDistribuicao> Atualizar(
            RegraDistribuicao regra)
        {
            _regrasContext
                .RegrasDistribuicoes
                .Update(regra);

            await _regrasContext.SaveChangesAsync();

            return regra;
        }

        // CONSULTAS

        public async Task<RegraDistribuicao?> BuscarPorId(int id)
        {
            return await _regrasContext
                .RegrasDistribuicoes
                .FindAsync(id);
        }

        // Carrega os critérios porque eles participam da escolha da regra
        // automática e das validações de categoria.
        public async Task<IEnumerable<RegraDistribuicao>> BuscarTodos()
        {
            return await _regrasContext
                .RegrasDistribuicoes
                .Include(x =>
                    x.Criterios)
                .ToListAsync();
        }

        // REMOÇÃO

        public async Task Remover(int id)
        {
            var regra = await BuscarPorId(id);

            _regrasContext.Remove(regra);

            await _regrasContext.SaveChangesAsync();
        }
    }
}
