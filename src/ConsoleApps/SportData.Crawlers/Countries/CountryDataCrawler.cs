namespace SportData.Crawlers.Countries;

using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using SportData.Common.Constants;
using SportData.Data.Models.Http;
using SportData.Services.Data.CrawlerStorageDb.Interfaces;
using SportData.Services.Interfaces;

public class CountryDataCrawler : BaseCrawler
{
    public CountryDataCrawler(ILogger<BaseCrawler> logger, IConfiguration configuration, IHttpService httpService, ICrawlersService crawlersService, IGroupsService groupsService)
        : base(logger, configuration, httpService, crawlersService, groupsService)
    {
    }

    public override async Task StartAsync()
    {
        this.Logger.LogInformation($"{this.GetType().FullName} Start!");

        try
        {
            var httpModel = await this.HttpService.GetAsync(this.Configuration.GetSection(CrawlerConstants.WORLD_COUNTRIES_URL).Value);
            var countryUrls = this.ExtractCountryUrls(httpModel);

            foreach (var url in countryUrls)
            {
                try
                {
                    var countryHttpModel = await this.HttpService.GetAsync(url);
                    await this.ProcessGroupAsync(countryHttpModel);
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, $"Failed to process url: {url}");
                }
            }
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, $"Failed to process url: {CrawlerConstants.WORLD_COUNTRIES_URL}");
        }

        this.Logger.LogInformation($"{this.GetType().FullName} End!");
    }

    private IReadOnlyCollection<string> ExtractCountryUrls(HttpModel httpModel)
    {
        var urls = httpModel
            .HtmlDocument
            .DocumentNode
            .SelectNodes("//ul[@class='flag-grid']/li/a")
            .Select(x => x.Attributes["href"]?.Value.Trim())
            .Select(x => this.CreateUrl(x, this.Configuration.GetSection(CrawlerConstants.WORLD_COUNTRIES_URL).Value))
            .Distinct()
            .ToList();

        return urls;
    }
}