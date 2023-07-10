namespace SportData.Services.Data;

using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using SportData.Data.Contexts;
using SportData.Data.Entities.Countries;
using SportData.Services.Data.Interfaces;

public class CountriesService : BaseService, ICountriesService
{
    public CountriesService(SportDataDbContext context)
        : base(context)
    {
    }

    public async Task<TEntity> AddAsync<TEntity>(TEntity entity)
    {
        return await this.BaseAddAsync(entity);
    }

    public async Task<Country> GetCountryAsync(string code)
    {
        var country = await this.Context
            .Countries
            .FirstOrDefaultAsync(c => c.Code == code);

        return country;
    }
}