using Domain.Entities;
using Domain.Enum;
using Domain.Interfaces;
using Infra.Context;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositores
{
    // Executa somente consultas da auditoria de chamados. Como os registros
    // são históricos e não são alterados, todas as leituras usam AsNoTracking.
    public class AuditoriaChamadoRepository
        : IAuditoriaChamadoRepository
    {
        private readonly AppDbContext _auditoriaChamadoContext;

        public AuditoriaChamadoRepository(
            AppDbContext auditoriaChamadoContext)
        {
            _auditoriaChamadoContext = auditoriaChamadoContext;
        }

        // CONSULTAS

        // Busca um único registro histórico pelo identificador da auditoria.
        public async Task<AuditoriaChamado?> BuscarPorId(
            int id)
        {
            return await _auditoriaChamadoContext
                .AuditoriasChamados
                .AsNoTracking()
                .FirstOrDefaultAsync(auditoria =>
                    auditoria.Id_AuditoriaChamado == id);
        }

        // Mantém a sequência cronológica dos eventos de um chamado.
        public async Task<IEnumerable<AuditoriaChamado>>
            BuscarPorChamado(int chamadoId)
        {
            return await _auditoriaChamadoContext
                .AuditoriasChamados
                .AsNoTracking()
                .Where(auditoria =>
                    auditoria.ChamadoId == chamadoId)
                .OrderBy(auditoria =>
                    auditoria.DataEvento)
                .ThenBy(auditoria =>
                    auditoria.Id_AuditoriaChamado)
                .ToListAsync();
        }

        public async Task<(
            IEnumerable<AuditoriaChamado> Auditorias,
            int TotalRegistros)> BuscarPaginado(
                DateTime dataInicial,
                DateTime dataFinal,
                string? numeroChamado,
                string? usuarioId,
                string? perfilUsuario,
                EventoAuditoriaChamadoEnum? evento,
                StatusChamadoEnum? status,
                int? categoriaAtendimentoId,
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

            var query = _auditoriaChamadoContext
                .AuditoriasChamados
                .AsNoTracking()
                .Where(auditoria =>
                    auditoria.DataEvento >= inicioPeriodo &&
                    auditoria.DataEvento < fimPeriodoExclusivo);

            if (!string.IsNullOrWhiteSpace(numeroChamado))
            {
                query = query.Where(auditoria =>
                    auditoria.NumeroChamado.Contains(
                        numeroChamado.Trim()));
            }

            if (!string.IsNullOrWhiteSpace(usuarioId))
            {
                query = query.Where(auditoria =>
                    auditoria.UsuarioId == usuarioId);
            }

            if (!string.IsNullOrWhiteSpace(perfilUsuario))
            {
                query = query.Where(auditoria =>
                    auditoria.PerfilUsuario == perfilUsuario);
            }

            if (evento.HasValue)
            {
                query = query.Where(auditoria =>
                    auditoria.Evento == evento.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(auditoria =>
                    auditoria.StatusNovo == status.Value);
            }

            if (categoriaAtendimentoId.HasValue)
            {
                query = query.Where(auditoria =>
                    auditoria.Chamado.CategoriaAtendimentoId ==
                    categoriaAtendimentoId.Value);
            }

            var totalRegistros = await query.CountAsync();

            // Os eventos mais recentes aparecem primeiro na consulta geral.
            var auditorias = await query
                .OrderByDescending(auditoria =>
                    auditoria.DataEvento)
                .ThenByDescending(auditoria =>
                    auditoria.Id_AuditoriaChamado)
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .ToListAsync();

            return (auditorias, totalRegistros);
        }
        // RETENÇÃO

        // Localiza o lote mais antigo de auditorias de chamado expiradas e
        // o remove diretamente no banco.
        public async Task<int> RemoverAnterioresA(
            DateTime dataLimite,
            int tamanhoLote)
        {
            var limiteUtc = DateTime.SpecifyKind(
                dataLimite,
                DateTimeKind.Utc);

            var idsParaRemover = await _auditoriaChamadoContext
                .AuditoriasChamados
                .AsNoTracking()
                .Where(auditoria =>
                    auditoria.DataEvento < limiteUtc)
                .OrderBy(auditoria =>
                    auditoria.DataEvento)
                .Select(auditoria =>
                    auditoria.Id_AuditoriaChamado)
                .Take(tamanhoLote)
                .ToListAsync();

            if (idsParaRemover.Count == 0)
            {
                return 0;
            }

            return await _auditoriaChamadoContext
                .AuditoriasChamados
                .Where(auditoria =>
                    idsParaRemover.Contains(
                        auditoria.Id_AuditoriaChamado))
                .ExecuteDeleteAsync();
        }
    }
}
