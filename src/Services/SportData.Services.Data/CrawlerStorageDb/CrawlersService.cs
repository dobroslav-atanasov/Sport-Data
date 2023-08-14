namespace SportData.Services.Data.CrawlerStorageDb;

using System.Threading.Tasks;

using SportData.Data.Entities.Crawlers;
using SportData.Data.Factories.Interfaces;
using SportData.Data.Repositories;
using SportData.Services.Data.CrawlerStorageDb.Interfaces;

public class CrawlersService : ICrawlersService
{
    //private readonly IRepository<Crawler> repository;
    private readonly IDbContextFactory dbContextFactory;

    public CrawlersService(CrawlerStorageRepository<Crawler> repository, IDbContextFactory dbContextFactory)
    {
        //this.repository = repository;
        this.dbContextFactory = dbContextFactory;
    }

    public async Task AddCrawler(string crawlerName)
    {
        using var context = this.dbContextFactory.CreateCrawlerStorageDbContext();
        var crawler = new Crawler { Name = crawlerName };
        //await this.repository.AddAsync(crawler);
        //await this.repository.SaveChangesAsync();

        await context.AddAsync(crawler);
        await context.SaveChangesAsync();
    }

    public async Task<int> GetCrawlerIdAsync(string crawlerName)
    {
        using var context = this.dbContextFactory.CreateCrawlerStorageDbContext();
        var crawler = context.Crawlers.FirstOrDefault(x => x.Name == crawlerName);
        //var crawler = await this.repository.GetAsync(x => x.Name == crawlerName);

        if (crawler == null)
        {
            await this.AddCrawler(crawlerName);
            return await this.GetCrawlerIdAsync(crawlerName);
        }

        return crawler.Id;
    }
}