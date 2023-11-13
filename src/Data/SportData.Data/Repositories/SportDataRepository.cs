namespace SportData.Data.Repositories;

using System.Linq.Expressions;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using SportData.Data.Common.Interfaces;
using SportData.Data.Contexts;

public class SportDataRepository<TEntity> : IRepository<TEntity>, IDeletableEntityRepository<TEntity>
    where TEntity : class, IDeletableEntity
{
    public SportDataRepository(SportDataDbContext context)
    {
        this.Context = context;
        this.DbSet = this.Context.Set<TEntity>();
    }

    protected SportDataDbContext Context { get; }

    protected DbSet<TEntity> DbSet { get; }

    public async Task AddAsync(TEntity entity) => await this.DbSet.AddAsync(entity).AsTask();

    public IQueryable<TEntity> All() => this.DbSet.Where(x => !x.IsDeleted);

    public IQueryable<TEntity> AllAsNoTracking() => this.DbSet.Where(x => !x.IsDeleted).AsNoTracking();

    public IQueryable<TEntity> AllWithDeleted() => this.DbSet.IgnoreQueryFilters();

    public IQueryable<TEntity> AllAsNoTrackingWithDeleted() => this.DbSet.AsNoTracking().IgnoreQueryFilters();

    public void Delete(TEntity entity)
    {
        entity.IsDeleted = true;
        entity.DeletedOn = DateTime.UtcNow;
        this.Update(entity);
    }

    public async Task<TEntity> GetAsync(params object[] id) => await this.DbSet.FindAsync(id);

    public async Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> expression) => await this.DbSet.Where(expression).FirstOrDefaultAsync();

    public void HardDelete(TEntity entity) => this.DbSet.Remove(entity);

    public async Task<int> SaveChangesAsync() => await this.Context.SaveChangesAsync();

    public void Undelete(TEntity entity)
    {
        entity.IsDeleted = false;
        entity.DeletedOn = null;
        this.Update(entity);
    }

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