namespace SportData.Services.Data.SportDataDb.Interfaces;

using SportData.Data.Entities.OlympicGames;
using SportData.Data.Models.Cache;

public interface ICitiesService
{
    Task<City> AddOrUpdateAsync(City city);

    Task<City> GetAsync(string name);

    ICollection<CityCacheModel> GetCityCacheModels();
}