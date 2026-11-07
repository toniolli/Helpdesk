using Application.Dto.UsuarioDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IPerfilService
    {
        Task<PerfilDto> Adicionar(PerfilDto perfilDto);
        Task<PerfilDto?> BuscarPorId(int id);
        Task<IEnumerable<PerfilDto>> BuscarTodos();
        Task<PerfilDto> Atualizar(int id ,PerfilDto perfilDto);
        Task Remover(int id);

        Task Ativar(int id);
        Task Desativar(int id);
    }
}
