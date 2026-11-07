using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IperfilRepository
    {
        Task<Perfil> Adicionar(Perfil perfil);
        Task<Perfil?> BuscarPorId(int id);
        Task<IEnumerable<Perfil>> BuscarTodos();
        Task<Perfil> Atualizar(Perfil perfil);
        Task Remover(int id);
    }
}
