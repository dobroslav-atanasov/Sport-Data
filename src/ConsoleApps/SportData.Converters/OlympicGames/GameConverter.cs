namespace SportData.Converters.OlympicGames;

using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using SportData.Data.Entities.Crawlers;
using SportData.Data.Entities.Enumerations;
using SportData.Data.Entities.OlympicGames;
using SportData.Services.Data.CrawlerStorageDb.Interfaces;
using SportData.Services.Data.SportDataDb.Interfaces;
using SportData.Services.Interfaces;

public class GameConverter : BaseConverter
{
    private readonly IDataCacheService dataCacheService;
    private readonly ICitiesService citiesService;

    public GameConverter(ILogger<BaseConverter> logger, ICrawlersService crawlersService, ILogsService logsService, IGroupsService groupsService, IZipService zipService,
        IRegExpService regExpService, INormalizeService normalizeService, IDataCacheService dataCacheService, ICitiesService citiesService)
        : base(logger, crawlersService, logsService, groupsService, zipService, regExpService, normalizeService)
    {
        this.dataCacheService = dataCacheService;
        this.citiesService = citiesService;
    }

    protected override async Task ProcessGroupAsync(Group group)
    {
        try
        {
            var document = this.CreateHtmlDocument(group.Documents.Single());
            var header = document.DocumentNode.SelectSingleNode("//h1").InnerText;

            var game = new Game();
            var gameMatch = this.RegExpService.Match(header, @"(\d+)\s*(summer|winter)");
            if (gameMatch != null)
            {
                var numberMatch = this.RegExpService.Match(document.DocumentNode.OuterHtml, @"<th>Number and Year<\/th>\s*<td>\s*([IVXLC]+)\s*\/(.*?)<\/td>");
                var hostCityMatch = this.RegExpService.Match(document.DocumentNode.OuterHtml, @"<tr>\s*<th>Host city<\/th>\s*<td>\s*([\w'\-\s.]+),\s*([\w'\-\s]+)");
                var hostCityName = this.NormalizeService.NormalizeHostCityName(hostCityMatch?.Groups[1].Value.Trim());

                game.Year = int.Parse(gameMatch.Groups[1].Value);
                game.Type = gameMatch.Groups[2].Value.Trim().ToLower() == "summer" ? OlympicGameType.Summer : OlympicGameType.Winter;
                game.Number = numberMatch?.Groups[1].Value.Trim();
                game.OfficialName = this.SetOfficialName(hostCityName, game.Year);

                var city = await this.ProceccCityAsync(hostCityName, game.Year, game.Type);
                if (hostCityMatch != null)
                {
                    //game.HostCity = this.NormalizeService.NormalizeHostCityName(hostCityMatch.Groups[1].Value.Trim());
                    //var country = this.DataCacheService.CountryCacheModels.FirstOrDefault(c => c.Name == hostCityMatch.Groups[2].Value.Trim());
                    //game.HostCountryId = country.Id;
                    //game.OfficialName = this.SetOfficialName(game.HostCity, game.Year);
                }
            }
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, $"Failed to process group: {group.Identifier}");
        }
    }

    private async Task<City> ProceccCityAsync(string cityName, int year, OlympicGameType type)
    {
        // 1956 stockholm 
        var nocCode = this.NormalizeService.MapCityNameAndYearToNOCCode(cityName, year);
        var noc = this.dataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == nocCode);
        if (noc != null)
        {
            var city = new City
            {
                Name = cityName,
                NOCId = noc.Id,
            };

            city = await this.citiesService.AddOrUpdateAsync(city);

            return city;
        }

        if (year == 1956 && type == OlympicGameType.Summer)
        {

        }
        return null;
        // noc in cache model
        // create city model
    }

    private string SetOfficialName(string hostCity, int year)
    {
        if (hostCity == "Rio de Janeiro" && year == 2016)
        {
            hostCity = "Rio";
        }
        else if (hostCity == "Los Angeles" && year == 2028)
        {
            hostCity = "LA";
        }
        else if (hostCity == "Milano-Cortina d'Ampezzo" && year == 2026)
        {
            hostCity = "Milano Cortina";
        }

        return $"{hostCity} {year}";
    }
}