namespace SportData.Services.Data.OlympicGamesDb.Interfaces;

using SportData.Data.Entities.OlympicGames;

public interface IResultsService
{
    Task<Result> AddOrUpdateAsync(Result result);
}