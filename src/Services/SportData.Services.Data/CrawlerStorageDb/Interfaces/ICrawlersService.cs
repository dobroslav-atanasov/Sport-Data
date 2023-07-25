namespace SportData.Services.Data.CrawlerStorageDb.Interfaces;

public interface ICrawlersService
{
    Task<int> GetCrawlerIdAsync(string crawlerName);

    Task AddCrawler(string crawlerName);
}