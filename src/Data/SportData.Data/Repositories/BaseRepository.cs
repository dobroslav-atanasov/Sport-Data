namespace SportData.Data.Repositories;

using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using SportData.Data.Common.Repositories;
using SportData.Data.Contexts;

public abstract class BaseRepository<TEntity> : IRepository<TEntity>
    where TEntity : class
{
    public BaseRepository(CrawlerStorageDbContext context)
    {
        this.Context = context;
        this.DbSet = this.Context.Set<TEntity>();
    }

    protected CrawlerStorageDbContext Context { get; set; }

    protected DbSet<TEntity> DbSet { get; set; }

    public Task AddAsync(TEntity entity)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(TEntity entity)
    {
        throw new NotImplementedException();
    }

    public Task<IQueryable<TEntity>> FindAllAsync()
    {
        throw new NotImplementedException();
    }

    public async Task SaveChangesAsync()
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(TEntity entity)
    {
        throw new NotImplementedException();
    }
}