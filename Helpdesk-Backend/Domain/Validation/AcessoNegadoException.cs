using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Validation
{
    public class AcessoNegadoException : Exception
    {
        public AcessoNegadoException(string message)
            : base(message)
        {
        }
    }
}
