namespace SportData.Data.Seeders;

using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SportData.Data.Seeders.Interfaces;

public class SportDataDbSeeder : ISeeder
{
    public async Task SeedAsync(IServiceProvider services)
    {
        var logger = services.GetService<ILogger<SportDataDbSeeder>>();

        try
        {
            var seeders = new List<ISeeder>
            {
                new RolesSeeder()
            };

            foreach (var seeder in seeders)
            {
                await seeder.SeedAsync(services);
                logger.LogInformation($"Seeder {seeder.GetType().Name} done.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message, ex);
        }
    }
}