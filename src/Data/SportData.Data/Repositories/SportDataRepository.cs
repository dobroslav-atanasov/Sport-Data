namespace SportData.Data.Repositories;

using System.Linq.Expressions;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using SportData.Data.Common.Interfaces;
using SportData.Data.Contexts;

public class SportDataRepository<TEntity> : IRepository<TEntity>
    where TEntity : class
{
    public SportDataRepository(SportDataDbContext context)
    {
        this.Context = context;
        this.DbSet = this.Context.Set<TEntity>();
    }

    protected SportDataDbContext Context { get; }

    protected DbSet<TEntity> DbSet { get; }

    public async Task AddAsync(TEntity entity) => await this.DbSet.AddAsync(entity).AsTask();

    public IQueryable<TEntity> All() => this.DbSet;

    public IQueryable<TEntity> AllAsNoTracking() => this.DbSet.AsNoTracking();

    public async Task<TEntity> GetAsync(params object[] id) => await this.DbSet.FindAsync(id);

    public async Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> expression) => await this.DbSet.Where(expression).FirstOrDefaultAsync();

    public async Task<int> SaveChangesAsync() => await this.Context.SaveChangesAsync();

    public void Update(TEntity entity)
    {
        var entry = this.Context.Entry(entity);
        if (entry.State == EntityState.Detached)
        {
            this.DbSet.Attach(entity);
        }

        entry.State = EntityState.Modified;
    }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.Context?.Dispose();
        }
    }
}