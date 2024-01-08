namespace SportData.Converters.OlympicGames;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using AutoMapper;

using HtmlAgilityPack;

using Microsoft.Extensions.Logging;

using SportData.Common.Constants;
using SportData.Data.Entities.Crawlers;
using SportData.Data.Entities.Enumerations;
using SportData.Data.Entities.OlympicGames;
using SportData.Data.Entities.OlympicGames.Enumerations;
using SportData.Data.Models.Cache;
using SportData.Data.Models.Converters;
using SportData.Data.Models.OlympicGames;
using SportData.Data.Models.OlympicGames.Disciplines;
using SportData.Services.Data.CrawlerStorageDb.Interfaces;
using SportData.Services.Data.OlympicGamesDb.Interfaces;
using SportData.Services.Interfaces;

using Document = Data.Entities.Crawlers.Document;

public class ResultConverter : BaseOlympediaConverter
{
    private readonly IDateService dateService;
    private readonly ITeamsService teamsService;
    private readonly IAthletesService athletesService;
    private readonly IParticipantsService participantsService;
    private readonly IResultsService resultsService;
    private readonly IMapper mapper;

    public ResultConverter(ILogger<BaseConverter> logger, ICrawlersService crawlersService, ILogsService logsService, IGroupsService groupsService, IZipService zipService,
        IRegExpService regExpService, INormalizeService normalizeService, IDataCacheService dataCacheService, IOlympediaService olympediaService, IDateService dateService,
        ITeamsService teamsService, IAthletesService athletesService, IParticipantsService participantsService, IResultsService resultsService, IMapper mapper)
        : base(logger, crawlersService, logsService, groupsService, zipService, regExpService, normalizeService, dataCacheService, olympediaService)
    {
        this.dateService = dateService;
        this.teamsService = teamsService;
        this.athletesService = athletesService;
        this.participantsService = participantsService;
        this.resultsService = resultsService;
        this.mapper = mapper;
    }

    protected override async Task ProcessGroupAsync(Group group)
    {
        try
        {
            var htmlDocument = this.CreateHtmlDocument(group.Documents.Single(x => x.Order == 1));
            var documents = group.Documents.Where(x => x.Order != 1).OrderBy(x => x.Order);
            var originalEventName = htmlDocument.DocumentNode.SelectSingleNode("//ol[@class='breadcrumb']/li[@class='active']").InnerText;
            var gameCacheModel = this.FindGame(htmlDocument);
            var disciplineCacheModel = this.FindDiscipline(htmlDocument);
            var eventModel = this.CreateEventModel(originalEventName, gameCacheModel, disciplineCacheModel);
            if (eventModel != null)
            {
                var eventCacheModel = this.DataCacheService
                    .EventCacheModels
                    .FirstOrDefault(x => x.OriginalName == eventModel.OriginalName && x.GameId == eventModel.GameId && x.DisciplineId == eventModel.DisciplineId);

                if (eventCacheModel != null)
                {
                    var tables = this.ExtractTables(htmlDocument, eventCacheModel, disciplineCacheModel, gameCacheModel);
                    var documentModels = this.ExtractDocuments(documents, eventCacheModel);

                    var options = new ConvertOptions
                    {
                        Discipline = disciplineCacheModel,
                        Event = eventCacheModel,
                        Game = gameCacheModel,
                        HtmlDocument = htmlDocument,
                        Tables = tables,
                        Documents = documentModels
                    };

                    switch (disciplineCacheModel.Name)
                    {
                        case DisciplineConstants.BASKETBALL_3X3:
                            await this.ProcessBasketball3X3Async(options);
                            break;
                        case DisciplineConstants.ALPINE_SKIING:
                            await this.ProcessAlpineSkiingAsync(options);
                            break;
                        case DisciplineConstants.ARCHERY:
                            await this.ProcessArcheryAsync(options);
                            break;
                        case DisciplineConstants.ARTISTIC_GYMNASTICS:
                            // TODO: Judges
                            await this.ProcessArtisticGymnasticsAsync(options);
                            break;
                        case DisciplineConstants.ARTISTIC_SWIMMING:
                            await this.ProcessArtisticSwimmingAsync(options);
                            break;
                        case DisciplineConstants.ATHLETICS:
                            await this.ProcessAthleticsAsync(options);
                            break;
                            //case DisciplineConstants.BADMINTON:
                            //    await this.ProcessBadmintonAsync(options);
                            //    break;
                            //case DisciplineConstants.BASEBALL:
                            //    await this.ProcessBaseballAsync(options);
                            //    break;
                            //case DisciplineConstants.BASKETBALL:
                            //    await this.ProcessBasketballAsync(options);
                            //    break;
                            //case DisciplineConstants.BASQUE_PELOTA:
                            //    await this.ProcessBasquePelotaAsync(options);
                            //    break;
                            //case DisciplineConstants.BEACH_VOLLEYBALL:
                            //    await this.ProcessBeachVolleyballAsync(options);
                            //    break;
                            //case DisciplineConstants.BIATHLON:
                            //    await this.ProcessBiathlonAsync(options);
                            //    break;
                            //case DisciplineConstants.BOBSLEIGH:
                            //    await this.ProcessBobsleighAsync(options);
                            //    break;
                            //case DisciplineConstants.BOXING:
                            //    await this.ProcessBoxingAsync(options);
                            //    break;
                            //case DisciplineConstants.CANOE_SLALOM:
                            //    await this.ProcessCanoeSlalomAsync(options);
                            //    break;
                            //case DisciplineConstants.CANOE_SPRINT:
                            //    await this.ProcessCanoeSprintAsync(options);
                            //    break;
                            //case DisciplineConstants.CRICKET:
                            //    await this.ProcessCricketAsync(options);
                            //    break;
                            //case DisciplineConstants.CROSS_COUNTRY_SKIING:
                            //    await this.ProcessCrossCountrySkiing(options);
                            //    break;
                            //case DisciplineConstants.CURLING:
                            //    await this.ProcessCurlingSkiing(options);
                            //    break;
                            //case DisciplineConstants.CYCLING_BMX_FREESTYLE:
                            //    await this.ProcessCyclingBMXFreestyleAsync(options);
                            //    break;
                            //case DisciplineConstants.CYCLING_BMX_RACING:
                            //    await this.ProcessCyclingBMXRacingAsync(options);
                            //    break;
                            //case DisciplineConstants.CYCLING_MOUNTAIN_BIKE:
                            //    await this.ProcessCyclingMountainBikeAsync(options);
                            //    break;
                            //case DisciplineConstants.CYCLING_ROAD:
                            //    await this.ProcessCyclingRoadAsync(options);
                            //    break;
                            //case DisciplineConstants.CYCLING_TRACK:
                            //    await this.ProcessCyclingTrackAsync(options);
                            //    break;
                            //case DisciplineConstants.DIVING:
                            //    await this.ProcessDivingAsync(options);
                            //    break;
                            //case DisciplineConstants.EQUESTRIAN_DRESSAGE:
                            //    TP comment in INDEX_POINTS
                            //    await this.ProcessEquestrianDressage(options);
                            //    break;
                            //case DisciplineConstants.EQUESTRIAN_DRIVING:
                            //    await this.ProcessEquestrianDriving(options);
                            //    break;
                            //case DisciplineConstants.EQUESTRIAN_EVENTING:
                            //    // TODO: Documents are not processed.
                            //    await this.ProcessEquestrianEventing(options);
                            //    break;
                            //case DisciplineConstants.EQUESTRIAN_JUMPING:
                            //    await this.ProcessEquestrianJumping(options);
                            //    break;
                            //case DisciplineConstants.EQUESTRIAN_VAULTING:
                            //    await this.ProcessEquestrianVaulting(options);
                            //    break;
                            //case DisciplineConstants.FENCING:
                            //    await this.ProcessFencingAsync(options);
                            //    break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, $"Failed to process group: {group.Identifier}");
        }
    }

    #region PRIVATE
    private List<DocumentModel> ExtractDocuments(IOrderedEnumerable<Document> documents, EventCacheModel eventCache)
    {
        var result = new List<DocumentModel>();
        foreach (var document in documents)
        {
            var htmlDocument = this.CreateHtmlDocument(document);
            var title = htmlDocument.DocumentNode.SelectSingleNode("//h1").InnerText;
            title = title.Replace(eventCache.OriginalName, string.Empty).Replace("–", string.Empty).Trim();
            var dateString = this.RegExpService.MatchFirstGroup(htmlDocument.DocumentNode.OuterHtml, @"<th>\s*Date\s*<\/th>\s*<td>(.*?)<\/td>");
            var dateModel = this.dateService.ParseDate(dateString);
            var format = this.RegExpService.MatchFirstGroup(htmlDocument.DocumentNode.OuterHtml, @"<th>Format<\/th>\s*<td(?:.*?)>(.*?)<\/td>");
            var id = int.Parse(new Uri(document.Url).Segments.Last());

            var documentModel = new DocumentModel
            {
                Id = id,
                Title = title,
                Html = htmlDocument.ParsedText,
                HtmlDocument = htmlDocument,
                From = dateModel.From,
                To = dateModel.To,
                Round = this.NormalizeService.MapRound(title),
                Format = format
            };

            var tables = htmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']");
            if (tables != null)
            {
                var order = 1;
                foreach (var table in tables)
                {
                    var tableModel = new TableModel
                    {
                        Html = table.OuterHtml,
                        Order = order++,
                    };

                    this.ExtractRows(tableModel);
                    documentModel.Tables.Add(tableModel);
                }
            }

            result.Add(documentModel);
        }

        return result;
    }

    private List<TableModel> ExtractTables(HtmlDocument htmlDocument, EventCacheModel eventCache, DisciplineCacheModel disciplineCache, GameCacheModel gameCache)
    {
        var standingTableHtml = htmlDocument.DocumentNode.SelectSingleNode("//table[@class='table table-striped']")?.OuterHtml;
        var format = this.RegExpService.MatchFirstGroup(htmlDocument.DocumentNode.OuterHtml, @"<th>Format<\/th>\s*<td(?:.*?)>(.*?)<\/td>");
        var dateString = this.RegExpService.MatchFirstGroup(htmlDocument.DocumentNode.OuterHtml, @"<th>\s*Date\s*<\/th>\s*<td>(.*?)<\/td>");
        var dateModel = this.dateService.ParseDate(dateString);

        var tables = new List<TableModel>();
        var order = 1;
        var roundTableModel = new TableModel
        {
            OriginalHtml = standingTableHtml,
            Html = standingTableHtml,
            EventId = eventCache.Id,
            Order = order++,
            Title = "Standing",
            Format = format,
            From = dateModel.From,
            To = dateModel.To
        };

        this.ExtractRows(roundTableModel);
        tables.Add(roundTableModel);

        var html = htmlDocument.ParsedText;
        html = html.Replace("<table class=\"biodata\"></table>", string.Empty);

        var rounds = this.RegExpService.Matches(html, @"<h2>(.*?)<\/h2>");
        if (rounds.Any())
        {
            var matches = this.RegExpService.Matches(html, @"<h2>(.*?)<\/h2>");
            if (matches.Any())
            {
                for (int i = 0; i < matches.Count; i++)
                {
                    var pattern = $@"{matches[i].Value}#$%";
                    if (i + 1 != matches.Count)
                    {
                        pattern = $@"{matches[i].Value}@$#$@{matches[i + 1].Value}";
                    }

                    pattern = pattern.Replace("(", @"\(").Replace(")", @"\)").Replace("/", @"\/").Replace("#$%", "(.*)").Replace("@$#$@", "(.*?)");
                    var match = this.RegExpService.Match(html, pattern);
                    if (match.Success)
                    {
                        var title = this.RegExpService.MatchFirstGroup(match.Groups[0].Value, "<h2>(.*?)</h2>");
                        title = this.RegExpService.CutHtml(title);
                        dateString = this.RegExpService.MatchFirstGroup(title, @"\((.*)\)");
                        var date = this.dateService.ParseDate(dateString, gameCache.Year);
                        format = this.RegExpService.MatchFirstGroup(match.Groups[0].Value, @"<i>(.*?)<\/i>");
                        title = this.RegExpService.CutHtml(title, @"\((.*)\)")?.Trim();

                        var table = new TableModel
                        {
                            Order = order++,
                            EventId = eventCache.Id,
                            OriginalHtml = match.Groups[0].Value,
                            From = date.From,
                            To = date.To,
                            Title = title,
                            Format = format,
                            Html = $"<table>{match.Groups[2].Value}</table>",
                            Round = this.NormalizeService.MapRound(title),
                        };

                        var groupMatches = this.RegExpService.Matches(table.OriginalHtml, @"<h3>(.*?)<table class=""table table-striped"">(.*?)<\/table>");
                        if (groupMatches.Any())
                        {
                            groupMatches.ToList().ForEach(x =>
                            {
                                var groupTitle = this.RegExpService.MatchFirstGroup(x.Groups[0].Value, "<h3>(.*?)<");
                                var group = this.NormalizeService.MapGroup(groupTitle, x.Groups[0].Value);
                                table.Groups.Add(group);
                            });
                        }

                        if (table.Round != null)
                        {
                            this.ExtractRows(table);
                            tables.Add(table);
                        }
                    }
                }
            }
        }

        return tables;
    }

    private void ExtractRows(TableModel round)
    {
        var rows = round.HtmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']//tr");
        if (round.Groups.Any())
        {
            foreach (var group in round.Groups)
            {
                rows = group.HtmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']//tr");
                var headers = rows.First().Elements("th").Where(x => this.OlympediaService.FindAthlete(x.OuterHtml) == null).Select(x => x.InnerText).ToList();
                group.Headers = headers;
                group.Rows = rows;
                group.Indexes = this.OlympediaService.GetIndexes(headers);
            }
        }
        else
        {
            if (rows == null)
            {
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(round.OriginalHtml);
                rows = htmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']//tr");
            }

            if (rows == null)
            {
                return;
            }

            var headers = rows.First().Elements("th").Where(x => this.OlympediaService.FindAthlete(x.OuterHtml) == null).Select(x => x.InnerText).ToList();
            round.Headers = headers;
            round.Rows = rows;
            round.Indexes = this.OlympediaService.GetIndexes(headers);
        }
    }

    private async Task<MatchModel> GetMatchAsync(MatchInputModel input)
    {
        var match = new MatchModel();

        if (!string.IsNullOrEmpty(input.Number))
        {
            match.Number = this.OlympediaService.FindMatchNumber(input.Number);
        }

        if (!string.IsNullOrEmpty(input.Location))
        {
            match.Location = input.Location;
        }

        if (!string.IsNullOrEmpty(input.Date))
        {
            var dateModel = this.dateService.ParseDate(input.Date, input.Year);
            match.Date = dateModel.From;
        }

        match.Decision = this.OlympediaService.FindDecision(input.Row);
        match.Info = this.OlympediaService.FindMatchInfo(input.Number);
        match.ResultId = this.OlympediaService.FindResultNumber(input.Number);
        match.Medal = this.OlympediaService.FindMedal(input.Number, input.Round);

        if (input.IsTeam)
        {
            var homeTeamNOCCode = this.OlympediaService.FindNOCCode(input.HomeNOC);
            var homeTeamNOC = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == homeTeamNOCCode);
            var homeTeam = await this.teamsService.GetAsync(homeTeamNOC.Id, input.EventId);
            homeTeam ??= await this.teamsService.GetAsync(input.HomeName, homeTeamNOC.Id, input.EventId);

            match.Team1.Id = homeTeam.Id;
            match.Team1.Name = homeTeam.Name;
            match.Team1.NOC = homeTeamNOCCode;

            if (match.Decision == DecisionType.None)
            {
                var awayTeamNOCCode = this.OlympediaService.FindNOCCode(input.AwayNOC);
                if (awayTeamNOCCode != null)
                {
                    var awayTeamNOC = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == awayTeamNOCCode);
                    var awayTeam = await this.teamsService.GetAsync(awayTeamNOC.Id, input.EventId);
                    awayTeam ??= await this.teamsService.GetAsync(input.AwayName, awayTeamNOC.Id, input.EventId);

                    match.Team2.Id = awayTeam.Id;
                    match.Team2.Name = awayTeam.Name;
                    match.Team2.NOC = awayTeamNOCCode;
                }
            }
        }
        else
        {
            var homeAthleteModel = this.OlympediaService.FindAthlete(input.HomeName);
            var homeAthleteNOCCode = this.OlympediaService.FindNOCCode(input.HomeNOC);
            var homeAthleteNOC = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == homeAthleteNOCCode);
            var homeAthlete = await this.participantsService.GetAsync(homeAthleteModel.Code, input.EventId);
            homeAthlete ??= await this.participantsService.GetAsync(homeAthleteModel.Code, input.EventId, homeAthleteNOC.Id);

            match.Team1.Id = homeAthlete.Id;
            match.Team1.Name = homeAthleteModel.Name;
            match.Team1.NOC = homeAthleteNOCCode;
            match.Team1.Code = homeAthleteModel.Code;

            if (match.Decision == DecisionType.None)
            {
                var awayAthleteModel = this.OlympediaService.FindAthlete(input.AwayName);
                var awayAthleteNOCCode = this.OlympediaService.FindNOCCode(input.AwayNOC);
                var awayAthleteNOC = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == awayAthleteNOCCode);
                var awayAthlete = await this.participantsService.GetAsync(awayAthleteModel.Code, input.EventId);
                awayAthlete ??= await this.participantsService.GetAsync(awayAthleteModel.Code, input.EventId, awayAthleteNOC.Id);

                match.Team2.Id = awayAthlete.Id;
                match.Team2.Name = awayAthleteModel.Name;
                match.Team2.NOC = awayAthleteNOCCode;
                match.Team2.Code = awayAthleteModel.Code;
            }
        }

