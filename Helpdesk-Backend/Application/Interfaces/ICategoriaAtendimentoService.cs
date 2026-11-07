using Application.Dto.CategoriaAtendimentoDto;


namespace Application.Interfaces
{
    public interface ICategoriaAtendimentoService
    {
        Task<CategoriaAtendimentoResponseDto> Criar(CategoriaAtendimentoRequestDto dto);
        Task<CategoriaAtendimentoResponseDto> Atualizar(int id,CategoriaAtendimentoRequestDto dto);
        Task<CategoriaAtendimentoResponseDto?> BuscarPorId(int id);
        Task<IEnumerable<CategoriaAtendimentoResponseDto>> BuscarTodos();
        Task Ativar(int id);
        Task Desativar(int id);
        Task Remover(int id);
    }
}