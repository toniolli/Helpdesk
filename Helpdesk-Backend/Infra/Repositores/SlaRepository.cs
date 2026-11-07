using Domain.Entities;
using Domain.Interfaces;
using Infra.Context;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositores
{
    // Centraliza a persistência e as consultas das configurações de SLA.
    public class SlaRepository : ISlaRepository
    {
        private readonly AppDbContext _slaContext;

        public SlaRepository(AppDbContext slaContext)
        {
            _slaContext = slaContext;
        }

        // PERSISTÊNCIA

        public async Task<SLA> Adicionar(SLA sla)
        {
            await _slaContext
                .Slas
                .AddAsync(sla);

            await _slaContext.SaveChangesAsync();

            return sla;
        }

        public async Task<SLA> Atualizar(SLA sla)
        {
            _slaContext
                .Slas
                .Update(sla);

            await _slaContext.SaveChangesAsync();

            return sla;
        }

        // CONSULTAS

        public async Task<SLA?> BuscarPorId(int id)
        {
            return await _slaContext
                .Slas
                .FindAsync(id);
        }

        // Retorna somente o SLA ativo da prioridade informada.
        public async Task<SLA> BuscarPrioridade(
            PrioridadeEnum prioridade)
        {
            return await _slaContext
                .Slas
                .FirstOrDefaultAsync(x =>
                    x.Prioridade == prioridade &&
                    x.Ativo);
        }

        public async Task<IEnumerable<SLA>> BuscarTodos()
        {
            return await _slaContext
                .Slas
                .ToListAsync();
        }

        // REMOÇÃO

        public async Task Remover(int id)
        {
            var Sla = await BuscarPorId(id);

            _slaContext.Slas.Remove(Sla);

            await _slaContext.SaveChangesAsync();
        }
    }
}
