namespace SportData.Services.Data.SportDataDb;

using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using SportData.Data.Entities.OlympicGames;
using SportData.Data.Factories.Interfaces;
using SportData.Data.Repositories;
using SportData.Services.Data.SportDataDb.Interfaces;

public class SquadsService : ISquadsService
{
    //private readonly SportDataRepository<Squad> repository;
    private readonly IDbContextFactory dbContextFactory;

    public SquadsService(SportDataRepository<Squad> repository, IDbContextFactory dbContextFactory)
    {
        //this.repository = repository;
        this.dbContextFactory = dbContextFactory;
    }

    public async Task<Squad> AddOrUpdateAsync(Squad squad)
    {
        using var context = this.dbContextFactory.CreateSportDataDbContext();

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

        //var dbSquad = await repository.GetAsync(x => x.ParticipantId == squad.ParticipantId && x.TeamId == squad.TeamId);
        //if (dbSquad == null)
        //{
        //    await repository.AddAsync(squad);
        //    await repository.SaveChangesAsync();
        //}
        //else
        //{
        //    squad = dbSquad;
        //}

        //return squad;
    }
}