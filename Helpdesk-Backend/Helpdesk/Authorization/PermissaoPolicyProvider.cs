using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Helpdesk.Authorization
{
    public class PermissaoPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        public PermissaoPolicyProvider(IOptions<AuthorizationOptions> options): base(options)
        {
        }

        public override async Task<AuthorizationPolicy?>GetPolicyAsync(string policyName)
        {
            if (policyName.StartsWith("Permissao:"))
            {
                var permissoes =
                    policyName
                        .Replace("Permissao:", "")
                        .Split(',');

                return new AuthorizationPolicyBuilder()
                    .AddRequirements(
                        new PermissaoRequirement(permissoes))
                    .Build();
            }

            return await base.GetPolicyAsync(policyName);
        }
    }
}

/// <summary>
/// Provider responsável por criar políticas de autorização
/// dinamicamente a partir do atributo Permissao.
/// Evita a necessidade de registrar manualmente uma policy
/// para cada permissão no Program.cs.
/// </summary>