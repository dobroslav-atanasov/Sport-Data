namespace SportData.Services.Data.SportDataDb.Interfaces;

using SportData.Data.Entities.OlympicGames;

public interface IResultsService
{
    Task<Result> AddOrUpdateAsync(Result result);
}