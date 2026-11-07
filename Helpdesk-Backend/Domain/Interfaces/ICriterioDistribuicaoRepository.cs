using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ICriterioDistribuicaoRepository
    {

        Task<CriterioDistribuicao> Adicionar(CriterioDistribuicao criterio);
        Task<CriterioDistribuicao> BuscarPorId(int id);
        Task<IEnumerable<CriterioDistribuicao>> BuscarTodos();
        Task Remover(int id);
        Task<IEnumerable<CriterioDistribuicao>> BuscarCriterioPorRegra(int regraId);


    }
}
