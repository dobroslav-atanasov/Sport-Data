namespace SportData.Services.Data.Interfaces;

public interface IAddable
{
    Task<TEntity> AddAsync<TEntity>(TEntity entity);
}