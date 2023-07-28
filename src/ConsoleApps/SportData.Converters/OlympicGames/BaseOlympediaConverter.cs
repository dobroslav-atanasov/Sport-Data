namespace SportData.Converters.OlympicGames;

using HtmlAgilityPack;

using Microsoft.Extensions.Logging;

using SportData.Common.Extensions;
using SportData.Data.Entities.Enumerations;
using SportData.Data.Models.Cache;
using SportData.Services.Data.CrawlerStorageDb.Interfaces;
using SportData.Services.Data.SportDataDb.Interfaces;
using SportData.Services.Interfaces;

public abstract class BaseOlympediaConverter : BaseConverter
{
    public BaseOlympediaConverter(ILogger<BaseConverter> logger, ICrawlersService crawlersService, ILogsService logsService, IGroupsService groupsService, IZipService zipService
        , IRegExpService regExpService, INormalizeService normalizeService, IDataCacheService dataCacheService)
        : base(logger, crawlersService, logsService, groupsService, zipService)
    {
        this.RegExpService = regExpService;
        this.NormalizeService = normalizeService;
        this.DataCacheService = dataCacheService;
    }

    protected IRegExpService RegExpService { get; }

    protected INormalizeService NormalizeService { get; }

    protected IDataCacheService DataCacheService { get; }

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
}