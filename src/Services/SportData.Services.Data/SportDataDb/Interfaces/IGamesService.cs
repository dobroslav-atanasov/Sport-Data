namespace SportData.Services.Data.SportDataDb.Interfaces;

using SportData.Data.Entities.OlympicGames;
using SportData.Data.Models.Cache;

public interface IGamesService
{
    Task<Game> AddOrUpdateAsync(Game game);

    ICollection<GameCacheModel> GetGameCacheModels();
}