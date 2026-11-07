using Microsoft.AspNetCore.Authorization;

namespace Helpdesk.Authorization
{
    public class PermissaoRequirement : IAuthorizationRequirement
    {
        public string[] Permissoes { get; }

        public PermissaoRequirement(string[] permissoes)
        {
            Permissoes = permissoes;
        }
    }
}

//Ele só carrega a permissão que será validada.