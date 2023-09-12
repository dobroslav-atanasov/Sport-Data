namespace SportData.Services.Data.SportDataDb.Interfaces;

using SportData.Data.Entities.Countries;

public interface ICountriesService
{
    Task<Country> AddOrUpdateAsync(Country country);

    Task<Country> GetAsync(string code);
}