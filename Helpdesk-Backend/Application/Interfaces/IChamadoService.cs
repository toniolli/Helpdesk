 using Application.Dto.Chamado;
using Application.Dto.Paginacao;
using Application.ViewModel;
using Domain.Entities;
using System.Globalization;


namespace Application.Interfaces
{
    public interface IChamadoService
    {
        Task<ChamadoResponseDto> Criar(string solicitanteId,ChamadoRequestDto dto);
        Task<ChamadoResponseDto> Atualizar(int id,ChamadoRequestDto dto,string usuarioLogadoId,bool podevisualizar);
        Task<ChamadoResponseDto?> BuscarPorId(int id ,string usuarioId, bool podeVisualizarTodos);
        Task<IEnumerable<ChamadoResponseDto>> BuscarTodos();
        Task <ChamadoResponseDto> AssumirAtendimento( int chamadoId,string tecnicoId);
        Task<ChamadoResponseDto> AguardarRetorno(int chamadoId, string usuarioLogadoId, bool podevisualizar);
        Task<ChamadoResponseDto> RetomarAtendimento(int chamadoId, string usuarioLogadoId, bool podevisualizar);
        Task<ChamadoResponseDto> EnviarEquipamento(int chamadoId,string usuarioLogadoId);
        Task<ChamadoResponseDto> ReceberEquipamento(int chamadoId, string usuarioLogadoId, bool podeVisualizar);
        Task<ChamadoResponseDto> TransferirChamado(int chamadoId, string destinoId,string usuarioLogadoId,bool podeVisualizar);
        Task<ChamadoResponseDto> AceitarChamado(int chamadoId,int notificacaoId, string usuarioLogadoId, bool podevisualizar);
        Task RecusarChamado(int notificacaoId, string usuarioLogadoId, bool podevisualizar);
        Task<ChamadoResponseDto> FecharChamado(int chamadoId, FecharChamadoDto dto, string usuarioLogadoId, bool podevisualizar);
        Task<ChamadoResponseDto> CancelarChamado(int chamadoId, string usuarioLogadoId, bool podevisualizar);
        Task Remover(int id);
        Task<IEnumerable<ChamadoResponseDto>> BuscarFilaAtendimento();
        Task<IEnumerable<ChamadoResponseDto>> BuscarMeusChamados(string tecnicoId);
        Task<IEnumerable<ChamadoResponseDto>> BuscarMeusChamadosSolicitante(string solicitanteId);
        //Paginação
        Task<PaginacaoResponseDto<ChamadoResponseDto>>BuscarHistoricoPaginado(string usuarioId,int perfilId, DateTime? dataInicial, DateTime? dataFinal, PaginacaoRequestDto request);
        Task<PaginacaoResponseDto<ChamadoResponseDto>>BuscarFilaAtendimentoPaginado(int prioridade, PaginacaoRequestDto request, DateTime? dataInicial, DateTime? dataFinal);
        Task<PaginacaoResponseDto<ChamadoResponseDto>>BuscarMeusChamadosPaginado(bool vizualizarTodos,string tecnicoId,PaginacaoRequestDto request);
        Task<PaginacaoResponseDto<ChamadoResponseDto>>BuscarMeusChamadosSolicitantePaginado(bool visualizarTodos,string solicitanteId, PaginacaoRequestDto request);
        Task<PaginacaoResponseDto<ChamadoResponseDto>> BuscarAnalisePaginada(AnaliseChamadoFiltroViewModel filtro, PaginacaoRequestDto request);
    }
}
