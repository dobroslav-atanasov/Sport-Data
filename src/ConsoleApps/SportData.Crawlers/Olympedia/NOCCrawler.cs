namespace SportData.Crawlers.Olympedia;

using System.Text.RegularExpressions;
using System.Threading.Tasks;

using HtmlAgilityPack;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using SportData.Common.Constants;
using SportData.Data.Entities.Crawlers;
using SportData.Data.Models.Http;
using SportData.Services.Data.CrawlerStorageDb.Interfaces;
using SportData.Services.Interfaces;

public class NOCCrawler : BaseOlympediaCrawler
{
    public NOCCrawler(ILogger<BaseCrawler> logger, IConfiguration configuration, IHttpService httpService, ICrawlersService crawlersService, IGroupsService groupsService)
        : base(logger, configuration, httpService, crawlersService, groupsService)
    {
    }

    public override async Task StartAsync()
    {
        this.Logger.LogInformation($"{this.GetType().FullName} Start!");

        try
        {
            var httpModel = await this.HttpService.GetAsync(this.Configuration.GetSection(CrawlerConstants.OLYMPEDIA_NOC_URL).Value);
            var countryUrls = this.ExtractCountryUrls(httpModel);

            foreach (var url in countryUrls)
            {
                try
                {
                    var countryHttpModel = await this.HttpService.GetAsync(url, true);
                    var nocUrl = this.ExtractNocUrl(countryHttpModel);
                    if (nocUrl == null)
                    {
                        await this.ProcessGroupAsync(countryHttpModel);
                    }
                    else
                    {
                        var nocHttpModel = await this.HttpService.GetAsync(nocUrl, true);
                        var documents = new List<Document>
                            {
                                this.CreateDocument(countryHttpModel),
                                this.CreateDocument(nocHttpModel)
                            };

                        documents[0].Order = 1;
                        documents[1].Order = 2;

                        await this.ProcessGroupAsync(countryHttpModel, documents);
                    }
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, $"Failed to process url: {url}");
                }
            }
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, $"Failed to process url: {this.Configuration.GetSection(CrawlerConstants.OLYMPEDIA_NOC_URL).Value}");
        }

        this.Logger.LogInformation($"{this.GetType().FullName} End!");
    }

    private string ExtractNocUrl(HttpModel httpModel)
    {
        var url = httpModel
            .HtmlDocument
            .DocumentNode
            .SelectNodes("//a")
            .Select(x => x.Attributes["href"]?.Value.Trim())
            .Where(x => x.StartsWith("/organizations/"))
            .Select(x => this.CreateUrl(x, this.Configuration.GetSection(CrawlerConstants.OLYMPEDIA_MAIN_URL).Value))
            .FirstOrDefault();

        return url;
    }

    private IReadOnlyCollection<string> ExtractCountryUrls(HttpModel httpModel)
    {
        var tableNode = httpModel
            .HtmlDocument
            .DocumentNode
            .SelectNodes("//table[@class='table table-striped sortable']")
            .Take(1)
            .ToList();

        var document = new HtmlDocument();
        document.LoadHtml(tableNode.FirstOrDefault().OuterHtml);

        var urls = document
            .DocumentNode
            .SelectNodes("//tr")
            .Where(x => x.OuterHtml.Contains("glyphicon-ok"))
            .Select(x => Regex.Match(x.OuterHtml, "href=\"(.*?)\"").Groups[1].Value.Trim())
            .Where(x => x != null)
            .Select(x => this.CreateUrl(x, this.Configuration.GetSection(CrawlerConstants.OLYMPEDIA_MAIN_URL).Value))
            .Distinct()
            .ToList();

        return urls;
    }
}