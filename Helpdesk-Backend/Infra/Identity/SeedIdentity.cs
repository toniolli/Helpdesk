using Domain.Entities;
using Infra.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infra.Identity
{
    // Inicializa a função e o usuário administrador, garantindo também seu
    // perfil e todas as permissões ativas da aplicação.
    public static class SeedIdentity
    {
        public const string AdminRole = "Admin";

        // FUNÇÃO ADMINISTRATIVA

        public static async Task SeedRolesAsync(
            RoleManager<IdentityRole> roleManager)
        {
            string[] roles =
            {
                AdminRole
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(
                        new IdentityRole(role));
                }
            }
        }

        // USUÁRIO ADMINISTRADOR

        // O seed é ignorado quando alguma configuração obrigatória não foi
        // informada, preservando o comportamento atual da inicialização.
        public static async Task SeedAdminAsync(
            UserManager<ApplicationUser> userManager,
            Microsoft.Extensions.Configuration.IConfiguration configuration,
            AppDbContext appDbContext)
        {
            var seed = configuration.GetSection("SeedIdentity");
            var adminUserName = seed["AdminUserName"];
            var adminEmail = seed["AdminEmail"];
            var adminPassword = seed["AdminPassword"];

            if (string.IsNullOrWhiteSpace(adminUserName) ||
                string.IsNullOrWhiteSpace(adminEmail) ||
                string.IsNullOrWhiteSpace(adminPassword))
            {
                return;
            }

            var admin =
                await userManager.FindByNameAsync(
                    adminUserName);

            admin ??=
                await userManager.FindByEmailAsync(
                    adminEmail);

            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminUserName,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    Ativo = true
                };

                var result = await userManager.CreateAsync(
                    admin,
                    adminPassword);

                if (!result.Succeeded)
                {
                    return;
                }
            }

            if (!await userManager.IsInRoleAsync(admin, AdminRole))
            {
                await userManager.AddToRoleAsync(
                    admin,
                    AdminRole);
            }

            await VincularPerfilEPermissoesAdministradorAsync(
                appDbContext,
                admin.Id);
        }

        // PERFIL E PERMISSÕES

        // Garante o perfil administrativo, vincula o usuário e adiciona
        // somente as permissões ativas que ainda não estão associadas.
        private static async Task VincularPerfilEPermissoesAdministradorAsync(
            AppDbContext appDbContext,
            string adminId)
        {
            const string nomePerfil = "Administrador";

            var perfilAdministrador = await appDbContext.Perfis
                .FirstOrDefaultAsync(perfil =>
                    perfil.Nome.ToLower() == nomePerfil.ToLower());

            if (perfilAdministrador == null)
            {
                perfilAdministrador = new Perfil(
                    nomePerfil,
                    "Perfil com acesso administrativo completo.");

                await appDbContext.Perfis.AddAsync(
                    perfilAdministrador);

                await appDbContext.SaveChangesAsync();
            }
            else if (!perfilAdministrador.Ativo)
            {
                perfilAdministrador.Ativar();
            }

            var usuarioPossuiPerfil =
                await appDbContext.UsuariosPerfis.AnyAsync(
                    usuarioPerfil =>
                        usuarioPerfil.UsuarioId == adminId &&
                        usuarioPerfil.PerfilId ==
                            perfilAdministrador.Id_Perfil);

            if (!usuarioPossuiPerfil)
            {
                await appDbContext.UsuariosPerfis.AddAsync(
                    new UsuarioPerfil(
                        adminId,
                        perfilAdministrador.Id_Perfil));
            }

            var permissoesAtivas = await appDbContext.Permissoes
                .Where(permissao =>
                    permissao.Ativo)
                .Select(permissao =>
                    permissao.Id_Permissao)
                .ToListAsync();

            var permissoesJaVinculadas =
                await appDbContext.PerfilPermissoes
                    .Where(perfilPermissao =>
                        perfilPermissao.PerfilId ==
                            perfilAdministrador.Id_Perfil)
                    .Select(perfilPermissao =>
                        perfilPermissao.PermissaoId)
                    .ToListAsync();

            var novosVinculos = permissoesAtivas
                .Except(permissoesJaVinculadas)
                .Select(permissaoId =>
                    new PerfilPermissao(
                        perfilAdministrador.Id_Perfil,
                        permissaoId));

            await appDbContext.PerfilPermissoes.AddRangeAsync(
                novosVinculos);

            await appDbContext.SaveChangesAsync();
        }
    }
}
