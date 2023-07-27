namespace SportData.Converters.OlympicGames;

using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using SportData.Common.Extensions;
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
    private readonly IGamesService gamesService;
    private readonly IHostsService hostsService;

    public GameConverter(ILogger<BaseConverter> logger, ICrawlersService crawlersService, ILogsService logsService, IGroupsService groupsService, IZipService zipService,
        IRegExpService regExpService, INormalizeService normalizeService, IDataCacheService dataCacheService, ICitiesService citiesService, IGamesService gamesService,
        IHostsService hostsService)
        : base(logger, crawlersService, logsService, groupsService, zipService, regExpService, normalizeService)
    {
        this.dataCacheService = dataCacheService;
        this.citiesService = citiesService;
        this.gamesService = gamesService;
        this.hostsService = hostsService;
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
                var openByMatch = this.RegExpService.Match(document.DocumentNode.OuterHtml, @"<tr>\s*<th>Officially opened by<\/th>\s*<td>(.*?)<\/td>\s*<\/tr>");
                var torchbearersMatch = this.RegExpService.Match(document.DocumentNode.OuterHtml, @"<tr>\s*<th>Torchbearer\(s\)<\/th>\s*<td>(.*?)<\/td>\s*<\/tr>");
                var athleteOathByMatch = this.RegExpService.Match(document.DocumentNode.OuterHtml, @"<tr>\s*<th>Taker of the Athlete's Oath<\/th>\s*<td>(.*?)<\/td>\s*<\/tr>");
                var judgeOathByMatch = this.RegExpService.Match(document.DocumentNode.OuterHtml, @"<tr>\s*<th>Taker of the Official's Oath<\/th>\s*<td>(.*?)<\/td>\s*<\/tr>");
                var coachOathByMatch = this.RegExpService.Match(document.DocumentNode.OuterHtml, @"<tr>\s*<th>Taker of the Coach's Oath<\/th>\s*<td>(.*?)<\/td>\s*<\/tr>");
                var olympicFlagBearersMatch = this.RegExpService.Match(document.DocumentNode.OuterHtml, @"<tr>\s*<th>Olympic Flag Bearers<\/th>\s*<td>(.*?)<\/td>\s*<\/tr>");
                var descriptionMatch = this.RegExpService.Match(document.DocumentNode.OuterHtml, @"<h2>\s*Overview\s*(?:<small><\/small>)?\s*<\/h2>\s*<div class=(?:'|"")description(?:'|"")>\s*(.*?)<\/div>");
                var bidProcessMatch = this.RegExpService.Match(document.DocumentNode.OuterHtml, @"<h2>Bid process<\/h2>\s*<div class=(?:'|"")description(?:'|"")>\s*(.*?)<\/div>");

                game.Year = int.Parse(gameMatch.Groups[1].Value);
                game.Type = gameMatch.Groups[2].Value.Trim().ToLower() == "summer" ? OlympicGameType.Summer : OlympicGameType.Winter;
                game.Number = numberMatch?.Groups[1].Value.Trim();
                game.OfficialName = this.SetOfficialName(hostCityName, game.Year);
                game.OpenBy = openByMatch != null ? this.RegExpService.CutHtml(openByMatch.Groups[1].Value) : null;
                game.Torchbearers = torchbearersMatch != null ? this.RegExpService.CutHtml(torchbearersMatch.Groups[1].Value) : null;
                game.AthleteOathBy = athleteOathByMatch != null ? this.RegExpService.CutHtml(athleteOathByMatch.Groups[1].Value) : null;
                game.JudgeOathBy = judgeOathByMatch != null ? this.RegExpService.CutHtml(judgeOathByMatch.Groups[1].Value) : null;
                game.CoachOathBy = coachOathByMatch != null ? this.RegExpService.CutHtml(coachOathByMatch.Groups[1].Value) : null;
                game.OlympicFlagBearers = olympicFlagBearersMatch != null ? this.RegExpService.CutHtml(olympicFlagBearersMatch.Groups[1].Value) : null;
                game.Description = descriptionMatch != null ? this.RegExpService.CutHtml(descriptionMatch.Groups[1].Value) : null;
                game.BidProcess = bidProcessMatch != null ? this.RegExpService.CutHtml(bidProcessMatch.Groups[1].Value) : null;

                var openDateMatch = this.RegExpService.Match(document.DocumentNode.OuterHtml, @"<tr>\s*<th>Opening ceremony<\/th>\s*<td>\s*([\d]+)\s*([A-Za-z]+)\s*(\d+)?\s*<\/td>\s*<\/tr>");
                if (openDateMatch != null)
                {
                    var day = int.Parse(openDateMatch.Groups[1].Value);
                    var month = openDateMatch.Groups[2].Value.GetMonthNumber();
                    game.OpenDate = DateTime.ParseExact($"{day}-{month}-{(game.Year != 2020 ? game.Year : game.Year + 1)}", "d-M-yyyy", null);
                }

                var closeDateMatch = this.RegExpService.Match(document.DocumentNode.OuterHtml, @"<tr>\s*<th>Closing ceremony<\/th>\s*<td>\s*([\d]+)\s*([A-Za-z]+)\s*(\d+)?\s*<\/td>\s*<\/tr>");
                if (closeDateMatch != null)
                {
                    var day = int.Parse(closeDateMatch.Groups[1].Value);
                    var month = closeDateMatch.Groups[2].Value.GetMonthNumber();
                    game.CloseDate = DateTime.ParseExact($"{day}-{month}-{(game.Year != 2020 ? game.Year : game.Year + 1)}", "d-M-yyyy", null);
                }

                var competitionDateMatch = this.RegExpService.Match(document.DocumentNode.OuterHtml, @"<tr>\s*<th>Competition dates<\/th>\s*<td>\s*(\d+)\s*([A-Za-z]+)?\s*–\s*(\d+)\s*([A-Za-z]+)\s*(\d+)?\s*<\/td>\s*<\/tr>");
                if (competitionDateMatch != null)
                {
                    var startDay = int.Parse(competitionDateMatch.Groups[1].Value);
                    var startMonth = competitionDateMatch.Groups[2].Value != string.Empty ? competitionDateMatch.Groups[2].Value.GetMonthNumber() : competitionDateMatch.Groups[4].Value.GetMonthNumber();
                    var endDay = int.Parse(competitionDateMatch.Groups[3].Value);
                    var endMonth = competitionDateMatch.Groups[4].Value.GetMonthNumber();

                    game.StartCompetitionDate = DateTime.ParseExact($"{startDay}-{startMonth}-{(game.Year != 2020 ? game.Year : game.Year + 1)}", "d-M-yyyy", null);
                    game.EndCompetitionDate = DateTime.ParseExact($"{endDay}-{endMonth}-{(game.Year != 2020 ? game.Year : game.Year + 1)}", "d-M-yyyy", null);
                }

                var city = await this.ProceccCityAsync(hostCityName, game.Year, game.Type);
                game = await this.gamesService.AddOrUpdateAsync(game);
                await this.hostsService.AddOrUpdateAsync(new Host { CityId = city.Id, GameId = game.Id });

                if (game.Year == 1956 && game.Type == OlympicGameType.Summer)
                {
                    city = await this.ProceccCityAsync("Stockholm", game.Year, game.Type);
                    await this.hostsService.AddOrUpdateAsync(new Host { CityId = city.Id, GameId = game.Id });
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

        return null;
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