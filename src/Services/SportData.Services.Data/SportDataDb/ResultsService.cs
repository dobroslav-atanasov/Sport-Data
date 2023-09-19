namespace SportData.Services.Data.SportDataDb;

using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using SportData.Data.Entities.OlympicGames;
using SportData.Data.Factories.Interfaces;
using SportData.Data.Repositories;
using SportData.Services.Data.SportDataDb.Interfaces;

public class ResultsService : IResultsService
{
    private readonly SportDataRepository<Result> repository;
    private readonly IDbContextFactory dbContextFactory;

    public ResultsService(SportDataRepository<Result> repository, IDbContextFactory dbContextFactory)
    {
        this.repository = repository;
        this.dbContextFactory = dbContextFactory;
    }

    public async Task<Result> AddOrUpdateAsync(Result result)
    {
        using var context = this.dbContextFactory.CreateSportDataDbContext();
        var dbResult = await context.Results.FirstOrDefaultAsync(x => x.EventId == result.EventId);
        if (dbResult == null)
        {
            await context.AddAsync(result);
            await context.SaveChangesAsync();
        }
        else
        {
            var isUpdated = dbResult.IsUpdated(result);
            if (isUpdated)
            {
                context.Update(dbResult);
                await context.SaveChangesAsync();
            }

            result = dbResult;
        }

        return result;
    }
}