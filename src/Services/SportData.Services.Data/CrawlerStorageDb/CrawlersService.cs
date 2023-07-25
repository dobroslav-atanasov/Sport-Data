namespace SportData.Services.Data.CrawlerStorageDb;

using System.Threading.Tasks;

using SportData.Data.Common.Interfaces;
using SportData.Data.Entities.Crawlers;
using SportData.Data.Repositories;
using SportData.Services.Data.CrawlerStorageDb.Interfaces;

public class CrawlersService : ICrawlersService
{
    private readonly IRepository<Crawler> repository;

    public CrawlersService(CrawlerStorageRepository<Crawler> repository)
    {
        this.repository = repository;
    }

    public async Task AddCrawler(string crawlerName)
    {
        var crawler = new Crawler { Name = crawlerName };
        await this.repository.AddAsync(crawler);
        await this.repository.SaveChangesAsync();
    }

    public async Task<int> GetCrawlerIdAsync(string crawlerName)
    {
        var crawler = await this.repository.GetAsync(x => x.Name == crawlerName);

        if (crawler == null)
        {
            await this.AddCrawler(crawlerName);
            return await this.GetCrawlerIdAsync(crawlerName);
        }

        return crawler.Id;
    }
}