using Domain.Entities;

using Domain.Enum;

namespace Domain.Interfaces
{
    public interface IChamadoRepository
    {
        Task<Chamado> Adicionar( Chamado chamado);
        Task<Chamado?> BuscarPorId(int id);
        Task<Chamado?> BuscarPorNumero(string numero);
        Task<int> BuscarUltimoSequencialPorAno(int ano);
        Task<Chamado> Atualizar(Chamado chamado);
        Task<Chamado?> BuscarDisponivelParaDistribuicao(int chamadoId);
        Task Remover(int id);
        Task<IEnumerable<Chamado>> BuscarTodos();
        Task<IEnumerable<Chamado>> BuscarFilaAtendimento();
        Task<IEnumerable<Chamado>> BuscarChamadosDistribuido();
        Task<IEnumerable<Chamado>> BuscarMeusChamados(string tecnicoID);
        Task<IEnumerable<Chamado>> BuscarMeusChamadosSolicitante(string solicitanteId);
        Task<IEnumerable<Chamado>> BuscarChamadosPorCategorias(List<int> categoriasIds);
        Task<IEnumerable<Chamado>> BuscarChamadosParaIndicadoresAdmin();
        Task<IEnumerable<Chamado>>BuscarChamadosParaIndicadoresTecnico(string tecnicoId);
        Task<IEnumerable<Chamado>>BuscarChamadosParaIndicadoresSolicitante(string solicitanteId);
        // Busca somente os chamados necessários para montar a fotografia
        // da Central de Atendimento, incluindo ativos e movimentados no dia.
        Task<IEnumerable<Chamado>> BuscarDadosCentralAtendimento(DateTime inicioDia,DateTime fimDia, CancellationToken cancellationToken = default);
        //Paginados
        Task<(IEnumerable<Chamado> Chamados, int TotalRegistros)> BuscarFilaAtendimentoPaginado(int prioridade,int pagina, int tamanhoPagina, DateTime? dataInicial,DateTime? dataFinal);
        Task<(IEnumerable<Chamado> Chamados, int TotalRegistros)> BuscarMeusChamadosPaginado(bool visualizarTodos,string tecnicoId, int pagina, int tamanhoPagina);
        Task<(IEnumerable<Chamado> Chamados, int TotalRegistros)> BuscarMeusChamadosSolicitantesPaginado(bool visualizarTodos,string solicitanteId, int pagina, int tamanhoPagina);
        Task<(IEnumerable<Chamado> Chamados, int TotalRegistros)> BuscarHistoricoPaginado(string usuarioId,int perfilId,DateTime? dataInicial,DateTime? dataFinal, int pagina, int tamanhoPagina);
        Task<(IEnumerable<Chamado> Chamados, int TotalRegistros)> BuscarAnalisePaginada(
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
            int tamanhoPagina);
        Task<IEnumerable<Permissao>> BuscarPermissoes(int perfilId);

    }
}
