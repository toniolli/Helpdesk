using Domain.Entities;
using Domain.Enum;
using Domain.Interfaces;
using Infra.Context;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositores
{
    // Persiste e consulta os eventos da auditoria geral. As consultas
    // administrativas são somente leitura e, por isso, usam AsNoTracking.
    public class AuditoriaRepository : IAuditoriaRepository
    {
        private readonly AppDbContext _auditoriaContext;

        public AuditoriaRepository(AppDbContext auditoriaContext)
        {
            _auditoriaContext = auditoriaContext;
        }

        // PERSISTÊNCIA

        // Adiciona um novo evento ao histórico de auditoria.
        public async Task<Auditoria> Adicionar(Auditoria auditoria)
        {
            await _auditoriaContext.Auditorias.AddAsync(auditoria);
            await _auditoriaContext.SaveChangesAsync();

            return auditoria;
        }

        // CONSULTAS

        // Busca um único evento pelo identificador da auditoria.
        public async Task<Auditoria?> BuscarPorId(int id)
        {
            return await _auditoriaContext
                .Auditorias
                .AsNoTracking()
                .FirstOrDefaultAsync(auditoria =>
                    auditoria.Id_Auditoria == id);
        }

        public async Task<(
            IEnumerable<Auditoria> Auditorias,
            int TotalRegistros)> BuscarPaginado(
                DateTime dataInicial,
                DateTime dataFinal,
                string? usuarioId,
                AcaoAuditoriaEnum? acao,
                string? entidade,
                int pagina,
                int tamanhoPagina)
        {
            // O limite final é exclusivo para incluir integralmente o último
            // dia selecionado, independentemente do horário do evento.
            var inicioPeriodo = DateTime.SpecifyKind(
                dataInicial.Date,
                DateTimeKind.Utc);

            var fimPeriodoExclusivo = DateTime.SpecifyKind(
                dataFinal.Date.AddDays(1),
                DateTimeKind.Utc);

            var query = _auditoriaContext
                .Auditorias
                .AsNoTracking()
                .Where(auditoria =>
                    auditoria.DataCriacao >= inicioPeriodo &&
                    auditoria.DataCriacao < fimPeriodoExclusivo);

            if (!string.IsNullOrWhiteSpace(usuarioId))
            {
                query = query.Where(auditoria =>
                    auditoria.UsuarioId == usuarioId);
            }

            if (acao.HasValue)
            {
                query = query.Where(auditoria =>
                    auditoria.Acao == acao.Value);
            }

            if (!string.IsNullOrWhiteSpace(entidade))
            {
                query = query.Where(auditoria =>
                    auditoria.Entidade == entidade);
            }

            var totalRegistros = await query.CountAsync();

            // Os eventos mais recentes aparecem primeiro na consulta geral.
            var auditorias = await query
                .OrderByDescending(auditoria =>
                    auditoria.DataCriacao)
                .ThenByDescending(auditoria =>
                    auditoria.Id_Auditoria)
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .ToListAsync();

            return (auditorias, totalRegistros);
        }

        // RETENÇÃO

        // Localiza o lote mais antigo dentro do prazo expirado e o remove
        // diretamente no banco, sem materializar as entidades completas.
        public async Task<int> RemoverAnterioresA(
            DateTime dataLimite,
            int tamanhoLote)
        {
            var limiteUtc = DateTime.SpecifyKind(
                dataLimite,
                DateTimeKind.Utc);

            var idsParaRemover = await _auditoriaContext
                .Auditorias
                .AsNoTracking()
                .Where(auditoria =>
                    auditoria.DataCriacao < limiteUtc)
                .OrderBy(auditoria =>
                    auditoria.DataCriacao)
                .Select(auditoria =>
                    auditoria.Id_Auditoria)
                .Take(tamanhoLote)
                .ToListAsync();

            if (idsParaRemover.Count == 0)
            {
                return 0;
            }

            return await _auditoriaContext
                .Auditorias
                .Where(auditoria =>
                    idsParaRemover.Contains(
                        auditoria.Id_Auditoria))
                .ExecuteDeleteAsync();
        }
    }
}
