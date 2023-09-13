namespace SportData.Services.Data.SportDataDb.Interfaces;

using SportData.Data.Entities.OlympicGames;

public interface INationalitiesService
{
    Task<Nationality> AddOrUpdateAsync(Nationality nationality);
}