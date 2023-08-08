namespace SportData.Converters.OlympicGames;

using System.Text.Json;
using System.Threading.Tasks;

using HtmlAgilityPack;

using Microsoft.Extensions.Logging;

using SportData.Common.Constants;
using SportData.Data.Entities.Crawlers;
using SportData.Data.Entities.Enumerations;
using SportData.Data.Entities.OlympicGames;
using SportData.Data.Models.Cache;
using SportData.Data.Models.Converters;
using SportData.Data.Models.OlympicGames.Basketball;
using SportData.Services.Data.CrawlerStorageDb.Interfaces;
using SportData.Services.Data.SportDataDb.Interfaces;
using SportData.Services.Interfaces;

public class ResultConverter : BaseOlympediaConverter
{
    private readonly IDateService dateService;
    private readonly ITeamsService teamsService;
    private readonly IAthletesService athletesService;
    private readonly IParticipantsService participantsService;
    private readonly IResultsService resultsService;

    public ResultConverter(ILogger<BaseConverter> logger, ICrawlersService crawlersService, ILogsService logsService, IGroupsService groupsService, IZipService zipService,
        IRegExpService regExpService, INormalizeService normalizeService, IDataCacheService dataCacheService, IOlympediaService olympediaService, IDateService dateService,
        ITeamsService teamsService, IAthletesService athletesService, IParticipantsService participantsService, IResultsService resultsService)
        : base(logger, crawlersService, logsService, groupsService, zipService, regExpService, normalizeService, dataCacheService, olympediaService)
    {
        this.dateService = dateService;
        this.teamsService = teamsService;
        this.athletesService = athletesService;
        this.participantsService = participantsService;
        this.resultsService = resultsService;
    }

