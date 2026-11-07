using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dto.UsuarioDto
{
    public class ChangePasswordDto
    {
        public string? PasswordAtual { get; set; } 
        public string? NovaSenha { get; set; }
        public string? ConfirmacaoNovaSenha { get; set; }
    }
}
