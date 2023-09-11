namespace SportData.Services.Data.SportDataDb;

using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using SportData.Data.Entities.OlympicGames;
using SportData.Data.Factories.Interfaces;
using SportData.Data.Repositories;
using SportData.Services.Data.SportDataDb.Interfaces;

public class TeamsService : ITeamsService
{
    //private readonly SportDataRepository<Team> repository;
    private readonly IDbContextFactory dbContextFactory;

    public TeamsService(SportDataRepository<Team> repository, IDbContextFactory dbContextFactory)
    {
        //this.repository = repository;
        this.dbContextFactory = dbContextFactory;
    }

    public async Task<Team> AddOrUpdateAsync(Team team)
    {
        using var context = this.dbContextFactory.CreateSportDataDbContext();
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

        //var dbTeam = await repository.GetAsync(x => x.Name == team.Name && x.EventId == team.EventId && x.NOCId == team.NOCId);
        //if (dbTeam == null)
        //{
        //    await repository.AddAsync(team);
        //    await repository.SaveChangesAsync();
        //}
        //else
        //{
        //    var isUpdated = dbTeam.Equals(team);
        //    if (isUpdated)
        //    {
        //        repository.Update(dbTeam);
        //        await repository.SaveChangesAsync();
        //    }
        //
        //     team = dbTeam;
        //}

        //return team;
    }

    public async Task<Team> GetAsync(string name, int nocId, int eventId)
    {
        using var context = this.dbContextFactory.CreateSportDataDbContext();
        var team = await context.Teams.FirstOrDefaultAsync(x => x.Name == name && x.NOCId == nocId && x.EventId == eventId);

        return team;
    }

    public async Task<Team> GetAsync(int nocId, int eventId)
    {
        using var context = this.dbContextFactory.CreateSportDataDbContext();
        var team = await context.Teams.FirstOrDefaultAsync(x => x.NOCId == nocId && x.EventId == eventId);

        return team;
    }
}