    protected override async Task ProcessGroupAsync(Group group)
    {
        try
        {
            var document = this.CreateHtmlDocument(group.Documents.Single(x => x.Order == 1));
            var documents = group.Documents.Where(x => x.Order != 1).OrderBy(x => x.Order);
            var originalEventName = document.DocumentNode.SelectSingleNode("//ol[@class='breadcrumb']/li[@class='active']").InnerText;
            var gameCacheModel = this.FindGame(document);
            var disciplineCacheModel = this.FindDiscipline(document);
            var eventModel = this.CreateEventModel(originalEventName, gameCacheModel, disciplineCacheModel);
            if (eventModel != null)
            {
                var eventCacheModel = this.DataCacheService
                    .EventCacheModels
                    .FirstOrDefault(x => x.OriginalName == eventModel.OriginalName && x.GameId == eventModel.GameId && x.DisciplineId == eventModel.DisciplineId);

                if (eventCacheModel != null)
                {
                    var standingTable = this.GetStandingTable(document, eventCacheModel);
                    var tables = this.GetTables(document, eventCacheModel);

                    switch (disciplineCacheModel.Name)
                    {
                        case DisciplineConstants.BASKETBALL_3X3:
                            await this.ProcessBasketball3x3Async(document, documents, gameCacheModel, disciplineCacheModel, eventCacheModel, standingTable, tables);
                            break;
                        case DisciplineConstants.BASKETBALL:
                            await this.ProcessBasketballAsync(document, documents, gameCacheModel, disciplineCacheModel, eventCacheModel, standingTable, tables);
                            break;
                    }
                }
            }

        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, $"Failed to process group: {group.Identifier}");
        }
    }

    private TableModel GetStandingTable(HtmlDocument document, EventCacheModel eventCacheModel)
    {
        var standingTableHtml = document.DocumentNode.SelectSingleNode("//table[@class='table table-striped']").OuterHtml;

        var table = new TableModel
        {
            Html = standingTableHtml,
            EventId = eventCacheModel.Id,
        };

        return table;
    }

    private IList<TableModel> GetTables(HtmlDocument document, EventCacheModel eventCacheModel)
    {
        var tables = new List<TableModel>();

        var html = document.ParsedText;
        html = html.Replace("<table class=\"biodata\"></table>", string.Empty);

        var headerMatches = this.RegExpService.Matches(html, @"<h2>(.*?)<\/h2>");
        if (headerMatches.Count > 0)
        {
            for (int i = 0; i < headerMatches.Count; i++)
            {
                var pattern = string.Empty;
                if (i + 1 == headerMatches.Count)
                {
                    pattern = $@"{headerMatches[i].Value}#$%";
                }
                else
                {
                    pattern = $@"{headerMatches[i].Value}@$#$@{headerMatches[i + 1].Value}";
                }

                pattern = pattern.Replace("(", @"\(")
                        .Replace(")", @"\)")
                        .Replace("/", @"\/")
                        .Replace("#$%", "(.*)")
                        .Replace("@$#$@", "(.*?)");

                var match = this.RegExpService.Match(html, pattern);
                if (match.Success && match.Value.Contains("<table class=\"table table-striped\">"))
                {
                    var table = new TableModel
                    {
                        Html = match.Groups[1].Value,
                        EventId = eventCacheModel.Id,
                        Title = headerMatches[i].Value
                    };

                    var title = this.RegExpService.CutHtml(table.Title);
                    var dateMatch = this.RegExpService.Match(title, @"\((.*)\)");
                    var dateString = dateMatch != null ? dateMatch.Groups[1].Value : string.Empty;

                    table.Title = this.RegExpService.CutHtml(title, @"\((.*)\)")?.Replace(",", string.Empty).Trim();
                    table.Round = this.NormalizeService.MapRoundType(table.Title);

                    if (table.Round == RoundType.None)
                    {
                        ;
                    }

                    if (!string.IsNullOrEmpty(dateString))
                    {
                        // todo date only for date!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        //var dates = this.GetDates(dateString);
                        //table.FromDate = dates.Item1;
                        //table.ToDate = dates.Item1;
                    }

                    tables.Add(table);
                }
            }
        }

        return tables;
    }

    private async Task<Guid?> ExtractRefereeAsync(string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            var number = int.Parse(text);
            var athlete = await this.athletesService.GetAsync(number);
            return athlete?.Id;
        }

        return null;
    }

    protected double? CalculatePercent(int? number, int? total)
    {
        if (number.HasValue && total.HasValue)
        {
            if (total.Value == 0)
            {
                return 0.0;
            }

            return ((double)number.Value / total.Value) * 100.0;
        }

        return null;
    }

    #region BASKETBALL
    private async Task ProcessBasketball3x3Async(HtmlDocument document, IOrderedEnumerable<Document> documents, GameCacheModel gameCacheModel, DisciplineCacheModel disciplineCacheModel,
        EventCacheModel eventCacheModel, TableModel standingTable, IList<TableModel> tables)
    {
        var matches = new List<BasketballMatch>();
        foreach (var table in tables)
        {
            var trRows = table.HtmlDocument.DocumentNode.SelectNodes("//tr").Where(x => this.OlympediaService.IsMatchNumber(x.InnerText)).ToList();
            foreach (var trRow in trRows)
            {
                var trDocument = new HtmlDocument();
                trDocument.LoadHtml(trRow.OuterHtml);

                var tdNodes = trDocument.DocumentNode.SelectNodes("//td");
                var homeTeamCode = tdNodes[3].OuterHtml;
                var resultString = tdNodes[4].InnerText;
                var awayTeamCode = tdNodes[6].OuterHtml;
                var resultModel = this.OlympediaService.GetResult(resultString);
                var homeNOCCacheModel = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == this.OlympediaService.FindCountryCode(homeTeamCode));
                var homeTeam = await this.teamsService.GetAsync(homeNOCCacheModel.Id, eventCacheModel.Id);
                var awayNOCCacheModel = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == this.OlympediaService.FindCountryCode(awayTeamCode));
                var awayTeam = await this.teamsService.GetAsync(awayNOCCacheModel.Id, eventCacheModel.Id);

                var match = new BasketballMatch
                {
                    MatchNumber = this.OlympediaService.FindMatchNumber(tdNodes[0].InnerText),
                    Round = table.Round == RoundType.FinalRound ? this.NormalizeService.MapFinalRoundMatch(tdNodes[0].InnerText) : table.Round,
                    Date = this.dateService.MatchDate(tdNodes[1].InnerText, gameCacheModel.Year),
                    ResultId = this.OlympediaService.FindResultNumber(tdNodes[0].OuterHtml),
                    Decision = resultModel.Decision,
                    HomeTeam = new BasketballTeam
                    {
                        Name = homeTeam.Name,
                        TeamId = homeTeam.Id,
                        Result = resultModel.HomeTeamResult,
                        Points = resultModel.HomeTeamPoints
                    },
                    AwayTeam = new BasketballTeam
                    {
                        Name = awayTeam.Name,
                        TeamId = awayTeam.Id,
                        Result = resultModel.AwayTeamResult,
                        Points = resultModel.AwayTeamPoints
                    }
                };

                var matchDocument = documents.FirstOrDefault(x => x.Url.EndsWith($"{match.ResultId}"));
                if (matchDocument != null)
                {
                    var htmlDocument = this.CreateHtmlDocument(matchDocument);
                    var firstReferee = this.RegExpService.MatchFirstGroup(htmlDocument.ParsedText, @"<th>Referee #1<\/th>(?:.*?)\/athletes\/(\d+)");
                    var secondReferee = this.RegExpService.MatchFirstGroup(htmlDocument.ParsedText, @"<th>Referee #2<\/th>(?:.*?)\/athletes\/(\d+)");
                    match.FirstRefereeId = await this.ExtractRefereeAsync(firstReferee);
                    match.SecondRefereeId = await this.ExtractRefereeAsync(secondReferee);

                    var teamTables = htmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']").Skip(1).ToList();
                    var homeTeamParticipants = await this.ExtractBasketball3x3ParticipantsAsync(teamTables[0].OuterHtml, eventCacheModel);
                    var awayTeamParticipants = await this.ExtractBasketball3x3ParticipantsAsync(teamTables[1].OuterHtml, eventCacheModel);

                    match.HomeTeam.Participants = homeTeamParticipants;
                    match.AwayTeam.Participants = awayTeamParticipants;

                    var homeTeamStatistic = new BasketballStatistic
                    {
                        Assists = homeTeamParticipants.Sum(x => x.Assists),
                        BlockedShots = homeTeamParticipants.Sum(x => x.BlockedShots),
                        DefensiveRebounds = homeTeamParticipants.Sum(x => x.DefensiveRebounds),
                        DisqualifyingFouls = homeTeamParticipants.Sum(x => x.DisqualifyingFouls),
                        FreeThrowsAttempts = homeTeamParticipants.Sum(x => x.FreeThrowsAttempts),
                        FreeThrowsGoals = homeTeamParticipants.Sum(x => x.FreeThrowsGoals),
                        OffensiveRebounds = homeTeamParticipants.Sum(x => x.OffensiveRebounds),
                        PersonalFouls = homeTeamParticipants.Sum(x => x.PersonalFouls),
                        PlusMinus = homeTeamParticipants.Sum(x => x.PlusMinus),
                        Steals = homeTeamParticipants.Sum(x => x.Steals),
                        ThreePointsAttempts = homeTeamParticipants.Sum(x => x.ThreePointsAttempts),
                        ThreePointsGoals = homeTeamParticipants.Sum(x => x.ThreePointsGoals),
                        TotalFieldGoals = homeTeamParticipants.Sum(x => x.TotalFieldGoals),
                        TotalFieldGoalsAttempts = homeTeamParticipants.Sum(x => x.TotalFieldGoalsAttempts),
                        TotalRebounds = homeTeamParticipants.Sum(x => x.DefensiveRebounds) + homeTeamParticipants.Sum(x => x.OffensiveRebounds),
                        Turnovers = homeTeamParticipants.Sum(x => x.Turnovers),
                        TwoPointsAttempts = homeTeamParticipants.Sum(x => x.TwoPointsAttempts),
                        TwoPointsGoals = homeTeamParticipants.Sum(x => x.TwoPointsGoals),
                    };

                    var awayTeamStatistic = new BasketballStatistic
                    {
                        Assists = awayTeamParticipants.Sum(x => x.Assists),
                        BlockedShots = awayTeamParticipants.Sum(x => x.BlockedShots),
                        DefensiveRebounds = awayTeamParticipants.Sum(x => x.DefensiveRebounds),
                        DisqualifyingFouls = awayTeamParticipants.Sum(x => x.DisqualifyingFouls),
                        FreeThrowsAttempts = awayTeamParticipants.Sum(x => x.FreeThrowsAttempts),
                        FreeThrowsGoals = awayTeamParticipants.Sum(x => x.FreeThrowsGoals),
                        OffensiveRebounds = awayTeamParticipants.Sum(x => x.OffensiveRebounds),
                        PersonalFouls = awayTeamParticipants.Sum(x => x.PersonalFouls),
                        PlusMinus = awayTeamParticipants.Sum(x => x.PlusMinus),
                        Steals = awayTeamParticipants.Sum(x => x.Steals),
                        ThreePointsAttempts = awayTeamParticipants.Sum(x => x.ThreePointsAttempts),
                        ThreePointsGoals = awayTeamParticipants.Sum(x => x.ThreePointsGoals),
                        TotalFieldGoals = awayTeamParticipants.Sum(x => x.TotalFieldGoals),
                        TotalFieldGoalsAttempts = awayTeamParticipants.Sum(x => x.TotalFieldGoalsAttempts),
                        TotalRebounds = awayTeamParticipants.Sum(x => x.DefensiveRebounds) + awayTeamParticipants.Sum(x => x.OffensiveRebounds),
                        Turnovers = awayTeamParticipants.Sum(x => x.Turnovers),
                        TwoPointsAttempts = awayTeamParticipants.Sum(x => x.TwoPointsAttempts),
                        TwoPointsGoals = awayTeamParticipants.Sum(x => x.TwoPointsGoals),
                    };

                    match.HomeTeam.Statistic = homeTeamStatistic;
                    match.AwayTeam.Statistic = awayTeamStatistic;
                }

                matches.Add(match);
            }
        }

        var json = JsonSerializer.Serialize(matches);
        var result = new Result
        {
            EventId = eventCacheModel.Id,
            Json = json
        };

        await this.resultsService.AddOrUpdateAsync(result);
    }

    private async Task<List<BasketballParticipant>> ExtractBasketball3x3ParticipantsAsync(string html, EventCacheModel eventCacheModel)
    {
        var document = new HtmlDocument();
        document.LoadHtml(html);
        var trNodes = document.DocumentNode.SelectNodes("//tr");

        var participants = new List<BasketballParticipant>();
        for (int i = 1; i < trNodes.Count - 1; i++)
        {
            var tdDocument = new HtmlDocument();
            tdDocument.LoadHtml(trNodes[i].OuterHtml);
            var tdNodes = tdDocument.DocumentNode.SelectNodes("//td");

            var olympediaNumber = this.OlympediaService.FindAthleteNumber(tdNodes[2].OuterHtml);
            var dbParticipant = await this.participantsService.GetAsync(olympediaNumber, eventCacheModel.Id);
            var onePointMatch = this.RegExpService.Match(tdNodes[9].InnerText, @"(\d+)\/(\d+)");
            var twoPointMatch = this.RegExpService.Match(tdNodes[11].InnerText, @"(\d+)\/(\d+)");
            var freeThrowPointMatch = this.RegExpService.Match(tdNodes[15].InnerText, @"(\d+)\/(\d+)");

            var participant = new BasketballParticipant
            {
                ParticipantId = dbParticipant.Id,
                Position = tdNodes[0]?.InnerText.Trim(),
                Number = this.RegExpService.MatchInt(tdNodes[1]?.InnerText),
                Points = this.RegExpService.MatchInt(tdNodes[4]?.InnerText),
                TimePlayed = this.RegExpService.MatchTime(tdNodes[5]?.InnerText),
                PlayerValue = this.RegExpService.MatchDouble(tdNodes[6].InnerText),
                PlusMinus = this.RegExpService.MatchInt(tdNodes[7]?.InnerText.Replace("+", string.Empty)),
                ShootingEfficiency = this.RegExpService.MatchDouble(tdNodes[8]?.InnerText),
                TwoPointsGoals = this.RegExpService.MatchInt(onePointMatch?.Groups[1].Value),
                TwoPointsAttempts = this.RegExpService.MatchInt(onePointMatch?.Groups[2].Value),
                ThreePointsGoals = this.RegExpService.MatchInt(twoPointMatch?.Groups[1].Value),
                ThreePointsAttempts = this.RegExpService.MatchInt(twoPointMatch?.Groups[2].Value),
                FreeThrowsGoals = this.RegExpService.MatchInt(freeThrowPointMatch?.Groups[1].Value),
                FreeThrowsAttempts = this.RegExpService.MatchInt(freeThrowPointMatch?.Groups[2].Value),
                OffensiveRebounds = this.RegExpService.MatchInt(tdNodes[18]?.InnerText),
                DefensiveRebounds = this.RegExpService.MatchInt(tdNodes[19]?.InnerText),
                BlockedShots = this.RegExpService.MatchInt(tdNodes[20]?.InnerText),
                Turnovers = this.RegExpService.MatchInt(tdNodes[21]?.InnerText)
            };

            participant.TotalFieldGoals = participant.TwoPointsGoals + participant.ThreePointsGoals;
            participant.TotalFieldGoalsAttempts = participant.TwoPointsAttempts + participant.ThreePointsAttempts;
            participant.TotalRebounds = participant.OffensiveRebounds + participant.DefensiveRebounds;

            participants.Add(participant);
        }

        return participants;
    }

    private async Task ProcessBasketballAsync(HtmlDocument document, IOrderedEnumerable<Document> documents, GameCacheModel gameCacheModel, DisciplineCacheModel disciplineCacheModel,
        EventCacheModel eventCacheModel, TableModel standingTable, IList<TableModel> tables)
    {
        var matches = new List<BasketballMatch>();
        foreach (var table in tables)
        {
            var trRows = table.HtmlDocument.DocumentNode.SelectNodes("//tr").Where(x => this.OlympediaService.IsMatchNumber(x.InnerText)).ToList();
            foreach (var trRow in trRows)
            {
                var trDocument = new HtmlDocument();
                trDocument.LoadHtml(trRow.OuterHtml);

                var tdNodes = trDocument.DocumentNode.SelectNodes("//td");
                var homeTeamCode = tdNodes[2].OuterHtml;
                var resultString = tdNodes[3].InnerText;
                var awayTeamCode = tdNodes[4].OuterHtml;

                if (gameCacheModel.Year >= 2020)
                {
                    homeTeamCode = tdNodes[3].OuterHtml;
                    resultString = tdNodes[4].InnerText;
                    awayTeamCode = tdNodes[6].OuterHtml;
                }

                var resultModel = this.OlympediaService.GetResult(resultString);
                var homeNOCCacheModel = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == this.OlympediaService.FindCountryCode(homeTeamCode));
                var homeTeam = await this.teamsService.GetAsync(homeNOCCacheModel.Id, eventCacheModel.Id);

                var match = new BasketballMatch
                {
                    MatchNumber = this.OlympediaService.FindMatchNumber(tdNodes[0].InnerText),
                    Round = table.Round == RoundType.FinalRound ? this.NormalizeService.MapFinalRoundMatch(tdNodes[0].InnerText) : table.Round,
                    Date = this.dateService.MatchDate(tdNodes[1].InnerText, gameCacheModel.Year),
                    ResultId = this.OlympediaService.FindResultNumber(tdNodes[0].OuterHtml),
                    Decision = resultModel.Decision,
                    HomeTeam = new BasketballTeam
                    {
                        Name = homeTeam.Name,
                        TeamId = homeTeam.Id,
                        Result = resultModel.HomeTeamResult,
                        Points = resultModel.HomeTeamPoints
                    }
                };

                if (match.Decision == DecisionType.None)
                {
                    var awayNOCCacheModel = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == this.OlympediaService.FindCountryCode(awayTeamCode));
                    var awayTeam = await this.teamsService.GetAsync(awayNOCCacheModel.Id, eventCacheModel.Id);

                    match.AwayTeam = new BasketballTeam
                    {
                        Name = awayTeam.Name,
                        TeamId = awayTeam.Id,
                        Result = resultModel.AwayTeamResult,
                        Points = resultModel.AwayTeamPoints
                    };

                    var matchDocument = documents.FirstOrDefault(x => x.Url.EndsWith($"{match.ResultId}"));
                    if (matchDocument != null)
                    {
                        var htmlDocument = this.CreateHtmlDocument(matchDocument);
                        // TODO
                        //var firstReferee = this.RegExpService.MatchFirstGroup(htmlDocument.ParsedText, @"<th>Referee #1<\/th>(?:.*?)\/athletes\/(\d+)");
                        //var secondReferee = this.RegExpService.MatchFirstGroup(htmlDocument.ParsedText, @"<th>Referee #2<\/th>(?:.*?)\/athletes\/(\d+)");
                        //match.FirstRefereeId = await this.ExtractRefereeAsync(firstReferee);
                        //match.SecondRefereeId = await this.ExtractRefereeAsync(secondReferee);

                        var teamTables = htmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']").ToList();
                        if (gameCacheModel.Year >= 2020)
                        {
                            teamTables = htmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']").Skip(1).ToList();
                        }
                        //var homeTeamParticipants = await this.ExtractBasketballParticipantsAsync(teamTables[0].OuterHtml, eventCacheModel);
                        //var awayTeamParticipants = await this.ExtractBasketballParticipantsAsync(teamTables[1].OuterHtml, eventCacheModel);

                        //match.HomeTeam.Participants = homeTeamParticipants;
                        //match.AwayTeam.Participants = awayTeamParticipants;

                        //var homeTeamStatistic = new BasketballStatistic
                        //{
                        //    Assists = homeTeamParticipants.Sum(x => x.Assists),
                        //    BlockedShots = homeTeamParticipants.Sum(x => x.BlockedShots),
                        //    DefensiveRebounds = homeTeamParticipants.Sum(x => x.DefensiveRebounds),
                        //    DisqualifyingFouls = homeTeamParticipants.Sum(x => x.DisqualifyingFouls),
                        //    FreeThrowsAttempts = homeTeamParticipants.Sum(x => x.FreeThrowsAttempts),
                        //    FreeThrowsGoals = homeTeamParticipants.Sum(x => x.FreeThrowsGoals),
                        //    OffensiveRebounds = homeTeamParticipants.Sum(x => x.OffensiveRebounds),
                        //    PersonalFouls = homeTeamParticipants.Sum(x => x.PersonalFouls),
                        //    PlusMinus = homeTeamParticipants.Sum(x => x.PlusMinus),
                        //    Steals = homeTeamParticipants.Sum(x => x.Steals),
                        //    ThreePointsAttempts = homeTeamParticipants.Sum(x => x.ThreePointsAttempts),
                        //    ThreePointsGoals = homeTeamParticipants.Sum(x => x.ThreePointsGoals),
                        //    TotalFieldGoals = homeTeamParticipants.Sum(x => x.TotalFieldGoals),
                        //    TotalFieldGoalsAttempts = homeTeamParticipants.Sum(x => x.TotalFieldGoalsAttempts),
                        //    TotalRebounds = homeTeamParticipants.Sum(x => x.DefensiveRebounds) + homeTeamParticipants.Sum(x => x.OffensiveRebounds),
                        //    Turnovers = homeTeamParticipants.Sum(x => x.Turnovers),
                        //    TwoPointsAttempts = homeTeamParticipants.Sum(x => x.TwoPointsAttempts),
                        //    TwoPointsGoals = homeTeamParticipants.Sum(x => x.TwoPointsGoals),
                        //};

                        //var awayTeamStatistic = new BasketballStatistic
                        //{
                        //    Assists = awayTeamParticipants.Sum(x => x.Assists),
                        //    BlockedShots = awayTeamParticipants.Sum(x => x.BlockedShots),
                        //    DefensiveRebounds = awayTeamParticipants.Sum(x => x.DefensiveRebounds),
                        //    DisqualifyingFouls = awayTeamParticipants.Sum(x => x.DisqualifyingFouls),
                        //    FreeThrowsAttempts = awayTeamParticipants.Sum(x => x.FreeThrowsAttempts),
                        //    FreeThrowsGoals = awayTeamParticipants.Sum(x => x.FreeThrowsGoals),
                        //    OffensiveRebounds = awayTeamParticipants.Sum(x => x.OffensiveRebounds),
                        //    PersonalFouls = awayTeamParticipants.Sum(x => x.PersonalFouls),
                        //    PlusMinus = awayTeamParticipants.Sum(x => x.PlusMinus),
                        //    Steals = awayTeamParticipants.Sum(x => x.Steals),
                        //    ThreePointsAttempts = awayTeamParticipants.Sum(x => x.ThreePointsAttempts),
                        //    ThreePointsGoals = awayTeamParticipants.Sum(x => x.ThreePointsGoals),
                        //    TotalFieldGoals = awayTeamParticipants.Sum(x => x.TotalFieldGoals),
                        //    TotalFieldGoalsAttempts = awayTeamParticipants.Sum(x => x.TotalFieldGoalsAttempts),
                        //    TotalRebounds = awayTeamParticipants.Sum(x => x.DefensiveRebounds) + awayTeamParticipants.Sum(x => x.OffensiveRebounds),
                        //    Turnovers = awayTeamParticipants.Sum(x => x.Turnovers),
                        //    TwoPointsAttempts = awayTeamParticipants.Sum(x => x.TwoPointsAttempts),
                        //    TwoPointsGoals = awayTeamParticipants.Sum(x => x.TwoPointsGoals),
                        //};

                        //match.HomeTeam.Statistic = homeTeamStatistic;
                        //match.AwayTeam.Statistic = awayTeamStatistic;
                    }
                }

                matches.Add(match);
            }
        }

        //var json = JsonSerializer.Serialize(matches);
        //var result = new Result
        //{
        //    EventId = eventCacheModel.Id,
        //    Json = json
        //};

        //await this.resultsService.AddOrUpdateAsync(result);
    }

    private async Task<List<BasketballParticipant>> ExtractBasketballParticipantsAsync(string html, EventCacheModel eventCacheModel)
    {
        var document = new HtmlDocument();
        document.LoadHtml(html);
        var trNodes = document.DocumentNode.SelectNodes("//tr");

        var participants = new List<BasketballParticipant>();
        for (int i = 1; i < trNodes.Count - 1; i++)
        {
            var tdDocument = new HtmlDocument();
            tdDocument.LoadHtml(trNodes[i].OuterHtml);
            var tdNodes = tdDocument.DocumentNode.SelectNodes("//td");

            var olympediaNumber = this.OlympediaService.FindAthleteNumber(tdNodes[2].OuterHtml);
            var dbParticipant = await this.participantsService.GetAsync(olympediaNumber, eventCacheModel.Id);
            var onePointMatch = this.RegExpService.Match(tdNodes[9].InnerText, @"(\d+)\/(\d+)");
            var twoPointMatch = this.RegExpService.Match(tdNodes[11].InnerText, @"(\d+)\/(\d+)");
            var freeThrowPointMatch = this.RegExpService.Match(tdNodes[15].InnerText, @"(\d+)\/(\d+)");

            var participant = new BasketballParticipant
            {
                ParticipantId = dbParticipant.Id,
                Position = tdNodes[0]?.InnerText.Trim(),
                Number = this.RegExpService.MatchInt(tdNodes[1]?.InnerText),
                Points = this.RegExpService.MatchInt(tdNodes[4]?.InnerText),
                TimePlayed = this.RegExpService.MatchTime(tdNodes[5]?.InnerText),
                PlayerValue = this.RegExpService.MatchDouble(tdNodes[6].InnerText),
                PlusMinus = this.RegExpService.MatchInt(tdNodes[7]?.InnerText.Replace("+", string.Empty)),
                ShootingEfficiency = this.RegExpService.MatchDouble(tdNodes[8]?.InnerText),
                TwoPointsGoals = this.RegExpService.MatchInt(onePointMatch?.Groups[1].Value),
                TwoPointsAttempts = this.RegExpService.MatchInt(onePointMatch?.Groups[2].Value),
                ThreePointsGoals = this.RegExpService.MatchInt(twoPointMatch?.Groups[1].Value),
                ThreePointsAttempts = this.RegExpService.MatchInt(twoPointMatch?.Groups[2].Value),
                FreeThrowsGoals = this.RegExpService.MatchInt(freeThrowPointMatch?.Groups[1].Value),
                FreeThrowsAttempts = this.RegExpService.MatchInt(freeThrowPointMatch?.Groups[2].Value),
                OffensiveRebounds = this.RegExpService.MatchInt(tdNodes[18]?.InnerText),
                DefensiveRebounds = this.RegExpService.MatchInt(tdNodes[19]?.InnerText),
                BlockedShots = this.RegExpService.MatchInt(tdNodes[20]?.InnerText),
                Turnovers = this.RegExpService.MatchInt(tdNodes[21]?.InnerText)
            };

            participant.TotalFieldGoals = participant.TwoPointsGoals + participant.ThreePointsGoals;
            participant.TotalFieldGoalsAttempts = participant.TwoPointsAttempts + participant.ThreePointsAttempts;
            participant.TotalRebounds = participant.OffensiveRebounds + participant.DefensiveRebounds;

            participants.Add(participant);
        }

        return participants;
    }
    #endregion BASKETBALL
}