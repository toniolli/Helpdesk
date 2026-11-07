using Domain.Entities;
using Domain.Interfaces;
using Infra.Context;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositores
{
    // Centraliza a persistência e as consultas dos comentários dos chamados.
    public class ComentarioRepository : IComentarioRepository
    {
        private readonly AppDbContext _comentariosContext;

        public ComentarioRepository(AppDbContext comentariosContext)
        {
            _comentariosContext = comentariosContext;
        }

        // PERSISTÊNCIA

        public async Task<Comentario> Adicionar(Comentario comentario)
        {
            await _comentariosContext
                .Comentarios
                .AddAsync(comentario);

            await _comentariosContext.SaveChangesAsync();

            return comentario;
        }

        // Carrega os anexos porque o comentário individual pode ser exibido
        // com todos os arquivos associados.
        public async Task<Comentario?> BuscarPorId(int id)
        {
            return await _comentariosContext
                .Comentarios
                .Include(x => x.Anexos)
                .FirstOrDefaultAsync(x =>
                    x.Id_Comentario == id);
        }

        // Retorna o histórico completo na ordem cronológica de criação.
        public async Task<IEnumerable<Comentario>> BuscarPorChamado(
            int chamadoId)
        {
            return await _comentariosContext
                .Comentarios
                .Where(x =>
                    x.ChamadoId == chamadoId)
                .Include(x =>
                    x.Anexos)
                .OrderBy(x =>
                    x.DataCriacao)
                .ToListAsync();
        }

        public async Task<Comentario> Atualizar(Comentario comentario)
        {
            _comentariosContext
                .Comentarios
                .Update(comentario);

            await _comentariosContext.SaveChangesAsync();

            return comentario;
        }

        public async Task Remover(int id)
        {
            var comentario = await BuscarPorId(id);

            _comentariosContext.Remove(comentario);

            await _comentariosContext.SaveChangesAsync();
        }

        // Retorna primeiro os comentários mais recentes dentro da página.
        public async Task<(
            IEnumerable<Comentario> Comentario,
            int TotalRegsitro)> BuscarPorChamadoPaginacao(
                int chamadoId,
                int pagina,
                int tamanhoPagina)
        {
            var query = _comentariosContext
                .Comentarios
                .Where(x =>
                    x.ChamadoId == chamadoId)
                .AsQueryable();

            var totalRegistro = await query.CountAsync();

            var comentarios = await query
                .Include(x=> x.Anexos)
                .OrderByDescending(x =>
                    x.DataCriacao)
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .ToListAsync();

            return (comentarios, totalRegistro);
        }
    }
}
