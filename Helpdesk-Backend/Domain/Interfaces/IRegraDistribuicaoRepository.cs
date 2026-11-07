using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IRegraDistribuicaoRepository
    {
        Task<RegraDistribuicao> Adicionar(RegraDistribuicao regra);
        Task<RegraDistribuicao> BuscarPorId(int id);
        Task<IEnumerable<RegraDistribuicao>> BuscarTodos();
        Task<RegraDistribuicao> Atualizar(RegraDistribuicao regra);
        Task Remover(int id);
        
        
    }
}
