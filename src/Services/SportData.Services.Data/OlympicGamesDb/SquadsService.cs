namespace SportData.Services.Data.OlympicGamesDb;

using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using SportData.Data.Entities.OlympicGames;
using SportData.Data.Factories.Interfaces;
using SportData.Services.Data.OlympicGamesDb.Interfaces;

public class SquadsService : ISquadsService
{
    private readonly IDbContextFactory dbContextFactory;

    public SquadsService(IDbContextFactory dbContextFactory)
    {
        this.dbContextFactory = dbContextFactory;
    }

    public async Task<Squad> AddOrUpdateAsync(Squad squad)
    {
        using var context = dbContextFactory.CreateOlympicGamesDbContext();

        var dbSquad = await context.Squads.FirstOrDefaultAsync(x => x.ParticipantId == squad.ParticipantId && x.TeamId == squad.TeamId);
        if (dbSquad == null)
        {
            await context.AddAsync(squad);
            await context.SaveChangesAsync();
        }
        else
        {
            squad = dbSquad;
        }

        return squad;
    }
}