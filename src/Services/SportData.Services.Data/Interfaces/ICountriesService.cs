namespace SportData.Services.Data.Interfaces;

public interface ICountriesService
{
    Task<TEntity> AddAsync<TEntity>(TEntity entity);
}