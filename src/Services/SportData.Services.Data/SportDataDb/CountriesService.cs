namespace SportData.Services.Data.SportDataDb;

using SportData.Data.Entities.Countries;
using SportData.Data.Repositories;
using SportData.Services.Data.SportDataDb.Interfaces;

public class CountriesService : ICountriesService
{
    private readonly SportDataRepository<Country> repository;

    public CountriesService(SportDataRepository<Country> repository)
    {
        this.repository = repository;
    }

    public async Task<Country> AddOrUpdateAsync(Country country)
    {
        var dbCountry = await repository.GetAsync(x => x.Code == country.Code);
        if (dbCountry == null)
        {
            await repository.AddAsync(country);
            await repository.SaveChangesAsync();
        }
        else
        {
            var isUpdated = dbCountry.IsUpdated(country);
            if (isUpdated)
            {
                repository.Update(dbCountry);
                await repository.SaveChangesAsync();
            }

            country = dbCountry;
        }

        return country;
    }

    public async Task<Country> GetAsync(string code)
    {
        var country = await repository.GetAsync(x => x.Code == code);
        return country;
    }
}