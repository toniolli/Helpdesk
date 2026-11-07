using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
    public class DashboardResumoViewModel
    {
        public int Total { get; set; }
        public int Abertos { get; set; }
        public int Atendimento { get; set; }
        public int Fechados { get; set; }
        public int Cancelados { get; set; }
       
    }
}
