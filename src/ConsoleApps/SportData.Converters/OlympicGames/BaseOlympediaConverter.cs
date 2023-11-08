namespace SportData.Converters.OlympicGames;

using HtmlAgilityPack;

using Microsoft.Extensions.Logging;

using SportData.Common.Extensions;
using SportData.Data.Entities.OlympicGames.Enumerations;
using SportData.Data.Models.Cache;
using SportData.Data.Models.Converters;
using SportData.Services.Data.CrawlerStorageDb.Interfaces;
using SportData.Services.Data.OlympicGamesDb.Interfaces;
using SportData.Services.Interfaces;

public abstract class BaseOlympediaConverter : BaseConverter
{
    public BaseOlympediaConverter(ILogger<BaseConverter> logger, ICrawlersService crawlersService, ILogsService logsService, IGroupsService groupsService, IZipService zipService
        , IRegExpService regExpService, INormalizeService normalizeService, IDataCacheService dataCacheService, IOlympediaService olympediaService)
        : base(logger, crawlersService, logsService, groupsService, zipService)
    {
        this.RegExpService = regExpService;
        this.NormalizeService = normalizeService;
        this.DataCacheService = dataCacheService;
        this.OlympediaService = olympediaService;
    }

    protected IRegExpService RegExpService { get; }

    protected INormalizeService NormalizeService { get; }

    protected IDataCacheService DataCacheService { get; }

    protected IOlympediaService OlympediaService { get; }

    protected GameCacheModel FindGame(HtmlDocument htmlDocument)
    {
        var headers = htmlDocument.DocumentNode.SelectSingleNode("//ol[@class='breadcrumb']");
        var gameMatch = this.RegExpService.Match(headers.OuterHtml, @"<a href=""\/editions\/(?:\d+)"">(\d+)\s*(\w+)\s*Olympics<\/a>");

        if (gameMatch != null)
        {
            var gameYear = int.Parse(gameMatch.Groups[1].Value);
            var gameType = gameMatch.Groups[2].Value.Trim();

            if (gameType.ToLower() == "equestrian")
            {
                gameType = "Summer";
            }

            var game = this.DataCacheService.GameCacheModels.FirstOrDefault(g => g.Year == gameYear && g.Type == gameType.ToEnum<OlympicGameType>());

            return game;
        }

        return null;
    }

    protected DisciplineCacheModel FindDiscipline(HtmlDocument htmlDocument)
    {
        var headers = htmlDocument.DocumentNode.SelectSingleNode("//ol[@class='breadcrumb']");
        var disciplineName = this.RegExpService.MatchFirstGroup(headers.OuterHtml, @"<a href=""\/editions\/[\d]+\/sports\/(?:.*?)"">(.*?)<\/a>");
        var eventName = this.RegExpService.MatchFirstGroup(headers.OuterHtml, @"<li\s*class=""active"">(.*?)<\/li>");

        if (disciplineName != null && eventName != null)
        {
            if (disciplineName.ToLower() == "wrestling")
            {
                if (eventName.ToLower().Contains("freestyle"))
                {
                    disciplineName = "Wrestling Freestyle";
                }
                else
                {
                    disciplineName = "Wrestling Greco-Roman";
                }
            }
            else if (disciplineName.ToLower() == "canoe marathon")
            {
                disciplineName = "Canoe Sprint";
            }

            var discipline = this.DataCacheService.DisciplineCacheModels.FirstOrDefault(d => d.Name == disciplineName);

            return discipline;
        }

        return null;
    }

