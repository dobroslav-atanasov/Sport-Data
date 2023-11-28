namespace SportData.Services.Data.OlympicGamesDb;

using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using SportData.Data.Entities.OlympicGames;
using SportData.Data.Factories.Interfaces;
using SportData.Services.Data.OlympicGamesDb.Interfaces;

public class SportsService : ISportsService
{
    private readonly IDbContextFactory dbContextFactory;

    public SportsService(IDbContextFactory dbContextFactory)
    {
        this.dbContextFactory = dbContextFactory;
    }

    public async Task<Sport> AddOrUpdateAsync(Sport sport)
    {
        using var context = dbContextFactory.CreateOlympicGamesDbContext();

        var dbSport = await context.Sports.FirstOrDefaultAsync(x => x.Name == sport.Name);
        if (dbSport == null)
        {
            await context.AddAsync(sport);
            await context.SaveChangesAsync();
        }
        else
        {
            var isUpdated = dbSport.IsUpdated(sport);
            if (isUpdated)
            {
                context.Update(dbSport);
                await context.SaveChangesAsync();
            }

            sport = dbSport;
        }

        return sport;
    }
}