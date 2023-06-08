namespace SportData.Data.Common.Repositories;

public interface IRepository<TEntity>
    where TEntity : class
{
    Task<IQueryable<TEntity>> FindAllAsync();

    Task AddAsync(TEntity entity);

    Task UpdateAsync(TEntity entity);

    Task DeleteAsync(TEntity entity);

    Task SaveChangesAsync();
}