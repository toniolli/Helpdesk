using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dto.UsuarioDto
{
    public class LoginDto
    {
       
        public string UserName { get; set; } = string.Empty;
       
        public string Password { get; set; } = string.Empty;

    }
}
