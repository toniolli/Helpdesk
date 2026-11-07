using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Validation
{
    public class ConcurrencyException : Exception
    {
        public ConcurrencyException(string mensagem)
            : base(mensagem)
        {
        }

        public ConcurrencyException(
            string mensagem,
            Exception innerException)
            : base(mensagem, innerException)
        {
        }
    }
}
