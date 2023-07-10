namespace SportData.Services.Data;

using SportData.Data.Contexts;

public abstract class BaseService
{
    public BaseService(SportDataDbContext context)
    {
        this.Context = context;
    }

    protected SportDataDbContext Context { get; }

    protected virtual async Task<TEntity> BaseAddAsync<TEntity>(TEntity entity)
    {
        await this.Context.AddAsync(entity);
        await this.Context.SaveChangesAsync();

        return entity;
    }
}