using Domain.Entities;
using Domain.Interfaces;
using Infra.Context;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositores
{
    // Centraliza a persistência e as consultas das categorias de atendimento.
    public class CategoriaAtendimentoRepository
        : ICategoriaAtendimentoRepository
    {
        private readonly AppDbContext _categoriasAtendimentoContext;

        public CategoriaAtendimentoRepository(
            AppDbContext categoriasAtendimentoContext)
        {
            _categoriasAtendimentoContext = categoriasAtendimentoContext;
        }

        // PERSISTÊNCIA

        public async Task<CategoriaAtendimento> Adicionar(
            CategoriaAtendimento categoriaAtendimento)
        {
            await _categoriasAtendimentoContext
                .CategoriasAtendimento
                .AddAsync(categoriaAtendimento);

            await _categoriasAtendimentoContext.SaveChangesAsync();

            return categoriaAtendimento;
        }

        public async Task<CategoriaAtendimento> Atualizar(
            CategoriaAtendimento categoriaAtendimento)
        {
            _categoriasAtendimentoContext
                .CategoriasAtendimento
                .Update(categoriaAtendimento);

            await _categoriasAtendimentoContext.SaveChangesAsync();

            return categoriaAtendimento;
        }

        public async Task Remover(int id)
        {
            var categoria = await BuscarPorId(id);

            _categoriasAtendimentoContext.Remove(categoria);

            await _categoriasAtendimentoContext.SaveChangesAsync();
        }

        // CONSULTAS

        // FindAsync preserva o rastreamento utilizado pelos fluxos de alteração.
        public async Task<CategoriaAtendimento?> BuscarPorId(int id)
        {
            return await _categoriasAtendimentoContext
                .CategoriasAtendimento
                .FindAsync(id);
        }

        public async Task<IEnumerable<CategoriaAtendimento>> BuscarTodos()
        {
            return await _categoriasAtendimentoContext
                .CategoriasAtendimento
                .ToListAsync();
        }
    }
}
