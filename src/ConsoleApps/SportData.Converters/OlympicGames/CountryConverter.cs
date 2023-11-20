namespace SportData.Converters.OlympicGames;

using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using SportData.Common.Constants;
using SportData.Data.Entities.Crawlers;
using SportData.Data.Entities.OlympicGames;
using SportData.Services.Data.CrawlerStorageDb.Interfaces;
using SportData.Services.Data.OlympicGamesDb.Interfaces;
using SportData.Services.Interfaces;

public class CountryConverter : BaseOlympediaConverter
{
    private readonly IHttpService httpService;
    private readonly IConfiguration configuration;
    private readonly ICountriesService countriesService;

    public CountryConverter(ILogger<BaseConverter> logger, ICrawlersService crawlersService, ILogsService logsService, IGroupsService groupsService, IZipService zipService,
        IRegExpService regExpService, INormalizeService normalizeService, IDataCacheService dataCacheService, IOlympediaService olympediaService, IHttpService httpService,
        IConfiguration configuration, ICountriesService countriesService)
        : base(logger, crawlersService, logsService, groupsService, zipService, regExpService, normalizeService, dataCacheService, olympediaService)
    {
        this.httpService = httpService;
        this.configuration = configuration;
        this.countriesService = countriesService;
    }

    protected override async Task ProcessGroupAsync(Group group)
    {
        try
        {
            var document = this.CreateHtmlDocument(group.Documents.Single());
            var header = document
                .DocumentNode
                .SelectSingleNode("//h1")
                .InnerText;

            var name = this.RegExpService.Match(header, @"Flag of (.*)").Groups[1].Value.Trim();
            var country = new Country { Name = name };

            var rows = document
                .DocumentNode
                .SelectNodes("//table[@class='table-dl']//tr");

            foreach (var row in rows)
            {
                var thTag = row.Elements("th").Single().InnerText.Trim();
                var tdTag = row.Elements("td").Single().InnerText.Trim();

                switch (thTag.ToLower())
                {
                    case "country codes":
                        var countryCodeMatch = this.RegExpService.Match(tdTag, @"([A-Z]{2}),\s*([A-Z]{3})");
                        if (countryCodeMatch != null)
                        {
                            country.TwoDigitsCode = countryCodeMatch.Groups[1].Value;
                            country.Code = countryCodeMatch.Groups[2].Value;
                        }
                        else
                        {
                            countryCodeMatch = this.RegExpService.Match(tdTag, @"([A-Z-]{6})");
                            if (countryCodeMatch != null)
                            {
                                country.Code = countryCodeMatch.Groups[1].Value;
                            }
                        }
                        break;
                    case "official name":
                        country.OfficialName = tdTag;
                        break;
                }
            }

            var coutnryCode = country.TwoDigitsCode != null ? country.TwoDigitsCode.ToLower() : country.Code.ToLower();
            var flag = await this.httpService.DownloadBytesAsync($"{this.configuration.GetSection(CrawlerConstants.WORLD_COUNTRIES_DOWNLOAD_IMAGE).Value}{coutnryCode}.png");
            country.Flag = flag;

            await this.countriesService.AddOrUpdateAsync(country);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, $"Failed to process group: {group.Identifier}");
        }
    }
}