namespace SportData.Data.Factories;

using Microsoft.EntityFrameworkCore;

using SportData.Data.Contexts;
using SportData.Data.Factories.Interfaces;

public class DbContextFactory : IDbContextFactory
{
    private readonly DbContextOptions<CrawlerStorageDbContext> crawlerStorageDbOptions;
    private readonly DbContextOptions<SportDataDbContext> sportStatsDbOptions;

    public DbContextFactory(DbContextOptions<CrawlerStorageDbContext> crawlerStorageDbOptions, DbContextOptions<SportDataDbContext> sportStatsDbOptions)
    {
        this.crawlerStorageDbOptions = crawlerStorageDbOptions;
        this.sportStatsDbOptions = sportStatsDbOptions;
    }

    public CrawlerStorageDbContext CreateCrawlerStorageDbContext()
    {
        return new CrawlerStorageDbContext(crawlerStorageDbOptions);
    }

    public SportDataDbContext CreateSportDataDbContext()
    {
        return new SportDataDbContext(sportStatsDbOptions);
    }
}