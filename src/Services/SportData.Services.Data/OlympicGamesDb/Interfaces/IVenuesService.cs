namespace SportData.Services.Data.OlympicGamesDb.Interfaces;

using SportData.Data.Entities.OlympicGames;
using SportData.Data.Models.Cache;

public interface IVenuesService
{
    Task<Venue> AddOrUpdateAsync(Venue venue);

    ICollection<VenueCacheModel> GetVenueCacheModels();
}