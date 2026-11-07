using Npgsql.Replication.PgOutput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infra.Services
{
    public class EmailService
    {
        public string Servidor { get; set; } = string.Empty;
        public int Porta { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public string Remetente { get; set; } = string.Empty;   

    }
}
