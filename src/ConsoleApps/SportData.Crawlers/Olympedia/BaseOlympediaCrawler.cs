namespace SportData.Crawlers.Olympedia;

using HtmlAgilityPack;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using SportData.Common.Constants;
using SportData.Data.Models.Http;
using SportData.Services.Data.CrawlerStorage.Interfaces;
using SportData.Services.Interfaces;

public abstract class BaseOlympediaCrawler : BaseCrawler
{
    protected BaseOlympediaCrawler(ILogger<BaseCrawler> logger, IConfiguration configuration, IHttpService httpService, ICrawlersService crawlersService, IGroupsService groupsService)
        : base(logger, configuration, httpService, crawlersService, groupsService)
    {
    }

    protected IReadOnlyCollection<string> ExtractGameUrls(HttpModel httpModel)
    {
        var tables = httpModel
            .HtmlDocument
            .DocumentNode
            .SelectNodes("//table[@class='table table-striped']")
            .Take(3)
            .ToList();

        var urls = new List<string>();
        foreach (var table in tables)
        {
            var document = new HtmlDocument();
            document.LoadHtml(table.OuterHtml);

            var currentUrls = document
                .DocumentNode
                .SelectNodes("//a")
                .Select(x => x.Attributes["href"]?.Value)
                .Where(x => x != null)
                .Select(x => this.CreateUrl(x, this.Configuration.GetSection(CrawlerConstants.OLYMPEDIA_MAIN_URL).Value))
                .Distinct()
                .ToList();

            urls.AddRange(currentUrls);
        }

        return urls;
    }

    protected IReadOnlyCollection<string> ExtractOlympediaDisciplineUrls(HttpModel httpModel)
    {
        var table = httpModel
            .HtmlDocument
            .DocumentNode
            .SelectNodes("//table[@class='table table-striped']")?
            .FirstOrDefault();

        if (table == null)
        {
            return null;
        }

        var document = new HtmlDocument();
        document.LoadHtml(table.OuterHtml);

        var disciplineUrls = document
            .DocumentNode
            .SelectNodes("//a")
            .Select(x => x.Attributes["href"]?.Value)
            .Where(x => x != null)
            .Select(x => this.CreateUrl(x, this.Configuration.GetSection(CrawlerConstants.OLYMPEDIA_MAIN_URL).Value))
            .Distinct()
            .ToList();

        return disciplineUrls;
    }

    protected IReadOnlyCollection<string> GetMedalDisciplineUrls(HttpModel httpModel)
    {
        var medalTable = httpModel
            .HtmlDocument
            .DocumentNode
            .SelectNodes("//table[@class='table table-striped']")
            .FirstOrDefault()
            .OuterHtml;

        //var medalTable = RegexHelper.ExtractFirstGroup(httpModel.HtmlDocument.DocumentNode.OuterHtml, @"<h2>Medals<\/h2>\s*<table class=(?:'|"")table table-striped(?:'|"")>(.*?)<\/table>");
        if (medalTable != null)
        {
            var document = new HtmlDocument();
            document.LoadHtml(medalTable);

            var url = document
                .DocumentNode
                .SelectNodes("//a")
                .Select(x => x.Attributes["href"]?.Value.Trim())
                .Where(x => x.StartsWith("/results/"))
                .Select(x => this.CreateUrl(x, this.Configuration.GetSection(CrawlerConstants.OLYMPEDIA_MAIN_URL).Value))
                .Distinct()
                .ToList();

            return url;
        }

        return null;
    }

    protected IReadOnlyCollection<string> ExtractResultUrls(HttpModel httpModel)
    {
        var urls = httpModel
            .HtmlDocument
            .DocumentNode
            .SelectNodes("//table//a")
            .Select(x => x.Attributes["href"]?.Value.Trim())
            .Where(x => x.StartsWith("/results/"))
            .Select(x => this.CreateUrl(x, this.Configuration.GetSection(CrawlerConstants.OLYMPEDIA_MAIN_URL).Value))
            .Distinct()
            .ToList();

        var additionalUrls = httpModel
            .HtmlDocument
            .DocumentNode
            .SelectNodes("//form[@class='form-inline']//option")?
            .Select(x => x.Attributes["value"]?.Value.Trim())
            .Where(x => !string.IsNullOrEmpty(x))
            .Select(x => this.CreateUrl($"/results/{x}", this.Configuration.GetSection(CrawlerConstants.OLYMPEDIA_MAIN_URL).Value))
            .Distinct()
            .ToList();

        if (additionalUrls != null && additionalUrls.Count > 0)
        {
            urls.AddRange(additionalUrls);
        }

        return urls;
    }
}