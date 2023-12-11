namespace SportData.Services.Data.OlympicGamesDb;

using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using SportData.Data.Entities.OlympicGames;
using SportData.Data.Factories.Interfaces;
using SportData.Services.Data.OlympicGamesDb.Interfaces;

public class ResultsService : IResultsService
{
    private readonly IDbContextFactory dbContextFactory;

    public ResultsService(IDbContextFactory dbContextFactory)
    {
        this.dbContextFactory = dbContextFactory;
    }

    public async Task<Result> AddOrUpdateAsync(Result result)
    {
        using var context = dbContextFactory.CreateOlympicGamesDbContext();

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