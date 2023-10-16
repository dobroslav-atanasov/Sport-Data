namespace SportData.Crawlers.Olympedia;

using System.Text.RegularExpressions;

using HtmlAgilityPack;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using SportData.Common.Constants;
using SportData.Data.Models.Http;
using SportData.Services.Data.CrawlerStorageDb.Interfaces;
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

        return new List<string>
        {
            "https://www.olympedia.org/editions/1",
            "https://www.olympedia.org/editions/2",
            "https://www.olympedia.org/editions/3",
            "https://www.olympedia.org/editions/5",
            "https://www.olympedia.org/editions/6",
            "https://www.olympedia.org/editions/50",
            "https://www.olympedia.org/editions/7",
            "https://www.olympedia.org/editions/8",
            "https://www.olympedia.org/editions/9",
            "https://www.olympedia.org/editions/10",
            "https://www.olympedia.org/editions/11",
            "https://www.olympedia.org/editions/51",
            "https://www.olympedia.org/editions/52",
            "https://www.olympedia.org/editions/12",
            "https://www.olympedia.org/editions/13",
            "https://www.olympedia.org/editions/14",
            "https://www.olympedia.org/editions/15",
            "https://www.olympedia.org/editions/16",
            "https://www.olympedia.org/editions/17",
            "https://www.olympedia.org/editions/18",
            "https://www.olympedia.org/editions/19",
            "https://www.olympedia.org/editions/20",
            "https://www.olympedia.org/editions/21",
            "https://www.olympedia.org/editions/22",
            "https://www.olympedia.org/editions/23",
            "https://www.olympedia.org/editions/24",
            "https://www.olympedia.org/editions/25",
            "https://www.olympedia.org/editions/26",
            "https://www.olympedia.org/editions/53",
            "https://www.olympedia.org/editions/54",
            "https://www.olympedia.org/editions/59",
            "https://www.olympedia.org/editions/61",
            "https://www.olympedia.org/editions/63",
            "https://www.olympedia.org/editions/64",
            "https://www.olympedia.org/editions/372",
            "https://www.olympedia.org/editions/29",
            "https://www.olympedia.org/editions/30",
            "https://www.olympedia.org/editions/31",
            "https://www.olympedia.org/editions/32",
            "https://www.olympedia.org/editions/55",
            "https://www.olympedia.org/editions/56",
            "https://www.olympedia.org/editions/33",
            "https://www.olympedia.org/editions/34",
            "https://www.olympedia.org/editions/35",
            "https://www.olympedia.org/editions/36",
            "https://www.olympedia.org/editions/37",
            "https://www.olympedia.org/editions/38",
            "https://www.olympedia.org/editions/39",
            "https://www.olympedia.org/editions/40",
            "https://www.olympedia.org/editions/41",
            "https://www.olympedia.org/editions/42",
            "https://www.olympedia.org/editions/43",
            "https://www.olympedia.org/editions/44",
            "https://www.olympedia.org/editions/45",
            "https://www.olympedia.org/editions/46",
            "https://www.olympedia.org/editions/47",
            "https://www.olympedia.org/editions/49",
            "https://www.olympedia.org/editions/57",
            "https://www.olympedia.org/editions/58",
            "https://www.olympedia.org/editions/60",
            "https://www.olympedia.org/editions/62",
            "https://www.olympedia.org/editions/72",
            "https://www.olympedia.org/editions/48",
        };

        return urls;
    }

    protected IReadOnlyCollection<string> ExtractOlympediaDisciplineUrls(HttpModel httpModel)
    {
        var table = httpModel
            .HtmlDocument
            .DocumentNode
            .SelectNodes("//table[@class='table table-striped']")?
            .Skip(1).FirstOrDefault();

        var asd = Regex.Match(httpModel.HtmlDocument.DocumentNode.OuterHtml, @"<h2>Medal Disciplines<\/h2>\s*<table(.*?)<\/table>", RegexOptions.Singleline);
        if (asd.Success)
        {
            if (table == null)
            {
                return null;
            }

            var document = new HtmlDocument();
            document.LoadHtml(asd.Groups[1].Value);

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

        return null;
        //if (table == null)
        //{
        //    return null;
        //}

        //var document = new HtmlDocument();
        //document.LoadHtml(table.OuterHtml);

        //var disciplineUrls = document
        //    .DocumentNode
        //    .SelectNodes("//a")
        //    .Select(x => x.Attributes["href"]?.Value)
        //    .Where(x => x != null)
        //    .Select(x => this.CreateUrl(x, this.Configuration.GetSection(CrawlerConstants.OLYMPEDIA_MAIN_URL).Value))
        //    .Distinct()
        //    .ToList();

        //return disciplineUrls;
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