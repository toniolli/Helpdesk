using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ISlaRepository
    {
        Task<SLA> Adicionar(SLA sla);
        Task<SLA?> BuscarPorId(int id);
        Task<SLA?> BuscarPrioridade(PrioridadeEnum prioridade);
        Task<IEnumerable<SLA>> BuscarTodos();
        Task<SLA> Atualizar(SLA sla);
        Task Remover(int id);
    }
}
