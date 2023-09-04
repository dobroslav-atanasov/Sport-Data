namespace SportData.Services.Data.SportDataDb;

using System.Collections.Generic;
using System.Threading.Tasks;

using SportData.Data.Entities.OlympicGames;
using SportData.Data.Models.Cache;
using SportData.Data.Repositories;
using SportData.Services.Data.SportDataDb.Interfaces;
using SportData.Services.Mapper.Extensions;

public class CitiesService : ICitiesService
{
    private readonly SportDataRepository<City> repository;

    public CitiesService(SportDataRepository<City> repository)
    {
        this.repository = repository;
    }

    public async Task<City> AddOrUpdateAsync(City city)
    {
        var dbCity = await repository.GetAsync(x => x.Name == city.Name);
        if (dbCity == null)
        {
            await repository.AddAsync(city);
            await repository.SaveChangesAsync();
        }
        else
        {
            var isUpdated = dbCity.IsUpdated(city);
            if (isUpdated)
            {
                repository.Update(dbCity);
                await repository.SaveChangesAsync();
            }

            city = dbCity;
        }

        return city;
    }

    public async Task<City> GetAsync(string name)
    {
        var city = await repository.GetAsync(x => x.Name == name);
        return city;
    }

    public ICollection<CityCacheModel> GetCityCacheModels()
    {
        var models = this.repository
            .AllAsNoTracking()
            .To<CityCacheModel>()
            .ToList();

        return models;
    }
}