using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public  interface IpermissaoRepository
    {
        Task<Permissao> Adicionar(Permissao permissao);
        Task<Permissao?> BuscarPorId(int id);
 
        Task<IEnumerable<Permissao>> BuscarTodos();
        Task<Permissao> Atualizar(Permissao permissao);
        Task Remover(int id);


    }
}
