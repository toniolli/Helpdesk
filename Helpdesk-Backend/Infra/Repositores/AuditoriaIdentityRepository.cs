using Domain.Entities;
using Domain.Enum;
using Domain.Interfaces;
using Infra.Context;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositores
{
    // Persiste e consulta os eventos de auditoria de Identity. As consultas
    // administrativas são somente leitura e, por isso, usam AsNoTracking.
    public class AuditoriaIdentityRepository : IAuditoriaIdentityRepository
    {
        private readonly AppDbContext _auditoriaIdentityContext;

        public AuditoriaIdentityRepository(
            AppDbContext auditoriaIdentityContext)
        {
            _auditoriaIdentityContext = auditoriaIdentityContext;
        }

        // PERSISTÊNCIA

        // Adiciona um novo evento ao histórico de auditoria.
        public async Task<AuditoriaIdentity> Adicionar(
            AuditoriaIdentity auditoriaIdentity)
        {
            await _auditoriaIdentityContext
                .AuditoriaIdentities
                .AddAsync(auditoriaIdentity);

            await _auditoriaIdentityContext.SaveChangesAsync();

            return auditoriaIdentity;
        }

        // CONSULTAS

        // Busca um único evento pelo identificador da auditoria.
        public async Task<AuditoriaIdentity?> BuscarPorId(int id)
        {
            return await _auditoriaIdentityContext
                .AuditoriaIdentities
                .AsNoTracking()
                .FirstOrDefaultAsync(auditoria =>
                    auditoria.Id_AuditoriaIdentity == id);
        }

        public async Task<(
            IEnumerable<AuditoriaIdentity> Auditorias,
            int TotalRegistros)> BuscarPaginado(
                DateTime dataInicial,
                DateTime dataFinal,
                string? usuarioId,
                EventoAuditoriaIdentityEnum? evento,
                bool? sucesso,
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

            var query = _auditoriaIdentityContext
                .AuditoriaIdentities
                .AsNoTracking()
                .Where(auditoria =>
                    auditoria.DataCriacao >= inicioPeriodo &&
                    auditoria.DataCriacao < fimPeriodoExclusivo);

            if (!string.IsNullOrWhiteSpace(usuarioId))
            {
                query = query.Where(auditoria =>
                    auditoria.UsuarioId == usuarioId);
            }

            if (evento.HasValue)
            {
                query = query.Where(auditoria =>
                    auditoria.Evento == evento.Value);
            }

            if (sucesso.HasValue)
            {
                query = query.Where(auditoria =>
                    auditoria.Sucesso == sucesso.Value);
            }

            var totalRegistros = await query.CountAsync();

            // Os eventos mais recentes aparecem primeiro na consulta geral.
            var auditorias = await query
                .OrderByDescending(auditoria =>
                    auditoria.DataCriacao)
                .ThenByDescending(auditoria =>
                    auditoria.Id_AuditoriaIdentity)
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

            var idsParaRemover = await _auditoriaIdentityContext
                .AuditoriaIdentities
                .AsNoTracking()
                .Where(auditoria =>
                    auditoria.DataCriacao < limiteUtc)
                .OrderBy(auditoria =>
                    auditoria.DataCriacao)
                .Select(auditoria =>
                    auditoria.Id_AuditoriaIdentity)
                .Take(tamanhoLote)
                .ToListAsync();

            if (idsParaRemover.Count == 0)
            {
                return 0;
            }

            return await _auditoriaIdentityContext
                .AuditoriaIdentities
                .Where(auditoria =>
                    idsParaRemover.Contains(
                        auditoria.Id_AuditoriaIdentity))
                .ExecuteDeleteAsync();
        }
    }
}
