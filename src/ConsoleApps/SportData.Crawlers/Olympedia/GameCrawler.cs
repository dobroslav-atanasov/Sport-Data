namespace SportData.Crawlers.Olympedia;

using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using SportData.Common.Constants;
using SportData.Services.Data.CrawlerStorageDb.Interfaces;
using SportData.Services.Interfaces;

public class GameCrawler : BaseOlympediaCrawler
{
    public GameCrawler(ILogger<BaseCrawler> logger, IConfiguration configuration, IHttpService httpService, ICrawlersService crawlersService, IGroupsService groupsService)
        : base(logger, configuration, httpService, crawlersService, groupsService)
    {
    }

    public override async Task StartAsync()
    {
        this.Logger.LogInformation($"{this.GetType().FullName} Start!");

        try
        {
            var httpModel = await this.HttpService.GetAsync(this.Configuration.GetSection(CrawlerConstants.OLYMPEDIA_GAMES_URL).Value);
            var urls = this.ExtractGameUrls(httpModel);

            foreach (var url in urls)
            {
                try
                {
                    var gameHttpModel = await this.HttpService.GetAsync(url);
                    await this.ProcessGroupAsync(gameHttpModel);
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, $"Failed to process data: {url};");
                }
            }
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, $"Failed to process url: {this.Configuration.GetSection(CrawlerConstants.OLYMPEDIA_GAMES_URL).Value};");
        }

        this.Logger.LogInformation($"{this.GetType().FullName} End!");
    }
}