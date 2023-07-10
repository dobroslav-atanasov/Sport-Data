namespace SportData.Services.Data;

using System.Threading.Tasks;

using SportData.Data.Contexts;
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
}