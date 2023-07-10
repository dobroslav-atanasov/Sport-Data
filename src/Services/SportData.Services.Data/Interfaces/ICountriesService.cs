namespace SportData.Services.Data.Interfaces;

using SportData.Data.Entities.Countries;

public interface ICountriesService : IAddable
{
    Task<Country> GetCountryAsync(string code);
}