        if (match.Decision == DecisionType.None && match.Team2.NOC != null)
        {
            input.Result = input.Result.Replace("[", string.Empty).Replace("]", string.Empty);

            if (input.AnyParts)
            {
                // TODO
                //        var match = this.regExpService.Match(text, @"(\d+)\s*-\s*(\d+)\s*,\s*(\d+)\s*-\s*(\d+)\s*,\s*(\d+)\s*-\s*(\d+)");
                //        if (match != null)
                //        {
                //            result.Games1 = new List<int?> { int.Parse(match.Groups[1].Value), int.Parse(match.Groups[3].Value), int.Parse(match.Groups[5].Value) };
                //            result.Games2 = new List<int?> { int.Parse(match.Groups[2].Value), int.Parse(match.Groups[4].Value), int.Parse(match.Groups[6].Value) };

                //            var points = result.Games1[0].Value > result.Games2[0].Value ? result.Points1++ : result.Points2++;
                //            points = result.Games1[1].Value > result.Games2[1].Value ? result.Points1++ : result.Points2++;
                //            points = result.Games1[2].Value > result.Games2[2].Value ? result.Points1++ : result.Points2++;

                //            this.SetWinAndLose(result);
                //            return result;
                //        }
                //        match = this.regExpService.Match(text, @"(\d+)\s*-\s*(\d+)\s*,\s*(\d+)\s*-\s*(\d+)");
                //        if (match != null)
                //        {
                //            result.Games1 = new List<int?> { int.Parse(match.Groups[1].Value), int.Parse(match.Groups[3].Value) };
                //            result.Games2 = new List<int?> { int.Parse(match.Groups[2].Value), int.Parse(match.Groups[4].Value) };

                //            var points = result.Games1[0].Value > result.Games2[0].Value ? result.Points1++ : result.Points2++;
                //            points = result.Games1[1].Value > result.Games2[1].Value ? result.Points1++ : result.Points2++;

                //            this.SetWinAndLose(result);
                //            return result;
                //        }
                //        match = this.regExpService.Match(text, @"(\d+)\s*-\s*(\d+)");
                //        if (match != null)
                //        {
                //            result.Games1 = new List<int?> { int.Parse(match.Groups[1].Value) };
                //            result.Games2 = new List<int?> { int.Parse(match.Groups[2].Value) };

                //            var points = result.Games1[0].Value > result.Games2[0].Value ? result.Points1++ : result.Points2++;

                //            result.Result1 = ResultType.Win;
                //            result.Result2 = ResultType.Lose;

                //            return result;
                //        }
            }
            else
            {
                var regexMatch = this.RegExpService.Match(input.Result, @"(\d+)\s*(?:-|–|—)\s*(\d+)");
                if (regexMatch != null)
                {
                    match.Team1.Points = int.Parse(regexMatch.Groups[1].Value.Trim());
                    match.Team2.Points = int.Parse(regexMatch.Groups[2].Value.Trim());

                    this.OlympediaService.SetWinAndLose(match);
                }

                regexMatch = this.RegExpService.Match(input.Result, @"(\d+)\.(\d+)\s*(?:-|–|—)\s*(\d+)\.(\d+)");
                if (regexMatch != null)
                {
                    match.Team1.Time = this.dateService.ParseTime($"{regexMatch.Groups[1].Value}.{regexMatch.Groups[2].Value}");
                    match.Team2.Time = this.dateService.ParseTime($"{regexMatch.Groups[3].Value}.{regexMatch.Groups[4].Value}");

                    if (match.Team1.Time < match.Team2.Time)
                    {
                        match.Team1.MatchResult = MatchResultType.Win;
                        match.Team2.MatchResult = MatchResultType.Lose;
                    }
                    else if (match.Team1.Time > match.Team2.Time)
                    {
                        match.Team1.MatchResult = MatchResultType.Lose;
                        match.Team2.MatchResult = MatchResultType.Win;
                    }
                }

                regexMatch = this.RegExpService.Match(input.Result, @"(\d+)\.(\d+)\s*(?:-|–|—)\s*DNF");
                if (regexMatch != null)
                {
                    match.Team1.Time = this.dateService.ParseTime($"{regexMatch.Groups[1].Value}.{regexMatch.Groups[2].Value}");
                    match.Team1.MatchResult = MatchResultType.Win;
                    match.Team2.MatchResult = MatchResultType.Lose;
                }

                regexMatch = this.RegExpService.Match(input.Result, @"DNF\s*(?:-|–|—)\s*(\d+)\.(\d+)");
                if (regexMatch != null)
                {
                    match.Team2.Time = this.dateService.ParseTime($"{regexMatch.Groups[1].Value}.{regexMatch.Groups[2].Value}");
                    match.Team2.MatchResult = MatchResultType.Win;
                    match.Team1.MatchResult = MatchResultType.Lose;
                }
            }
        }

