namespace SportData.Services.Data.OlympicGamesDb;

using Microsoft.EntityFrameworkCore;

using SportData.Data.Entities.OlympicGames;
using SportData.Data.Factories.Interfaces;
using SportData.Services.Data.OlympicGamesDb.Interfaces;

public class NationalitiesService : INationalitiesService
{
    private readonly IDbContextFactory dbContextFactory;

    public NationalitiesService(IDbContextFactory dbContextFactory)
    {
        this.dbContextFactory = dbContextFactory;
    }

    public async Task<Nationality> AddOrUpdateAsync(Nationality nationality)
    {
        using var context = dbContextFactory.CreateOlympicGamesDbContext();

        var dbNationality = await context.Nationalities.FirstOrDefaultAsync(x => x.AthleteId == nationality.AthleteId && x.NOCId == nationality.NOCId);
        if (dbNationality == null)
        {
            await context.AddAsync(nationality);
            await context.SaveChangesAsync();
        }
        else
        {
            nationality = dbNationality;
        }

        return nationality;
    }
}