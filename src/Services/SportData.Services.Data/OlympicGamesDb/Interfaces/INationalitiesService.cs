namespace SportData.Services.Data.OlympicGamesDb.Interfaces;

using SportData.Data.Entities.OlympicGames;

public interface INationalitiesService
{
    Task<Nationality> AddOrUpdateAsync(Nationality nationality);
}