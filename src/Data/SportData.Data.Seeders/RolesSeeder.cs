namespace SportData.Data.Seeders;

using System;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

using SportData.Common.Constants;
using SportData.Data.Contexts;
using SportData.Data.Entities;
using SportData.Data.Seeders.Interfaces;

public class RolesSeeder : ISeeder
{
    public async Task SeedAsync(IServiceProvider services)
    {
        var dbContext = services.GetService<SportDataDbContext>();
        var roleManager = services.GetService<RoleManager<ApplicationRole>>();

        await SeedRoleAsync(roleManager, ApplicationRoles.SUPERADMIN);
        await SeedRoleAsync(roleManager, ApplicationRoles.ADMIN);
        await SeedRoleAsync(roleManager, ApplicationRoles.EDITOR);
        await SeedRoleAsync(roleManager, ApplicationRoles.USER);
    }

    private async Task SeedRoleAsync(RoleManager<ApplicationRole> roleManager, string roleName)
    {
        var role = await roleManager.FindByNameAsync(roleName);
        if (role == null)
        {
            var result = await roleManager.CreateAsync(new ApplicationRole { Name = roleName, CreatedOn = DateTime.UtcNow });
            if (!result.Succeeded)
            {
                throw new Exception(string.Join(Environment.NewLine, result.Errors.Select(e => e.Description)));
            }
        }
    }
}