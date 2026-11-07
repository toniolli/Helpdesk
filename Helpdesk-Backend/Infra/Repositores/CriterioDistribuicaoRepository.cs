using Domain.Entities;
using Domain.Interfaces;
using Infra.Context;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositores
{
    // Centraliza a persistência e as consultas dos critérios de distribuição.
    public class CriterioDistribuicaoRepository
        : ICriterioDistribuicaoRepository
    {
        private readonly AppDbContext _criterioContext;

        public CriterioDistribuicaoRepository(
            AppDbContext criterioContext)
        {
            _criterioContext = criterioContext;
        }

        // PERSISTÊNCIA

        public async Task<CriterioDistribuicao> Adicionar(
            CriterioDistribuicao criterio)
        {
            await _criterioContext
                .CriterioDistribuicoes
                .AddAsync(criterio);

            await _criterioContext.SaveChangesAsync();

            return criterio;
        }

        // A consulta por regra é somente leitura e não precisa de rastreamento.
        public async Task<IEnumerable<CriterioDistribuicao>>
            BuscarCriterioPorRegra(int regraId)
        {
            return await _criterioContext
                .CriterioDistribuicoes
                .AsNoTracking()
                .Where(x =>
                    x.RegraDistribuicaoId == regraId)
                .ToListAsync();
        }

        // Mantém o rastreamento utilizado pelo fluxo de remoção.
        public async Task<CriterioDistribuicao> BuscarPorId(int id)
        {
            return await _criterioContext
                .CriterioDistribuicoes
                .FindAsync(id);
        }

        public async Task<IEnumerable<CriterioDistribuicao>> BuscarTodos()
        {
            return await _criterioContext
                .CriterioDistribuicoes
                .ToListAsync();
        }

        public async Task Remover(int id)
        {
            var criterio = await BuscarPorId(id);

            _criterioContext.Remove(criterio);

            await _criterioContext.SaveChangesAsync();
        }
    }
}
