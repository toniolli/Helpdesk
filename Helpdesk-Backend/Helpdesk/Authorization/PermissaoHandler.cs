using Microsoft.AspNetCore.Authorization;

namespace Helpdesk.Authorization
{
    public class PermissaoHandler : AuthorizationHandler<PermissaoRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,PermissaoRequirement requirement)
        {
            if (requirement.Permissoes.Any(
                permissao =>
                    context.User.HasClaim(
                        "Permissao",
                        permissao)))
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}

/// <summary>
/// Handler responsável por validar se o usuário possui
/// a permissão exigida pelo endpoint através das claims
/// presentes no token JWT.
/// </summary>