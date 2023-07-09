namespace SportData.Services.Data.CrawlerStorage.Interfaces;

public interface ICrawlersService
{
    Task<int> GetCrawlerIdAsync(string crawlerName);

    Task AddCrawler(string crawlerName);
}