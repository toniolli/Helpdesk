using Microsoft.AspNetCore.Authorization;

namespace Helpdesk.Authorization
{
    public class PermissaoAttribute : AuthorizeAttribute
    {
        public PermissaoAttribute(params string[] permissoes)
        {
            Policy = $"Permissao:{string.Join(",", permissoes)}";
        }
    }
}
//[Authorize(Policy = "Permissao:GERENCIAR_USUARIOS")] transforma em [Permissao("GERENCIAR_USUARIOS")]