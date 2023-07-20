namespace SportData.Data.Repositories;

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using SportData.Data.Common.Interfaces;
using SportData.Data.Contexts;

public class CrawlerStorageRepository<TEntity> : IRepository<TEntity>
    where TEntity : class
{
    public CrawlerStorageRepository(CrawlerStorageDbContext context)
    {
        this.Context = context;
        this.DbSet = this.Context.Set<TEntity>();
    }

    protected CrawlerStorageDbContext Context { get; set; }

    protected DbSet<TEntity> DbSet { get; set; }

    public async Task AddAsync(TEntity entity) => await this.DbSet.AddAsync(entity).AsTask();

    public IQueryable<TEntity> All() => this.DbSet;

    public IQueryable<TEntity> AllAsNoTracking() => this.DbSet.AsNoTracking();

    public void Delete(TEntity entity) => this.DbSet.Remove(entity);

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