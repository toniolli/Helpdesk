using Domain.Entities;
using Domain.Interfaces;
using Infra.Context;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositores
{
    // Centraliza a persistência e as consultas dos perfis.
    public class PerfilRepository : IperfilRepository
    {
        private readonly AppDbContext _perfilContext;

        public PerfilRepository(AppDbContext perfilContext)
        {
            _perfilContext = perfilContext;
        }

        // PERSISTÊNCIA

        public async Task<Perfil> Adicionar(Perfil perfil)
        {
            await _perfilContext
                .Perfis
                .AddAsync(perfil);

            await _perfilContext.SaveChangesAsync();

            return perfil;
        }

        // CONSULTAS

        // Carrega todo o histórico de períodos porque ele é necessário para
        // atualizar, versionar e validar a exclusão do perfil.
        public async Task<Perfil?> BuscarPorId(int id)
        {
            return await _perfilContext
                .Perfis
                .Include(perfil =>
                    perfil.PeriodosTrabalho)
                .FirstOrDefaultAsync(perfil =>
                    perfil.Id_Perfil == id);
        }

        // Na listagem, carrega somente o período de trabalho vigente.
        public async Task<IEnumerable<Perfil>> BuscarTodos()
        {
            return await _perfilContext
                .Perfis
                .Include(perfil =>
                    perfil.PeriodosTrabalho
                        .Where(periodo =>
                            periodo.DataFimVigencia == null))
                .ToListAsync();
        }

        // ALTERAÇÃO E REMOÇÃO

        public async Task<Perfil> Atualizar(Perfil perfil)
        {
            _perfilContext
                .Perfis
                .Update(perfil);

            await _perfilContext.SaveChangesAsync();

            return perfil;
        }

        public async Task Remover(int id)
        {
            var perfil = await BuscarPorId(id);

            _perfilContext.Remove(perfil);

            await _perfilContext.SaveChangesAsync();
        }
    }
}