    protected EventModel CreateEventModel(string originalEventName, GameCacheModel gameCacheModel, DisciplineCacheModel disciplineCacheModel)
    {
        if (gameCacheModel != null && disciplineCacheModel != null)
        {
            var eventModel = new EventModel
            {
                OriginalName = originalEventName,
                GameId = gameCacheModel.Id,
                GameYear = gameCacheModel.Year,
                DisciplineId = disciplineCacheModel.Id,
                DisciplineName = disciplineCacheModel.Name,
                Name = this.NormalizeService.NormalizeEventName(originalEventName, gameCacheModel.Year, disciplineCacheModel.Name)
            };

            var parts = eventModel.Name.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var gender = parts.Last().Trim();
            eventModel.Name = string.Join("|", parts.Take(parts.Count - 1).Select(x => x.Trim()).ToList());

            this.AddInfo(eventModel);

            if (disciplineCacheModel.Name == "Wrestling Freestyle")
            {
                eventModel.Name = eventModel.Name.Replace("Freestyle", string.Empty);
            }
            else if (disciplineCacheModel.Name == "Wrestling Greco-Roman")
            {
                eventModel.Name = eventModel.Name.Replace("Greco-Roman", string.Empty);
            }

            if (this.RegExpService.IsMatch(eventModel.Name, @"Team"))
            {
                eventModel.Name = this.RegExpService.Replace(eventModel.Name, @"Team", string.Empty);
                eventModel.Name = $"Team|{eventModel.Name}";
            }

            if (this.RegExpService.IsMatch(eventModel.Name, @"Individual"))
            {
                eventModel.Name = this.RegExpService.Replace(eventModel.Name, @"Individual", string.Empty);
                eventModel.Name = $"Individual|{eventModel.Name}";
            }

            var nameParts = eventModel.Name.Split(new[] { " ", "|" }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.UpperFirstChar()).ToList();
            var name = string.Join(" ", nameParts);

            eventModel.Name = $"{gender} {name}";
            var prefix = this.ConvertEventPrefix(gender);
            eventModel.NormalizedName = $"{prefix} {name.ToLower()}";

            return eventModel;
        }

        return null;
    }

    protected bool CheckForbiddenEvent(string eventName, string disciplineName, int year)
    {
        var list = new List<string>
        {
            "1900-Archery-Unknown Event, Men",
            "1920-Shooting-Unknown Event, Men",
            "1904-Artistic Gymnastics-Individual All-Around, Field Sports, Men"
        };

        var isForbidden = list.Any(x => x == $"{year}-{disciplineName}-{eventName}");
        return isForbidden;
    }

    protected Dictionary<string, int> GetHeaderIndexes(HtmlDocument document)
    {
        var headers = document
            .DocumentNode
            .SelectSingleNode("//table[@class='table table-striped']/thead/tr")
            .Elements("th")
            .Select(x => x.InnerText.ToLower().Trim())
            .Where(x => !string.IsNullOrEmpty(x))
            .ToList();

        var indexes = this.OlympediaService.FindIndexes(headers);

        return indexes;
    }

    private string ConvertEventPrefix(string gender)
    {

        switch (gender.ToLower())
        {
            case "men":
                gender = "Men's";
                break;
            case "women":
                gender = "Women's";
                break;
            case "mixed":
            case "open":
                gender = "Mixed";
                break;
        }

        return gender;
    }

    private void AddInfo(EventModel eventModel)
    {
        var match = this.RegExpService.Match(eventModel.Name, @"\(.*?\)");
        if (match != null)
        {
            var text = match.Groups[0].Value;
            eventModel.Name = eventModel.Name.Replace(text, string.Empty).Trim();

            var poundMatch = this.RegExpService.Match(text, @"(\+|-)([\d\.]+)\s*pounds");
            var kilogramMatch = this.RegExpService.Match(text, @"(\+|-)([\d\.]+)\s*kilograms");
            var otherMatch = this.RegExpService.Match(text, @"\((.*?)\)");
            if (poundMatch != null)
            {
                var weight = double.Parse(poundMatch.Groups[2].Value.Replace(".", ",")).ConvertPoundToKilograms();
                eventModel.AdditionalInfo = $"{poundMatch.Groups[1].Value.Trim()}{weight:F2}kg";
            }
            else if (kilogramMatch != null)
            {
                var weight = double.Parse(kilogramMatch.Groups[2].Value.Replace(".", ","));
                eventModel.AdditionalInfo = $"{kilogramMatch.Groups[1].Value.Trim()}{weight}kg";
            }
            else if (otherMatch != null)
            {
                eventModel.AdditionalInfo = otherMatch.Value.Replace("(", string.Empty).Replace(")", string.Empty).Trim();
            }
        }
    }
}