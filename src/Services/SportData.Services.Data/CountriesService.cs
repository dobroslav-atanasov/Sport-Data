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
        using var context = this.Context;

        await context.AddAsync(entity);
        await context.SaveChangesAsync();

        return entity;
    }
}