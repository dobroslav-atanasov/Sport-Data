namespace SportData.Services.Data.OlympicGamesDb;

using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using SportData.Data.Entities.OlympicGames;
using SportData.Data.Factories.Interfaces;
using SportData.Services.Data.OlympicGamesDb.Interfaces;

public class TeamsService : ITeamsService
{
    private readonly IDbContextFactory dbContextFactory;

    public TeamsService(IDbContextFactory dbContextFactory)
    {
        this.dbContextFactory = dbContextFactory;
    }

    public async Task<Team> AddOrUpdateAsync(Team team)
    {
        using var context = dbContextFactory.CreateOlympicGamesDbContext();

        var dbTeam = await context.Teams.FirstOrDefaultAsync(x => x.Name == team.Name && x.EventId == team.EventId && x.NOCId == team.NOCId);
        if (dbTeam == null)
        {
            await context.AddAsync(team);
            await context.SaveChangesAsync();
        }
        else
        {
            var isUpdated = dbTeam.IsUpdated(team);
            if (isUpdated)
            {
                context.Update(dbTeam);
                await context.SaveChangesAsync();
            }

            team = dbTeam;
        }

        return team;
    }

    public async Task<Team> GetAsync(string name, int nocId, int eventId)
    {
        using var context = dbContextFactory.CreateOlympicGamesDbContext();
        var team = await context.Teams.FirstOrDefaultAsync(x => x.Name == name && x.NOCId == nocId && x.EventId == eventId);

        return team;
    }

    public async Task<Team> GetAsync(int nocId, int eventId)
    {
        using var context = dbContextFactory.CreateOlympicGamesDbContext();
        var team = await context.Teams.FirstOrDefaultAsync(x => x.NOCId == nocId && x.EventId == eventId);

        return team;
    }
}