using Domain.Entities;
using Domain.Enum;
using Domain.Interfaces;
using Domain.Validation;
using Infra.Context;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositores
{
    // Centraliza a persistência e as consultas de chamados.
    // As consultas que alimentam cálculos de SLA devem partir de ConsultarComPeriodoSla
    // para carregar a fotografia do período de trabalho vinculada ao chamado.
    public class ChamadoRepository : IChamadoRepository
    {
        private readonly AppDbContext _chamadosContext;

        public ChamadoRepository(AppDbContext chamadosContext)
        {
            _chamadosContext = chamadosContext;
        }

        // Mantém o carregamento do período de SLA consistente entre as consultas.
        // Não adicionar AsNoTracking aqui: alguns fluxos reutilizam esta base para atualizar o chamado.
        private IQueryable<Chamado> ConsultarComPeriodoSla()
        {
            return _chamadosContext.Chamados
                .Include(chamado =>
                    chamado.PeriodoTrabalhoPerfil);
        }

        // ==========================
        // PERSISTÊNCIA
        // ==========================

        public async Task<Chamado> Adicionar(Chamado chamado)
        {
            await _chamadosContext.Chamados.AddAsync(chamado);
            await _chamadosContext.SaveChangesAsync();
            return chamado;
        }

        public async Task<Chamado> Atualizar(Chamado chamado)
        {
            _chamadosContext.Chamados.Update(chamado);

            try
            {
                await _chamadosContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new ConcurrencyException(
                    "O chamado foi alterado por outro usuario!",
                    ex);
            }

            return chamado;
        }

        public async Task Remover(int id)
        {
            var chamado = await BuscarPorId(id);
            _chamadosContext.Remove(chamado);
            await _chamadosContext.SaveChangesAsync();
        }

        // ==========================
        // CONSULTAS INDIVIDUAIS
        // ==========================

        public async Task<Chamado?> BuscarPorId(int id)
        {
            return await ConsultarComPeriodoSla()
                .Include(x => x.CategoriaAtendimento)
                .Include(x => x.Comentarios)
                .FirstOrDefaultAsync(x => x.Id_Chamado == id);
        }

        public async Task<Chamado?> BuscarPorNumero(string numero)
        {
            return await ConsultarComPeriodoSla()
                .FirstOrDefaultAsync(x => x.Numero == numero);
        }

        public async Task<int> BuscarUltimoSequencialPorAno(int ano)
        {
            var ultimoNumero = await _chamadosContext.Chamados
                .Where(x => x.Numero != null &&
                            x.Numero.StartsWith($"CH-{ano}-"))
                .OrderByDescending(x => x.Numero)
                .Select(x => x.Numero)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(ultimoNumero))
                return 0;

            var sequencial = ultimoNumero.Split('-')[2];

            return int.Parse(sequencial);
        }

        // ==========================
        // LISTAGENS OPERACIONAIS
        // ==========================

        public async Task<IEnumerable<Chamado>> BuscarTodos()
        {
            return await ConsultarComPeriodoSla()
                .Include(x => x.CategoriaAtendimento)
                .ToListAsync();
        }

        public async Task<IEnumerable<Chamado>> BuscarFilaAtendimento()
        {
            return await ConsultarComPeriodoSla()
                .Where(x => x.Status == StatusChamadoEnum.Aberto)
                .OrderByDescending(x => x.Prioridade)
                .ThenBy(x => x.DataAbertura)
                .ToListAsync();
        }

        public async Task<IEnumerable<Chamado>> BuscarMeusChamados(
            string tecnicoID)
        {
            return await ConsultarComPeriodoSla()
                .Where(x => x.TecnicoResponsavelId == tecnicoID && (
                    x.Status == StatusChamadoEnum.EmAtendimento ||
                    x.Status == StatusChamadoEnum.AguardandoRetorno ||
                    x.Status == StatusChamadoEnum.AguardandoEquipamento))
                .ToListAsync();
        }

        public async Task<IEnumerable<Chamado>> BuscarMeusChamadosSolicitante(
            string solicitanteId)
        {
            return await ConsultarComPeriodoSla()
                .Where(x => x.SolicitanteId == solicitanteId && (
                    x.Status == StatusChamadoEnum.AguardandoEquipamento ||
                    x.Status == StatusChamadoEnum.EmAtendimento ||
                    x.Status == StatusChamadoEnum.AguardandoRetorno ||
                    x.Status == StatusChamadoEnum.AguardandoEquipamento))
                .ToListAsync();
        }
        // Busca somente os chamados necessários para montar a fotografia
        // atual da Central de Atendimento.
        public async Task<IEnumerable<Chamado>>
            BuscarDadosCentralAtendimento(
                DateTime inicioDia,
                DateTime fimDia,
                CancellationToken cancellationToken = default)
        {
            return await ConsultarComPeriodoSla()
                .AsNoTracking()
                .Include(chamado =>
                    chamado.PeriodoTrabalhoPerfil)
                .ThenInclude(periodo =>
                    periodo!.Perfil)
                .Where(chamado =>
                    (
                        chamado.Status != StatusChamadoEnum.Fechado &&
                        chamado.Status != StatusChamadoEnum.Cancelado
                    ) ||
                    (
                        chamado.DataPrimeiroAtendimento.HasValue &&
                        chamado.DataPrimeiroAtendimento.Value >= inicioDia &&
                        chamado.DataPrimeiroAtendimento.Value < fimDia
                    ) ||
                    (
                        chamado.DataFechamento.HasValue &&
                        chamado.DataFechamento.Value >= inicioDia &&
                        chamado.DataFechamento.Value < fimDia
                    ))
                .ToListAsync(cancellationToken);
        }

        // ==========================
        // CONSULTAS PAGINADAS
        // ==========================

        public async Task<(IEnumerable<Chamado> Chamados, int TotalRegistros)>
            BuscarFilaAtendimentoPaginado(
                int prioridade,
                int pagina,
                int tamanhoPagina,
                DateTime? dataInicial,
                DateTime? dataFinal)
        {
            var query = ConsultarComPeriodoSla()
                .Include(x => x.CategoriaAtendimento)
                .Where(x =>
                    x.Status == StatusChamadoEnum.Aberto &&
                    (int)x.Prioridade == prioridade)
                .AsQueryable();

            var totalRegistros =
                await query.CountAsync();

            if (dataInicial.HasValue)
            {
                query = query.Where(x =>
                    x.DataAbertura >= dataInicial.Value);
            }

            if (dataFinal.HasValue)
            {
                var fimDoDia = dataFinal.Value.Date
                    .AddDays(1)
                    .AddTicks(-1);

                query = query.Where(x =>
                    x.DataAbertura <= fimDoDia);
            }

            var chamados = await query
                .OrderBy(x => x.DataAbertura)
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .ToListAsync();

            return (chamados, totalRegistros);
        }

        public async Task<(IEnumerable<Chamado> Chamados, int TotalRegistros)>
            BuscarMeusChamadosPaginado(
                bool visualizarTodos,
                string tecnicoId,
                int pagina,
                int tamanhoPagina)
        {
            var query = ConsultarComPeriodoSla()
                .Include(x => x.CategoriaAtendimento)
                .Where(x =>
                    x.Status == StatusChamadoEnum.EmAtendimento ||
                    x.Status == StatusChamadoEnum.AguardandoRetorno ||
                    x.Status == StatusChamadoEnum.AguardandoEquipamento)
                .AsQueryable();

            if (!visualizarTodos)
            {
                query = query.Where(x =>
                    x.TecnicoResponsavelId == tecnicoId);
            }

            var totalRegistros = await query.CountAsync();

            var chamados = await query
                .OrderByDescending(x => x.DataAbertura)
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .ToListAsync();

            return (chamados, totalRegistros);
        }

        public async Task<(IEnumerable<Chamado> Chamados, int TotalRegistros)>
            BuscarMeusChamadosSolicitantesPaginado(
                bool visualizarTodos,
                string solicitanteId,
                int pagina,
                int tamanhoPagina)
        {
            var query = ConsultarComPeriodoSla()
                .Include(x => x.CategoriaAtendimento)
                .Where(x =>
                    x.Status == StatusChamadoEnum.Aberto ||
                    x.Status == StatusChamadoEnum.EmAtendimento ||
                    x.Status == StatusChamadoEnum.AguardandoRetorno ||
                    x.Status == StatusChamadoEnum.AguardandoEquipamento)
                .AsQueryable();

            if (!visualizarTodos)
            {
                query = query.Where(x =>
                    x.SolicitanteId == solicitanteId);
            }

            var totalRegistros = await query.CountAsync();

            var chamados = await query
                .OrderByDescending(x => x.DataAbertura)
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .ToListAsync();

            return (chamados, totalRegistros);
        }

        public async Task<(IEnumerable<Chamado> Chamados, int TotalRegistros)>
            BuscarHistoricoPaginado(
                string usuarioId,
                int perfilId,
                DateTime? dataInicial,
                DateTime? dataFinal,
                int pagina,
                int tamanhoPagina)
        {
            var query = ConsultarComPeriodoSla()
                .Include(x => x.CategoriaAtendimento)
                .AsQueryable();

            var permissao = await BuscarPermissoes(perfilId);

            var podeVerTodos = permissao.Any(x =>
                x.Nome == "VISUALIZAR_TODOS_CHAMADOS");

            var ehTecnico = permissao.Any(x =>
                x.Nome == "ASSUMIR_CHAMADO");

            if (!podeVerTodos)
            {
                if (ehTecnico)
                {
                    query = query.Where(x =>
                        x.TecnicoResponsavelId == usuarioId);
                }
                else
                {
                    query = query.Where(x =>
                        x.SolicitanteId == usuarioId);
                }
            }

            if (dataInicial.HasValue)
            {
                query = query.Where(x =>
                    x.DataAbertura >= dataInicial.Value);
            }

            if (dataFinal.HasValue)
            {
                var fimDoDia = dataFinal.Value.Date
                    .AddDays(1)
                    .AddTicks(-1);

                query = query.Where(x =>
                    x.DataAbertura <= fimDoDia);
            }

            var totalRegistros =
                await query.CountAsync();

            var chamados = await query
                .OrderByDescending(x => x.DataAbertura)
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .ToListAsync();

            return (chamados, totalRegistros);
        }

        public async Task<(IEnumerable<Chamado> Chamados, int TotalRegistros)>
            BuscarAnalisePaginada(
                string? numeroChamado,
                DateTime? dataInicial,
                DateTime? dataFinal,
                TipoUsuarioDashboardEnum tipoUsuario,
                string? usuarioId,
                IReadOnlyCollection<string>? usuariosPerfilIds,
                int? categoriaAtendimentoId,
                PrioridadeEnum? prioridade,
                StatusChamadoEnum? status,
                int pagina,
                int tamanhoPagina)
        {
            // Consulta exclusivamente de leitura usada pela análise gerencial.
            var query = ConsultarComPeriodoSla()
                .AsNoTracking()
                .Include(chamado => chamado.CategoriaAtendimento)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(numeroChamado))
            {
                var numero = numeroChamado.Trim();

                query = query.Where(chamado =>
                    chamado.Numero != null &&
                    chamado.Numero.Contains(numero));
            }

            if (dataInicial.HasValue)
            {
                var inicio = dataInicial.Value.Date;

                query = query.Where(chamado =>
                    chamado.DataAbertura >= inicio);
            }

            if (dataFinal.HasValue)
            {
                var fimExclusivo = dataFinal.Value.Date.AddDays(1);

                query = query.Where(chamado =>
                    chamado.DataAbertura < fimExclusivo);
            }

            if (categoriaAtendimentoId.HasValue)
            {
                query = query.Where(chamado =>
                    chamado.CategoriaAtendimentoId ==
                    categoriaAtendimentoId.Value);
            }

            if (prioridade.HasValue)
            {
                query = query.Where(chamado =>
                    chamado.Prioridade == prioridade.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(chamado =>
                    chamado.Status == status.Value);
            }

            if (usuariosPerfilIds != null)
            {
                if (tipoUsuario == TipoUsuarioDashboardEnum.Tecnico)
                {
                    query = query.Where(chamado =>
                        chamado.TecnicoResponsavelId != null &&
                        usuariosPerfilIds.Contains(
                            chamado.TecnicoResponsavelId));
                }
                else
                {
                    query = query.Where(chamado =>
                        chamado.SolicitanteId != null &&
                        usuariosPerfilIds.Contains(
                            chamado.SolicitanteId));
                }
            }

            if (!string.IsNullOrWhiteSpace(usuarioId))
            {
                if (tipoUsuario == TipoUsuarioDashboardEnum.Tecnico)
                {
                    query = query.Where(chamado =>
                        chamado.TecnicoResponsavelId == usuarioId);
                }
                else
                {
                    query = query.Where(chamado =>
                        chamado.SolicitanteId == usuarioId);
                }
            }

            var totalRegistros = await query.CountAsync();

            var chamados = await query
                .OrderByDescending(chamado => chamado.DataAbertura)
                .ThenByDescending(chamado => chamado.Id_Chamado)
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .ToListAsync();

            return (chamados, totalRegistros);
        }

        // ==========================
        // APOIO ÀS REGRAS DE ACESSO E DISTRIBUIÇÃO
        // ==========================

        public async Task<IEnumerable<Permissao>> BuscarPermissoes(int perfilId)
        {
            var permissoesPerfil = await _chamadosContext.PerfilPermissoes
                .Where(x => x.PerfilId == perfilId)
                .Select(x => x.Permissao)
                .ToListAsync();

            return permissoesPerfil;
        }

        public async Task<IEnumerable<Chamado>> BuscarChamadosPorCategorias(
            List<int> categoriasIds)
        {
            return await ConsultarComPeriodoSla()
                .Where(x =>
                    categoriasIds.Contains(x.CategoriaAtendimentoId))
                .ToListAsync();
        }

        public async Task<IEnumerable<Chamado>> BuscarChamadosDistribuido()
        {
            return await ConsultarComPeriodoSla()
                .AsNoTracking()
                .Where(chamado =>
                    chamado.Status == StatusChamadoEnum.Distribuido)
                .ToListAsync();
        }

        public async Task<Chamado?> BuscarDisponivelParaDistribuicao(
            int chamadoId)
        {
            return await ConsultarComPeriodoSla()
                .FirstOrDefaultAsync(chamado =>
                    chamado.Id_Chamado == chamadoId &&
                    chamado.Status == StatusChamadoEnum.Aberto &&
                    string.IsNullOrWhiteSpace(
                        chamado.TecnicoResponsavelId));
        }

        // ==========================
        // CONSULTAS PARA INDICADORES
        // ==========================

        // Indicadores são somente leitura; AsNoTracking evita rastreamento desnecessário pelo EF.
        public async Task<IEnumerable<Chamado>> BuscarChamadosParaIndicadoresAdmin()
        {
            return await ConsultarComPeriodoSla()
                .AsNoTracking()
                .Include(chamado =>
                    chamado.CategoriaAtendimento)
                .ToListAsync();
        }

        public async Task<IEnumerable<Chamado>> BuscarChamadosParaIndicadoresTecnico(
            string tecnicoId)
        {
            return await ConsultarComPeriodoSla()
                .AsNoTracking()
                .Include(chamado => chamado.CategoriaAtendimento)
                .Where(chamado =>
                    chamado.TecnicoResponsavelId == tecnicoId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Chamado>> BuscarChamadosParaIndicadoresSolicitante(
            string solicitanteId)
        {
            return await ConsultarComPeriodoSla()
                .AsNoTracking()
                .Include(chamado => chamado.CategoriaAtendimento)
                .Where(chamado =>
                    chamado.SolicitanteId == solicitanteId)
                .ToListAsync();
        }
    }
}
