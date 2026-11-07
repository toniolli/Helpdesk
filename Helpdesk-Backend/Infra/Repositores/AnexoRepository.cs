using Domain.Entities;
using Domain.Interfaces;
using Infra.Context;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositores
{
    // Centraliza a persistência dos metadados dos anexos.
    // O armazenamento e a remoção do arquivo físico pertencem ao AnexoService.
    public class AnexoRepository : IAnexoRepository
    {
        private readonly AppDbContext _anexoContext;

        public AnexoRepository(AppDbContext anexoContext)
        {
            _anexoContext = anexoContext;
        }

        // ==========================
        // PERSISTÊNCIA
        // ==========================

        public async Task<Anexo> Adicionar(Anexo anexo)
        {
            await _anexoContext.Anexos.AddAsync(anexo);
            await _anexoContext.SaveChangesAsync();
            return anexo;
        }

        public async Task<Anexo> Atualizar(Anexo anexo)
        {
            _anexoContext.Anexos.Update(anexo);

            await _anexoContext.SaveChangesAsync();

            return anexo;
        }

        public async Task Remover(int id)
        {
            var anexo = await BuscarPorId(id);

            if (anexo == null)
                return;

            _anexoContext.Anexos.Remove(anexo);

            await _anexoContext.SaveChangesAsync();
        }

        // ==========================
        // CONSULTAS
        // ==========================

        // Mantém o rastreamento porque os fluxos atuais compartilham o mesmo AppDbContext.
        public async Task<Anexo?> BuscarPorId(int id)
        {
            return await _anexoContext.Anexos
                .FirstOrDefaultAsync(x => x.Id_Anexo == id);
        }

        public async Task<IEnumerable<Anexo>> BuscarPorComentario(
            int comentarioId)
        {
            return await _anexoContext.Anexos
                .Where(x => x.ComentarioId == comentarioId)
                .ToListAsync();
        }
    }
}
