namespace SportData.Crawlers.Olympedia;

using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using SportData.Common.Constants;
using SportData.Data.Models.Http;
using SportData.Services.Data.CrawlerStorageDb.Interfaces;
using SportData.Services.Interfaces;

public class VenueCrawler : BaseOlympediaCrawler
{
    private readonly IRegExpService regExpService;

    public VenueCrawler(ILogger<BaseCrawler> logger, IConfiguration configuration, IHttpService httpService, ICrawlersService crawlersService, IGroupsService groupsService,
        IRegExpService regExpSevice)
        : base(logger, configuration, httpService, crawlersService, groupsService)
    {
        this.regExpService = regExpSevice;
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
                    var editionNumberMatch = this.regExpService.Match(url, @"\/(\d+)");
                    if (editionNumberMatch != null)
                    {
                        var gameVenuesHttpModel = await this.HttpService.GetAsync($"{this.Configuration.GetSection(CrawlerConstants.OLYMPEDIA_VENUES_ULR).Value}" +
                            $"{editionNumberMatch.Groups[1].Value}");
                        var venueUrls = this.ExtractVenueUrls(gameVenuesHttpModel);

                        foreach (var venueUrl in venueUrls)
                        {
                            try
                            {
                                var venueHttpModel = await this.HttpService.GetAsync(venueUrl);
                                await this.ProcessGroupAsync(venueHttpModel);
                            }
                            catch (Exception ex)
                            {
                                this.Logger.LogError(ex, $"Failed to process data: {venueUrl};");
                            }
                        }
                    }
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

    private IReadOnlyCollection<string> ExtractVenueUrls(HttpModel gameVenuesHttpModel)
    {
        var urls = gameVenuesHttpModel
            .HtmlDocument
            .DocumentNode
            .SelectNodes("//table//a")
            .Select(x => x.Attributes["href"]?.Value.Trim())
            .Where(x => this.regExpService.Match(x, @"\/venues\/(\d+)") != null)
            .Select(x => this.CreateUrl(x, this.Configuration.GetSection(CrawlerConstants.OLYMPEDIA_MAIN_URL).Value))
            .Distinct()
            .ToList();

        return urls;
    }
}