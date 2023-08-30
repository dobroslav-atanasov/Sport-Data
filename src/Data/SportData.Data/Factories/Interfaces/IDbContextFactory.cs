namespace SportData.Data.Factories.Interfaces;

using SportData.Data.Contexts;

public interface IDbContextFactory
{
    CrawlerStorageDbContext CreateCrawlerStorageDbContext();

    SportDataDbContext CreateSportDataDbContext();
}