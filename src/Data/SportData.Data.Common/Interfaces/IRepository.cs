namespace SportData.Data.Common.Interfaces;

using System.Linq.Expressions;

public interface IRepository<TEntity> : IDisposable
    where TEntity : class
{
    Task AddAsync(TEntity entity);

    Task<TEntity> GetAsync(params object[] id);

    Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> expression);

    Task<int> SaveChangesAsync();
}