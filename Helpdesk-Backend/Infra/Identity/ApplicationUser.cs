using Microsoft.AspNetCore.Identity;

namespace Infra.Identity
{
    // Estende o usuário do Identity com o estado utilizado pela aplicação e
    // com o controle de troca obrigatória de senha.
    public class ApplicationUser : IdentityUser
    {
        public bool Ativo { get; set; }
        public bool DeveTrocarSenha { get; set; }
        public ICollection<SessaoRefresh> SessoesRefresh { get; private set; } = new List<SessaoRefresh>();

        // ESTADO DO USUÁRIO

        public void Ativar()
        {
            Ativo = true;
        }

        public void Desativar()
        {
            Ativo = false;
        }

        // CONTROLE DE SENHA

        public void ExigirTrocaSenha()
        {
            DeveTrocarSenha = true;
        }

        public void ConcluirTrocaSenha()
        {
            DeveTrocarSenha = false;
        }

        // DADOS DO USUÁRIO

        public void Atualizar(
            string username,
            string email,
            bool ativo)
        {
            UserName = username;
            Email = email;
            Ativo = ativo;
        }
    }
}
