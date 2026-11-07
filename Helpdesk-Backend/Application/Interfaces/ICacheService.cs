using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ICacheService
    {
        Task<T> ObterOuCriar<T>(string chave,Func<Task<T>> buscarNoBanco,TimeSpan expiracao);
        Task Remover(string chave);
    }
}