        return match;
    }

    private async Task<List<Judge>> GetJudgesAsync(string html)
    {
        var matches = this.RegExpService.Matches(html, @"<tr class=""(?:referees|hidden_referees)""(?:.*?)<th>(.*?)<\/th>(.*?)<\/tr>");
        var judges = new List<Judge>();
        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            var judgeMatch = this.RegExpService.Match(html, @$"<th>{match.Groups[1].Value}<\/th>(.*)");
            if (judgeMatch != null)
            {
                var athleteModel = this.OlympediaService.FindAthlete(judgeMatch.Groups[1].Value);
                var nocCode = this.OlympediaService.FindNOCCode(judgeMatch.Groups[1].Value);
                var nocCodeCache = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == nocCode);
                if (nocCodeCache != null)
                {
                    var athlete = await this.athletesService.GetAsync(athleteModel.Code);

                    var judge = new Judge
                    {
                        Id = athlete == null ? Guid.Empty : athlete.Id,
                        Code = athleteModel.Code,
                        Name = athleteModel.Name,
                        NOC = nocCode,
                        Title = $"{match.Groups[1].Value}"
                    };

                    judges.Add(judge);
                }
            }
        }

        judges.RemoveAll(x => x == null);
        return judges;
    }

    private int? GetInt(Dictionary<string, int> indexes, string name, List<HtmlNode> nodes)
    {
        return indexes.TryGetValue(name, out int value) ? this.RegExpService.MatchInt(nodes[value].InnerText) : null;
    }

    private double? GetDouble(Dictionary<string, int> indexes, string name, List<HtmlNode> nodes)
    {
        return indexes.TryGetValue(name, out int value) ? this.RegExpService.MatchDouble(nodes[value].InnerText) : null;
    }

    private string GetString(Dictionary<string, int> indexes, string name, List<HtmlNode> nodes)
    {
        var data = indexes.TryGetValue(name, out int value) ? nodes[value].InnerText : null;
        if (string.IsNullOrEmpty(data))
        {
            return null;
        }

        return data;
    }

    private TimeSpan? GetTime(Dictionary<string, int> indexes, string name, List<HtmlNode> nodes)
    {
        return indexes.TryGetValue(name, out int value) ? this.dateService.ParseTime(nodes[value].InnerText) : null;
    }

    private Round<TModel> CreateRound<TModel>(DateTime? from, DateTime? to, string format, RoundModel roundModel, string eventName, Track track)
    {
        return new Round<TModel>
        {
            FromDate = from,
            ToDate = to,
            Format = format,
            EventName = eventName,
            RoundModel = roundModel,
            Track = track
        };
    }

    private async Task ProcessJsonAsync<TModel>(List<Round<TModel>> rounds, ConvertOptions options)
    {
        var resultModel = new Result<TModel>
        {
            Event = new Data.Models.OlympicGames.Event
            {
                Id = options.Event.Id,
                Gender = options.Event.Gender,
                IsTeamEvent = options.Event.IsTeamEvent,
                Name = options.Event.Name,
                NormalizedName = options.Event.NormalizedName,
                OriginalName = options.Event.OriginalName
            },
            Discipline = new Data.Models.OlympicGames.Discipline
            {
                Id = options.Discipline.Id,
                Name = options.Discipline.Name
            },
            Game = new Data.Models.OlympicGames.Game
            {
                Id = options.Game.Id,
                Type = options.Game.Type,
                Year = options.Game.Year
            },
            Rounds = rounds
        };

        var json = JsonSerializer.Serialize(resultModel, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });

        var result = new Result
        {
            EventId = options.Event.Id,
            Json = json
        };

        //await this.resultsService.AddOrUpdateAsync(result);
    }
    #endregion PRIVATE

    #region ATHLETICS
    private async Task ProcessAthleticsAsync(ConvertOptions options)
    {
        var eventType = this.MapAthleticsEventType(options.Event.Name);

        switch (eventType)
        {
            case AthleticsEventType.TrackEvents:
                break;
            case AthleticsEventType.RoadEvents:
                break;
            case AthleticsEventType.FieldEvents:
                break;
            case AthleticsEventType.CombinedEvents:
                break;
            case AthleticsEventType.CrossCountryEvent:
                break;
        }
    }

    private AthleticsEventType MapAthleticsEventType(string name)
    {
        switch (name)
        {
            case "Men 10000m":
            case "Men 100m":
            case "Men 10km Race Walk":
            case "Men 10miles Race Walk":
            case "Men 110m Hurdles":
            case "Men 1500m":
            case "Men 1600m Medley Relay":
            case "Men 200m":
            case "Men 200m Hurdles":
            case "Men 2500m Steeplechase":
            case "Men 2590m Steeplechase":
            case "Men 3000m Race Walk":
            case "Men 3000m Steeplechase":
            case "Men 3200m Steeplechase":
            case "Men 3500m Race Walk":
            case "Men 4000m Steeplechase":
            case "Men 400m":
            case "Men 400m Hurdles":
            case "Men 4x100m Relay":
            case "Men 4x400m Relay":
            case "Men 5000m":
            case "Men 5miles":
            case "Men 60m":
            case "Men 800m":
            case "Men Team 3000m":
            case "Men Team 3miles":
            case "Men Team 4miles":
            case "Men Team 5000m":
            case "Mixed 4x400m Relay":
            case "Women 10000m":
            case "Women 100m":
            case "Women 100m Hurdles":
            case "Women 10km Race Walk":
            case "Women 1500m":
            case "Women 200m":
            case "Women 3000m":
            case "Women 3000m Steeplechase":
            case "Women 400m":
            case "Women 400m Hurdles":
            case "Women 4x100m Relay":
            case "Women 4x400m Relay":
            case "Women 5000m":
            case "Women 800m":
            case "Women 80m Hurdles":
                return AthleticsEventType.TrackEvents;
            case "Men 56-pound Weight Throw":
            case "Men Discus Throw":
            case "Men Discus Throw Both Hands":
            case "Men Discus Throw Greek Style":
            case "Men Hammer Throw":
            case "Men High Jump":
            case "Men Javelin Throw":
            case "Men Javelin Throw Both Hands":
            case "Men Javelin Throw Freestyle":
            case "Men Long Jump":
            case "Men Pole Vault":
            case "Men Shot Put":
            case "Men Shot Put Both Hands":
            case "Men Standing High Jump":
            case "Men Standing Long Jump":
            case "Men Standing Triple Jump":
            case "Men Triple Jump":
            case "Women Discus Throw":
            case "Women Hammer Throw":
            case "Women High Jump":
            case "Women Javelin Throw":
            case "Women Long Jump":
            case "Women Pole Vault":
            case "Women Shot Put":
            case "Women Triple Jump":
                return AthleticsEventType.FieldEvents;
            case "Men 20km Race Walk":
            case "Men 50km Race Walk":
            case "Men Marathon":
            case "Women 20km Race Walk":
            case "Women Marathon":
                return AthleticsEventType.RoadEvents;
            case "Men All-Around Championship":
            case "Men Decathlon":
            case "Men Pentathlon":
            case "Women Heptathlon":
            case "Women Pentathlon":
                return AthleticsEventType.CombinedEvents;
            case "Men Individual Cross-Country":
            case "Men Team Cross-Country":
                return AthleticsEventType.CrossCountryEvent;
            default:
                return AthleticsEventType.None;
        }
    }
    #endregion ATHLETICS

    #region ARTISTIC SWIMMING
    private async Task ProcessArtisticSwimmingAsync(ConvertOptions options)
    {
        var rounds = new List<Round<ArtisticSwimming>>();

        if (options.Game.Year == 2020)
        {
            ;
        }
        if (options.Tables.Count == 1)
        {
            var table = options.Tables.FirstOrDefault();
            var round = this.CreateRound<ArtisticSwimming>(table.From, table.To, table.Format, new RoundModel { Type = RoundType.FinalRound }, options.Event.Name, null);
            await this.SetArtisticSwimmingTeamsAsync(round, table, options.Event.Id, options.Event.IsTeamEvent);
            rounds.Add(round);
        }
        else
        {
            foreach (var table in options.Tables.Skip(1))
            {
                var round = this.CreateRound<ArtisticSwimming>(table.From, table.To, table.Format, table.Round, options.Event.Name, null);
                await this.SetArtisticSwimmingTeamsAsync(round, table, options.Event.Id, options.Event.IsTeamEvent);
                rounds.Add(round);
            }
        }

        foreach (var document in options.Documents)
        {
            var round = this.CreateRound<ArtisticSwimming>(document.From, document.To, document.Format, document.Round, options.Event.Name, null);
            await this.SetArtisticSwimmingTeamsAsync(round, document.Tables.FirstOrDefault(), options.Event.Id, options.Event.IsTeamEvent);
            var judges = await this.GetJudgesAsync(document.Html);
            round.Judges = judges;

            if (round.Teams.Count == rounds.FirstOrDefault(x => x.RoundModel.Type == RoundType.Qualification)?.Teams?.Count)
            {
                round.RoundModel.Type = RoundType.Qualification;
                round.RoundModel.SubType = document.Round.Type;
            }
            if (round.Teams.Count == rounds.FirstOrDefault(x => x.RoundModel.Type == RoundType.FinalRound || x.RoundModel.Type == RoundType.Final)?.Teams?.Count)
            {
                round.RoundModel.Type = RoundType.FinalRound;
                round.RoundModel.SubType = document.Round.Type;
            }
            rounds.Add(round);
        }

        await this.ProcessJsonAsync(rounds, options);
    }

    private async Task SetArtisticSwimmingTeamsAsync(Round<ArtisticSwimming> round, TableModel table, int eventId, bool isTeamEvent)
    {
        ArtisticSwimming team = null;

        foreach (var row in table.Rows.Skip(1))
        {
            var noc = this.OlympediaService.FindNOCCode(row.OuterHtml);
            var data = row.Elements("td").ToList();

            if (isTeamEvent)
            {
                var athleteModels = this.OlympediaService.FindAthletes(row.OuterHtml);

                if (noc != null)
                {
                    var teamName = data[table.Indexes[ConverterConstants.Name]].InnerText;
                    var nocCache = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == noc);
                    var dbTeam = await this.teamsService.GetAsync(teamName, nocCache.Id, eventId);
                    dbTeam ??= await this.teamsService.GetAsync(nocCache.Id, eventId);

                    if (dbTeam != null)
                    {
                        team = new ArtisticSwimming
                        {
                            Id = dbTeam.Id,
                            Name = dbTeam.Name,
                            NOC = noc,
                            FinishStatus = this.OlympediaService.FindStatus(row.OuterHtml),
                            Qualification = this.OlympediaService.FindQualification(row.OuterHtml),
                            Points = this.GetDouble(table.Indexes, ConverterConstants.Points, data),
                            FigurePoints = this.GetDouble(table.Indexes, ConverterConstants.Figures, data),
                            MusicalRoutinePoints = this.GetDouble(table.Indexes, ConverterConstants.MusicalRoutinePoints, data),
                            FreeRoutinePoints = this.GetDouble(table.Indexes, ConverterConstants.FreeRoutine, data),
                            TechnicalRoutinePoints = this.GetDouble(table.Indexes, ConverterConstants.TechnicalRoutine, data),
                            ArtisticImpression = this.GetDouble(table.Indexes, ConverterConstants.ArtisticImpression, data),
                            ArtisticImpressionChoreography = this.GetDouble(table.Indexes, ConverterConstants.ArtisticImpressionChoreographyPoints, data),
                            ArtisticImpressionJudge1 = this.GetDouble(table.Indexes, ConverterConstants.ArtisticImpressionJudge1Points, data),
                            ArtisticImpressionJudge2 = this.GetDouble(table.Indexes, ConverterConstants.ArtisticImpressionJudge2Points, data),
                            ArtisticImpressionJudge3 = this.GetDouble(table.Indexes, ConverterConstants.ArtisticImpressionJudge3Points, data),
                            ArtisticImpressionJudge4 = this.GetDouble(table.Indexes, ConverterConstants.ArtisticImpressionJudge4Points, data),
                            ArtisticImpressionJudge5 = this.GetDouble(table.Indexes, ConverterConstants.ArtisticImpressionJudge5Points, data),
                            ArtisticImpressionMusicInterpretation = this.GetDouble(table.Indexes, ConverterConstants.ArtisticImpressionMusicInterpretationPoints, data),
                            ArtisticImpressionMannerOfPresentation = this.GetDouble(table.Indexes, ConverterConstants.ArtisticImpressionMannerofPresentationPoints, data),
                            Difficulty = this.GetDouble(table.Indexes, ConverterConstants.Difficulty, data),
                            DifficultyJudge1 = this.GetDouble(table.Indexes, ConverterConstants.DifficultyJudge1, data),
                            DifficultyJudge2 = this.GetDouble(table.Indexes, ConverterConstants.DifficultyJudge2, data),
                            DifficultyJudge3 = this.GetDouble(table.Indexes, ConverterConstants.DifficultyJudge3, data),
                            DifficultyJudge4 = this.GetDouble(table.Indexes, ConverterConstants.DifficultyJudge4, data),
                            DifficultyJudge5 = this.GetDouble(table.Indexes, ConverterConstants.DifficultyJudge5, data),
                            Execution = this.GetDouble(table.Indexes, ConverterConstants.DifficultyJudge5, data),
                            ExecutionJudge1 = this.GetDouble(table.Indexes, ConverterConstants.ExecutionJudge1, data),
                            ExecutionJudge2 = this.GetDouble(table.Indexes, ConverterConstants.ExecutionJudge2, data),
                            ExecutionJudge3 = this.GetDouble(table.Indexes, ConverterConstants.ExecutionJudge3, data),
                            ExecutionJudge4 = this.GetDouble(table.Indexes, ConverterConstants.ExecutionJudge4, data),
                            ExecutionJudge5 = this.GetDouble(table.Indexes, ConverterConstants.ExecutionJudge5, data),
                            ExecutionJudge6 = this.GetDouble(table.Indexes, ConverterConstants.ExecutionJudge6, data),
                            ExecutionJudge7 = this.GetDouble(table.Indexes, ConverterConstants.ExecutionJudge7, data),
                            OverallImpression = this.GetDouble(table.Indexes, ConverterConstants.OverallImpression, data),
                            OverallImpressionJudge1 = this.GetDouble(table.Indexes, ConverterConstants.OverallImpressionJudge1, data),
                            OverallImpressionJudge2 = this.GetDouble(table.Indexes, ConverterConstants.OverallImpressionJudge2, data),
                            OverallImpressionJudge3 = this.GetDouble(table.Indexes, ConverterConstants.OverallImpressionJudge3, data),
                            OverallImpressionJudge4 = this.GetDouble(table.Indexes, ConverterConstants.OverallImpressionJudge4, data),
                            OverallImpressionJudge5 = this.GetDouble(table.Indexes, ConverterConstants.OverallImpressionJudge5, data),
                            OverallImpressionJudge6 = this.GetDouble(table.Indexes, ConverterConstants.OverallImpressionJudge6, data),
                            OverallImpressionJudge7 = this.GetDouble(table.Indexes, ConverterConstants.OverallImpressionJudge7, data),
                            Penalties = this.GetDouble(table.Indexes, ConverterConstants.Penalties, data),
                            RequiredElementPenalty = this.GetDouble(table.Indexes, ConverterConstants.RequiredElementPenalty, data),
                            Routine1Points = this.GetDouble(table.Indexes, ConverterConstants.Routine1, data),
                            Routine2Points = this.GetDouble(table.Indexes, ConverterConstants.Routine2, data),
                            Routine3Points = this.GetDouble(table.Indexes, ConverterConstants.Routine3, data),
                            Routine4Points = this.GetDouble(table.Indexes, ConverterConstants.Routine4, data),
                            Routine5Points = this.GetDouble(table.Indexes, ConverterConstants.Routine5, data),
                            Routine1DegreeOfDifficulty = this.GetDouble(table.Indexes, ConverterConstants.Routine1DegreeOfDifficulty, data),
                            Routine2DegreeOfDifficulty = this.GetDouble(table.Indexes, ConverterConstants.Routine2DegreeOfDifficulty, data),
                            Routine3DegreeOfDifficulty = this.GetDouble(table.Indexes, ConverterConstants.Routine3DegreeOfDifficulty, data),
                            Routine4DegreeOfDifficulty = this.GetDouble(table.Indexes, ConverterConstants.Routine4DegreeOfDifficulty, data),
                            Routine5DegreeOfDifficulty = this.GetDouble(table.Indexes, ConverterConstants.Routine5DegreeOfDifficulty, data),
                            ReducedPoints = this.GetDouble(table.Indexes, ConverterConstants.ReducedPoints, data),
                            TechnicalMerit = this.GetDouble(table.Indexes, ConverterConstants.TechnicalMerit, data),
                            TechnicalMeritDifficulty = this.GetDouble(table.Indexes, ConverterConstants.TechnicalMeritDifficultyPoints, data),
                            TechnicalMeritExecution = this.GetDouble(table.Indexes, ConverterConstants.TechnicalMeritExecutionPoints, data),
                            TechnicalMeritSynchronization = this.GetDouble(table.Indexes, ConverterConstants.TechnicalMeritSynchronizationPoints, data),
                        };

                        team.Execution ??= this.GetDouble(table.Indexes, ConverterConstants.ExecutionPoints, data);

                        if (athleteModels.Count != 0)
                        {
                            foreach (var athleteModel in athleteModels)
                            {
                                var participant = await this.participantsService.GetAsync(athleteModel.Code, eventId);
                                var athlete = new ArtisticSwimming
                                {
                                    Id = participant != null ? participant.Id : Guid.Empty,
                                    Code = athleteModel.Code,
                                    Name = athleteModel.Name,
                                    NOC = noc,
                                    FinishStatus = this.OlympediaService.FindStatus(row.OuterHtml),
                                    Qualification = this.OlympediaService.FindQualification(row.OuterHtml),
                                };

                                team.Athletes.Add(athlete);
                            }
                        }

                        round.Teams.Add(team);
                    }
                }
                else
                {
                    foreach (var athleteModel in athleteModels)
                    {
                        var participant = await this.participantsService.GetAsync(athleteModel.Code, eventId);
                        var athlete = new ArtisticSwimming
                        {
                            Id = participant != null ? participant.Id : Guid.Empty,
                            Code = athleteModel.Code,
                            Name = athleteModel.Name,
                            NOC = noc,
                            FinishStatus = this.OlympediaService.FindStatus(row.OuterHtml),
                            Qualification = this.OlympediaService.FindQualification(row.OuterHtml),
                        };

                        team.Athletes.Add(athlete);
                    }
                }
            }
            else
            {
                var athleteModel = this.OlympediaService.FindAthlete(row.OuterHtml);
                var participant = await this.participantsService.GetAsync(athleteModel.Code, eventId);
                var athlete = new ArtisticSwimming
                {
                    Id = participant != null ? participant.Id : Guid.Empty,
                    Code = athleteModel.Code,
                    Name = athleteModel.Name,
                    NOC = noc,
                    FinishStatus = this.OlympediaService.FindStatus(row.OuterHtml),
                    Qualification = this.OlympediaService.FindQualification(row.OuterHtml),
                    Points = this.GetDouble(table.Indexes, ConverterConstants.Points, data),
                    FigurePoints = this.GetDouble(table.Indexes, ConverterConstants.Figures, data),
                    MusicalRoutinePoints = this.GetDouble(table.Indexes, ConverterConstants.MusicalRoutinePoints, data),
                    FreeRoutinePoints = this.GetDouble(table.Indexes, ConverterConstants.FreeRoutine, data),
                    TechnicalRoutinePoints = this.GetDouble(table.Indexes, ConverterConstants.TechnicalRoutine, data),
                };

                round.Athletes.Add(athlete);
            }
        }
    }
    #endregion ARTISTIC SWIMMING

    #region ARTISTIC GYMNASTICS
    private async Task ProcessArtisticGymnasticsAsync(ConvertOptions options)
    {
        var rounds = new List<Round<ArtisticGymnastics>>();
        if (options.Tables.Count == 1)
        {
            var table = options.Tables.FirstOrDefault();
            var round = this.CreateRound<ArtisticGymnastics>(table.From, table.To, table.Format, new RoundModel { Type = RoundType.FinalRound }, options.Event.Name, null);
            if (options.Event.IsTeamEvent)
            {
                await this.SetArtisticGymnasticsTeamsAsync(round, table, options.Event.Id, options.Game.Year, false, null);
            }
            else
            {
                await this.SetArtisticGymnasticsAthletesAsync(round, table, options.Event.Id, options.Game.Year, false, null);
            }
            rounds.Add(round);
        }
        else
        {
            if (options.Event.IsTeamEvent)
            {
                if (options.Game.Year <= 1996)
                {
                    var firstTable = options.Tables.FirstOrDefault();
                    var round = this.CreateRound<ArtisticGymnastics>(firstTable.From, firstTable.To, firstTable.Format, new RoundModel { Type = RoundType.FinalRound }, options.Event.Name, null);
                    await this.SetArtisticGymnasticsTeamsAsync(round, firstTable, options.Event.Id, options.Game.Year, false, null);

                    foreach (var table in options.Tables.Skip(1))
                    {
                        await this.SetArtisticGymnasticsTeamsAsync(round, table, options.Event.Id, options.Game.Year, true, table.Title);
                    }
                    rounds.Add(round);
                }
                else
                {
                    Round<ArtisticGymnastics> round = null;
                    foreach (var table in options.Tables.Skip(1))
                    {
                        round = this.CreateRound<ArtisticGymnastics>(table.From, table.To, table.Format, table.Round, options.Event.Name, null);
                        await this.SetArtisticGymnasticsTeamsAsync(round, table, options.Event.Id, options.Game.Year, false, null);
                        rounds.Add(round);
                    }

                    foreach (var document in options.Documents)
                    {
                        var parts = document.Title.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        var type = RoundType.Final;
                        if (parts[0].Trim() == "Qualification" || parts[0].Trim() == "Qualifying")
                        {
                            type = RoundType.Qualification;
                        }

                        round = rounds.FirstOrDefault(x => x.RoundModel.Type == type);
                        await this.SetArtisticGymnasticsTeamsAsync(round, document.Tables.FirstOrDefault(), options.Event.Id, options.Game.Year, true, parts[1].Trim());
                    }
                }
            }
            else
            {
                var firstTable = options.Tables.FirstOrDefault();
                var round = this.CreateRound<ArtisticGymnastics>(firstTable.From, firstTable.To, firstTable.Format, new RoundModel { Type = RoundType.FinalRound }, options.Event.Name, null);

                if (options.Game.Year <= 1932)
                {
                    await this.SetArtisticGymnasticsAthletesAsync(round, firstTable, options.Event.Id, options.Game.Year, false, null);
                    foreach (var table in options.Tables.Skip(1))
                    {
                        await this.SetArtisticGymnasticsAthletesAsync(round, table, options.Event.Id, options.Game.Year, true, table.Title);
                    }
                    rounds.Add(round);
                }
                else
                {
                    foreach (var table in options.Tables.Skip(1))
                    {
                        round = this.CreateRound<ArtisticGymnastics>(table.From, table.To, table.Format, table.Round, options.Event.Name, null);
                        await this.SetArtisticGymnasticsAthletesAsync(round, table, options.Event.Id, options.Game.Year, false, null);
                        rounds.Add(round);
                    }

                    foreach (var document in options.Documents)
                    {
                        var parts = document.Title.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        var type = RoundType.Final;
                        if (parts[0].Trim() == "Qualification" || parts[0].Trim() == "Qualifying")
                        {
                            type = RoundType.Qualification;
                        }

                        round = rounds.FirstOrDefault(x => x.RoundModel.Type == type);
                        await this.SetArtisticGymnasticsAthletesAsync(round, document.Tables.FirstOrDefault(), options.Event.Id, options.Game.Year, true, parts[1].Trim());
                    }
                }
            }
        }

        await this.ProcessJsonAsync(rounds, options);
    }

    private void SetArtisticGymnasticsScores(ArtisticGymnastics team, TableModel table, List<HtmlNode> data)
    {
        var floorExercise = this.GetDouble(table.Indexes, ConverterConstants.FloorExercise, data);
        if (floorExercise != null)
        {
            team.Scores.Add(new ArtisticGymnasticsScore { Points = floorExercise, Name = "Floor Exercise" });
        }

        var horseVault = this.GetDouble(table.Indexes, ConverterConstants.HorseVault, data);
        if (horseVault != null)
        {
            team.Scores.Add(new ArtisticGymnasticsScore { Points = horseVault, Name = "Horse Vault" });
        }

        var parallelBars = this.GetDouble(table.Indexes, ConverterConstants.ParallelBars, data);
        if (parallelBars != null)
        {
            team.Scores.Add(new ArtisticGymnasticsScore { Points = parallelBars, Name = "Parallel Bars" });
        }

        var horizontalBar = this.GetDouble(table.Indexes, ConverterConstants.HorizontalBar, data);
        if (horizontalBar != null)
        {
            team.Scores.Add(new ArtisticGymnasticsScore { Points = horizontalBar, Name = "Horizontal Bar" });
        }

        var rings = this.GetDouble(table.Indexes, ConverterConstants.Rings, data);
        if (rings != null)
        {
            team.Scores.Add(new ArtisticGymnasticsScore { Points = rings, Name = "Rings" });
        }

        var pommellHorse = this.GetDouble(table.Indexes, ConverterConstants.PommellHorse, data);
        if (pommellHorse != null)
        {
            team.Scores.Add(new ArtisticGymnasticsScore { Points = pommellHorse, Name = "Pommell Horse" });
        }

        var unevenBars = this.GetDouble(table.Indexes, ConverterConstants.UnevenBars, data);
        if (unevenBars != null)
        {
            team.Scores.Add(new ArtisticGymnasticsScore { Points = unevenBars, Name = "Uneven Bars" });
        }

        var balanceBeam = this.GetDouble(table.Indexes, ConverterConstants.BalanceBeam, data);
        if (balanceBeam != null)
        {
            team.Scores.Add(new ArtisticGymnasticsScore { Points = balanceBeam, Name = "Balance beam" });
        }
    }

    private async Task SetArtisticGymnasticsTeamsAsync(Round<ArtisticGymnastics> round, TableModel table, int eventId, int year, bool isScore, string scoreName)
    {
        ArtisticGymnastics team = null;
        string lastNOC = null;
        foreach (var row in table.Rows.Skip(1))
        {
            var noc = this.OlympediaService.FindNOCCode(row.OuterHtml);
            var data = row.Elements("td").ToList();
            if (noc != null)
            {
                lastNOC = noc;
                if (isScore)
                {
                    if (year < 2012)
                    {
                        var currentTeam = round.Teams.FirstOrDefault(x => x.NOC == noc);
                        currentTeam?.Scores.Add(new ArtisticGymnasticsScore
                        {
                            ApparatusPoints = this.GetDouble(table.Indexes, ConverterConstants.ApparatusPoints, data),
                            CompulsoryPoints = this.GetDouble(table.Indexes, ConverterConstants.CompulsaryPoints, data),
                            DrillPoints = this.GetDouble(table.Indexes, ConverterConstants.DrillPoints, data),
                            GroupExercisePoints = this.GetDouble(table.Indexes, ConverterConstants.GroupExercise, data),
                            Vault1 = this.GetDouble(table.Indexes, ConverterConstants.HorseVault, data),
                            HalfTeamPoints = this.GetDouble(table.Indexes, ConverterConstants.HalfTeamPoints, data),
                            OptionalPoints = this.GetDouble(table.Indexes, ConverterConstants.OptionalPoints, data),
                            LongJumpPoints = this.GetDouble(table.Indexes, ConverterConstants.LongJump, data),
                            PrecisionPoins = this.GetDouble(table.Indexes, ConverterConstants.PrecisionPoints, data),
                            Points = this.GetDouble(table.Indexes, ConverterConstants.Points, data),
                            ShotPutPoints = this.GetDouble(table.Indexes, ConverterConstants.ShotPut, data),
                            Yards100Points = this.GetDouble(table.Indexes, ConverterConstants.Y100, data),
                            Name = scoreName
                        });
                    }
                }
                else
                {
                    var teamName = data[table.Indexes[ConverterConstants.Name]].InnerText;
                    var nocCache = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == noc);
                    var dbTeam = await this.teamsService.GetAsync(teamName, nocCache.Id, eventId);
                    dbTeam ??= await this.teamsService.GetAsync(nocCache.Id, eventId);

                    team = new ArtisticGymnastics
                    {
                        Id = dbTeam.Id,
                        Name = dbTeam.Name,
                        NOC = noc,
                        FinishStatus = this.OlympediaService.FindStatus(row.OuterHtml),
                        Qualification = this.OlympediaService.FindQualification(row.OuterHtml),
                        ApparatusPoints = this.GetDouble(table.Indexes, ConverterConstants.ApparatusPoints, data),
                        CompulsoryPoints = this.GetDouble(table.Indexes, ConverterConstants.CompulsaryPoints, data),
                        DrillPoints = this.GetDouble(table.Indexes, ConverterConstants.DrillPoints, data),
                        GroupExercisePoints = this.GetDouble(table.Indexes, ConverterConstants.GroupExercise, data),
                        Vault1 = this.GetDouble(table.Indexes, ConverterConstants.HorseVault, data),
                        HalfTeamPoints = this.GetDouble(table.Indexes, ConverterConstants.HalfTeamPoints, data),
                        OptionalPoints = this.GetDouble(table.Indexes, ConverterConstants.OptionalPoints, data),
                        LongJumpPoints = this.GetDouble(table.Indexes, ConverterConstants.LongJump, data),
                        PrecisionPoins = this.GetDouble(table.Indexes, ConverterConstants.PrecisionPoints, data),
                        Points = this.GetDouble(table.Indexes, ConverterConstants.Points, data),
                        ShotPutPoints = this.GetDouble(table.Indexes, ConverterConstants.ShotPut, data),
                        Yards100Points = this.GetDouble(table.Indexes, ConverterConstants.Y100, data),
                    };

                    team.Points ??= this.GetDouble(table.Indexes, ConverterConstants.TeamPoints, data);

                    this.SetArtisticGymnasticsScores(team, table, data);

                    round.Teams.Add(team);
                }
            }
            else
            {
                var athleteModels = this.OlympediaService.FindAthletes(row.OuterHtml);
                if (athleteModels.Count > 0)
                {
                    foreach (var athleteModel in athleteModels)
                    {
                        var participant = await this.participantsService.GetAsync(athleteModel.Code, eventId);

                        if (isScore)
                        {
                            var currentTeam = round.Teams.FirstOrDefault(x => x.NOC == lastNOC);
                            var athlete = currentTeam.Athletes.FirstOrDefault(x => x.Code == athleteModel.Code);
                            athlete?.Scores.Add(new ArtisticGymnasticsScore
                            {
                                Points = this.GetDouble(table.Indexes, ConverterConstants.Points, data),
                                Time = this.GetDouble(table.Indexes, ConverterConstants.ShotPut, data),
                                DScore = this.GetDouble(table.Indexes, ConverterConstants.Dscore, data),
                                EScore = this.GetDouble(table.Indexes, ConverterConstants.Escore, data),
                                LinePenalty = this.GetDouble(table.Indexes, ConverterConstants.LinePenalty, data),
                                OtherPenalty = this.GetDouble(table.Indexes, ConverterConstants.OtherPenalty, data),
                                Penalty = this.GetDouble(table.Indexes, ConverterConstants.Penalty, data),
                                Name = scoreName
                            });
                        }
                        else
                        {
                            var athlete = new ArtisticGymnastics
                            {
                                Id = participant != null ? participant.Id : Guid.Empty,
                                Code = athleteModel.Code,
                                Name = athleteModel.Name,
                                NOC = team.NOC,
                                FinishStatus = this.OlympediaService.FindStatus(row.OuterHtml),
                                Qualification = this.OlympediaService.FindQualification(row.OuterHtml),
                            };

                            if (athleteModels.Count == 1)
                            {
                                athlete.IndividualPoints = this.GetDouble(table.Indexes, ConverterConstants.IndividualPoints, data);
                                athlete.ApparatusPoints = this.GetDouble(table.Indexes, ConverterConstants.ApparatusPoints, data);
                                athlete.CompulsoryPoints = this.GetDouble(table.Indexes, ConverterConstants.CompulsaryPoints, data);
                                athlete.DrillPoints = this.GetDouble(table.Indexes, ConverterConstants.DrillPoints, data);
                                athlete.GroupExercisePoints = this.GetDouble(table.Indexes, ConverterConstants.GroupExercise, data);
                                athlete.Vault1 = this.GetDouble(table.Indexes, ConverterConstants.HorseVault, data);
                                athlete.OptionalPoints = this.GetDouble(table.Indexes, ConverterConstants.OptionalPoints, data);
                                athlete.LongJumpPoints = this.GetDouble(table.Indexes, ConverterConstants.IndividualPoints, data);
                                athlete.PrecisionPoins = this.GetDouble(table.Indexes, ConverterConstants.PrecisionPoints, data);
                                athlete.Points = this.GetDouble(table.Indexes, ConverterConstants.Points, data);
                                athlete.ShotPutPoints = this.GetDouble(table.Indexes, ConverterConstants.ShotPut, data);
                                athlete.Yards100Points = this.GetDouble(table.Indexes, ConverterConstants.Y100, data);
                            }

                            if (year == 2008)
                            {
                                this.SetArtisticGymnasticsScores(athlete, table, data);
                            }

                            team.Athletes.Add(athlete);
                        }
                    }
                }
            }
        }
    }

    private async Task SetArtisticGymnasticsAthletesAsync(Round<ArtisticGymnastics> round, TableModel table, int eventId, int year, bool isScore, string scoreName)
    {
        foreach (var row in table.Rows.Skip(1))
        {
            var data = row.Elements("td").ToList();
            var athleteModel = this.OlympediaService.FindAthlete(data[table.Indexes[ConverterConstants.Name]].OuterHtml);

            if (isScore)
            {
                var athlete = round.Athletes.FirstOrDefault(x => x.Code == athleteModel.Code);
                athlete?.Scores.Add(new ArtisticGymnasticsScore
                {
                    Points = this.GetDouble(table.Indexes, ConverterConstants.Points, data),
                    CompulsoryPoints = this.GetDouble(table.Indexes, ConverterConstants.CompulsaryPoints, data),
                    OptionalPoints = this.GetDouble(table.Indexes, ConverterConstants.OptionalPoints, data),
                    Time = this.GetDouble(table.Indexes, ConverterConstants.Time, data),
                    Vault1 = this.GetDouble(table.Indexes, ConverterConstants.Vault1, data),
                    Vault2 = this.GetDouble(table.Indexes, ConverterConstants.Vault2, data),
                    Distance = this.GetDouble(table.Indexes, ConverterConstants.Distance, data),
                    DScore = this.GetDouble(table.Indexes, ConverterConstants.Dscore, data),
                    EScore = this.GetDouble(table.Indexes, ConverterConstants.Escore, data),
                    FinalPoints = this.GetDouble(table.Indexes, ConverterConstants.Final, data),
                    LinePenalty = this.GetDouble(table.Indexes, ConverterConstants.LinePenalty, data),
                    OtherPenalty = this.GetDouble(table.Indexes, ConverterConstants.OtherPenalty, data),
                    Penalty = this.GetDouble(table.Indexes, ConverterConstants.Penalty, data),
                    QualificationHalfPoints = this.GetDouble(table.Indexes, ConverterConstants.HalfQualificationPoints, data),
                    QualificationPoints = this.GetDouble(table.Indexes, ConverterConstants.Qualification, data),
                    TimePenalty = this.GetDouble(table.Indexes, ConverterConstants.TimePenalty, data),
                    Name = scoreName
                });
            }
            else
            {
                if (athleteModel != null)
                {
                    var participant = await this.participantsService.GetAsync(athleteModel.Code, eventId);
                    var noc = this.OlympediaService.FindNOCCode(data[table.Indexes[ConverterConstants.NOC]].OuterHtml);
                    var athlete = new ArtisticGymnastics
                    {
                        Id = participant != null ? participant.Id : Guid.Empty,
                        Name = athleteModel.Name,
                        NOC = noc,
                        Code = athleteModel.Code,
                        FinishStatus = this.OlympediaService.FindStatus(row.OuterHtml),
                        Qualification = this.OlympediaService.FindQualification(row.OuterHtml),
                        Points = this.GetDouble(table.Indexes, ConverterConstants.Points, data),
                        CompulsoryPoints = this.GetDouble(table.Indexes, ConverterConstants.CompulsaryPoints, data),
                        Height = this.GetDouble(table.Indexes, ConverterConstants.Height, data),
                        OptionalPoints = this.GetDouble(table.Indexes, ConverterConstants.OptionalPoints, data),
                        Time = this.GetDouble(table.Indexes, ConverterConstants.ShotPut, data),
                        Vault1 = this.GetDouble(table.Indexes, ConverterConstants.Vault1, data),
                        Vault2 = this.GetDouble(table.Indexes, ConverterConstants.Vault2, data),
                        VaultOff1 = this.GetDouble(table.Indexes, ConverterConstants.VaultOff1, data),
                        VaultOff2 = this.GetDouble(table.Indexes, ConverterConstants.VaultOff2, data),
                        FirstTimeTrial = this.GetDouble(table.Indexes, ConverterConstants.FirstTimeTrial, data),
                        SecondTimeTrial = this.GetDouble(table.Indexes, ConverterConstants.SecondTimeTrial, data),
                        ThirdTimeTrial = this.GetDouble(table.Indexes, ConverterConstants.ThirdTimeTrial, data),
                    };

                    if (year == 2008)
                    {
                        this.SetArtisticGymnasticsScores(athlete, table, data);
                    }

                    round.Athletes.Add(athlete);
                }
            }
        }
    }
    #endregion ARTISTIC GYMNASTICS

    #region ARCHERY
    private async Task ProcessArcheryAsync(ConvertOptions options)
    {
        var rounds = new List<Round<Archery>>();

        if (options.Tables.Count == 1)
        {
            var table = options.Tables.FirstOrDefault();
            var round = this.CreateRound<Archery>(table.From, table.To, table.Format, new RoundModel { Type = RoundType.FinalRound }, options.Event.Name, null);
            if (options.Event.IsTeamEvent)
            {
                await this.SetArcheryTeamsAsync(round, table, options.Event.Id);
            }
            else
            {
                await this.SetArcheryAthletesAsync(round, table, options.Event.Id);
            }
            rounds.Add(round);
        }
        else
        {
            foreach (var table in options.Tables.Skip(1))
            {
                if (options.Game.Year == 1988 || (options.Game.Year >= 1992 && table.Round.Type == RoundType.RankingRound))
                {
                    var round = this.CreateRound<Archery>(table.From, table.To, table.Format, table.Round, options.Event.Name, null);
                    if (options.Event.IsTeamEvent)
                    {
                        await this.SetArcheryTeamsAsync(round, table, options.Event.Id);
                    }
                    else
                    {
                        await this.SetArcheryAthletesAsync(round, table, options.Event.Id);
                    }
                    rounds.Add(round);
                }
                else
                {
                    var round = this.CreateRound<Archery>(table.From, table.To, table.Format, table.Round, options.Event.Name, null);
                    if (options.Event.IsTeamEvent)
                    {
                        await this.SetArcheryTeamMatchesAsync(round, table, options);
                    }
                    else
                    {
                        await this.SetArcheryAthleteMatchesAsync(round, table, options);
                    }
                    rounds.Add(round);
                }
            }
        }

        if (!options.Event.IsTeamEvent)
        {
            foreach (var document in options.Documents)
            {
                var roundModel = this.NormalizeService.MapRound(document.Title);
                if (roundModel != null)
                {
                    var round = this.CreateRound<Archery>(document.From, document.To, null, roundModel, options.Event.Name, null);
                    await this.SetArcheryAthletesAsync(round, document.Tables.FirstOrDefault(), options.Event.Id);
                    rounds.Add(round);
                }
            }
        }

        await this.ProcessJsonAsync(rounds, options);
    }

    private async Task SetArcheryTeamMatchesAsync(Round<Archery> round, TableModel table, ConvertOptions options)
    {
        foreach (var row in table.Rows.Skip(1))
        {
            var data = row.Elements("td").ToList();
            var matchInputModel = new MatchInputModel
            {
                Row = row.OuterHtml,
                Number = data[0].OuterHtml,
                Date = data[1].InnerText,
                Year = options.Game.Year,
                EventId = options.Event.Id,
                IsTeam = true,
                HomeName = data[2].OuterHtml,
                HomeNOC = data[3].OuterHtml,
                Result = data[4].InnerHtml,
                AwayName = data[5].OuterHtml,
                AwayNOC = data[6].OuterHtml,
                AnyParts = false,
                Round = table.Round.Type,
                Location = null
            };
            var matchModel = await this.GetMatchAsync(matchInputModel);
            var match = this.mapper.Map<TeamMatch<Archery>>(matchModel);

            var document = options.Documents.FirstOrDefault(x => x.Id == match.ResultId);
            if (document != null)
            {
                var firstTable = document.Tables.FirstOrDefault();
                if (firstTable != null)
                {
                    for (int i = 1; i < firstTable.Rows.Count; i++)
                    {
                        var firstTableData = firstTable.Rows[i].Elements("td").ToList();
                        var record = this.OlympediaService.FindRecord(firstTable.Rows[i].OuterHtml);
                        var sets = this.GetInt(firstTable.Indexes, ConverterConstants.Sets, firstTableData);
                        var set1 = this.GetInt(firstTable.Indexes, ConverterConstants.Set1, firstTableData);
                        var set2 = this.GetInt(firstTable.Indexes, ConverterConstants.Set2, firstTableData);
                        var set3 = this.GetInt(firstTable.Indexes, ConverterConstants.Set3, firstTableData);
                        var set4 = this.GetInt(firstTable.Indexes, ConverterConstants.Set4, firstTableData);
                        var set5 = this.GetInt(firstTable.Indexes, ConverterConstants.Set5, firstTableData);

                        if (i == 1)
                        {
                            match.Team1.Record = record;
                            match.Team1.Sets = sets;
                            match.Team1.Set1 = set1;
                            match.Team1.Set2 = set2;
                            match.Team1.Set3 = set3;
                            match.Team1.Set4 = set4;
                            match.Team1.Set5 = set5;
                        }
                        else
                        {
                            match.Team2.Record = record;
                            match.Team2.Sets = sets;
                            match.Team2.Set1 = set1;
                            match.Team2.Set2 = set2;
                            match.Team2.Set3 = set3;
                            match.Team2.Set4 = set4;
                            match.Team2.Set5 = set5;
                        }
                    }
                }

                var secondTable = document.Tables.ElementAtOrDefault(1);
                if (secondTable != null)
                {
                    foreach (var secondTableRow in secondTable.Rows.Skip(1))
                    {
                        var secondTableData = secondTableRow.Elements("td").ToList();
                        var athleteModel = this.OlympediaService.FindAthlete(secondTableRow.OuterHtml);
                        if (athleteModel != null)
                        {
                            var participant = await this.participantsService.GetAsync(athleteModel.Code, options.Event.Id);
                            var athlete = new Archery
                            {
                                Id = participant.Id,
                                Code = athleteModel.Code,
                                Name = athleteModel.Name,
                                NOC = match.Team1.NOC,
                                Target = this.GetString(secondTable.Indexes, ConverterConstants.Target, secondTableData),
                                ScoreXs = this.GetInt(secondTable.Indexes, ConverterConstants.Xs, secondTableData),
                                Score10s = this.GetInt(secondTable.Indexes, ConverterConstants.S10, secondTableData),
                                Points = this.GetInt(secondTable.Indexes, ConverterConstants.Points, secondTableData),
                                Tiebreak1 = this.GetInt(secondTable.Indexes, ConverterConstants.TieBreak1, secondTableData),
                                Tiebreak2 = this.GetInt(secondTable.Indexes, ConverterConstants.TieBreak2, secondTableData),
                                ShootOff = this.GetInt(secondTable.Indexes, ConverterConstants.ShootOff, secondTableData),
                            };

                            athlete.Points ??= this.GetInt(secondTable.Indexes, ConverterConstants.TotalPoints, secondTableData);
                            athlete.ShootOff ??= this.GetInt(secondTable.Indexes, ConverterConstants.ShootOffArrow, secondTableData);
                            athlete.Tiebreak1 ??= this.GetInt(secondTable.Indexes, ConverterConstants.TieBreak, secondTableData);

                            foreach (var kvp in secondTable.Indexes.Where(x => x.Key.StartsWith("Arrow")))
                            {
                                var arrowNumber = int.Parse(kvp.Key.Replace("Arrow", string.Empty).Trim());
                                var points = this.GetString(secondTable.Indexes, $"Arrow{arrowNumber}", secondTableData);
                                if (!string.IsNullOrEmpty(points))
                                {
                                    athlete.Arrows.Add(new Arrow { Number = arrowNumber, Points = (!string.IsNullOrEmpty(points) ? points : null) });
                                }
                            }

                            match.Team1.Athletes.Add(athlete);
                        }
                    }
                }

                var thirdTable = document.Tables.ElementAtOrDefault(2);
                if (thirdTable != null)
                {
                    foreach (var thirdTableRow in thirdTable.Rows.Skip(1))
                    {
                        var thirdTableData = thirdTableRow.Elements("td").ToList();
                        var athleteModel = this.OlympediaService.FindAthlete(thirdTableRow.OuterHtml);
                        if (athleteModel != null)
                        {
                            var participant = await this.participantsService.GetAsync(athleteModel.Code, options.Event.Id);
                            var athlete = new Archery
                            {
                                Id = participant.Id,
                                Code = athleteModel.Code,
                                Name = athleteModel.Name,
                                NOC = match.Team2.NOC,
                                Target = this.GetString(secondTable.Indexes, ConverterConstants.Target, thirdTableData),
                                ScoreXs = this.GetInt(secondTable.Indexes, ConverterConstants.Xs, thirdTableData),
                                Score10s = this.GetInt(secondTable.Indexes, ConverterConstants.S10, thirdTableData),
                                Points = this.GetInt(secondTable.Indexes, ConverterConstants.Points, thirdTableData),
                                Tiebreak1 = this.GetInt(secondTable.Indexes, ConverterConstants.TieBreak1, thirdTableData),
                                Tiebreak2 = this.GetInt(secondTable.Indexes, ConverterConstants.TieBreak2, thirdTableData),
                                ShootOff = this.GetInt(secondTable.Indexes, ConverterConstants.ShootOff, thirdTableData),
                            };

                            athlete.Points ??= this.GetInt(secondTable.Indexes, ConverterConstants.TotalPoints, thirdTableData);
                            athlete.ShootOff ??= this.GetInt(secondTable.Indexes, ConverterConstants.ShootOffArrow, thirdTableData);
                            athlete.Tiebreak1 ??= this.GetInt(secondTable.Indexes, ConverterConstants.TieBreak, thirdTableData);

                            foreach (var kvp in secondTable.Indexes.Where(x => x.Key.StartsWith("Arrow")))
                            {
                                var arrowNumber = int.Parse(kvp.Key.Replace("Arrow", string.Empty).Trim());
                                var points = this.GetString(secondTable.Indexes, $"Arrow{arrowNumber}", thirdTableData);
                                if (!string.IsNullOrEmpty(points))
                                {
                                    athlete.Arrows.Add(new Arrow { Number = arrowNumber, Points = (!string.IsNullOrEmpty(points) ? points : null) });
                                }
                            }

                            match.Team2.Athletes.Add(athlete);
                        }
                    }
                }
            }

            round.TeamMatches.Add(match);
        }
    }

    private async Task SetArcheryTeamsAsync(Round<Archery> round, TableModel table, int eventId)
    {
        Archery team = null;
        foreach (var row in table.Rows.Skip(1))
        {
            var noc = this.OlympediaService.FindNOCCode(row.OuterHtml);
            var data = row.Elements("td").ToList();
            if (noc != null)
            {
                var teamName = data[table.Indexes[ConverterConstants.Name]].InnerText;
                var nocCache = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == noc);
                var dbTeam = await this.teamsService.GetAsync(teamName, nocCache.Id, eventId);
                dbTeam ??= await this.teamsService.GetAsync(nocCache.Id, eventId);

                team = new Archery
                {
                    Id = dbTeam.Id,
                    Name = dbTeam.Name,
                    NOC = noc,
                    FinishStatus = this.OlympediaService.FindStatus(row.OuterHtml),
                    Record = this.OlympediaService.FindRecord(row.OuterHtml),
                    Points = this.GetInt(table.Indexes, ConverterConstants.Points, data),
                    Qualification = this.OlympediaService.FindQualification(row.OuterHtml),
                    TargetsHit = this.GetInt(table.Indexes, ConverterConstants.TargetsHit, data),
                    Score10s = this.GetInt(table.Indexes, ConverterConstants.S10, data),
                    Score9s = this.GetInt(table.Indexes, ConverterConstants.S9, data),
                    ScoreXs = this.GetInt(table.Indexes, ConverterConstants.Xs, data),
                    ShootOff = this.GetInt(table.Indexes, ConverterConstants.ShootOff, data),
                    Meters30 = this.GetInt(table.Indexes, ConverterConstants.M30, data),
                    Meters50 = this.GetInt(table.Indexes, ConverterConstants.M50, data),
                    Meters70 = this.GetInt(table.Indexes, ConverterConstants.M70, data),
                    Meters90 = this.GetInt(table.Indexes, ConverterConstants.M90, data),
                };

                team.Points ??= this.GetInt(table.Indexes, ConverterConstants.TeamPoints, data);

                round.Teams.Add(team);
            }
            else
            {
                var athleteModel = this.OlympediaService.FindAthlete(row.OuterHtml);
                if (athleteModel != null)
                {
                    var participant = await this.participantsService.GetAsync(athleteModel.Code, eventId);
                    var athlete = new Archery
                    {
                        Id = participant.Id,
                        Code = athleteModel.Code,
                        Name = athleteModel.Name,
                        NOC = team.NOC,
                        FinishStatus = this.OlympediaService.FindStatus(row.OuterHtml),
                        Record = this.OlympediaService.FindRecord(row.OuterHtml),
                        Points = this.GetInt(table.Indexes, ConverterConstants.Points, data),
                        Qualification = this.OlympediaService.FindQualification(row.OuterHtml),
                        TargetsHit = this.GetInt(table.Indexes, ConverterConstants.TargetsHit, data),
                        Score10s = this.GetInt(table.Indexes, ConverterConstants.S10, data),
                        Score9s = this.GetInt(table.Indexes, ConverterConstants.S9, data),
                        ScoreXs = this.GetInt(table.Indexes, ConverterConstants.Xs, data),
                        ShootOff = this.GetInt(table.Indexes, ConverterConstants.ShootOff, data),
                        Meters30 = this.GetInt(table.Indexes, ConverterConstants.M30, data),
                        Meters50 = this.GetInt(table.Indexes, ConverterConstants.M50, data),
                        Meters70 = this.GetInt(table.Indexes, ConverterConstants.M70, data),
                        Meters90 = this.GetInt(table.Indexes, ConverterConstants.M90, data),
                    };

                    athlete.Points ??= this.GetInt(table.Indexes, ConverterConstants.IndividualPoints, data);
                    athlete.Meters90 ??= this.GetInt(table.Indexes, ConverterConstants.M60, data);

                    team.Athletes.Add(athlete);
                }
            }
        }
    }

    private async Task SetArcheryAthleteMatchesAsync(Round<Archery> round, TableModel table, ConvertOptions options)
    {
        foreach (var row in table.Rows.Skip(1))
        {
            var data = row.Elements("td").ToList();
            var matchInputModel = new MatchInputModel
            {
                Row = row.OuterHtml,
                Number = data[0].OuterHtml,
                Date = data[1].InnerText,
                Year = options.Game.Year,
                EventId = options.Event.Id,
                IsTeam = false,
                HomeName = data[2].OuterHtml,
                HomeNOC = data[3].OuterHtml,
                Result = data[4].InnerHtml,
                AwayName = data[5].OuterHtml,
                AwayNOC = data[6].OuterHtml,
                AnyParts = false,
                Round = table.Round.Type,
                Location = null
            };
            var matchModel = await this.GetMatchAsync(matchInputModel);
            var match = this.mapper.Map<AthleteMatch<Archery>>(matchModel);

            var document = options.Documents.FirstOrDefault(x => x.Id == match.ResultId);
            if (document != null)
            {
                var firstTable = document.Tables.FirstOrDefault();
                if (firstTable != null)
                {
                    for (int i = 1; i < firstTable.Rows.Count; i++)
                    {
                        var firstTableData = firstTable.Rows[i].Elements("td").ToList();
                        var record = this.OlympediaService.FindRecord(firstTable.Rows[i].OuterHtml);
                        var sets = this.GetInt(firstTable.Indexes, ConverterConstants.Sets, firstTableData);
                        var set1 = this.GetInt(firstTable.Indexes, ConverterConstants.Set1, firstTableData);
                        var set2 = this.GetInt(firstTable.Indexes, ConverterConstants.Set2, firstTableData);
                        var set3 = this.GetInt(firstTable.Indexes, ConverterConstants.Set3, firstTableData);
                        var set4 = this.GetInt(firstTable.Indexes, ConverterConstants.Set4, firstTableData);
                        var set5 = this.GetInt(firstTable.Indexes, ConverterConstants.Set5, firstTableData);

                        if (i == 1)
                        {
                            match.Athlete1.Record = record;
                            match.Athlete1.Sets = sets;
                            match.Athlete1.Set1 = set1;
                            match.Athlete1.Set2 = set2;
                            match.Athlete1.Set3 = set3;
                            match.Athlete1.Set4 = set4;
                            match.Athlete1.Set5 = set5;
                        }
                        else
                        {
                            match.Athlete2.Record = record;
                            match.Athlete2.Sets = sets;
                            match.Athlete2.Set1 = set1;
                            match.Athlete2.Set2 = set2;
                            match.Athlete2.Set3 = set3;
                            match.Athlete2.Set4 = set4;
                            match.Athlete2.Set5 = set5;
                        }
                    }
                }

                var secondTable = document.Tables.LastOrDefault();
                if (secondTable != null)
                {
                    foreach (var secondTableRows in secondTable.Rows.Skip(1))
                    {
                        var header = secondTableRows.Element("th")?.InnerText;
                        var secondTableData = secondTableRows.Elements("td").ToList();

                        if (header != null)
                        {
                            if (header.StartsWith("Arrow"))
                            {
                                var arrowNumber = int.Parse(header.Replace("Arrow", string.Empty).Trim());
                                var points1 = secondTableData[0]?.InnerText.Replace("–", string.Empty);
                                var points2 = secondTableData[1]?.InnerText.Replace("–", string.Empty);
                                if (!string.IsNullOrEmpty(points1) && !string.IsNullOrEmpty(points2))
                                {
                                    match.Athlete1.Arrows.Add(new Arrow { Number = arrowNumber, Points = (!string.IsNullOrEmpty(points1) ? points1 : null) });
                                    match.Athlete2.Arrows.Add(new Arrow { Number = arrowNumber, Points = (!string.IsNullOrEmpty(points2) ? points2 : null) });
                                }
                            }
                            else
                            {
                                switch (header.Trim())
                                {
                                    case "Points":
                                        match.Athlete1.Points = this.RegExpService.MatchInt(secondTableData[0]?.InnerText);
                                        match.Athlete2.Points = this.RegExpService.MatchInt(secondTableData[1]?.InnerText);
                                        break;
                                    case "10s":
                                        match.Athlete1.Score10s = this.RegExpService.MatchInt(secondTableData[0]?.InnerText);
                                        match.Athlete2.Score10s = this.RegExpService.MatchInt(secondTableData[1]?.InnerText);
                                        break;
                                    case "Xs":
                                        match.Athlete1.ScoreXs = this.RegExpService.MatchInt(secondTableData[0]?.InnerText);
                                        match.Athlete2.ScoreXs = this.RegExpService.MatchInt(secondTableData[1]?.InnerText);
                                        break;
                                    case "Tie-Break":
                                    case "Tiebreak 1":
                                        match.Athlete1.Tiebreak1 = this.RegExpService.MatchInt(secondTableData[0]?.InnerText);
                                        match.Athlete2.Tiebreak1 = this.RegExpService.MatchInt(secondTableData[1]?.InnerText);
                                        break;
                                    case "Tiebreak 2":
                                        match.Athlete1.Tiebreak2 = this.RegExpService.MatchInt(secondTableData[0]?.InnerText);
                                        match.Athlete2.Tiebreak2 = this.RegExpService.MatchInt(secondTableData[1]?.InnerText);
                                        break;
                                    case "Total Points":
                                        match.Athlete1.Points = this.RegExpService.MatchInt(secondTableData[0]?.InnerText);
                                        match.Athlete2.Points = this.RegExpService.MatchInt(secondTableData[1]?.InnerText);
                                        break;
                                    case "Shoot-off":
                                    case "Shoot-Off Points":
                                        match.Athlete1.ShootOff = this.RegExpService.MatchInt(secondTableData[0]?.InnerText);
                                        match.Athlete2.ShootOff = this.RegExpService.MatchInt(secondTableData[1]?.InnerText);
                                        break;
                                }
                            }
                        }
                    }
                }
            }

            round.AthleteMatches.Add(match);
        }
    }

    private async Task SetArcheryAthletesAsync(Round<Archery> round, TableModel table, int eventId)
    {
        foreach (var row in table.Rows.Skip(1))
        {
            var data = row.Elements("td").ToList();
            var athleteModel = this.OlympediaService.FindAthlete(data[table.Indexes[ConverterConstants.Name]].OuterHtml);
            var participant = await this.participantsService.GetAsync(athleteModel.Code, eventId);
            var noc = this.OlympediaService.FindNOCCode(data[table.Indexes[ConverterConstants.NOC]].OuterHtml);
            var athlete = new Archery
            {
                Id = participant.Id,
                Name = athleteModel.Name,
                NOC = noc,
                Code = athleteModel.Code,
                FinishStatus = this.OlympediaService.FindStatus(row.OuterHtml),
                Qualification = this.OlympediaService.FindQualification(row.OuterHtml),
                Number = this.GetInt(table.Indexes, ConverterConstants.Number, data),
                TargetsHit = this.GetInt(table.Indexes, ConverterConstants.TargetsHit, data),
                Golds = this.GetInt(table.Indexes, ConverterConstants.Golds, data),
                Points = this.GetDouble(table.Indexes, ConverterConstants.Points, data),
                Record = this.OlympediaService.FindRecord(row.OuterHtml),
                Score10s = this.GetInt(table.Indexes, ConverterConstants.S10, data),
                Score9s = this.GetInt(table.Indexes, ConverterConstants.S9, data),
                ScoreXs = this.GetInt(table.Indexes, ConverterConstants.Xs, data),
                Target = this.GetString(table.Indexes, ConverterConstants.Target, data),
                Score = this.GetInt(table.Indexes, ConverterConstants.Score, data),
                ShootOff = this.GetInt(table.Indexes, ConverterConstants.ShootOff, data),
                Meters30 = this.GetInt(table.Indexes, ConverterConstants.M30, data),
                Meters50 = this.GetInt(table.Indexes, ConverterConstants.M50, data),
                Meters70 = this.GetInt(table.Indexes, ConverterConstants.M70, data),
                Meters90 = this.GetInt(table.Indexes, ConverterConstants.M90, data),
                Part1 = this.GetInt(table.Indexes, ConverterConstants.Part1, data),
                Part2 = this.GetInt(table.Indexes, ConverterConstants.Part2, data),
                Yards30 = this.GetInt(table.Indexes, ConverterConstants.Y30, data),
                Yards40 = this.GetInt(table.Indexes, ConverterConstants.Y40, data),
                Yards50 = this.GetInt(table.Indexes, ConverterConstants.Y50, data),
                Yards60 = this.GetInt(table.Indexes, ConverterConstants.Y60, data),
                Yards80 = this.GetInt(table.Indexes, ConverterConstants.Y80, data),
                Yards100 = this.GetInt(table.Indexes, ConverterConstants.Y100, data),
            };

            round.Athletes.Add(athlete);
        }
    }
    #endregion ARCHERY

    #region ALPINE SKIING
    private async Task ProcessAlpineSkiingAsync(ConvertOptions options)
    {
        var rounds = new List<Round<AlpineSkiing>>();

        if (options.Event.IsTeamEvent)
        {
            var track = await this.SetAlpineSkiingTrackAsync(options.HtmlDocument.DocumentNode.OuterHtml);
            foreach (var table in options.Tables.Where(x => x.Round != null))
            {
                var round = this.CreateRound<AlpineSkiing>(table.From, table.To, table.Format, table.Round, options.Event.Name, track);
                foreach (var row in table.Rows.Where(x => this.OlympediaService.FindMatchNumber(x.OuterHtml) != 0))
                {
                    var data = row.Elements("td").ToList();
                    var matchInputModel = new MatchInputModel
                    {
                        Row = row.OuterHtml,
                        Number = data[0].OuterHtml,
                        Date = data[1].InnerText,
                        Year = options.Game.Year,
                        EventId = options.Event.Id,
                        IsTeam = true,
                        HomeName = data[3].OuterHtml,
                        HomeNOC = data[4].OuterHtml,
                        Result = data[5].InnerHtml,
                        AwayName = data[6].OuterHtml,
                        AwayNOC = data[7].OuterHtml,
                        AnyParts = false,
                        Round = table.Round.Type,
                        Location = data[2].InnerText
                    };
                    var matchModel = await this.GetMatchAsync(matchInputModel);
                    var match = this.mapper.Map<TeamMatch<AlpineSkiing>>(matchModel);

                    var documentModel = options.Documents.FirstOrDefault(x => x.Id == match.ResultId);
                    if (documentModel != null)
                    {
                        await this.SetAlpineSkiingMatchesAsync(match, documentModel.Tables.Last(), options.Event.Id, options.Game.Year);
                    }

                    round.TeamMatches.Add(match);
                }
            }
        }
        else
        {
            var track = await this.SetAlpineSkiingTrackAsync(options.HtmlDocument.DocumentNode.OuterHtml);

            if (options.Tables.Count == 1)
            {
                var table = options.Tables.FirstOrDefault();
                var round = this.CreateRound<AlpineSkiing>(table.From, table.To, table.Format, new RoundModel { Type = RoundType.FinalRound }, options.Event.Name, track);
                await this.SetAlpineSkiingAthletesAsync(round, table, options.Event.Id);
                rounds.Add(round);
            }
            else
            {
                foreach (var table in options.Tables.Skip(1))
                {
                    var round = this.CreateRound<AlpineSkiing>(table.From, table.To, table.Format, table.Round, options.Event.Name, track);
                    if (table.Groups.Count != 0)
                    {
                        foreach (var group in table.Groups)
                        {
                            await this.SetAlpineSkiingAthletesAsync(round, group, options.Event.Id);
                        }
                    }
                    else
                    {
                        await this.SetAlpineSkiingAthletesAsync(round, table, options.Event.Id);
                    }
                    rounds.Add(round);
                }
            }

            foreach (var document in options.Documents)
            {
                track = await this.SetAlpineSkiingTrackAsync(document.Html);
                var roundModel = this.NormalizeService.MapRound(document.Title);
                var round = this.CreateRound<AlpineSkiing>(document.From, null, null, roundModel, options.Event.Name, track);
                await this.SetAlpineSkiingAthletesAsync(round, document.Tables.FirstOrDefault(), options.Event.Id);
                rounds.Add(round);
            }
        }

        await this.ProcessJsonAsync(rounds, options);
    }

    private async Task SetAlpineSkiingMatchesAsync(TeamMatch<AlpineSkiing> match, TableModel table, int eventId, int year)
    {
        foreach (var row in table.Rows.Skip(1))
        {
            var data = row.Elements("td").ToList();
            var matchInputModel = new MatchInputModel
            {
                Row = row.OuterHtml,
                Number = data[0].OuterHtml,
                Date = data[1].InnerText,
                Year = year,
                EventId = eventId,
                IsTeam = false,
                HomeName = data[3].OuterHtml,
                HomeNOC = data[4].OuterHtml,
                Result = data[5].InnerHtml,
                AwayName = data[6].OuterHtml,
                AwayNOC = data[7].OuterHtml,
                AnyParts = false,
                Round = table.Round.Type,
                Location = data[2].InnerText
            };

            var athleteMatch = await this.GetMatchAsync(matchInputModel);
            match.Team1.Athletes.Add(new AlpineSkiing
            {
                Id = athleteMatch.Team1.Id,
                Name = athleteMatch.Team1.Name,
                Code = athleteMatch.Team1.Code,
                NOC = athleteMatch.Team1.NOC,
                Time = athleteMatch.Team1.Time,
                MatchResult = athleteMatch.Team1.MatchResult,
                Race = athleteMatch.Number,
            });

            match.Team2.Athletes.Add(new AlpineSkiing
            {
                Id = athleteMatch.Team2.Id,
                Name = athleteMatch.Team2.Name,
                Code = athleteMatch.Team2.Code,
                NOC = athleteMatch.Team2.NOC,
                Time = athleteMatch.Team2.Time,
                MatchResult = athleteMatch.Team2.MatchResult,
                Race = athleteMatch.Number,
            });
        }
    }

    private async Task<Track> SetAlpineSkiingTrackAsync(string html)
    {
        var courseSetterMatch = this.RegExpService.Match(html, @"<th>\s*Course Setter\s*<\/th>\s*<td(?:.*?)>(.*?)<\/td>");
        var gatesMatch = this.RegExpService.MatchFirstGroup(html, @"Gates:(.*?)<br>");
        var lengthMatch = this.RegExpService.MatchFirstGroup(html, @"Length:(.*?)<br>");
        var startAltitudeMatch = this.RegExpService.MatchFirstGroup(html, @"Start Altitude:(.*?)<br>");
        var verticalDropMatch = this.RegExpService.MatchFirstGroup(html, @"Vertical Drop:(.*?)<\/td>");
        var athleteModel = courseSetterMatch != null ? this.OlympediaService.FindAthlete(courseSetterMatch.Groups[1].Value) : null;
        var courseSetter = athleteModel != null ? await this.athletesService.GetAsync(athleteModel.Code) : null;

        var gates = this.RegExpService.MatchInt(gatesMatch);
        var length = this.RegExpService.MatchInt(lengthMatch);
        var startAltitude = this.RegExpService.MatchInt(startAltitudeMatch);
        var verticalDrop = this.RegExpService.MatchInt(verticalDropMatch);

        return new Track
        {
            Turns = gates,
            Length = length,
            StartAltitude = startAltitude,
            HeightDifference = verticalDrop,
            PersonId = courseSetter != null ? courseSetter.Id : Guid.Empty,
            PersonName = athleteModel?.Name
        };
    }

    private async Task SetAlpineSkiingAthletesAsync(Round<AlpineSkiing> round, TableModel table, int eventId)
    {
        foreach (var row in table.Rows.Skip(1))
        {
            var data = row.Elements("td").ToList();
            var athleteModel = this.OlympediaService.FindAthlete(data[table.Indexes[ConverterConstants.Name]].OuterHtml);
            var participant = await this.participantsService.GetAsync(athleteModel.Code, eventId);
            var noc = this.OlympediaService.FindNOCCode(data[table.Indexes[ConverterConstants.NOC]].OuterHtml);
            var athlete = new AlpineSkiing
            {
                Id = participant.Id,
                Name = athleteModel.Name,
                NOC = noc,
                Code = athleteModel.Code,
                FinishStatus = this.OlympediaService.FindStatus(row.OuterHtml),
                Qualification = this.OlympediaService.FindQualification(row.OuterHtml),
                Number = this.GetInt(table.Indexes, ConverterConstants.Number, data),
                Downhill = this.GetTime(table.Indexes, ConverterConstants.Downhill, data),
                PenaltyTime = this.GetTime(table.Indexes, ConverterConstants.PenaltyTime, data),
                Points = this.GetDouble(table.Indexes, ConverterConstants.Points, data),
                Run1Time = this.GetTime(table.Indexes, ConverterConstants.Run1, data),
                Run2Time = this.GetTime(table.Indexes, ConverterConstants.Run2, data),
                Slalom = this.GetTime(table.Indexes, ConverterConstants.Slalom, data),
                Time = this.GetTime(table.Indexes, ConverterConstants.Time, data),
                GroupNumber = table.IsGroup ? table.Number : 0,
            };

            round.Athletes.Add(athlete);
        }
    }
    #endregion ALPINE SKIING

    #region BASKETBALL 3X3
    private async Task ProcessBasketball3X3Async(ConvertOptions options)
    {
        var rounds = new List<Round<Basketball>>();

        foreach (var table in options.Tables.Where(x => x.Round != null))
        {
            var round = this.CreateRound<Basketball>(table.From, table.To, table.Format, table.Round, options.Event.Name, null);

            foreach (var row in table.Rows.Where(x => this.OlympediaService.FindMatchNumber(x.OuterHtml) != 0))
            {
                var data = row.Elements("td").ToList();
                var matchInputModel = new MatchInputModel
                {
                    Row = row.OuterHtml,
                    Number = data[0].OuterHtml,
                    Date = data[1].InnerText,
                    Year = options.Game.Year,
                    EventId = options.Event.Id,
                    IsTeam = true,
                    HomeName = data[2].OuterHtml,
                    HomeNOC = data[3].OuterHtml,
                    Result = data[4].InnerHtml,
                    AwayName = data[5].OuterHtml,
                    AwayNOC = data[6].OuterHtml,
                    AnyParts = false,
                    Round = table.Round.Type,
                    Location = null
                };
                var matchModel = await this.GetMatchAsync(matchInputModel);
                var match = this.mapper.Map<TeamMatch<Basketball>>(matchModel);

                var documentModel = options.Documents.FirstOrDefault(x => x.Id == match.ResultId);
                if (documentModel != null)
                {
                    match.Judges = await this.GetJudgesAsync(documentModel.Html);
                    match.Location = this.OlympediaService.FindLocation(documentModel.Html);

                    await this.SetBasketball3x3AthletesAsync(match.Team1, documentModel.Tables[1], options.Event.Id);
                    await this.SetBasketball3x3AthletesAsync(match.Team2, documentModel.Tables[2], options.Event.Id);

                    this.SetBasketball3x3TeamStats(match.Team1);
                    this.SetBasketball3x3TeamStats(match.Team2);
                }

                round.TeamMatches.Add(match);
            }

            rounds.Add(round);
        }

        await this.ProcessJsonAsync(rounds, options);
    }

    private void SetBasketball3x3TeamStats(Basketball team)
    {
        team.OnePointsGoals = team.Athletes.Sum(x => x.OnePointsGoals);
        team.OnePointsAttempts = team.Athletes.Sum(x => x.OnePointsAttempts);
        team.TwoPointsGoals = team.Athletes.Sum(x => x.TwoPointsGoals);
        team.TwoPointsAttempts = team.Athletes.Sum(x => x.TwoPointsAttempts);
        team.FreeThrowsGoals = team.Athletes.Sum(x => x.FreeThrowsGoals);
        team.FreeThrowsAttempts = team.Athletes.Sum(x => x.FreeThrowsAttempts);
        team.OffensiveRebounds = team.Athletes.Sum(x => x.OffensiveRebounds);
        team.DefensiveRebounds = team.Athletes.Sum(x => x.DefensiveRebounds);
        team.TotalRebounds = team.OffensiveRebounds + team.DefensiveRebounds;
        team.Blocks = team.Athletes.Sum(x => x.Blocks);
        team.Turnovers = team.Athletes.Sum(x => x.Turnovers);
    }

    private async Task SetBasketball3x3AthletesAsync(Basketball team, TableModel table, int eventId)
    {
        foreach (var row in table.Rows.Skip(1).Take(table.Rows.Count - 2))
        {
            var data = row.Elements("td").ToList();
            var athleteModel = this.OlympediaService.FindAthlete(data[table.Indexes[ConverterConstants.Name]].OuterHtml);
            var participant = await this.participantsService.GetAsync(athleteModel.Code, eventId);
            var onePointMatch = this.RegExpService.Match(data[9].InnerText, @"(\d+)\/(\d+)");
            var twoPointMatch = this.RegExpService.Match(data[11].InnerText, @"(\d+)\/(\d+)");
            var freeThrowPointMatch = this.RegExpService.Match(data[15].InnerText, @"(\d+)\/(\d+)");

            var athlete = new Basketball
            {
                Id = participant.Id,
                Name = athleteModel.Name,
                NOC = team.NOC,
                Code = athleteModel.Code,
                Number = this.GetInt(table.Indexes, ConverterConstants.Number, data),
                Position = this.GetString(table.Indexes, ConverterConstants.Position, data),
                Points = this.GetInt(table.Indexes, ConverterConstants.Points, data),
                TimePlayed = this.GetTime(table.Indexes, ConverterConstants.TimePlayed, data),
                Value = this.GetDouble(table.Indexes, ConverterConstants.Value, data),
                PlusMinus = this.GetInt(table.Indexes, ConverterConstants.PlusMinus, data),
                ShootingEfficiency = this.GetDouble(table.Indexes, ConverterConstants.ShootingEfficiency, data),
                OnePointsGoals = this.RegExpService.MatchInt(onePointMatch?.Groups[1].Value),
                OnePointsAttempts = this.RegExpService.MatchInt(onePointMatch?.Groups[2].Value),
                TwoPointsGoals = this.RegExpService.MatchInt(twoPointMatch?.Groups[1].Value),
                TwoPointsAttempts = this.RegExpService.MatchInt(twoPointMatch?.Groups[2].Value),
                FreeThrowsGoals = this.RegExpService.MatchInt(freeThrowPointMatch?.Groups[1].Value),
                FreeThrowsAttempts = this.RegExpService.MatchInt(freeThrowPointMatch?.Groups[2].Value),
                OffensiveRebounds = this.GetInt(table.Indexes, ConverterConstants.OffensiveRebounds, data),
                DefensiveRebounds = this.GetInt(table.Indexes, ConverterConstants.DefensiveRebounds, data),
                Blocks = this.GetInt(table.Indexes, ConverterConstants.Blocks, data),
                Turnovers = this.GetInt(table.Indexes, ConverterConstants.Turnovers, data)
            };

            athlete.TotalFieldGoals = athlete.OnePointsGoals + athlete.TwoPointsGoals;
            athlete.TotalFieldGoalsAttempts = athlete.OnePointsAttempts + athlete.TwoPointsAttempts;
            athlete.TotalRebounds = athlete.OffensiveRebounds + athlete.DefensiveRebounds;

            team.Athletes.Add(athlete);
        }
    }

    #endregion BASKETBALL 3X3
}