using Application.Dto.Comentario;
using Application.Dto.Paginacao;

namespace Application.Interfaces
{
    public interface IComentarioService
    {
        Task<ComentarioResponseDto> Criar(ComentarioRequestDto dto, string usuarioLogadoId, bool podeVisualizar, bool agendarAtualizacaoTempoReal = true);
        Task<ComentarioResponseDto> Atualizar(int id,ComentarioRequestDto dto, string usuarioLogadoId, bool podeVisualizar);
        Task<ComentarioResponseDto?> BuscarPorId(int id, string usuarioLogadoId, bool podeVisualizar);
        Task<IEnumerable<ComentarioResponseDto>> BuscarPorChamado(int chamadoId, string usuarioLogadoId, bool podeVisualizar);
        Task Remover(int id, string usuarioLogadoId, bool podeVisualizar);
        //Paginação
        Task<PaginacaoResponseDto<ComentarioResponseDto>>BuscarPorChamadoPaginado(int chamadoId,PaginacaoRequestDto request, string usuarioLogadoId, bool podeVisualizar);
       

    }
}
