namespace SportData.Converters.OlympicGames;

using System;
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
using SportData.Data.Models.Enumerations;
using SportData.Data.Models.OlympicGames;
using SportData.Data.Models.OlympicGames.Aquatics.ArtisticSwimming;
using SportData.Data.Models.OlympicGames.Archery;
using SportData.Data.Models.OlympicGames.Athletics;
using SportData.Data.Models.OlympicGames.Badminton;
using SportData.Data.Models.OlympicGames.Base;
using SportData.Data.Models.OlympicGames.Basketball;
using SportData.Data.Models.OlympicGames.Gymnastics;
using SportData.Data.Models.OlympicGames.Gymnastics.ArtisticGymnastics;
using SportData.Data.Models.OlympicGames.Skiing.AlpineSkiing;
using SportData.Services.Data.CrawlerStorageDb.Interfaces;
using SportData.Services.Data.SportDataDb.Interfaces;
using SportData.Services.Interfaces;

using Document = Data.Entities.Crawlers.Document;

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
                    var standingTable = this.GetStandingTable(htmlDocument, eventCacheModel);
                    var tables = this.GetTables(htmlDocument, eventCacheModel, disciplineCacheModel);

                    var options = new ConvertOptions
                    {
                        Discipline = disciplineCacheModel,
                        Documents = documents,
                        Event = eventCacheModel,
                        Game = gameCacheModel,
                        HtmlDocument = htmlDocument,
                        StandingTable = standingTable,
                        Tables = tables
                    };

                    switch (disciplineCacheModel.Name)
                    {
                        //case DisciplineConstants.BASKETBALL_3X3:
                        //case DisciplineConstants.BASKETBALL:
                        //    await this.ProcessBasketballAsync(options);
                        //    break;
                        //case DisciplineConstants.ALPINE_SKIING:
                        //    await this.ProcessAlpineSkiingAsync(options);
                        //    break;
                        //case DisciplineConstants.ARCHERY:
                        //    await this.ProcessArcheryAsync(options);
                        //    break;
                        //case DisciplineConstants.ARTISTIC_GYMNASTICS:
                        //    // JUDGES ?????????????????????????????????????????????///
                        //    await this.ProcessArtisticGymnasticsAsync(options);
                        //    break;
                        //case DisciplineConstants.ARTISTIC_SWIMMING:
                        //    await this.ProcessArtisticSwimmingAsync(options);
                        //    break;
                        //case DisciplineConstants.ATHLETICS:
                        //    await this.ProcessAthleticsAsync(options);
                        //    break;
                        case DisciplineConstants.BADMINTON:
                            await this.ProcessBadmintonAsync(options);
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

    //    var sb = new StringBuilder();
    //    sb.AppendLine($"{options.Event.Name} - {options.Event.OriginalName}");
    //            sb.AppendLine($"{options.Game.Year}");
    //            sb.AppendLine($"--------------------- Standing table ---------------------");
    //            if (!options.Tables.Any())
    //            {
    //                var standingHeaders = options.StandingTable.HtmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']//th").Where(x => !string.IsNullOrEmpty(x.InnerText)).Select(x => x.InnerText).ToList();
    //                foreach (var item in standingHeaders)
    //                {
    //                    sb.AppendLine(item);
    //                }
    //            }

    //            if (options.Tables.Any())
    //{
    //    sb.AppendLine($"--------------------- Tables ---------------------");
    //    foreach (var table in options.Tables)
    //    {
    //        var headers = table.HtmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']//th").Where(x => !string.IsNullOrEmpty(x.InnerText)).Select(x => x.InnerText).ToList();
    //        foreach (var item in headers)
    //        {
    //            sb.AppendLine(item);
    //        }
    //    }
    //}

    //if (options.Documents.Any())
    //{
    //    sb.AppendLine("--------------------- Documents ---------------------");
    //    foreach (var document in options.Documents)
    //    {
    //        var htmlDocument = this.CreateHtmlDocument(document);
    //        var table = this.GetStandingTable(htmlDocument, options.Event);
    //        var title = htmlDocument.DocumentNode.SelectSingleNode("//h1").InnerText;
    //        sb.AppendLine(title);
    //        var headers = table.HtmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']//th").Where(x => !string.IsNullOrEmpty(x.InnerText)).Select(x => x.InnerText).ToList();
    //        foreach (var item in headers)
    //        {
    //            sb.AppendLine(item);
    //        }
    //    }
    //}

    //sb.AppendLine("=======================================================================")
    //;

    //var asd = File.ReadAllLines("tables.txt").ToList();
    //asd.Add(sb.ToString());
    //File.WriteAllLines("tables.txt", asd);

    #region PRIVATE
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

    private IList<TableModel> GetTables(HtmlDocument document, EventCacheModel eventCache, DisciplineCacheModel disciplineCache)
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
                        EventId = eventCache.Id,
                        Title = headerMatches[i].Value
                    };

                    var title = this.RegExpService.CutHtml(table.Title);
                    var dateMatch = this.RegExpService.Match(title, @"\((.*)\)");
                    var dateString = dateMatch != null ? dateMatch.Groups[1].Value : string.Empty;

                    table.Title = this.RegExpService.CutHtml(title, @"\((.*)\)")?.Replace(",", string.Empty).Trim();
                    table.Round = this.NormalizeService.MapRoundType(table.Title);

                    if (disciplineCache.Name == DisciplineConstants.ATHLETICS)
                    {
                        if (table.Title.StartsWith("Qualifying Round") || table.Title.StartsWith("Group"))
                        {
                            table.Round = RoundType.Qualification;
                            table.GroupType = this.NormalizeService.MapGroupType(table.Title.Replace("Qualifying Round", string.Empty));
                        }
                    }

                    if (table.Round == RoundType.Group)
                    {
                        var groupMatch = this.RegExpService.Match(table.Title, "Group (.*)");
                        table.RoundInfo = groupMatch != null ? groupMatch.Groups[1].Value.Trim() : null;
                    }

                    if (table.Round == RoundType.Classification)
                    {
                        var classificationMatch = this.RegExpService.Match(table.Title, @"Classification Round (\d+)-(\d+)");
                        table.RoundInfo = classificationMatch != null ? $"{classificationMatch.Groups[1].Value.Trim()}-{classificationMatch.Groups[2].Value.Trim()}" : null;
                    }

                    if (!string.IsNullOrEmpty(dateString))
                    {
                        var dates = this.dateService.ParseDate(dateString);
                        table.FromDate = dates.From;
                        table.ToDate = dates.To;
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

    private IList<HeatModel> SplitHeats(TableModel table)
    {
        var heats = new List<HeatModel>();
        var html = table.Html;
        html = html.Replace("<table class=\"biodata\"></table>", string.Empty);

        var matches = this.RegExpService.Matches(html, @"<h3>(.*?)<\/table>");
        matches.ToList().ForEach(x =>
        {
            var title = this.RegExpService.MatchFirstGroup(x.Groups[0].Value, "<h3>(.*?)<");
            var windString = this.RegExpService.MatchFirstGroup(x.Groups[0].Value, @"<th>Wind<\/th>\s*<td>\s*(.*?)\s*m\/s<\/td>");
            heats.Add(new HeatModel
            {
                Html = x.Groups[0].Value,
                Title = title,
                Wind = windString != null ? this.RegExpService.MatchDouble(windString) : null,
            });
        });

        return heats;
    }

    private IList<HeatModel> SplitGroups(string html)
    {
        var heats = new List<HeatModel>();

        html = html.Replace("<table class=\"biodata\"></table>", string.Empty);
        html = this.RegExpService.Replace(html, @"<table class=""biodata"">(.*?)</table>", string.Empty);

        var matches = this.RegExpService.Matches(html, @"<h3>(.*?)<\/table>");
        matches.ToList().ForEach(x =>
        {
            var title = this.RegExpService.MatchFirstGroup(x.Groups[0].Value, "<h3>(.*?)<");
            var windString = this.RegExpService.MatchFirstGroup(x.Groups[0].Value, @"<th>Wind<\/th>\s*<td>\s*(.*?)\s*m\/s<\/td>");
            heats.Add(new HeatModel
            {
                Html = x.Groups[0].Value,
                Title = title,
                Wind = windString != null ? this.RegExpService.MatchDouble(windString) : null,
            });
        });

        return heats;
    }

    private ResultJson<TModel> CreateResult<TModel>(EventCacheModel @event, DisciplineCacheModel discipline, GameCacheModel game)
    {
        var result = new ResultJson<TModel>
        {
            Event = new EventJson
            {
                Id = @event.Id,
                Gender = @event.Gender,
                IsTeamEvent = @event.IsTeamEvent,
                Name = @event.Name,
                NormalizedName = @event.NormalizedName,
                OriginalName = @event.OriginalName
            },
            Discipline = new DisciplineJson
            {
                Id = discipline.Id,
                Name = discipline.Name
            },
            Game = new GameJson
            {
                Id = game.Id,
                Type = game.Type,
                Year = game.Year
            },
            Rounds = new List<TModel>()
        };

        return result;
    }

    private EventRoundModel<TModel> CreateEventRound<TModel>(HtmlDocument htmlDocument, string eventName)
    {
        var format = this.RegExpService.MatchFirstGroup(htmlDocument.DocumentNode.OuterHtml, @"<th>Format<\/th>\s*<td(?:.*?)>(.*?)<\/td>");
        var dateString = this.RegExpService.MatchFirstGroup(htmlDocument.DocumentNode.OuterHtml, @"<th>\s*Date\s*<\/th>\s*<td>(.*?)<\/td>");
        var dateModel = this.dateService.ParseDate(dateString);
        eventName = this.NormalizeService.CleanEventName(eventName);

        return new EventRoundModel<TModel>
        {
            Format = this.RegExpService.CutHtml(format),
            EventName = eventName,
            Dates = dateModel
        };
    }

    #endregion PRIVATE

    #region BADMINTON
    private async Task ProcessBadmintonAsync(ConvertOptions options)
    {
        var eventRound = this.CreateEventRound<BDMRound>(options.HtmlDocument, options.Event.Name);

        foreach (var table in options.Tables)
        {
            var round = this.CreateBasketballRound(eventRound.Dates.From, eventRound.Format, table.Round, eventRound.EventName);
            var rows = table.HtmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']//tr");

            foreach (var row in rows.Where(x => this.OlympediaService.IsMatchNumber(x.InnerText)))
            {
                var data = row.Elements("td").ToList();

                if (options.Event.IsTeamEvent)
                {
                    var pairs = await this.ConvertBadmintonPairsAsync(options.StandingTable, options.Event);

                    var team1NOCCode = this.OlympediaService.FindNOCCode(data[3].OuterHtml);
                    var team1Seed = this.OlympediaService.FindSeedNumber(data[2].OuterHtml);
                    var team1AthleteModels = this.OlympediaService.FindAthletes(data[2].OuterHtml);
                    string location = null;

                    if (options.Game.Year >= 2000)
                    {
                        team1NOCCode = this.OlympediaService.FindNOCCode(data[4].OuterHtml);
                        team1Seed = this.OlympediaService.FindSeedNumber(data[3].OuterHtml);
                        team1AthleteModels = this.OlympediaService.FindAthletes(data[3].OuterHtml);
                        location = data[2].InnerText;
                    }

                    var team1NOCCodeCacheModel = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == team1NOCCode);
                    var team1 = pairs[$"{string.Join(string.Empty, team1AthleteModels.Select(x => x.Number))}"];
                    team1 ??= pairs[$"{string.Join(string.Empty, team1AthleteModels.Select(x => x.Number).Reverse())}"];

                    var match = new BDMTeamMatch
                    {
                        MatchNumber = this.OlympediaService.FindMatchNumber(data[0].InnerText),
                        Round = table.Round,
                        RoundInfo = table.RoundInfo,
                        MatchType = this.OlympediaService.FindMatchType(table.Round, data[0].InnerText),
                        MatchInfo = this.OlympediaService.FindMatchInfo(data[0].InnerText),
                        Date = this.dateService.ParseDate(data[1].InnerText, options.Game.Year).From,
                        ResultId = this.OlympediaService.FindResultNumber(data[0].OuterHtml),
                        Decision = this.OlympediaService.FindDecision(row.OuterHtml),
                        Location = location,
                        Team1 = new BDMTeam
                        {
                            Seed = team1Seed,
                            Name = team1.Name,
                            NOCCode = team1NOCCode,
                            TeamId = team1.Id
                        }
                    };

                    foreach (var athleteModel in team1AthleteModels)
                    {
                        var participant = await this.participantsService.GetAsync(athleteModel.Number, options.Event.Id, team1NOCCodeCacheModel.Id);
                        var player = new BDMPlayer
                        {
                            AthleteNumber = athleteModel.Number,
                            Name = // TODO find athlete full name ???????????????????///
                        };
                    }

                    if (table.Round == RoundType.Group)
                    {
                        match.Group = this.NormalizeService.MapGroupType(table.Title);
                    }
                }
                else
                {
                    var player1NOCCode = this.OlympediaService.FindNOCCode(data[3].OuterHtml);
                    var athleteModel1 = this.OlympediaService.FindAthlete(data[2].OuterHtml);
                    var player1Seed = this.OlympediaService.FindSeedNumber(data[2].OuterHtml);
                    string location = null;

                    if (options.Game.Year >= 2000)
                    {
                        player1NOCCode = this.OlympediaService.FindNOCCode(data[4].OuterHtml);
                        athleteModel1 = this.OlympediaService.FindAthlete(data[3].OuterHtml);
                        player1Seed = this.OlympediaService.FindSeedNumber(data[3].OuterHtml);
                        location = data[2].InnerText;
                    }

                    var player1NOCCodeCacheModel = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == player1NOCCode);
                    var player1 = await this.participantsService.GetAsync(athleteModel1.Number, options.Event.Id, player1NOCCodeCacheModel.Id);

                    var match = new BDMMatch
                    {
                        MatchNumber = this.OlympediaService.FindMatchNumber(data[0].InnerText),
                        Round = table.Round,
                        RoundInfo = table.RoundInfo,
                        MatchType = this.OlympediaService.FindMatchType(table.Round, data[0].InnerText),
                        MatchInfo = this.OlympediaService.FindMatchInfo(data[0].InnerText),
                        Date = this.dateService.ParseDate(data[1].InnerText, options.Game.Year).From,
                        ResultId = this.OlympediaService.FindResultNumber(data[0].OuterHtml),
                        Decision = this.OlympediaService.FindDecision(row.OuterHtml),
                        Location = location,
                        Player1 = new BDMPlayer
                        {
                            AthleteNumber = athleteModel1.Number,
                            Name = athleteModel1.Name,
                            NOCCode = player1NOCCode,
                            ParticipantId = player1.Id,
                            Seed = player1Seed,
                        }
                    };

                    if (table.Round == RoundType.Group)
                    {
                        match.Group = this.NormalizeService.MapGroupType(table.Title);
                    }

                    if (match.Decision == DecisionType.None)
                    {
                        var matchResult = this.OlympediaService.GetMatchResult(data[4].OuterHtml, MatchResultType.Games);
                        var player2NOCCode = this.OlympediaService.FindNOCCode(data[6].OuterHtml);
                        var athleteModel2 = this.OlympediaService.FindAthlete(data[5].OuterHtml);
                        var player2Seed = this.OlympediaService.FindSeedNumber(data[5].OuterHtml);
                        if (options.Game.Year >= 2000)
                        {
                            matchResult = this.OlympediaService.GetMatchResult(data[5].OuterHtml, MatchResultType.Games);
                            player2NOCCode = this.OlympediaService.FindNOCCode(data[7].OuterHtml);
                            athleteModel2 = this.OlympediaService.FindAthlete(data[6].OuterHtml);
                            player2Seed = this.OlympediaService.FindSeedNumber(data[6].OuterHtml);
                        }

                        match.Player1.Points = matchResult.Points1;
                        match.Player1.Result = matchResult.Result1;
                        match.Player1.Game1Points = matchResult.Games1.ElementAtOrDefault(0);
                        match.Player1.Game2Points = matchResult.Games1.ElementAtOrDefault(1);
                        match.Player1.Game3Points = matchResult.Games1.ElementAtOrDefault(2);

                        var player2NOCCodeCacheModel = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == player2NOCCode);
                        var player2 = await this.participantsService.GetAsync(athleteModel2.Number, options.Event.Id, player2NOCCodeCacheModel.Id);
                        match.Player2 = new BDMPlayer
                        {
                            AthleteNumber = athleteModel2.Number,
                            Name = athleteModel2.Name,
                            NOCCode = player2NOCCode,
                            ParticipantId = player2.Id,
                            Seed = player2Seed,
                            Points = matchResult.Points2,
                            Result = matchResult.Result2,
                            Game1Points = matchResult.Games2.ElementAtOrDefault(0),
                            Game2Points = matchResult.Games2.ElementAtOrDefault(1),
                            Game3Points = matchResult.Games2.ElementAtOrDefault(2),
                        };

                        var document = options.Documents.FirstOrDefault(x => x.Url.EndsWith($"{match.ResultId}"));
                        if (document != null)
                        {
                            await this.ConvertBadmintonInfoAsync(match, document, options.Event);
                        }
                    }
                }
            }
        }

        var resultJson = this.CreateResult<BDMRound>(options.Event, options.Discipline, options.Game);
        resultJson.Rounds = eventRound.Rounds;

        var json = JsonSerializer.Serialize(resultJson);
        var result = new Result
        {
            EventId = options.Event.Id,
            Json = json
        };

        //await this.resultsService.AddOrUpdateAsync(result);
    }

    private async Task<Dictionary<string, Team>> ConvertBadmintonPairsAsync(TableModel table, EventCacheModel eventCache)
    {
        var rows = table.HtmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']//tr");
        var headers = rows.First().Elements("th").Select(x => x.InnerText).ToList();
        var indexes = this.OlympediaService.FindIndexes(headers);

        var result = new Dictionary<string, Team>();
        foreach (var row in rows.Skip(1))
        {
            var data = row.Elements("td").ToList();
            var name = data[indexes[ConverterConstants.INDEX_NAME]].InnerText;
            var nocCode = this.OlympediaService.FindNOCCode(row.OuterHtml);
            var nocCodeCache = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == nocCode);
            var team = await this.teamsService.GetAsync(name, nocCodeCache.Id, eventCache.Id);

            var athleteModels = this.OlympediaService.FindAthletes(row.OuterHtml);
            var key = $"{string.Join(string.Empty, athleteModels.Select(x => x.Number))}";
            result[key] = team;
        }

        return result;
    }

    private async Task ConvertBadmintonInfoAsync(BDMMatch match, Document document, EventCacheModel eventCache)
    {
        var htmlDocument = this.CreateHtmlDocument(document);
        var dateString = this.RegExpService.MatchFirstGroup(htmlDocument.DocumentNode.OuterHtml, @"<th>\s*Date\s*<\/th>\s*<td>(.*?)<\/td>");
        var dateModel = this.dateService.ParseDate(dateString);

        match.Date = dateModel.From;

        var umpireMatch = this.RegExpService.Match(htmlDocument.DocumentNode.OuterHtml, @"<th>Umpire<\/th>(.*?)<\/tr>");
        if (umpireMatch != null)
        {
            var umpireAthleteModel = this.OlympediaService.FindAthlete(umpireMatch.Groups[1].Value);
            var umpireNOCCode = this.OlympediaService.FindNOCCode(umpireMatch.Groups[1].Value);
            var umpireNOCCodeCache = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == umpireNOCCode);
            var umpire = await this.participantsService.GetAsync(umpireAthleteModel.Number, eventCache.Id, umpireNOCCodeCache.Id);

            var judge = new BaseJudge
            {
                AthleteNumber = umpireAthleteModel.Number,
                Name = umpireAthleteModel.Name,
                NOCCode = umpireNOCCode,
                ParticipantId = umpire == null ? Guid.Empty : umpire.Id,
                Title = "Umpire"
            };

            match.Umpire = judge;
        }

        var serviceMatch = this.RegExpService.Match(htmlDocument.DocumentNode.OuterHtml, @"<th>Umpire<\/th>(.*?)<\/tr>");
        if (serviceMatch != null)
        {
            var serviceAthleteModel = this.OlympediaService.FindAthlete(serviceMatch.Groups[1].Value);
            var serviceNOCCode = this.OlympediaService.FindNOCCode(serviceMatch.Groups[1].Value);
            var serviceNOCCodeCache = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == serviceNOCCode);
            var service = await this.participantsService.GetAsync(serviceAthleteModel.Number, eventCache.Id, serviceNOCCodeCache.Id);

            var judge = new BaseJudge
            {
                AthleteNumber = serviceAthleteModel.Number,
                Name = serviceAthleteModel.Name,
                NOCCode = serviceNOCCode,
                ParticipantId = service == null ? Guid.Empty : service.Id,
                Title = "Service Judge"
            };

            match.ServiceJudge = judge;
        }
    }

    private BDMRound CreateBadmintonRound(DateTime? date, string format, RoundType roundType, string eventName)
    {
        return new BDMRound
        {
            Date = date,
            Format = format,
            EventName = eventName,
            Round = roundType
        };
    }

    #endregion BADMINTON

    #region ATHLETICS
    private async Task ProcessAthleticsAsync(ConvertOptions options)
    {
        var eventRound = this.CreateEventRound<ATHRound>(options.HtmlDocument, options.Event.Name);
        var eventGroup = this.NormalizeService.MapAthleticsEventGroup(options.Event.Name);

        if (eventGroup == ATHEventGroup.TrackEvents)
        {
            if (options.Tables.Any())
            {
                foreach (var table in options.Tables.Where(x => x.Round != RoundType.None))
                {
                    var format = this.RegExpService.MatchFirstGroup(table.HtmlDocument.DocumentNode.OuterHtml, @"<i>(.*?)<\/i>");
                    var round = this.CreateAthleticsRound(eventRound.Dates.From, format, table.Round, eventRound.EventName, eventGroup);
                    round.Track = new ATHTrack();

                    var heats = this.SplitHeats(table);
                    if (heats.Any())
                    {
                        foreach (var heat in heats)
                        {
                            var heatType = this.NormalizeService.MapHeats(heat.Title);
                            await this.SetATHTrackAthletesAsync(round, heat.HtmlDocument, options.Event, heatType, heat.Wind, true);
                        }
                    }
                    else
                    {
                        var windString = this.RegExpService.MatchFirstGroup(table.HtmlDocument.DocumentNode.OuterHtml, @"<th>Wind<\/th>\s*<td>\s*(.*?)\s*m\/s<\/td>");
                        var wind = windString != null ? this.RegExpService.MatchDouble(windString) : null;
                        await this.SetATHTrackAthletesAsync(round, table.HtmlDocument, options.Event, HeatType.None, wind, true);
                    }

                    eventRound.Rounds.Add(round);
                }
            }
            else
            {
                var round = this.CreateAthleticsRound(eventRound.Dates.From, eventRound.Format, RoundType.Final, eventRound.EventName, eventGroup);
                round.Track = new ATHTrack();
                this.SetAthleticsSlipts(round, options.HtmlDocument, HeatType.None);
                await this.SetATHTrackAthletesAsync(round, options.StandingTable.HtmlDocument, options.Event, HeatType.None, null, false);
                eventRound.Rounds.Add(round);
            }
        }
        else if (eventGroup == ATHEventGroup.RoadEvents)
        {
            var round = this.CreateAthleticsRound(eventRound.Dates.From, eventRound.Format, RoundType.Final, eventRound.EventName, eventGroup);
            round.Road = new ATHRoad();
            await this.SetATHRoadAthletesAsync(round, options.StandingTable.HtmlDocument, options.Event);
            eventRound.Rounds.Add(round);
        }
        else if (eventGroup == ATHEventGroup.CrossCountryEvents)
        {
            var round = this.CreateAthleticsRound(eventRound.Dates.From, eventRound.Format, RoundType.Final, eventRound.EventName, eventGroup);
            round.CrossCountry = new ATHCrossCountry();
            await this.SetATHCrossCountryAsync(round, options.StandingTable.HtmlDocument, options.Event);
            eventRound.Rounds.Add(round);
        }
        else if (eventGroup == ATHEventGroup.FieldEvents)
        {
            if (!options.Tables.Any() && !options.Documents.Any())
            {
                var round = this.CreateAthleticsRound(eventRound.Dates.From, eventRound.Format, RoundType.Final, eventRound.EventName, eventGroup);
                round.Field = new ATHField();
                await this.SetATHFieldAthletesAsync(round, options.StandingTable.HtmlDocument, options.Event);
                eventRound.Rounds.Add(round);
            }
            else if (options.Tables.Any())
            {
                var tables = options.Tables;
                if (tables.Count(x => x.Round == RoundType.Qualification) > 1)
                {
                    tables = options.Tables.Where(x => !(x.Round == RoundType.Qualification && x.GroupType == GroupType.None)).ToList();
                }

                foreach (var table in tables)
                {
                    var format = this.RegExpService.MatchFirstGroup(table.HtmlDocument.DocumentNode.OuterHtml, @"<i>(.*?)<\/i>");
                    var round = this.CreateAthleticsRound(table.FromDate, format, table.Round, eventRound.EventName, eventGroup);
                    round.Group = table.GroupType;
                    round.Field = new ATHField();
                    await this.SetATHFieldAthletesAsync(round, table.HtmlDocument, options.Event);
                    eventRound.Rounds.Add(round);
                }
            }
        }
        else if (eventGroup == ATHEventGroup.CombinedEvents)
        {
            var round = this.CreateAthleticsRound(eventRound.Dates.From, eventRound.Format, RoundType.Final, eventRound.EventName, eventGroup);
            round.Combined = new ATHCombined();
            await this.SetATHCombinedAthletesAsync(round, options.StandingTable.HtmlDocument, options.Event);
            this.SetATHCombinedResults(round, options.Documents, options.Event, options.Discipline, options.Game);
            eventRound.Rounds.Add(round);
        }

        var resultJson = this.CreateResult<ATHRound>(options.Event, options.Discipline, options.Game);
        resultJson.Rounds = eventRound.Rounds;

        var json = JsonSerializer.Serialize(resultJson);
        var result = new Result
        {
            EventId = options.Event.Id,
            Json = json
        };

        await this.resultsService.AddOrUpdateAsync(result);
    }

    private void SetATHCombinedResults(ATHRound round, IOrderedEnumerable<Document> documents, EventCacheModel eventCache, DisciplineCacheModel disciplineCache, GameCacheModel gameCache)
    {
        var order = 1;
        foreach (var document in documents)
        {
            var htmlDocument = this.CreateHtmlDocument(document);
            var standingTable = this.GetStandingTable(htmlDocument, eventCache);
            var tables = this.GetTables(htmlDocument, eventCache, disciplineCache);
            var title = htmlDocument.DocumentNode.SelectSingleNode("//h1").InnerText;
            title = title.Replace(eventCache.OriginalName, string.Empty).Replace("–", string.Empty).Trim();
            if (!title.StartsWith("Standing"))
            {
                var eventGroup = this.NormalizeService.MapAthleticsEventGroup(eventCache.Name);
                var dateString = this.RegExpService.MatchFirstGroup(htmlDocument.DocumentNode.OuterHtml, @"<th>\s*Date\s*<\/th>\s*<td>(.*?)<\/td>");
                var dateModel = this.dateService.ParseDate(dateString);
                round.Combined.Events.Add(new ATHCombinedEvent
                {
                    Date = dateModel.From,
                    EventName = this.NormalizeService.MapAthleticsCombinedEvents(title),
                    Order = order
                });
                order++;

                this.AddATHCombinedResult(round, standingTable.HtmlDocument, HeatType.None, GroupType.None, title, eventGroup, true, null);
                var groups = this.SplitGroups(htmlDocument.DocumentNode.OuterHtml);
                if (groups.Any())
                {
                    foreach (var group in groups)
                    {
                        var heatType = HeatType.None;
                        var groupType = GroupType.None;
                        if (eventGroup == ATHEventGroup.TrackEvents)
                        {
                            heatType = this.NormalizeService.MapHeats(group.Title);
                        }
                        else if (eventGroup == ATHEventGroup.FieldEvents)
                        {
                            groupType = this.NormalizeService.MapGroupType(group.Title);
                        }

                        this.AddATHCombinedResult(round, group.HtmlDocument, heatType, groupType, title, eventGroup, false, group.Wind);
                    }
                }
            }
        }
    }

    private void AddATHCombinedResult(ATHRound round, HtmlDocument htmlDocument, HeatType heatType, GroupType groupType, string eventName, ATHEventGroup eventGroup, bool isStandingTable, double? wind)
    {
        var rows = htmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']//tr");
        var headers = rows.First().Elements("th").Select(x => x.InnerText).ToList();
        var indexes = this.OlympediaService.FindIndexes(headers);

        foreach (var row in rows.Skip(1))
        {
            var data = row.Elements("td").ToList();
            var athleteModel = this.OlympediaService.FindAthlete(row.OuterHtml);
            var athlete = round.Combined.Athletes.FirstOrDefault(x => x.AthleteNumber == athleteModel.Number);

            if (isStandingTable)
            {
                var measurement = indexes.TryGetValue(ConverterConstants.INDEX_HEIGHT, out int value1) ? this.RegExpService.MatchDouble(data[value1].InnerText) : null;
                measurement ??= indexes.TryGetValue(ConverterConstants.INDEX_DISTANCE, out int value2) ? this.RegExpService.MatchDouble(data[value2].InnerText) : null;

                athlete.Results.Add(new ATHCombinedResult
                {
                    EventName = eventName,
                    FinishStatus = this.OlympediaService.FindStatus(row.OuterHtml),
                    Record = this.OlympediaService.FindRecord(row.OuterHtml),
                    Points = indexes.TryGetValue(ConverterConstants.INDEX_POINTS, out int value3) ? this.RegExpService.MatchDouble(data[value3].InnerText) : null,
                    BestMeasurement = measurement,
                    SecondBestMeasurement = indexes.TryGetValue(ConverterConstants.INDEX_SECOND_DISTANCE, out int value4) ? this.RegExpService.MatchDouble(data[value4].InnerText) : null,
                    Misses = indexes.TryGetValue(ConverterConstants.INDEX_MISSES, out int value5) ? this.RegExpService.MatchInt(data[value5].InnerText) : null,
                    TotalAttempts = indexes.TryGetValue(ConverterConstants.INDEX_TOTAL_ATTEMPTS, out int value6) ? this.RegExpService.MatchInt(data[value6].InnerText) : null,
                    TotalMisses = indexes.TryGetValue(ConverterConstants.INDEX_TOTAL_MISSES, out int value7) ? this.RegExpService.MatchInt(data[value7].InnerText) : null,
                    TieBreakingTime = indexes.TryGetValue(ConverterConstants.INDEX_TIE_BREAKING_TIME, out int value9) ? this.dateService.ParseTime(data[value9].InnerText) : null,
                    Time = indexes.TryGetValue(ConverterConstants.INDEX_TIME, out int value10) ? this.dateService.ParseTime(data[value10].InnerText) : null,
                    TimeAutomatic = indexes.TryGetValue(ConverterConstants.INDEX_TIME_AUTOMATIC, out int value11) ? this.dateService.ParseTime(data[value11].InnerText) : null,
                    TimeHand = indexes.TryGetValue(ConverterConstants.INDEX_TIME_HAND, out int value8) ? this.dateService.ParseTime(data[value8].InnerText) : null,
                });
            }
            else
            {
                var result = athlete.Results.FirstOrDefault(x => x.EventName == eventName);
                if (result != null)
                {
                    if (eventGroup == ATHEventGroup.TrackEvents)
                    {
                        result.Lane = indexes.TryGetValue(ConverterConstants.INDEX_LANE, out int value1) ? this.RegExpService.MatchInt(data[value1].InnerText) : null;
                        result.Heat = heatType;
                        result.Wind = wind;
                        result.FinishStatus = this.OlympediaService.FindStatus(row.OuterHtml);
                        result.Record = this.OlympediaService.FindRecord(row.OuterHtml);
                        result.ReactionTime = indexes.TryGetValue(ConverterConstants.INDEX_REACTION_TIME, out int value7) ? this.RegExpService.MatchDouble(data[value7].InnerText) : null;
                        result.TieBreakingTime = indexes.TryGetValue(ConverterConstants.INDEX_TIE_BREAKING_TIME, out int value9) ? this.dateService.ParseTime(data[value9].InnerText) : null;
                        result.Time = indexes.TryGetValue(ConverterConstants.INDEX_TIME, out int value10) ? this.dateService.ParseTime(data[value10].InnerText) : null;
                        result.TimeAutomatic = indexes.TryGetValue(ConverterConstants.INDEX_TIME_AUTOMATIC, out int value11) ? this.dateService.ParseTime(data[value11].InnerText) : null;
                        result.TimeHand = indexes.TryGetValue(ConverterConstants.INDEX_TIME_HAND, out int value8) ? this.dateService.ParseTime(data[value8].InnerText) : null;
                    }
                    else
                    {
                        var measurement = indexes.TryGetValue(ConverterConstants.INDEX_HEIGHT, out int value1) ? this.RegExpService.MatchDouble(data[value1].InnerText) : null;
                        measurement ??= indexes.TryGetValue(ConverterConstants.INDEX_DISTANCE, out int value2) ? this.RegExpService.MatchDouble(data[value2].InnerText) : null;
                        result.GroupType = groupType;
                        result.FinishStatus = this.OlympediaService.FindStatus(row.OuterHtml);
                        result.Record = this.OlympediaService.FindRecord(row.OuterHtml);
                        result.BestMeasurement = measurement;
                        result.SecondBestMeasurement = indexes.TryGetValue(ConverterConstants.INDEX_SECOND_DISTANCE, out int value4) ? this.RegExpService.MatchDouble(data[value4].InnerText) : null;
                        result.Misses = indexes.TryGetValue(ConverterConstants.INDEX_MISSES, out int value5) ? this.RegExpService.MatchInt(data[value5].InnerText) : null;
                        result.TotalAttempts = indexes.TryGetValue(ConverterConstants.INDEX_TOTAL_ATTEMPTS, out int value6) ? this.RegExpService.MatchInt(data[value6].InnerText) : null;
                        result.TotalMisses = indexes.TryGetValue(ConverterConstants.INDEX_TOTAL_MISSES, out int value7) ? this.RegExpService.MatchInt(data[value7].InnerText) : null;

                        if (eventName == "High Jump" || eventName == "Pole Vault")
                        {
                            var attemptOrder = 1;
                            for (int i = 0; i < headers.Count; i++)
                            {
                                var heightMatch = this.RegExpService.Match(headers[i], @"([\d\.\,]+)");
                                if (heightMatch is not null)
                                {
                                    var attempt = new ATHFieldAttempt
                                    {
                                        Measurement = this.RegExpService.MatchDouble(heightMatch.Groups[1].Value.Trim()),
                                        Record = this.OlympediaService.FindRecord(row.OuterHtml),
                                        Tries = this.MapATHTries(data[i].InnerText, round.EventName),
                                        AttemptOrder = attemptOrder
                                    };
                                    result.Attempts.Add(attempt);
                                    attemptOrder++;
                                }
                            }
                        }
                        else
                        {
                            if (indexes.ContainsKey(ConverterConstants.INDEX_ATH_ROUND_1))
                            {
                                result.Attempts.Add(new ATHFieldAttempt
                                {
                                    Measurement = indexes.TryGetValue(ConverterConstants.INDEX_ATH_ROUND_1, out int value10) ? this.RegExpService.MatchDouble(data[value10].InnerText) : null,
                                    Tries = this.MapATHTries(data[value10].InnerText.Trim(), round.EventName),
                                    AttemptOrder = 1,
                                    Record = this.OlympediaService.FindRecord(row.OuterHtml),
                                });
                            }
                            if (indexes.ContainsKey(ConverterConstants.INDEX_ATH_ROUND_2))
                            {
                                result.Attempts.Add(new ATHFieldAttempt
                                {
                                    Measurement = indexes.TryGetValue(ConverterConstants.INDEX_ATH_ROUND_2, out int value11) ? this.RegExpService.MatchDouble(data[value11].InnerText) : null,
                                    Tries = this.MapATHTries(data[value11].InnerText.Trim(), round.EventName),
                                    AttemptOrder = 2,
                                    Record = this.OlympediaService.FindRecord(row.OuterHtml),
                                });
                            }
                            if (indexes.ContainsKey(ConverterConstants.INDEX_ATH_ROUND_3))
                            {
                                result.Attempts.Add(new ATHFieldAttempt
                                {
                                    Measurement = indexes.TryGetValue(ConverterConstants.INDEX_ATH_ROUND_3, out int value12) ? this.RegExpService.MatchDouble(data[value12].InnerText) : null,
                                    Tries = this.MapATHTries(data[value12].InnerText.Trim(), round.EventName),
                                    AttemptOrder = 3,
                                    Record = this.OlympediaService.FindRecord(row.OuterHtml),
                                });
                            }
                        }
                    }
                }
            }
        }
    }

    private async Task SetATHCombinedAthletesAsync(ATHRound round, HtmlDocument htmlDocument, EventCacheModel eventCache)
    {
        var rows = htmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']//tr");
        var headers = rows.First().Elements("th").Select(x => x.InnerText).ToList();
        var indexes = this.OlympediaService.FindIndexes(headers);

        foreach (var row in rows.Skip(1))
        {
            var data = row.Elements("td").ToList();
            var nocCode = this.OlympediaService.FindNOCCode(row.OuterHtml);
            var nocCache = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == nocCode);
            var athleteModel = this.OlympediaService.FindAthlete(data[indexes[ConverterConstants.INDEX_NAME]].OuterHtml);
            var participant = await this.participantsService.GetAsync(athleteModel.Number, eventCache.Id, nocCache.Id);
            if (participant != null)
            {
                var athlete = new ATHCombinedAthlete
                {
                    Name = athleteModel.Name,
                    NOCCode = nocCode,
                    ParticipantId = participant.Id,
                    AthleteNumber = athleteModel.Number,
                    FinishStatus = this.OlympediaService.FindStatus(row.OuterHtml),
                    Record = this.OlympediaService.FindRecord(row.OuterHtml),
                    Number = indexes.TryGetValue(ConverterConstants.INDEX_NR, out int value1) ? this.RegExpService.MatchInt(data[value1].InnerText) : null,
                    Points = indexes.TryGetValue(ConverterConstants.INDEX_POINTS, out int value2) ? this.RegExpService.MatchDouble(data[value2].InnerText) : null,
                };

                round.Combined.Athletes.Add(athlete);
            }
        }
    }

    private async Task SetATHFieldAthletesAsync(ATHRound round, HtmlDocument htmlDocument, EventCacheModel eventCache)
    {
        var rows = htmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']//tr");
        var headers = rows.First().Elements("th").Select(x => x.InnerText).ToList();
        var indexes = this.OlympediaService.FindIndexes(headers);

        foreach (var row in rows.Skip(1))
        {
            if (row.InnerHtml.StartsWith("<th>"))
            {
                return;
            }

            var data = row.Elements("td").ToList();
            var nocCode = this.OlympediaService.FindNOCCode(row.OuterHtml);
            var nocCache = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == nocCode);
            var athleteModel = this.OlympediaService.FindAthlete(data[indexes[ConverterConstants.INDEX_NAME]].OuterHtml);
            var participant = await this.participantsService.GetAsync(athleteModel.Number, eventCache.Id, nocCache.Id);
            if (participant is not null)
            {
                var measurement = indexes.TryGetValue(ConverterConstants.INDEX_HEIGHT, out int value1) ? this.RegExpService.MatchDouble(data[value1].InnerText) : null;
                measurement ??= indexes.TryGetValue(ConverterConstants.INDEX_DISTANCE, out int value2) ? this.RegExpService.MatchDouble(data[value2].InnerText) : null;

                var athlete = new ATHFieldAthlete
                {
                    Name = athleteModel.Name,
                    NOCCode = nocCode,
                    ParticipantId = participant.Id,
                    AthleteNumber = athleteModel.Number,
                    FinishStatus = this.OlympediaService.FindStatus(row.OuterHtml),
                    Qualification = this.OlympediaService.FindQualification(row.OuterHtml),
                    Record = this.OlympediaService.FindRecord(row.OuterHtml),
                    BestMeasurement = measurement,
                    SecondBestMeasurement = indexes.TryGetValue(ConverterConstants.INDEX_SECOND_DISTANCE, out int value4) ? this.RegExpService.MatchDouble(data[value4].InnerText) : null,
                    Number = indexes.TryGetValue(ConverterConstants.INDEX_NR, out int value3) ? this.RegExpService.MatchInt(data[value3].InnerText) : null,
                    Misses = indexes.TryGetValue(ConverterConstants.INDEX_MISSES, out int value5) ? this.RegExpService.MatchInt(data[value5].InnerText) : null,
                    TotalAttempts = indexes.TryGetValue(ConverterConstants.INDEX_TOTAL_ATTEMPTS, out int value6) ? this.RegExpService.MatchInt(data[value6].InnerText) : null,
                    TotalMisses = indexes.TryGetValue(ConverterConstants.INDEX_TOTAL_MISSES, out int value7) ? this.RegExpService.MatchInt(data[value7].InnerText) : null,
                };

                if (indexes.ContainsKey(ConverterConstants.INDEX_ORDER))
                {
                    var order = indexes.TryGetValue(ConverterConstants.INDEX_TOTAL_ATTEMPTS, out int value20) ? this.RegExpService.MatchInt(data[value20].InnerText) : null;
                    if (order != null)
                    {
                        athlete.Orders.Add((int)order);
                    }
                }

                if (indexes.ContainsKey(ConverterConstants.INDEX_ORDER))
                {
                    var order = indexes.TryGetValue(ConverterConstants.INDEX_TOTAL_ATTEMPTS, out int value20) ? this.RegExpService.MatchInt(data[value20].InnerText) : null;
                    if (order != null)
                    {
                        athlete.Orders.Add((int)order);
                    }
                }

                if (indexes.ContainsKey(ConverterConstants.INDEX_ATH_ORDER))
                {
                    var order = indexes.TryGetValue(ConverterConstants.INDEX_ATH_ORDER, out int value21) ? data[value21].InnerText : null;
                    if (order != null)
                    {
                        athlete.Orders = order.Split(new[] { "-" }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                    }
                }

                var attemptOrder = 1;
                if (round.EventName == "High Jump" || round.EventName == "Standing High Jump" || round.EventName == "Pole Vault")
                {
                    for (int i = 0; i < headers.Count; i++)
                    {
                        var heightMatch = this.RegExpService.Match(headers[i], @"([\d\.\,]+)\s*m");
                        if (heightMatch is not null)
                        {
                            var attempt = new ATHFieldAttempt
                            {
                                Measurement = this.RegExpService.MatchDouble(heightMatch.Groups[1].Value.Trim()),
                                Record = this.OlympediaService.FindRecord(row.OuterHtml),
                                Tries = this.MapATHTries(data[i].InnerText, round.EventName),
                                AttemptOrder = attemptOrder
                            };
                            athlete.Attempts.Add(attempt);
                            attemptOrder++;
                        }
                    }
                }
                else
                {
                    if (indexes.ContainsKey(ConverterConstants.INDEX_ATH_ROUND_1))
                    {
                        athlete.Attempts.Add(new ATHFieldAttempt
                        {
                            Measurement = indexes.TryGetValue(ConverterConstants.INDEX_ATH_ROUND_1, out int value10) ? this.RegExpService.MatchDouble(data[value10].InnerText) : null,
                            Tries = this.MapATHTries(data[value10].InnerText.Trim(), round.EventName),
                            AttemptOrder = 1,
                            Record = this.OlympediaService.FindRecord(row.OuterHtml),
                        });
                    }
                    if (indexes.ContainsKey(ConverterConstants.INDEX_ATH_ROUND_2))
                    {
                        athlete.Attempts.Add(new ATHFieldAttempt
                        {
                            Measurement = indexes.TryGetValue(ConverterConstants.INDEX_ATH_ROUND_2, out int value11) ? this.RegExpService.MatchDouble(data[value11].InnerText) : null,
                            Tries = this.MapATHTries(data[value11].InnerText.Trim(), round.EventName),
                            AttemptOrder = 2,
                            Record = this.OlympediaService.FindRecord(row.OuterHtml),
                        });
                    }
                    if (indexes.ContainsKey(ConverterConstants.INDEX_ATH_ROUND_3))
                    {
                        athlete.Attempts.Add(new ATHFieldAttempt
                        {
                            Measurement = indexes.TryGetValue(ConverterConstants.INDEX_ATH_ROUND_3, out int value12) ? this.RegExpService.MatchDouble(data[value12].InnerText) : null,
                            Tries = this.MapATHTries(data[value12].InnerText.Trim(), round.EventName),
                            AttemptOrder = 3,
                            Record = this.OlympediaService.FindRecord(row.OuterHtml),
                        });
                    }
                    if (indexes.ContainsKey(ConverterConstants.INDEX_ATH_ROUND_4))
                    {
                        athlete.Attempts.Add(new ATHFieldAttempt
                        {
                            Measurement = indexes.TryGetValue(ConverterConstants.INDEX_ATH_ROUND_4, out int value13) ? this.RegExpService.MatchDouble(data[value13].InnerText) : null,
                            Tries = this.MapATHTries(data[value13].InnerText.Trim(), round.EventName),
                            AttemptOrder = 4,
                            Record = this.OlympediaService.FindRecord(row.OuterHtml),
                        });
                    }
                    if (indexes.ContainsKey(ConverterConstants.INDEX_ATH_ROUND_5))
                    {
                        athlete.Attempts.Add(new ATHFieldAttempt
                        {
                            Measurement = indexes.TryGetValue(ConverterConstants.INDEX_ATH_ROUND_5, out int value14) ? this.RegExpService.MatchDouble(data[value14].InnerText) : null,
                            Tries = this.MapATHTries(data[value14].InnerText.Trim(), round.EventName),
                            AttemptOrder = 5,
                            Record = this.OlympediaService.FindRecord(row.OuterHtml),
                        });
                    }
                    if (indexes.ContainsKey(ConverterConstants.INDEX_ATH_ROUND_6))
                    {
                        athlete.Attempts.Add(new ATHFieldAttempt
                        {
                            Measurement = indexes.TryGetValue(ConverterConstants.INDEX_ATH_ROUND_6, out int value15) ? this.RegExpService.MatchDouble(data[value15].InnerText) : null,
                            Tries = this.MapATHTries(data[value15].InnerText.Trim(), round.EventName),
                            AttemptOrder = 6,
                            Record = this.OlympediaService.FindRecord(row.OuterHtml),
                        });
                    }
                }

                round.Field.Athletes.Add(athlete);
            }
        }
    }

    private List<ATHFieldTry> MapATHTries(string symbols, string eventName)
    {
        var result = new List<ATHFieldTry>();
        if (string.IsNullOrEmpty(symbols))
        {
            return result;
        }

        if (eventName == "High Jump" || eventName == "Standing High Jump" || eventName == "Pole Vault")
        {
            foreach (var symbol in symbols)
            {
                var @try = ATHFieldTry.None;
                switch (symbol)
                {
                    case '-': @try = ATHFieldTry.Skip; break;
                    case 'o': @try = ATHFieldTry.Success; break;
                    case 'x': @try = ATHFieldTry.Fail; break;
                }
                result.Add(@try);
            }
        }
        else
        {
            var firstChar = symbols.Trim()[0];
            switch (firstChar)
            {
                case '×':
                case 'x':
                    result.Add(ATHFieldTry.Fail);
                    break;
                case 'p':
                    result.Add(ATHFieldTry.None);
                    break;
                case '–':
                    result.Add(ATHFieldTry.Skip);
                    break;
                default:
                    result.Add(ATHFieldTry.Success);
                    break;
            }
        }


        return result;
    }

    private async Task SetATHCrossCountryAsync(ATHRound round, HtmlDocument htmlDocument, EventCacheModel eventCache)
    {
        var rows = htmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']//tr");
        var headers = rows.First().Elements("th").Select(x => x.InnerText).ToList();
        var indexes = this.OlympediaService.FindIndexes(headers);

        foreach (var row in rows.Skip(1))
        {
            var data = row.Elements("td").ToList();
            var nocCode = this.OlympediaService.FindNOCCode(row.OuterHtml);
            var name = data[indexes[ConverterConstants.INDEX_NAME]].InnerText;

            if (eventCache.IsTeamEvent && nocCode is not null)
            {
                var nocCache = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == nocCode);
                var team = await this.teamsService.GetAsync(nocCache.Id, eventCache.Id);
                var crossCountryTeam = new ATHCrossCountryTeam
                {
                    Name = name,
                    NOCCode = nocCode,
                    TeamId = team.Id,
                    FinishStatus = this.OlympediaService.FindStatus(row.OuterHtml),
                    Points = indexes.TryGetValue(ConverterConstants.INDEX_POINTS, out int value1) ? this.RegExpService.MatchInt(data[value1].InnerText) : null,
                    Time = indexes.TryGetValue(ConverterConstants.INDEX_TIME, out int value2) ? this.dateService.ParseTime(data[value2].InnerText) : null,
                };

                round.CrossCountry.Teams.Add(crossCountryTeam);
            }
            else
            {
                nocCode ??= round.CrossCountry.Teams.Last().NOCCode;
                var nocCache = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == nocCode);
                var athleteModel = this.OlympediaService.FindAthlete(data[indexes[ConverterConstants.INDEX_NAME]].OuterHtml);
                var participant = await this.participantsService.GetAsync(athleteModel.Number, eventCache.Id, nocCache.Id);
                if (participant != null)
                {
                    var athlete = new ATHCrossCountryAthlete
                    {
                        AthleteNumber = athleteModel.Number,
                        ParticipantId = participant.Id,
                        NOCCode = nocCode,
                        Name = name,
                        FinishStatus = this.OlympediaService.FindStatus(row.OuterHtml),
                        Number = indexes.TryGetValue(ConverterConstants.INDEX_NR, out int value1) ? this.RegExpService.MatchInt(data[value1].InnerText) : null,
                        Time = indexes.TryGetValue(ConverterConstants.INDEX_TIME, out int value2) ? this.dateService.ParseTime(data[value2].InnerText) : null,
                        Points = indexes.TryGetValue(ConverterConstants.INDEX_POINTS, out int value3) ? this.RegExpService.MatchInt(data[value3].InnerText) : null
                    };

                    if (eventCache.IsTeamEvent)
                    {
                        round.CrossCountry.Teams.Last().Athletes.Add(athlete);
                    }
                    else
                    {
                        round.CrossCountry.Athletes.Add(athlete);
                    }
                }
            }
        }
    }

    private async Task SetATHRoadAthletesAsync(ATHRound round, HtmlDocument htmlDocument, EventCacheModel eventCache)
    {
        var rows = htmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']//tr");
        var headers = rows.First().Elements("th").Select(x => x.InnerText).ToList();
        var indexes = this.OlympediaService.FindIndexes(headers);

        foreach (var row in rows.Skip(1))
        {
            var data = row.Elements("td").ToList();
            var nocCode = this.OlympediaService.FindNOCCode(row.OuterHtml);
            var nocCache = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == nocCode);
            var athleteModel = this.OlympediaService.FindAthlete(row.OuterHtml);
            var participant = await this.participantsService.GetAsync(athleteModel.Number, eventCache.Id, nocCache.Id);

            if (participant is not null)
            {
                var athlete = new ATHRoadAthlete
                {
                    Name = athleteModel.Name,
                    ParticipantId = participant.Id,
                    AthleteNumber = athleteModel.Number,
                    NOCCode = nocCode,
                    FinishStatus = this.OlympediaService.FindStatus(row.OuterHtml),
                    Record = this.OlympediaService.FindRecord(row.OuterHtml),
                    Number = indexes.TryGetValue(ConverterConstants.INDEX_NR, out int value1) ? this.RegExpService.MatchInt(data[value1].InnerText) : null,
                    Time = indexes.TryGetValue(ConverterConstants.INDEX_TIME, out int value2) ? this.dateService.ParseTime(data[value2].InnerText) : null,
                    BentKneeWarnings = indexes.TryGetValue(ConverterConstants.INDEX_BENT_KNEE, out int value3) ? this.RegExpService.MatchInt(data[value3].InnerText) : null,
                    LostOfContactWarnings = indexes.TryGetValue(ConverterConstants.INDEX_LOST_OF_CONTACT, out int value4) ? this.RegExpService.MatchInt(data[value4].InnerText) : null,
                    TotalWarnings = indexes.TryGetValue(ConverterConstants.INDEX_WARNINGS, out int value5) ? this.RegExpService.MatchInt(data[value5].InnerText) : null,
                    HalfSplit = indexes.TryGetValue(ConverterConstants.INDEX_HALF_SPLIT, out int value6) ? this.dateService.ParseTime(data[value6].InnerText) : null,
                    KM1Split = indexes.TryGetValue(ConverterConstants.INDEX_KM1_SPLIT, out int value11) ? this.dateService.ParseTime(data[value11].InnerText) : null,
                    KM2Split = indexes.TryGetValue(ConverterConstants.INDEX_KM2_SPLIT, out int value12) ? this.dateService.ParseTime(data[value12].InnerText) : null,
                    KM3Split = indexes.TryGetValue(ConverterConstants.INDEX_KM3_SPLIT, out int value13) ? this.dateService.ParseTime(data[value13].InnerText) : null,
                    KM4Split = indexes.TryGetValue(ConverterConstants.INDEX_KM4_SPLIT, out int value14) ? this.dateService.ParseTime(data[value14].InnerText) : null,
                    KM5Split = indexes.TryGetValue(ConverterConstants.INDEX_KM5_SPLIT, out int value15) ? this.dateService.ParseTime(data[value15].InnerText) : null,
                    KM6Split = indexes.TryGetValue(ConverterConstants.INDEX_KM6_SPLIT, out int value16) ? this.dateService.ParseTime(data[value16].InnerText) : null,
                    KM7Split = indexes.TryGetValue(ConverterConstants.INDEX_KM7_SPLIT, out int value17) ? this.dateService.ParseTime(data[value17].InnerText) : null,
                    KM8Split = indexes.TryGetValue(ConverterConstants.INDEX_KM8_SPLIT, out int value18) ? this.dateService.ParseTime(data[value18].InnerText) : null,
                    KM9Split = indexes.TryGetValue(ConverterConstants.INDEX_KM9_SPLIT, out int value19) ? this.dateService.ParseTime(data[value19].InnerText) : null,
                    KM10Split = indexes.TryGetValue(ConverterConstants.INDEX_KM10_SPLIT, out int value20) ? this.dateService.ParseTime(data[value20].InnerText) : null,
                    KM11Split = indexes.TryGetValue(ConverterConstants.INDEX_KM11_SPLIT, out int value21) ? this.dateService.ParseTime(data[value21].InnerText) : null,
                    KM12Split = indexes.TryGetValue(ConverterConstants.INDEX_KM12_SPLIT, out int value22) ? this.dateService.ParseTime(data[value22].InnerText) : null,
                    KM13Split = indexes.TryGetValue(ConverterConstants.INDEX_KM13_SPLIT, out int value23) ? this.dateService.ParseTime(data[value23].InnerText) : null,
                    KM14Split = indexes.TryGetValue(ConverterConstants.INDEX_KM14_SPLIT, out int value24) ? this.dateService.ParseTime(data[value24].InnerText) : null,
                    KM15Split = indexes.TryGetValue(ConverterConstants.INDEX_KM15_SPLIT, out int value25) ? this.dateService.ParseTime(data[value25].InnerText) : null,
                    KM16Split = indexes.TryGetValue(ConverterConstants.INDEX_KM16_SPLIT, out int value26) ? this.dateService.ParseTime(data[value26].InnerText) : null,
                    KM17Split = indexes.TryGetValue(ConverterConstants.INDEX_KM17_SPLIT, out int value27) ? this.dateService.ParseTime(data[value27].InnerText) : null,
                    KM18Split = indexes.TryGetValue(ConverterConstants.INDEX_KM18_SPLIT, out int value28) ? this.dateService.ParseTime(data[value28].InnerText) : null,
                    KM19Split = indexes.TryGetValue(ConverterConstants.INDEX_KM19_SPLIT, out int value29) ? this.dateService.ParseTime(data[value29].InnerText) : null,
                    KM20Split = indexes.TryGetValue(ConverterConstants.INDEX_KM20_SPLIT, out int value30) ? this.dateService.ParseTime(data[value30].InnerText) : null,
                    KM25Split = indexes.TryGetValue(ConverterConstants.INDEX_KM25_SPLIT, out int value31) ? this.dateService.ParseTime(data[value31].InnerText) : null,
                    KM26Split = indexes.TryGetValue(ConverterConstants.INDEX_KM26_SPLIT, out int value32) ? this.dateService.ParseTime(data[value32].InnerText) : null,
                    KM28Split = indexes.TryGetValue(ConverterConstants.INDEX_KM28_SPLIT, out int value33) ? this.dateService.ParseTime(data[value33].InnerText) : null,
                    KM30Split = indexes.TryGetValue(ConverterConstants.INDEX_KM30_SPLIT, out int value34) ? this.dateService.ParseTime(data[value34].InnerText) : null,
                    KM31Split = indexes.TryGetValue(ConverterConstants.INDEX_KM31_SPLIT, out int value35) ? this.dateService.ParseTime(data[value35].InnerText) : null,
                    KM35Split = indexes.TryGetValue(ConverterConstants.INDEX_KM35_SPLIT, out int value36) ? this.dateService.ParseTime(data[value36].InnerText) : null,
                    KM36Split = indexes.TryGetValue(ConverterConstants.INDEX_KM36_SPLIT, out int value37) ? this.dateService.ParseTime(data[value37].InnerText) : null,
                    KM37Split = indexes.TryGetValue(ConverterConstants.INDEX_KM37_SPLIT, out int value38) ? this.dateService.ParseTime(data[value38].InnerText) : null,
                    KM38Split = indexes.TryGetValue(ConverterConstants.INDEX_KM38_SPLIT, out int value39) ? this.dateService.ParseTime(data[value39].InnerText) : null,
                    KM40Split = indexes.TryGetValue(ConverterConstants.INDEX_KM40_SPLIT, out int value40) ? this.dateService.ParseTime(data[value40].InnerText) : null,
                    KM45Split = indexes.TryGetValue(ConverterConstants.INDEX_KM45_SPLIT, out int value41) ? this.dateService.ParseTime(data[value41].InnerText) : null,
                    KM46Split = indexes.TryGetValue(ConverterConstants.INDEX_KM46_SPLIT, out int value42) ? this.dateService.ParseTime(data[value42].InnerText) : null,
                };

                round.Road.Athletes.Add(athlete);
            }
        }
    }

    private async Task SetATHTrackAthletesAsync(ATHRound round, HtmlDocument htmlDocument, EventCacheModel eventCache, HeatType heat, double? wind, bool checkSplits)
    {
        if (checkSplits)
        {
            htmlDocument = this.SetAthleticsSlipts(round, htmlDocument, heat);
        }

        var rows = htmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']//tr");
        if (rows is not null)
        {
            var headers = rows.First().Elements("th").Select(x => x.InnerText).ToList();
            var indexes = this.OlympediaService.FindIndexes(headers);

            foreach (var row in rows.Skip(1))
            {
                var data = row.Elements("td").ToList();
                var nocCode = this.OlympediaService.FindNOCCode(row.OuterHtml);
                var name = data[indexes[ConverterConstants.INDEX_NAME]].InnerText;

                if (eventCache.IsTeamEvent && nocCode is not null)
                {
                    var nocCache = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == nocCode);
                    var team = await this.teamsService.GetAsync(nocCache.Id, eventCache.Id);
                    var trackTeam = new ATHTrackTeam
                    {
                        Name = name,
                        NOCCode = nocCode,
                        TeamId = team.Id,
                        FinishStatus = this.OlympediaService.FindStatus(row.OuterHtml),
                        Qualification = this.OlympediaService.FindQualification(row.OuterHtml),
                        Record = this.OlympediaService.FindRecord(row.OuterHtml),
                        Heat = heat,
                        Wind = wind,
                        Points = indexes.TryGetValue(ConverterConstants.INDEX_POINTS, out int value1) ? this.RegExpService.MatchInt(data[value1].InnerText) : null,
                        Lane = indexes.TryGetValue(ConverterConstants.INDEX_LANE, out int value2) ? this.RegExpService.MatchInt(data[value2].InnerText) : null,
                        ReactionTime = indexes.TryGetValue(ConverterConstants.INDEX_REACTION_TIME, out int value4) ? this.RegExpService.MatchDouble(data[value4].InnerText) : null,
                        TieBreakingTime = indexes.TryGetValue(ConverterConstants.INDEX_TIE_BREAKING_TIME, out int value5) ? this.dateService.ParseTime(data[value5].InnerText) : null,
                        Time = indexes.TryGetValue(ConverterConstants.INDEX_TIME, out int value6) ? this.dateService.ParseTime(data[value6].InnerText) : null,
                        TimeAutomatic = indexes.TryGetValue(ConverterConstants.INDEX_TIME_AUTOMATIC, out int value7) ? this.dateService.ParseTime(data[value7].InnerText) : null,
                        TimeHand = indexes.TryGetValue(ConverterConstants.INDEX_TIME_HAND, out int value8) ? this.dateService.ParseTime(data[value8].InnerText) : null,
                    };

                    round.Track.Teams.Add(trackTeam);
                }
                else
                {
                    nocCode ??= round.Track.Teams.Last().NOCCode;
                    var nocCache = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == nocCode);
                    var athleteModels = this.OlympediaService.FindAthletes(data[indexes[ConverterConstants.INDEX_NAME]].OuterHtml);
                    foreach (var athleteModel in athleteModels)
                    {
                        var participant = await this.participantsService.GetAsync(athleteModel.Number, eventCache.Id, nocCache.Id);
                        if (participant != null)
                        {
                            if (athleteModels.Count > 1)
                            {
                                round.Track.Teams.Last().Athletes.Add(new ATHTrackAthlete
                                {
                                    AthleteNumber = athleteModel.Number,
                                    Name = athleteModel.Name,
                                    ParticipantId = participant.Id,
                                    NOCCode = nocCode
                                });
                            }
                            else
                            {
                                var athlete = new ATHTrackAthlete
                                {
                                    AthleteNumber = athleteModel.Number,
                                    ParticipantId = participant.Id,
                                    NOCCode = nocCode,
                                    Name = name,
                                    Heat = heat,
                                    Wind = wind,
                                    FinishStatus = this.OlympediaService.FindStatus(row.OuterHtml),
                                    Qualification = this.OlympediaService.FindQualification(row.OuterHtml),
                                    Record = this.OlympediaService.FindRecord(row.OuterHtml),
                                    Number = indexes.TryGetValue(ConverterConstants.INDEX_NR, out int value1) ? this.RegExpService.MatchInt(data[value1].InnerText) : null,
                                    Lane = indexes.TryGetValue(ConverterConstants.INDEX_LANE, out int value2) ? this.RegExpService.MatchInt(data[value2].InnerText) : null,
                                    Order = indexes.TryGetValue(ConverterConstants.INDEX_ORDER, out int value3) ? this.RegExpService.MatchInt(data[value3].InnerText) : null,
                                    ReactionTime = indexes.TryGetValue(ConverterConstants.INDEX_REACTION_TIME, out int value4) ? this.RegExpService.MatchDouble(data[value4].InnerText) : null,
                                    TieBreakingTime = indexes.TryGetValue(ConverterConstants.INDEX_REACTION_TIME, out int value5) ? this.dateService.ParseTime(data[value5].InnerText) : null,
                                    Time = indexes.TryGetValue(ConverterConstants.INDEX_TIME, out int value6) ? this.dateService.ParseTime(data[value6].InnerText) : null,
                                    TimeAutomatic = indexes.TryGetValue(ConverterConstants.INDEX_TIME_AUTOMATIC, out int value7) ? this.dateService.ParseTime(data[value7].InnerText) : null,
                                    TimeHand = indexes.TryGetValue(ConverterConstants.INDEX_TIME_HAND, out int value8) ? this.dateService.ParseTime(data[value8].InnerText) : null,
                                    ExchangeTime = indexes.TryGetValue(ConverterConstants.INDEX_EXCHANGE_TIME, out int value9) ? this.dateService.ParseTime(data[value9].InnerText) : null,
                                    SplitTime = indexes.TryGetValue(ConverterConstants.INDEX_SPLIT_TIME, out int value10) ? this.dateService.ParseTime(data[value10].InnerText) : null,
                                    SplitRank = indexes.TryGetValue(ConverterConstants.INDEX_SPLIT_RANK, out int value11) ? this.RegExpService.MatchInt(data[value11].InnerText) : null,
                                    Points = indexes.TryGetValue(ConverterConstants.INDEX_POINTS, out int value12) ? this.RegExpService.MatchInt(data[value12].InnerText) : null
                                };

                                if (eventCache.IsTeamEvent)
                                {
                                    round.Track.Teams.Last().Athletes.Add(athlete);
                                }
                                else
                                {
                                    round.Track.Athletes.Add(athlete);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private HtmlDocument SetAthleticsSlipts(ATHRound round, HtmlDocument htmlDocument, HeatType heat)
    {
        var patterns = new Dictionary<int, string>
        {
            { 1, @"Splits<\/h4>\s*<table class=""table table-striped"">(.*?)<\/table>" },
            { 2, @"Split Times<\/h2>\s*<table class=""table table-striped"">(.*?)<\/table>" },
        };

        foreach (var kvp in patterns)
        {
            var match = this.RegExpService.Match(htmlDocument.DocumentNode.OuterHtml, kvp.Value);
            if (match is not null)
            {
                var document = new HtmlDocument();
                document.LoadHtml(match.Groups[1].Value);
                var rows = document.DocumentNode.SelectNodes("//tr");

                switch (kvp.Key)
                {
                    case 1:
                        foreach (var row in rows.Where(x => !string.IsNullOrEmpty(x.InnerHtml)))
                        {
                            var data = row.Elements("td").ToList();
                            round.Track.Splits.Add(new ATHTrackSplit
                            {
                                Heat = heat,
                                Distance = this.RegExpService.MatchInt(data[0].InnerText),
                                Number = this.OlympediaService.FindAthlete(row.OuterHtml).Number,
                                Time = this.dateService.ParseTime(data[1].InnerText)
                            });
                        }
                        break;
                    case 2:
                        var headers = rows.First().Elements("th").Select(x => x.InnerText).ToList();
                        var indexes = this.OlympediaService.FindIndexes(headers);
                        foreach (var row in rows.Skip(1))
                        {
                            var data = row.Elements("td").ToList();
                            round.Track.Splits.Add(new ATHTrackSplit
                            {
                                Heat = heat,
                                Distance = this.RegExpService.MatchInt(data[0].InnerText),
                                Number = this.OlympediaService.FindAthlete(row.OuterHtml).Number,
                                Time = indexes.TryGetValue(ConverterConstants.INDEX_RESULT, out int value1) ? this.dateService.ParseTime(data[value1].InnerText) : null,
                            });
                        }
                        break;
                }

                var html = htmlDocument.DocumentNode.OuterHtml.Replace(match.Groups[0].Value, string.Empty);
                htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(html);
            }
        }

        return htmlDocument;
    }

    private ATHRound CreateAthleticsRound(DateTime? date, string format, RoundType roundType, string eventName, ATHEventGroup group)
    {
        return new ATHRound
        {
            Date = date,
            Format = format,
            Round = roundType,
            EventName = eventName,
            EventGroup = group
        };
    }

    #endregion ATHLETICS

    #region ARTISTIC SWIMMING
    private async Task ProcessArtisticSwimmingAsync(ConvertOptions options)
    {
        var eventRound = this.CreateEventRound<SWARound>(options.HtmlDocument, options.Event.Name);

        if (eventRound.EventName == "Team")
        {
            var round = this.CreateArtisticSwimmingRound(eventRound.Dates.From, eventRound.Format, RoundType.Final, eventRound.EventName);
            await this.SetSWAEventResultsAsync(round, options.StandingTable, options.Event, null);
            eventRound.Rounds.Add(round);

            foreach (var document in options.Documents)
            {
                var htmlDocument = this.CreateHtmlDocument(document);
                var table = this.GetStandingTable(htmlDocument, options.Event);
                var info = htmlDocument.DocumentNode.SelectSingleNode("//h1").InnerText;
                info = info.Replace(options.Event.OriginalName, string.Empty).Replace("–", string.Empty).Trim();

                await this.SetSWAEventResultsAsync(round, table, options.Event, info);
            }
        }
        else
        {
            foreach (var table in options.Tables)
            {
                var dateString = this.RegExpService.MatchFirstGroup(table.HtmlDocument.DocumentNode.OuterHtml, @"<th>\s*Date\s*<\/th>\s*<td>(.*?)<\/td>");
                var dateModel = this.dateService.ParseDate(dateString);
                var format = this.RegExpService.MatchFirstGroup(table.HtmlDocument.DocumentNode.OuterHtml, @"<th>Format<\/th>\s*<td(?:.*?)>(.*?)<\/td>");

                var roundType = this.NormalizeService.MapRoundType(table.Title);
                var round = this.CreateArtisticSwimmingRound(dateModel.From, format, roundType, eventRound.EventName);
                await this.SetSWAEventResultsAsync(round, table, options.Event, null);
                eventRound.Rounds.Add(round);
            }

            foreach (var document in options.Documents)
            {
                var htmlDocument = this.CreateHtmlDocument(document);
                var table = this.GetStandingTable(htmlDocument, options.Event);
                var info = htmlDocument.DocumentNode.SelectSingleNode("//h1").InnerText;
                info = info.Replace(options.Event.OriginalName, string.Empty).Replace("–", string.Empty).Trim();
                var rowsCount = table.HtmlDocument.DocumentNode.SelectNodes("//tr").Skip(1).Count();
                var currentEvent = eventRound.Rounds.Where(x => x.Duets.Count == rowsCount).FirstOrDefault();

                await this.SetSWAEventResultsAsync(currentEvent, table, options.Event, info);
            }
        }

        var resultJson = this.CreateResult<SWARound>(options.Event, options.Discipline, options.Game);
        resultJson.Rounds = eventRound.Rounds;
        var json = JsonSerializer.Serialize(resultJson);
        var result = new Result
        {
            EventId = options.Event.Id,
            Json = json
        };

        await this.resultsService.AddOrUpdateAsync(result);
    }

    private async Task SetSWAEventResultsAsync(SWARound round, TableModel table, EventCacheModel eventCache, string info)
    {
        var rows = table.HtmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']//tr");
        var headers = rows.First().Elements("th").Select(x => x.InnerText).ToList();
        var indexes = this.OlympediaService.FindIndexes(headers);

        foreach (var row in rows.Skip(1))
        {
            var data = row.Elements("td").ToList();
            var nocCode = this.OlympediaService.FindNOCCode(row.OuterHtml);
            var noc = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == nocCode);
            var athleteModels = this.OlympediaService.FindAthletes(row.OuterHtml);

            if (!string.IsNullOrEmpty(info))
            {
                if (string.IsNullOrEmpty(nocCode))
                {
                    continue;
                }

                var technicalRoutine = new SWATechnicalRoutine();
                var freeRoutine = new SWAFreeRoutine();
                if (info == "Technical Routine")
                {
                    technicalRoutine.Points = indexes.TryGetValue(ConverterConstants.INDEX_POINTS, out int value4) ? this.RegExpService.MatchDouble(data[value4].InnerText) : null;
                    technicalRoutine.ArtisticImpression = indexes.TryGetValue(ConverterConstants.INDEX_SWA_ARTISTIC_IMPRESSION, out int value5) ? this.RegExpService.MatchDouble(data[value5].InnerText) : null;
                    technicalRoutine.Difficulty = indexes.TryGetValue(ConverterConstants.INDEX_SWA_DIFFICULTY, out int value6) ? this.RegExpService.MatchDouble(data[value6].InnerText) : null;
                    technicalRoutine.Execution = indexes.TryGetValue(ConverterConstants.INDEX_SWA_EXECUTION, out int value7) ? this.RegExpService.MatchDouble(data[value7].InnerText) : null;
                    technicalRoutine.ExecutionJudge1 = indexes.TryGetValue(ConverterConstants.INDEX_SWA_EXECUTION_JUDGE_1_POINTS, out int value8) ? this.RegExpService.MatchDouble(data[value8].InnerText) : null;
                    technicalRoutine.ExecutionJudge2 = indexes.TryGetValue(ConverterConstants.INDEX_SWA_EXECUTION_JUDGE_2_POINTS, out int value9) ? this.RegExpService.MatchDouble(data[value9].InnerText) : null;
                    technicalRoutine.ExecutionJudge3 = indexes.TryGetValue(ConverterConstants.INDEX_SWA_EXECUTION_JUDGE_3_POINTS, out int value10) ? this.RegExpService.MatchDouble(data[value10].InnerText) : null;
                    technicalRoutine.ExecutionJudge4 = indexes.TryGetValue(ConverterConstants.INDEX_SWA_EXECUTION_JUDGE_4_POINTS, out int value11) ? this.RegExpService.MatchDouble(data[value11].InnerText) : null;
                    technicalRoutine.ExecutionJudge5 = indexes.TryGetValue(ConverterConstants.INDEX_SWA_EXECUTION_JUDGE_5_POINTS, out int value12) ? this.RegExpService.MatchDouble(data[value12].InnerText) : null;
                    technicalRoutine.ExecutionJudge6 = indexes.TryGetValue(ConverterConstants.INDEX_SWA_EXECUTION_JUDGE_6_POINTS, out int value13) ? this.RegExpService.MatchDouble(data[value13].InnerText) : null;
                    technicalRoutine.ExecutionJudge7 = indexes.TryGetValue(ConverterConstants.INDEX_SWA_EXECUTION_JUDGE_7_POINTS, out int value14) ? this.RegExpService.MatchDouble(data[value14].InnerText) : null;
                    technicalRoutine.OverallImpression = indexes.TryGetValue(ConverterConstants.INDEX_SWA_OVERALL_IMPRESSION, out int value15) ? this.RegExpService.MatchDouble(data[value15].InnerText) : null;
                    technicalRoutine.OverallImpressionJudge1 = indexes.TryGetValue(ConverterConstants.INDEX_SWA_OVERALL_IMPRESSION_JUDGE_1_POINTS, out int value16) ? this.RegExpService.MatchDouble(data[value16].InnerText) : null;
                    technicalRoutine.OverallImpressionJudge2 = indexes.TryGetValue(ConverterConstants.INDEX_SWA_OVERALL_IMPRESSION_JUDGE_2_POINTS, out int value17) ? this.RegExpService.MatchDouble(data[value17].InnerText) : null;
                    technicalRoutine.OverallImpressionJudge3 = indexes.TryGetValue(ConverterConstants.INDEX_SWA_OVERALL_IMPRESSION_JUDGE_3_POINTS, out int value18) ? this.RegExpService.MatchDouble(data[value18].InnerText) : null;
                    technicalRoutine.OverallImpressionJudge4 = indexes.TryGetValue(ConverterConstants.INDEX_SWA_OVERALL_IMPRESSION_JUDGE_4_POINTS, out int value19) ? this.RegExpService.MatchDouble(data[value19].InnerText) : null;
                    technicalRoutine.OverallImpressionJudge5 = indexes.TryGetValue(ConverterConstants.INDEX_SWA_OVERALL_IMPRESSION_JUDGE_5_POINTS, out int value20) ? this.RegExpService.MatchDouble(data[value20].InnerText) : null;
                    technicalRoutine.OverallImpressionJudge6 = indexes.TryGetValue(ConverterConstants.INDEX_SWA_OVERALL_IMPRESSION_JUDGE_6_POINTS, out int value21) ? this.RegExpService.MatchDouble(data[value21].InnerText) : null;
                    technicalRoutine.OverallImpressionJudge7 = indexes.TryGetValue(ConverterConstants.INDEX_SWA_OVERALL_IMPRESSION_JUDGE_7_POINTS, out int value22) ? this.RegExpService.MatchDouble(data[value22].InnerText) : null;
                    technicalRoutine.RequiredElementPenalty = indexes.TryGetValue(ConverterConstants.INDEX_SWA_PENALTIES, out int value23) ? this.RegExpService.MatchDouble(data[value23].InnerText) : null;
                    technicalRoutine.Routine1DegreeOfDifficulty = indexes.TryGetValue(ConverterConstants.INDEX_SWA_ROUTINE_1_DEGREE_OF_DIFFICULTY, out int value24) ? this.RegExpService.MatchDouble(data[value24].InnerText) : null;
                    technicalRoutine.Routine2DegreeOfDifficulty = indexes.TryGetValue(ConverterConstants.INDEX_SWA_ROUTINE_2_DEGREE_OF_DIFFICULTY, out int value25) ? this.RegExpService.MatchDouble(data[value25].InnerText) : null;
                    technicalRoutine.Routine3DegreeOfDifficulty = indexes.TryGetValue(ConverterConstants.INDEX_SWA_ROUTINE_3_DEGREE_OF_DIFFICULTY, out int value26) ? this.RegExpService.MatchDouble(data[value26].InnerText) : null;
                    technicalRoutine.Routine4DegreeOfDifficulty = indexes.TryGetValue(ConverterConstants.INDEX_SWA_ROUTINE_4_DEGREE_OF_DIFFICULTY, out int value27) ? this.RegExpService.MatchDouble(data[value27].InnerText) : null;
                    technicalRoutine.Routine5DegreeOfDifficulty = indexes.TryGetValue(ConverterConstants.INDEX_SWA_ROUTINE_5_DEGREE_OF_DIFFICULTY, out int value28) ? this.RegExpService.MatchDouble(data[value28].InnerText) : null;
                    technicalRoutine.Routine1Points = indexes.TryGetValue(ConverterConstants.INDEX_SWA_ROUTINE_1_POINTS, out int value29) ? this.RegExpService.MatchDouble(data[value29].InnerText) : null;
                    technicalRoutine.Routine2Points = indexes.TryGetValue(ConverterConstants.INDEX_SWA_ROUTINE_2_POINTS, out int value30) ? this.RegExpService.MatchDouble(data[value30].InnerText) : null;
                    technicalRoutine.Routine3Points = indexes.TryGetValue(ConverterConstants.INDEX_SWA_ROUTINE_3_POINTS, out int value31) ? this.RegExpService.MatchDouble(data[value31].InnerText) : null;
                    technicalRoutine.Routine4Points = indexes.TryGetValue(ConverterConstants.INDEX_SWA_ROUTINE_4_POINTS, out int value32) ? this.RegExpService.MatchDouble(data[value32].InnerText) : null;
                    technicalRoutine.Routine5Points = indexes.TryGetValue(ConverterConstants.INDEX_SWA_ROUTINE_5_POINTS, out int value33) ? this.RegExpService.MatchDouble(data[value33].InnerText) : null;
                    technicalRoutine.TechnicalMerit = indexes.TryGetValue(ConverterConstants.INDEX_SWA_TECHNICAL_MERIT, out int value34) ? this.RegExpService.MatchDouble(data[value34].InnerText) : null;

                    if (round.EventName == "Duet")
                    {
                        var duet = round.Duets.FirstOrDefault(x => x.NOCCode == nocCode);
                        duet.TechnicalRoutine = technicalRoutine;
                    }
                    else
                    {
                        var team = round.Teams.FirstOrDefault(x => x.NOCCode == nocCode);
                        if (team is not null)
                        {
                            team.TechnicalRoutine = technicalRoutine;
                        }
                    }
                }

                if (info == "Free Routine")
                {
                    freeRoutine.Points = indexes.TryGetValue(ConverterConstants.INDEX_POINTS, out int value5) ? this.RegExpService.MatchDouble(data[value5].InnerText) : null;
                    freeRoutine.ArtisticImpression = indexes.TryGetValue(ConverterConstants.INDEX_SWA_ARTISTIC_IMPRESSION, out int value35) ? this.RegExpService.MatchDouble(data[value35].InnerText) : null;
                    freeRoutine.ArtisticImpressionChoreography = indexes.TryGetValue(ConverterConstants.INDEX_SWA_ARTISTIC_IMPRESSION_CHOREOGRAPHY_POINTS, out int value36) ? this.RegExpService.MatchDouble(data[value36].InnerText) : null;
                    freeRoutine.ArtisticImpressionJudge1 = indexes.TryGetValue(ConverterConstants.INDEX_SWA_OVERALL_IMPRESSION_JUDGE_1_POINTS, out int value37) ? this.RegExpService.MatchDouble(data[value37].InnerText) : null;
                    freeRoutine.ArtisticImpressionJudge2 = indexes.TryGetValue(ConverterConstants.INDEX_SWA_OVERALL_IMPRESSION_JUDGE_2_POINTS, out int value38) ? this.RegExpService.MatchDouble(data[value38].InnerText) : null;
                    freeRoutine.ArtisticImpressionJudge3 = indexes.TryGetValue(ConverterConstants.INDEX_SWA_OVERALL_IMPRESSION_JUDGE_3_POINTS, out int value39) ? this.RegExpService.MatchDouble(data[value39].InnerText) : null;
                    freeRoutine.ArtisticImpressionJudge4 = indexes.TryGetValue(ConverterConstants.INDEX_SWA_OVERALL_IMPRESSION_JUDGE_4_POINTS, out int value40) ? this.RegExpService.MatchDouble(data[value40].InnerText) : null;
                    freeRoutine.ArtisticImpressionJudge5 = indexes.TryGetValue(ConverterConstants.INDEX_SWA_OVERALL_IMPRESSION_JUDGE_5_POINTS, out int value41) ? this.RegExpService.MatchDouble(data[value41].InnerText) : null;
                    freeRoutine.ArtisticImpressionMannerOfPresentation = indexes.TryGetValue(ConverterConstants.INDEX_SWA_ARTISTIC_IMPRESSION_MANNER_OF_PRESENTATION_POINTS, out int value42) ? this.RegExpService.MatchDouble(data[value42].InnerText) : null;
                    freeRoutine.ArtisticImpressionMusicInterpretation = indexes.TryGetValue(ConverterConstants.INDEX_SWA_ARTISTIC_IMPRESSION_MUSIC_INTERPRETATION_POINTS, out int value43) ? this.RegExpService.MatchDouble(data[value43].InnerText) : null;
                    freeRoutine.Difficulty = indexes.TryGetValue(ConverterConstants.INDEX_SWA_DIFFICULTY, out int value44) ? this.RegExpService.MatchDouble(data[value44].InnerText) : null;
                    freeRoutine.DifficultyJudge1 = indexes.TryGetValue(ConverterConstants.INDEX_SWA_DIFFICULTY_JUDGE_1_POINTS, out int value45) ? this.RegExpService.MatchDouble(data[value45].InnerText) : null;
                    freeRoutine.DifficultyJudge2 = indexes.TryGetValue(ConverterConstants.INDEX_SWA_DIFFICULTY_JUDGE_2_POINTS, out int value46) ? this.RegExpService.MatchDouble(data[value46].InnerText) : null;
                    freeRoutine.DifficultyJudge3 = indexes.TryGetValue(ConverterConstants.INDEX_SWA_DIFFICULTY_JUDGE_3_POINTS, out int value47) ? this.RegExpService.MatchDouble(data[value47].InnerText) : null;
                    freeRoutine.DifficultyJudge4 = indexes.TryGetValue(ConverterConstants.INDEX_SWA_DIFFICULTY_JUDGE_4_POINTS, out int value48) ? this.RegExpService.MatchDouble(data[value48].InnerText) : null;
                    freeRoutine.DifficultyJudge5 = indexes.TryGetValue(ConverterConstants.INDEX_SWA_DIFFICULTY_JUDGE_5_POINTS, out int value49) ? this.RegExpService.MatchDouble(data[value49].InnerText) : null;
                    freeRoutine.Execution = indexes.TryGetValue(ConverterConstants.INDEX_SWA_EXECUTION, out int value50) ? this.RegExpService.MatchDouble(data[value50].InnerText) : null;
                    freeRoutine.ExecutionJudge1 = indexes.TryGetValue(ConverterConstants.INDEX_SWA_EXECUTION_JUDGE_1_POINTS, out int value51) ? this.RegExpService.MatchDouble(data[value51].InnerText) : null;
                    freeRoutine.ExecutionJudge2 = indexes.TryGetValue(ConverterConstants.INDEX_SWA_EXECUTION_JUDGE_2_POINTS, out int value52) ? this.RegExpService.MatchDouble(data[value52].InnerText) : null;
                    freeRoutine.ExecutionJudge3 = indexes.TryGetValue(ConverterConstants.INDEX_SWA_EXECUTION_JUDGE_3_POINTS, out int value53) ? this.RegExpService.MatchDouble(data[value53].InnerText) : null;
                    freeRoutine.ExecutionJudge4 = indexes.TryGetValue(ConverterConstants.INDEX_SWA_EXECUTION_JUDGE_4_POINTS, out int value54) ? this.RegExpService.MatchDouble(data[value54].InnerText) : null;
                    freeRoutine.ExecutionJudge5 = indexes.TryGetValue(ConverterConstants.INDEX_SWA_EXECUTION_JUDGE_5_POINTS, out int value55) ? this.RegExpService.MatchDouble(data[value55].InnerText) : null;
                    freeRoutine.OverallImpression = indexes.TryGetValue(ConverterConstants.INDEX_SWA_OVERALL_IMPRESSION, out int value56) ? this.RegExpService.MatchDouble(data[value56].InnerText) : null;
                    freeRoutine.Penalties = indexes.TryGetValue(ConverterConstants.INDEX_SWA_PENALTIES, out int value57) ? this.RegExpService.MatchDouble(data[value57].InnerText) : null;
                    freeRoutine.TechnicalMerit = indexes.TryGetValue(ConverterConstants.INDEX_SWA_TECHNICAL_MERIT, out int value58) ? this.RegExpService.MatchDouble(data[value58].InnerText) : null;
                    freeRoutine.TechnicalMeritDifficulty = indexes.TryGetValue(ConverterConstants.INDEX_SWA_TECHNICAL_MERIT_DIFFICULTY_POINTS, out int value59) ? this.RegExpService.MatchDouble(data[value59].InnerText) : null;
                    freeRoutine.TechnicalMeritExecution = indexes.TryGetValue(ConverterConstants.INDEX_SWA_TECHNICAL_MERIT_EXECUTION_POINTS, out int value60) ? this.RegExpService.MatchDouble(data[value60].InnerText) : null;
                    freeRoutine.TechnicalMeritSynchronization = indexes.TryGetValue(ConverterConstants.INDEX_SWA_TECHNICAL_MERIT_SYNCHRONIZATION_POINTS, out int value61) ? this.RegExpService.MatchDouble(data[value61].InnerText) : null;

                    if (round.EventName == "Duet")
                    {
                        var duet = round.Duets.FirstOrDefault(x => x.NOCCode == nocCode);
                        duet.FreeRoutine = freeRoutine;
                    }
                    else
                    {
                        var team = round.Teams.FirstOrDefault(x => x.NOCCode == nocCode);
                        if (team is not null)
                        {
                            team.FreeRoutine = freeRoutine;
                        }
                    }
                }

                continue;
            }

            if (round.EventName == "Solo")
            {
                var participant = await this.participantsService.GetAsync(athleteModels[0].Number, eventCache.Id, noc.Id);

                var solo = new SWASolo
                {
                    Name = data[indexes[ConverterConstants.INDEX_NAME]].InnerText,
                    ParticipantId = participant.Id,
                    AthleteNumber = athleteModels[0].Number,
                    NOCCode = noc.Code,
                    Points = indexes.TryGetValue(ConverterConstants.INDEX_POINTS, out int value1) ? this.RegExpService.MatchDouble(data[value1].InnerText) : null,
                    FigurePoints = indexes.TryGetValue(ConverterConstants.INDEX_SWA_FIGURE_POINTS, out int value2) ? this.RegExpService.MatchDouble(data[value2].InnerText) : null,
                    MusicalRoutinePoints = indexes.TryGetValue(ConverterConstants.INDEX_SWA_MUSICAL_ROUTINE_POINTS, out int value3) ? this.RegExpService.MatchDouble(data[value3].InnerText) : null,
                    Qualification = this.OlympediaService.FindQualification(row.OuterHtml)
                };

                round.Solos.Add(solo);
            }
            else if (round.EventName == "Duet")
            {
                var teamName = data[indexes[ConverterConstants.INDEX_NAME]].InnerText;
                var team = await this.teamsService.GetAsync(teamName, noc.Id, eventCache.Id);
                team ??= await this.teamsService.GetAsync(noc.Id, eventCache.Id);

                var swimmers = new List<BaseAthlete>();
                foreach (var athleteModel in athleteModels)
                {
                    var participant = await this.participantsService.GetAsync(athleteModel.Number, eventCache.Id, noc.Id);
                    if (participant is not null)
                    {
                        swimmers.Add(new BaseAthlete { ParticipantId = participant.Id, AthleteNumber = athleteModel.Number, NOCCode = noc.Code, Name = athleteModel.Name });
                    }
                }

                var duet = new SWADuet
                {
                    Name = teamName,
                    TeamId = team.Id,
                    NOCCode = nocCode,
                    Swimmers = swimmers,
                    Points = indexes.TryGetValue(ConverterConstants.INDEX_POINTS, out int value1) ? this.RegExpService.MatchDouble(data[value1].InnerText) : null,
                    FigurePoints = indexes.TryGetValue(ConverterConstants.INDEX_SWA_FIGURE_POINTS, out int value2) ? this.RegExpService.MatchDouble(data[value2].InnerText) : null,
                    MusicalRoutinePoints = indexes.TryGetValue(ConverterConstants.INDEX_SWA_MUSICAL_ROUTINE_POINTS, out int value3) ? this.RegExpService.MatchDouble(data[value3].InnerText) : null,
                    Qualification = this.OlympediaService.FindQualification(row.OuterHtml)
                };

                round.Duets.Add(duet);
            }
            else if (round.EventName == "Team")
            {
                if (athleteModels.Any())
                {
                    var currentTeam = round.Teams.Last();
                    var currentNoc = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == currentTeam.NOCCode);
                    foreach (var athleteModel in athleteModels)
                    {
                        var participant = await this.participantsService.GetAsync(athleteModel.Number, eventCache.Id, currentNoc.Id);
                        if (participant is not null)
                        {
                            round.Teams.Last().Swimmers.Add(new BaseAthlete { ParticipantId = participant.Id, AthleteNumber = athleteModel.Number, NOCCode = currentNoc.Code, Name = athleteModel.Name });
                        }
                    }
                }
                else
                {
                    var teamName = data[indexes[ConverterConstants.INDEX_NAME]].InnerText;
                    var team = await this.teamsService.GetAsync(teamName, noc.Id, eventCache.Id);

                    var swaTeam = new SWATeam
                    {
                        Name = teamName,
                        TeamId = team.Id,
                        NOCCode = nocCode,
                        Points = indexes.TryGetValue(ConverterConstants.INDEX_POINTS, out int value1) ? this.RegExpService.MatchDouble(data[value1].InnerText) : null,
                    };

                    round.Teams.Add(swaTeam);
                }
            }
        }
    }

    private SWARound CreateArtisticSwimmingRound(DateTime? date, string format, RoundType roundType, string eventName)
    {
        return new SWARound
        {
            Date = date,
            Format = format,
            Round = roundType,
            EventName = eventName
        };
    }
    #endregion ARTISITC SWIMMING

    #region ARTISTIC GYMNASTICS
    private async Task ProcessArtisticGymnasticsAsync(ConvertOptions options)
    {
        var eventRound = this.CreateEventRound<GARRound>(options.HtmlDocument, options.Event.Name);
        var type = this.NormalizeService.MapGymnasticsType(options.Event.Name);

        if (options.Event.IsTeamEvent)
        {
            if (!options.Tables.Any() || options.Game.Year == 1924)
            {
                var round = this.CreateArtisticGymnasticsRound(eventRound.Dates.From, eventRound.Format, RoundType.Final, eventRound.EventName, type);
                await this.SetGARTeamsAsync(round, options.StandingTable, options.Event);
                this.ConvertGARTeamResults(round, options.StandingTable, options.Event, options.Game.Year, null);
                eventRound.Rounds.Add(round);
            }
            else if (options.Tables.Any() && options.Game.Year <= 1996)
            {
                var round = this.CreateArtisticGymnasticsRound(eventRound.Dates.From, eventRound.Format, RoundType.Final, eventRound.EventName, type);
                await this.SetGARTeamsAsync(round, options.StandingTable, options.Event);

                foreach (var table in options.Tables)
                {
                    var currentType = this.NormalizeService.MapGymnasticsType(table.Title);
                    if (currentType != GYMType.None)
                    {
                        this.ConvertGARTeamResults(round, table, options.Event, options.Game.Year, table.Title);
                    }
                }

                eventRound.Rounds.Add(round);
            }
            else if (options.Game.Year == 2000 || options.Game.Year == 2004)
            {
                foreach (var table in options.Tables)
                {
                    var round = this.CreateArtisticGymnasticsRound(table.FromDate, eventRound.Format, table.Round, eventRound.EventName, type);
                    await this.SetGARTeamsAsync(round, table, options.Event);
                    eventRound.Rounds.Add(round);
                }

                foreach (var document in options.Documents)
                {
                    var htmlDocument = this.CreateHtmlDocument(document);
                    var table = this.GetStandingTable(htmlDocument, options.Event);
                    var title = htmlDocument.DocumentNode.SelectSingleNode("//h1").InnerText;
                    title = title.Replace(options.Event.OriginalName, string.Empty).Replace("–", string.Empty).Trim();
                    var parts = title.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    var roundType = this.NormalizeService.MapRoundType(parts.FirstOrDefault());

                    var round = eventRound.Rounds.FirstOrDefault(x => x.Round == roundType);

                    if (round.Teams.Count >= table.HtmlDocument.DocumentNode.SelectNodes("//tr").Skip(1).Count())
                    {
                        this.ConvertGARTeamResults(round, table, options.Event, options.Game.Year, parts.LastOrDefault());
                    }
                }
            }
            else if (options.Game.Year >= 2008)
            {
                foreach (var table in options.Tables)
                {
                    var round = this.CreateArtisticGymnasticsRound(table.FromDate, eventRound.Format, table.Round, eventRound.EventName, type);
                    await this.SetGARTeamsAsync(round, table, options.Event);
                    this.ConvertGARTeamResults(round, table, options.Event, options.Game.Year, null);
                    eventRound.Rounds.Add(round);
                }

                if (options.Game.Year >= 2012)
                {
                    foreach (var document in options.Documents)
                    {
                        var htmlDocument = this.CreateHtmlDocument(document);
                        var table = this.GetStandingTable(htmlDocument, options.Event);
                        var title = htmlDocument.DocumentNode.SelectSingleNode("//h1").InnerText;
                        title = title.Replace(options.Event.OriginalName, string.Empty).Replace("–", string.Empty).Trim();
                        var parts = title.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        var roundType = this.NormalizeService.MapRoundType(parts.FirstOrDefault());

                        var round = eventRound.Rounds.FirstOrDefault(x => x.Round == roundType);

                        this.ConvertGARTeamResults(round, table, options.Event, options.Game.Year, parts.LastOrDefault());
                    }
                }
            }
        }
        else
        {
            if (options.Tables.Count == 0)
            {
                var round = this.CreateArtisticGymnasticsRound(eventRound.Dates.From, eventRound.Format, RoundType.Final, eventRound.EventName, type);
                await this.SetGARGymnastsAsync(round, options.StandingTable, options.Event, null);
                eventRound.Rounds.Add(round);
            }
            else
            {
                if (type != GYMType.Individual)
                {
                    foreach (var table in options.Tables)
                    {
                        var format = this.RegExpService.MatchFirstGroup(table.HtmlDocument.DocumentNode.OuterHtml, @"<i>(.*?)<\/i>");
                        string info = null;
                        if (type == GYMType.Triathlon)
                        {
                            table.Round = RoundType.Final;
                            info = table.Title;
                        }

                        var round = this.CreateArtisticGymnasticsRound(eventRound.Dates.From, format, table.Round, eventRound.EventName, type);
                        await this.SetGARGymnastsAsync(round, table, options.Event, info);
                        eventRound.Rounds.Add(round);
                    }

                    if (options.Game.Year >= 2012 && type == GYMType.Vault)
                    {
                        foreach (var document in options.Documents)
                        {
                            await this.ConvertGARDocumentsAsync(document, options.Event, eventRound.Rounds);
                        }
                    }
                }
                else
                {
                    if (options.Documents.Any())
                    {
                        foreach (var table in options.Tables)
                        {
                            var round = this.CreateArtisticGymnasticsRound(table.FromDate, null, table.Round, eventRound.EventName, type);
                            await this.SetGARGymnastsAsync(round, table, options.Event, table.Title);
                            eventRound.Rounds.Add(round);
                        }

                        foreach (var document in options.Documents)
                        {
                            await this.ConvertGARDocumentsAsync(document, options.Event, eventRound.Rounds);
                        }
                    }
                    else
                    {
                        if (options.Tables.Count == 2)
                        {
                            foreach (var table in options.Tables)
                            {
                                var round = this.CreateArtisticGymnasticsRound(table.FromDate, null, table.Round, eventRound.EventName, type);
                                await this.SetGARGymnastsAsync(round, table, options.Event, table.Title);
                                eventRound.Rounds.Add(round);
                            }
                        }
                        else
                        {
                            var round = this.CreateArtisticGymnasticsRound(eventRound.Dates.From, eventRound.Format, RoundType.Final, eventRound.EventName, type);
                            foreach (var table in options.Tables)
                            {
                                await this.SetGARGymnastsAsync(round, table, options.Event, table.Title);
                            }

                            eventRound.Rounds.Add(round);
                        }
                    }
                }

                foreach (var round in eventRound.Rounds)
                {
                    round.Gymnasts.ForEach(x => x.Points = x.Scores.Sum(x => x.Points));
                }
            }
        }

        var resultJson = this.CreateResult<GARRound>(options.Event, options.Discipline, options.Game);
        resultJson.Rounds = eventRound.Rounds;
        var json = JsonSerializer.Serialize(resultJson);
        var result = new Result
        {
            EventId = options.Event.Id,
            Json = json
        };

        await this.resultsService.AddOrUpdateAsync(result);
    }

    private async Task SetGARTeamsAsync(GARRound round, TableModel table, EventCacheModel eventCache)
    {
        var rows = table.HtmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']//tr");
        var headers = rows.First().Elements("th").Select(x => x.InnerText).ToList();
        var indexes = this.OlympediaService.FindIndexes(headers);

        GARTeam team = null;
        foreach (var row in rows.Skip(1))
        {
            var data = row.Elements("td").ToList();
            var nocCode = this.OlympediaService.FindNOCCode(row.OuterHtml);
            if (nocCode != null)
            {
                var teamName = data[indexes[ConverterConstants.INDEX_NAME]].InnerHtml;
                var noc = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == nocCode);
                var teamDb = await this.teamsService.GetAsync(teamName, noc.Id, eventCache.Id);

                team = new GARTeam
                {
                    Name = teamName,
                    TeamId = teamDb.Id,
                    NOCCode = nocCode,
                    Qualification = this.OlympediaService.FindQualification(row.OuterHtml),
                };

                round.Teams.Add(team);
            }
            else
            {
                var athleteModels = this.OlympediaService.FindAthletes(row.OuterHtml);
                foreach (var athleteModel in athleteModels)
                {
                    var participant = await this.participantsService.GetAsync(athleteModel.Number, eventCache.Id);
                    if (participant != null)
                    {
                        round.Teams.Last().Gymnasts.Add(new GARGymnast
                        {
                            AthleteNumber = athleteModel.Number,
                            ParticipantId = participant.Id,
                            Name = athleteModel.Name,
                            NOCCode = round.Teams.Last().NOCCode,
                        });
                    }
                }
            }
        }
    }

    private void ConvertGARTeamResults(GARRound round, TableModel table, EventCacheModel eventCache, int year, string info)
    {
        var rows = table.HtmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']//tr");
        var headers = rows.First().Elements("th").Select(x => x.InnerText).ToList();
        var indexes = this.OlympediaService.FindIndexes(headers);

        GARTeam team = null;
        var isMainTeam = true;
        foreach (var row in rows.Skip(1))
        {
            var data = row.Elements("td").ToList();
            var nocCode = this.OlympediaService.FindNOCCode(row.OuterHtml);
            var isAthleteNumber = this.OlympediaService.IsAthleteNumber(row.OuterHtml);
            if (nocCode != null && !isAthleteNumber)
            {
                isMainTeam = false;
                var teamName = data[indexes[ConverterConstants.INDEX_NAME]].InnerHtml;
                team = round.Teams.FirstOrDefault(x => x.Name == teamName && x.NOCCode == nocCode);

                if (year == 2008)
                {
                    var floorExercise = indexes.TryGetValue(ConverterConstants.INDEX_FLOOR_EXERCISE, out int value41) ? this.RegExpService.MatchDouble(data[value41].InnerText) : null;
                    var vault = indexes.TryGetValue(ConverterConstants.INDEX_HORSE_VAULT, out int value42) ? this.RegExpService.MatchDouble(data[value42].InnerText) : null;
                    var parallelBars = indexes.TryGetValue(ConverterConstants.INDEX_PARALLEL_BARS, out int value43) ? this.RegExpService.MatchDouble(data[value43].InnerText) : null;
                    var horizontalBar = indexes.TryGetValue(ConverterConstants.INDEX_HORIZONTAL_BAR, out int value44) ? this.RegExpService.MatchDouble(data[value44].InnerText) : null;
                    var rings = indexes.TryGetValue(ConverterConstants.INDEX_RINGS, out int value45) ? this.RegExpService.MatchDouble(data[value45].InnerText) : null;
                    var pommelHorse = indexes.TryGetValue(ConverterConstants.INDEX_POMMELLED_HORSE, out int value46) ? this.RegExpService.MatchDouble(data[value46].InnerText) : null;
                    var unevenBars = indexes.TryGetValue(ConverterConstants.INDEX_UNEVEN_BARS, out int value47) ? this.RegExpService.MatchDouble(data[value47].InnerText) : null;
                    var balanceBeam = indexes.TryGetValue(ConverterConstants.INDEX_BALANCE_BEAM, out int value48) ? this.RegExpService.MatchDouble(data[value48].InnerText) : null;

                    if (eventCache.Name.StartsWith("Men"))
                    {
                        team.Scores = new List<GARTeamScore>
                        {
                            new GARTeamScore { Points = floorExercise, Type = GYMType.FloorExercise, Info = "Floor Exerciese", EventName = round.EventName },
                            new GARTeamScore { Points = vault, Type = GYMType.Vault, Info = "Vault", EventName = round.EventName },
                            new GARTeamScore { Points = parallelBars, Type = GYMType.ParallelBars, Info = "Parallel Bars", EventName = round.EventName },
                            new GARTeamScore { Points = horizontalBar, Type = GYMType.HorizontalBar, Info = "Horizontal Bar", EventName = round.EventName },
                            new GARTeamScore { Points = rings, Type = GYMType.Rings, Info = "Rings", EventName = round.EventName },
                            new GARTeamScore { Points = pommelHorse, Type = GYMType.PommelHorse, Info = "Pommel Horse", EventName = round.EventName }
                        };
                    }
                    else
                    {
                        team.Scores = new List<GARTeamScore>
                        {
                            new GARTeamScore { Points = floorExercise, Type = GYMType.FloorExercise, Info = "Floor Exerciese", EventName = round.EventName },
                            new GARTeamScore { Points = vault, Type = GYMType.Vault, Info = "Vault", EventName = round.EventName },
                            new GARTeamScore { Points = unevenBars, Type = GYMType.UnevenBars, Info = "Uneven Bars", EventName = round.EventName },
                            new GARTeamScore { Points = balanceBeam, Type = GYMType.BalanceBeam, Info = "Balance Beam", EventName = round.EventName },
                        };
                    }
                }
                else
                {
                    team.Scores.Add(new GARTeamScore
                    {
                        EventName = round.EventName,
                        Info = info,
                        Type = round.Type,
                        Points = indexes.TryGetValue(ConverterConstants.INDEX_POINTS, out int value1) ? this.RegExpService.MatchDouble(data[value1].InnerText) : null,
                        AdjustedPoints = indexes.TryGetValue(ConverterConstants.INDEX_ADJUSTED_TEAM_POINS, out int value2) ? this.RegExpService.MatchDouble(data[value2].InnerText) : null,
                        ApparatusPoints = indexes.TryGetValue(ConverterConstants.INDEX_APPARATUS_POINTS, out int value3) ? this.RegExpService.MatchDouble(data[value3].InnerText) : null,
                        CompulsoryPoints = indexes.TryGetValue(ConverterConstants.INDEX_COMPULSORY_EXERCISES_POINTS, out int value4) ? this.RegExpService.MatchDouble(data[value4].InnerText) : null,
                        DrillPoints = indexes.TryGetValue(ConverterConstants.INDEX_TEAM_DRILL_POINTS, out int value5) ? this.RegExpService.MatchDouble(data[value5].InnerText) : null,
                        GroupExercisePoints = indexes.TryGetValue(ConverterConstants.INDEX_GROUP_EXERCISES_POINTS, out int value6) ? this.RegExpService.MatchDouble(data[value6].InnerText) : null,
                        GroupRoundOnePoints = indexes.TryGetValue(ConverterConstants.INDEX_ROUND_ONE_POINTS, out int value7) ? this.RegExpService.MatchDouble(data[value7].InnerText) : null,
                        GroupRoundTwoPoints = indexes.TryGetValue(ConverterConstants.INDEX_ROUND_TWO_POINTS, out int value8) ? this.RegExpService.MatchDouble(data[value8].InnerText) : null,
                        IndividualPoints = indexes.TryGetValue(ConverterConstants.INDEX_INDIVIDUAL_POINTS, out int value9) ? this.RegExpService.MatchDouble(data[value9].InnerText) : null,
                        LongJumpPoints = indexes.TryGetValue(ConverterConstants.INDEX_LONG_JUMP_POINTS, out int value10) ? this.RegExpService.MatchDouble(data[value10].InnerText) : null,
                        OptionalPoints = indexes.TryGetValue(ConverterConstants.INDEX_OPTIONAL_EXERCISES_POINTS, out int value11) ? this.RegExpService.MatchDouble(data[value11].InnerText) : null,
                        PrecisionPoins = indexes.TryGetValue(ConverterConstants.INDEX_TEAM_PRECISION_POINTS, out int value12) ? this.RegExpService.MatchDouble(data[value12].InnerText) : null,
                        ShotPutPoints = indexes.TryGetValue(ConverterConstants.INDEX_SHOT_PUT_POINTS, out int value13) ? this.RegExpService.MatchDouble(data[value13].InnerText) : null,
                        Yards100Points = indexes.TryGetValue(ConverterConstants.INDEX_POINTS_100, out int value14) ? this.RegExpService.MatchDouble(data[value14].InnerText) : null,
                    });
                }
            }
            else
            {
                if (isMainTeam)
                {
                    team = round.Teams.FirstOrDefault(x => x.NOCCode == nocCode);
                }

                var athleteModels = this.OlympediaService.FindAthletes(row.OuterHtml);
                foreach (var athleteModel in athleteModels)
                {
                    var gymnast = team.Gymnasts.FirstOrDefault(x => x.AthleteNumber == athleteModel.Number);
                    if (gymnast != null && athleteModels.Count == 1)
                    {
                        if (year == 2008)
                        {
                            var floorExercise = indexes.TryGetValue(ConverterConstants.INDEX_FLOOR_EXERCISE, out int value41) ? this.RegExpService.MatchDouble(data[value41].InnerText) : null;
                            var vault = indexes.TryGetValue(ConverterConstants.INDEX_HORSE_VAULT, out int value42) ? this.RegExpService.MatchDouble(data[value42].InnerText) : null;
                            var parallelBars = indexes.TryGetValue(ConverterConstants.INDEX_PARALLEL_BARS, out int value43) ? this.RegExpService.MatchDouble(data[value43].InnerText) : null;
                            var horizontalBar = indexes.TryGetValue(ConverterConstants.INDEX_HORIZONTAL_BAR, out int value44) ? this.RegExpService.MatchDouble(data[value44].InnerText) : null;
                            var rings = indexes.TryGetValue(ConverterConstants.INDEX_RINGS, out int value45) ? this.RegExpService.MatchDouble(data[value45].InnerText) : null;
                            var pommelHorse = indexes.TryGetValue(ConverterConstants.INDEX_POMMELLED_HORSE, out int value46) ? this.RegExpService.MatchDouble(data[value46].InnerText) : null;
                            var unevenBars = indexes.TryGetValue(ConverterConstants.INDEX_UNEVEN_BARS, out int value47) ? this.RegExpService.MatchDouble(data[value47].InnerText) : null;
                            var balanceBeam = indexes.TryGetValue(ConverterConstants.INDEX_BALANCE_BEAM, out int value48) ? this.RegExpService.MatchDouble(data[value48].InnerText) : null;

                            if (eventCache.Name.StartsWith("Men"))
                            {
                                gymnast.Scores = new List<GARScore>
                                {
                                    new GARScore { Points = floorExercise, Type = GYMType.FloorExercise, Info = "Floor Exerciese", EventName = round.EventName },
                                    new GARScore { Points = vault, Type = GYMType.Vault, Info = "Vault", EventName = round.EventName },
                                    new GARScore { Points = parallelBars, Type = GYMType.ParallelBars, Info = "Parallel Bars", EventName = round.EventName },
                                    new GARScore { Points = horizontalBar, Type = GYMType.HorizontalBar, Info = "Horizontal Bar", EventName = round.EventName },
                                    new GARScore { Points = rings, Type = GYMType.Rings, Info = "Rings", EventName = round.EventName },
                                    new GARScore { Points = pommelHorse, Type = GYMType.PommelHorse, Info = "Pommel Horse", EventName = round.EventName }
                                };
                            }
                            else
                            {
                                gymnast.Scores = new List<GARScore>
                                {
                                    new GARScore { Points = floorExercise, Type = GYMType.FloorExercise, Info = "Floor Exerciese", EventName = round.EventName },
                                    new GARScore { Points = vault, Type = GYMType.Vault, Info = "Vault", EventName = round.EventName },
                                    new GARScore { Points = unevenBars, Type = GYMType.UnevenBars, Info = "Uneven Bars", EventName = round.EventName },
                                    new GARScore { Points = balanceBeam, Type = GYMType.BalanceBeam, Info = "Balance Beam", EventName = round.EventName },
                                };
                            }
                        }
                        else
                        {
                            var score = new GARScore
                            {
                                EventName = round.EventName,
                                Type = round.Type,
                                Info = info,
                                Points = indexes.TryGetValue(ConverterConstants.INDEX_POINTS, out int value1) ? this.RegExpService.MatchDouble(data[value1].InnerText) : null,
                                CompulsoryPoints = indexes.TryGetValue(ConverterConstants.INDEX_COMPULSORY_EXERCISES_POINTS, out int value2) ? this.RegExpService.MatchDouble(data[value2].InnerText) : null,
                                OptionalPoints = indexes.TryGetValue(ConverterConstants.INDEX_OPTIONAL_EXERCISES_POINTS, out int value3) ? this.RegExpService.MatchDouble(data[value3].InnerText) : null,
                                QualificationHalfPoints = indexes.TryGetValue(ConverterConstants.INDEX_QUALIFICATION_HALF_POINTS, out int value4) ? this.RegExpService.MatchDouble(data[value4].InnerText) : null,
                                FinalPoints = indexes.TryGetValue(ConverterConstants.INDEX_FINAL_POINTS, out int value5) ? this.RegExpService.MatchDouble(data[value5].InnerText) : null,
                                QualificationOptionalPoints = indexes.TryGetValue(ConverterConstants.INDEX_QUALIFYING_OPTIONAL_POINTS, out int value6) ? this.RegExpService.MatchDouble(data[value6].InnerText) : null,
                                DScore = indexes.TryGetValue(ConverterConstants.INDEX_D_SCORE, out int value7) ? this.RegExpService.MatchDouble(data[value7].InnerText) : null,
                                EScore = indexes.TryGetValue(ConverterConstants.INDEX_E_SCORE, out int value8) ? this.RegExpService.MatchDouble(data[value8].InnerText) : null,
                                LinePenalty = indexes.TryGetValue(ConverterConstants.INDEX_LINE_PENALTY, out int value9) ? this.RegExpService.MatchDouble(data[value9].InnerText) : null,
                                TimePenalty = indexes.TryGetValue(ConverterConstants.INDEX_TIME_PENALTY, out int value10) ? this.RegExpService.MatchDouble(data[value10].InnerText) : null,
                                OtherPenalty = indexes.TryGetValue(ConverterConstants.INDEX_OTHER_PENALTY, out int value11) ? this.RegExpService.MatchDouble(data[value11].InnerText) : null,
                                Time = indexes.TryGetValue(ConverterConstants.INDEX_TIME, out int value12) ? this.RegExpService.MatchDouble(data[value12].InnerText) : null,
                                Penalty = indexes.TryGetValue(ConverterConstants.INDEX_PENALTY, out int value13) ? this.RegExpService.MatchDouble(data[value13].InnerText) : null,
                                Vault1 = indexes.TryGetValue(ConverterConstants.INDEX_VAULT_1, out int value14) ? this.RegExpService.MatchDouble(data[value14].InnerText) : null,
                                Vault2 = indexes.TryGetValue(ConverterConstants.INDEX_VAULT_2, out int value15) ? this.RegExpService.MatchDouble(data[value15].InnerText) : null,
                                VaultOff1 = indexes.TryGetValue(ConverterConstants.INDEX_VAULT_OFF_1, out int value16) ? this.RegExpService.MatchDouble(data[value16].InnerText) : null,
                                VaultOff2 = indexes.TryGetValue(ConverterConstants.INDEX_VAULT_OFF_2, out int value17) ? this.RegExpService.MatchDouble(data[value17].InnerText) : null,
                                VaultOffPoints = indexes.TryGetValue(ConverterConstants.INDEX_VAULT_OFF_POINTS, out int value18) ? this.RegExpService.MatchDouble(data[value18].InnerText) : null,
                                Height = indexes.TryGetValue(ConverterConstants.INDEX_HEIGHT, out int value19) ? this.RegExpService.MatchDouble(data[value19].InnerText) : null,
                            };

                            if (score.Points == null)
                            {
                                score.Points = indexes.TryGetValue(ConverterConstants.INDEX_INDIVIDUAL_POINTS, out int value20) ? this.RegExpService.MatchDouble(data[value20].InnerText) : null;
                            }

                            gymnast.Scores.Add(score);
                        }
                    }
                }
            }
        }
    }

    private async Task ConvertGARDocumentsAsync(Document document, EventCacheModel eventCache, List<GARRound> rounds)
    {
        var htmlDocument = this.CreateHtmlDocument(document);
        var table = this.GetStandingTable(htmlDocument, eventCache);
        var title = htmlDocument.DocumentNode.SelectSingleNode("//h1").InnerText;
        title = title.Replace(eventCache.OriginalName, string.Empty).Replace("–", string.Empty).Trim();
        var parts = title.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
        var roundType = this.NormalizeService.MapRoundType(parts.FirstOrDefault());
        var round = rounds.FirstOrDefault(x => x.Round == roundType);

        await this.SetGARGymnastsAsync(round, table, eventCache, parts.LastOrDefault().Trim());
    }

    private async Task SetGARGymnastsAsync(GARRound round, TableModel table, EventCacheModel eventCache, string info)
    {
        var rows = table.HtmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']//tr");
        var headers = rows.First().Elements("th").Select(x => x.InnerText).ToList();
        var indexes = this.OlympediaService.FindIndexes(headers);
        foreach (var row in rows.Skip(1))
        {
            var data = row.Elements("td").ToList();
            var athleteModel = this.OlympediaService.FindAthlete(data[indexes[ConverterConstants.INDEX_NAME]].OuterHtml);

            var score = new GARScore
            {
                EventName = round.EventName,
                Info = info,
                Type = round.Type,
                Points = indexes.TryGetValue(ConverterConstants.INDEX_POINTS, out int value1) ? this.RegExpService.MatchDouble(data[value1].InnerText) : null,
                CompulsoryPoints = indexes.TryGetValue(ConverterConstants.INDEX_COMPULSORY_EXERCISES_POINTS, out int value2) ? this.RegExpService.MatchDouble(data[value2].InnerText) : null,
                OptionalPoints = indexes.TryGetValue(ConverterConstants.INDEX_OPTIONAL_EXERCISES_POINTS, out int value3) ? this.RegExpService.MatchDouble(data[value3].InnerText) : null,
                QualificationHalfPoints = indexes.TryGetValue(ConverterConstants.INDEX_QUALIFICATION_HALF_POINTS, out int value4) ? this.RegExpService.MatchDouble(data[value4].InnerText) : null,
                FinalPoints = indexes.TryGetValue(ConverterConstants.INDEX_FINAL_POINTS, out int value5) ? this.RegExpService.MatchDouble(data[value5].InnerText) : null,
                QualificationOptionalPoints = indexes.TryGetValue(ConverterConstants.INDEX_QUALIFYING_OPTIONAL_POINTS, out int value6) ? this.RegExpService.MatchDouble(data[value6].InnerText) : null,
                DScore = indexes.TryGetValue(ConverterConstants.INDEX_D_SCORE, out int value7) ? this.RegExpService.MatchDouble(data[value7].InnerText) : null,
                EScore = indexes.TryGetValue(ConverterConstants.INDEX_E_SCORE, out int value8) ? this.RegExpService.MatchDouble(data[value8].InnerText) : null,
                LinePenalty = indexes.TryGetValue(ConverterConstants.INDEX_LINE_PENALTY, out int value9) ? this.RegExpService.MatchDouble(data[value9].InnerText) : null,
                TimePenalty = indexes.TryGetValue(ConverterConstants.INDEX_TIME_PENALTY, out int value10) ? this.RegExpService.MatchDouble(data[value10].InnerText) : null,
                OtherPenalty = indexes.TryGetValue(ConverterConstants.INDEX_OTHER_PENALTY, out int value11) ? this.RegExpService.MatchDouble(data[value11].InnerText) : null,
                Time = indexes.TryGetValue(ConverterConstants.INDEX_TIME, out int value12) ? this.RegExpService.MatchDouble(data[value12].InnerText) : null,
                Penalty = indexes.TryGetValue(ConverterConstants.INDEX_PENALTY, out int value13) ? this.RegExpService.MatchDouble(data[value13].InnerText) : null,
                Vault1 = indexes.TryGetValue(ConverterConstants.INDEX_VAULT_1, out int value14) ? this.RegExpService.MatchDouble(data[value14].InnerText) : null,
                Vault2 = indexes.TryGetValue(ConverterConstants.INDEX_VAULT_2, out int value15) ? this.RegExpService.MatchDouble(data[value15].InnerText) : null,
                VaultOff1 = indexes.TryGetValue(ConverterConstants.INDEX_VAULT_OFF_1, out int value16) ? this.RegExpService.MatchDouble(data[value16].InnerText) : null,
                VaultOff2 = indexes.TryGetValue(ConverterConstants.INDEX_VAULT_OFF_2, out int value17) ? this.RegExpService.MatchDouble(data[value17].InnerText) : null,
                VaultOffPoints = indexes.TryGetValue(ConverterConstants.INDEX_VAULT_OFF_POINTS, out int value18) ? this.RegExpService.MatchDouble(data[value18].InnerText) : null,
                Height = indexes.TryGetValue(ConverterConstants.INDEX_HEIGHT, out int value19) ? this.RegExpService.MatchDouble(data[value19].InnerText) : null,
            };

            if (round.Type == GYMType.Triathlon)
            {
                score.Info = table.Title;
            }

            var gymnast = round.Gymnasts.FirstOrDefault(x => x.AthleteNumber == athleteModel.Number);
            if (gymnast == null)
            {
                var nocCode = this.OlympediaService.FindNOCCode(data[indexes[ConverterConstants.INDEX_NOC]].OuterHtml);
                var nocCacheModel = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == nocCode);
                if (nocCacheModel is null)
                {
                    continue;
                }

                var participant = await this.participantsService.GetAsync(athleteModel.Number, eventCache.Id, nocCacheModel.Id);
                if (participant == null)
                {
                    continue;
                }

                gymnast = new GARGymnast
                {
                    AthleteNumber = athleteModel.Number,
                    Name = athleteModel.Name,
                    NOCCode = nocCacheModel.Code,
                    Number = indexes.TryGetValue(ConverterConstants.INDEX_NR, out int value22) ? this.RegExpService.MatchInt(data[value22].InnerText) : null,
                    ParticipantId = participant.Id,
                    Qualification = this.OlympediaService.FindQualification(row.OuterHtml),
                    Scores = new List<GARScore> { score },
                };

                round.Gymnasts.Add(gymnast);
            }
            else
            {
                gymnast.Scores.Add(score);
            }

            if (round.Type == GYMType.RopeClimbing && round.Date.HasValue && round.Date.Value.Year == 1932)
            {
                gymnast.Scores.Add(new GARScore
                {
                    Time = indexes.TryGetValue(ConverterConstants.INDEX_TRIAL_TIME_1, out int value30) ? this.RegExpService.MatchDouble(data[value30].InnerText) : null,
                    Info = "TT1"
                });

                gymnast.Scores.Add(new GARScore
                {
                    Time = indexes.TryGetValue(ConverterConstants.INDEX_TRIAL_TIME_2, out int value31) ? this.RegExpService.MatchDouble(data[value31].InnerText) : null,
                    Info = "TT2"
                });

                gymnast.Scores.Add(new GARScore
                {
                    Time = indexes.TryGetValue(ConverterConstants.INDEX_TRIAL_TIME_3, out int value32) ? this.RegExpService.MatchDouble(data[value32].InnerText) : null,
                    Info = "TT3"
                });
            }
            else if (round.Type == GYMType.Individual && round.Date.HasValue && round.Date.Value.Year == 2008)
            {
                var floorExercise = indexes.TryGetValue(ConverterConstants.INDEX_FLOOR_EXERCISE, out int value41) ? this.RegExpService.MatchDouble(data[value41].InnerText) : null;
                var vault = indexes.TryGetValue(ConverterConstants.INDEX_HORSE_VAULT, out int value42) ? this.RegExpService.MatchDouble(data[value42].InnerText) : null;
                var parallelBars = indexes.TryGetValue(ConverterConstants.INDEX_PARALLEL_BARS, out int value43) ? this.RegExpService.MatchDouble(data[value43].InnerText) : null;
                var horizontalBar = indexes.TryGetValue(ConverterConstants.INDEX_HORIZONTAL_BAR, out int value44) ? this.RegExpService.MatchDouble(data[value44].InnerText) : null;
                var rings = indexes.TryGetValue(ConverterConstants.INDEX_RINGS, out int value45) ? this.RegExpService.MatchDouble(data[value45].InnerText) : null;
                var pommelHorse = indexes.TryGetValue(ConverterConstants.INDEX_POMMELLED_HORSE, out int value46) ? this.RegExpService.MatchDouble(data[value46].InnerText) : null;
                var unevenBars = indexes.TryGetValue(ConverterConstants.INDEX_UNEVEN_BARS, out int value47) ? this.RegExpService.MatchDouble(data[value47].InnerText) : null;
                var balanceBeam = indexes.TryGetValue(ConverterConstants.INDEX_BALANCE_BEAM, out int value48) ? this.RegExpService.MatchDouble(data[value48].InnerText) : null;

                if (eventCache.Name.StartsWith("Men"))
                {
                    gymnast.Scores = new List<GARScore>
                    {
                        new GARScore { Points = floorExercise, Type = GYMType.FloorExercise, EventName = "Floor Exercise" },
                        new GARScore { Points = vault, Type = GYMType.Vault, EventName = "Vault" },
                        new GARScore { Points = parallelBars, Type = GYMType.ParallelBars, EventName = "Parallel Bars" },
                        new GARScore { Points = horizontalBar, Type = GYMType.HorizontalBar, EventName = "Horizontal Bar" },
                        new GARScore { Points = rings, Type = GYMType.Rings, EventName = "Rings" },
                        new GARScore { Points = pommelHorse, Type = GYMType.PommelHorse, EventName = "Pommel Horse" }
                    };
                }
                else
                {
                    gymnast.Scores = new List<GARScore>
                    {
                        new GARScore { Points = floorExercise, Type = GYMType.FloorExercise, EventName = "Floor Exercise" },
                        new GARScore { Points = vault, Type = GYMType.Vault, EventName = "Vault" },
                        new GARScore { Points = unevenBars, Type = GYMType.UnevenBars, EventName = "Uneven Bars" },
                        new GARScore { Points = balanceBeam, Type = GYMType.BalanceBeam, EventName = "Balance Beam" },
                    };
                }
            }
        }
    }

    private GARRound CreateArtisticGymnasticsRound(DateTime? date, string format, RoundType roundType, string eventName, GYMType type)
    {
        return new GARRound
        {
            Date = date,
            Format = format,
            Round = roundType,
            EventName = eventName,
            Type = type
        };
    }
    #endregion

    #region ARCHERY
    private async Task ProcessArcheryAsync(ConvertOptions options)
    {
        var eventRound = this.CreateEventRound<ARCRound>(options.HtmlDocument, options.Event.Name);

        if (options.Event.IsTeamEvent)
        {
            if (options.Game.Year <= 1920)
            {
                var round = this.CreateArcheryRound(eventRound.Dates.From, eventRound.Format, RoundType.Final, eventRound.EventName);
                await this.SetARCTeamsAsync(round, options.StandingTable, options.Event);
                eventRound.Rounds.Add(round);
            }
            else if (options.Game.Year == 1988)
            {
                foreach (var table in options.Tables)
                {
                    var dateString = this.RegExpService.MatchFirstGroup(table.HtmlDocument.DocumentNode.OuterHtml, @"<th>\s*Date\s*<\/th>\s*<td>(.*?)<\/td>");
                    var dateModel = this.dateService.ParseDate(dateString);
                    var format = this.RegExpService.MatchFirstGroup(table.HtmlDocument.DocumentNode.OuterHtml, @"<i>(.*?)<\/i>");
                    var round = this.CreateArcheryRound(dateModel.From, format, table.Round, eventRound.EventName);
                    await this.SetARCTeamsAsync(round, table, options.Event);
                    eventRound.Rounds.Add(round);
                }
            }
            else
            {
                var rankingTable = options.Tables.FirstOrDefault();
                var dateString = this.RegExpService.MatchFirstGroup(rankingTable.HtmlDocument.DocumentNode.OuterHtml, @"<th>\s*Date\s*<\/th>\s*<td>(.*?)<\/td>");
                var dateModel = this.dateService.ParseDate(dateString);
                var format = this.RegExpService.MatchFirstGroup(rankingTable.HtmlDocument.DocumentNode.OuterHtml, @"<i>(.*?)<\/i>");
                var rankingRound = this.CreateArcheryRound(dateModel.From, format, rankingTable.Round, eventRound.EventName);
                await this.SetARCTeamsAsync(rankingRound, rankingTable, options.Event);

                if (options.Game.Year == 1992 || options.Game.Year == 1996)
                {
                    var results = this.OlympediaService.FindResults(rankingTable.HtmlDocument.DocumentNode.OuterHtml);
                    this.ConvertArcheryTeamAdditionalInfo(rankingRound, results, options.Documents);
                }
                eventRound.Rounds.Add(rankingRound);

                foreach (var table in options.Tables.Skip(1))
                {
                    dateString = this.RegExpService.MatchFirstGroup(table.HtmlDocument.DocumentNode.OuterHtml, @"<th>\s*Date\s*<\/th>\s*<td>(.*?)<\/td>");
                    dateModel = this.dateService.ParseDate(dateString);
                    format = this.RegExpService.MatchFirstGroup(table.HtmlDocument.DocumentNode.OuterHtml, @"<i>(.*?)<\/i>");
                    var round = this.CreateArcheryRound(dateModel.From, format, table.Round, eventRound.EventName);

                    await this.SetARCTeamMatchesAsync(round, table, options.Event, options.Documents);
                    eventRound.Rounds.Add(round);
                }
            }
        }
        else
        {
            if (options.Game.Year <= 1920)
            {
                var round = this.CreateArcheryRound(eventRound.Dates.From, eventRound.Format, RoundType.Final, eventRound.EventName);
                await this.SetARCArchersAsync(round, options.StandingTable, options.Event);
                eventRound.Rounds.Add(round);
            }
            else if (options.Game.Year >= 1972 && options.Game.Year <= 1984)
            {
                var round = this.CreateArcheryRound(eventRound.Dates.From, eventRound.Format, RoundType.FinalRound, eventRound.EventName);
                await this.SetARCArchersAsync(round, options.StandingTable, options.Event);
                eventRound.Rounds.Add(round);

                foreach (var document in options.Documents)
                {
                    var htmlDocument = this.CreateHtmlDocument(document);
                    var table = this.GetStandingTable(htmlDocument, options.Event);
                    var title = htmlDocument.DocumentNode.SelectSingleNode("//h1").InnerText;
                    title = title.Replace(options.Event.OriginalName, string.Empty).Replace("–", string.Empty).Trim();
                    var parts = title.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    var roundType = this.NormalizeService.MapRoundType(parts.FirstOrDefault());

                    if (roundType != RoundType.None)
                    {
                        var dateString = this.RegExpService.MatchFirstGroup(table.HtmlDocument.DocumentNode.OuterHtml, @"<th>\s*Date\s*<\/th>\s*<td>(.*?)<\/td>");
                        var dateModel = this.dateService.ParseDate(dateString);
                        round = this.CreateArcheryRound(dateModel.From, eventRound.Format, roundType, eventRound.EventName);
                        await this.SetARCArchersAsync(round, table, options.Event);
                        eventRound.Rounds.Add(round);
                    }
                }
            }
            else if (options.Game.Year == 1988)
            {
                foreach (var table in options.Tables)
                {
                    var dateString = this.RegExpService.MatchFirstGroup(table.HtmlDocument.DocumentNode.OuterHtml, @"<th>\s*Date\s*<\/th>\s*<td>(.*?)<\/td>");
                    var dateModel = this.dateService.ParseDate(dateString);

                    var round = this.CreateArcheryRound(dateModel.From, eventRound.Format, table.Round, eventRound.EventName);
                    await this.SetARCArchersAsync(round, table, options.Event);
                    eventRound.Rounds.Add(round);
                }
            }
            else
            {
                var rankingTable = options.Tables.FirstOrDefault();
                var dateString = this.RegExpService.MatchFirstGroup(rankingTable.HtmlDocument.DocumentNode.OuterHtml, @"<th>\s*Date\s*<\/th>\s*<td>(.*?)<\/td>");
                var dateModel = this.dateService.ParseDate(dateString);
                var format = this.RegExpService.MatchFirstGroup(rankingTable.HtmlDocument.DocumentNode.OuterHtml, @"<i>(.*?)<\/i>");
                var rankingRound = this.CreateArcheryRound(dateModel.From, format, rankingTable.Round, eventRound.EventName);
                await this.SetARCArchersAsync(rankingRound, rankingTable, options.Event);
                var results = this.OlympediaService.FindResults(rankingTable.HtmlDocument.DocumentNode.OuterHtml);
                this.ConvertArcheryRanking(rankingRound, results, options.Documents);
                eventRound.Rounds.Add(rankingRound);

                foreach (var table in options.Tables.Skip(1))
                {
                    dateString = this.RegExpService.MatchFirstGroup(rankingTable.HtmlDocument.DocumentNode.OuterHtml, @"<th>\s*Date\s*<\/th>\s*<td>(.*?)<\/td>");
                    dateModel = this.dateService.ParseDate(dateString);
                    format = this.RegExpService.MatchFirstGroup(rankingTable.HtmlDocument.DocumentNode.OuterHtml, @"<i>(.*?)<\/i>");
                    var round = this.CreateArcheryRound(dateModel.From, format, table.Round, eventRound.EventName);

                    await this.SetARCMatchesAsync(round, table, options.Event, options.Documents);
                    eventRound.Rounds.Add(round);
                }
            }
        }

        var resultJson = this.CreateResult<ARCRound>(options.Event, options.Discipline, options.Game);
        resultJson.Rounds = eventRound.Rounds;
        var json = JsonSerializer.Serialize(resultJson);
        var result = new Result
        {
            EventId = options.Event.Id,
            Json = json
        };

        await this.resultsService.AddOrUpdateAsync(result);
    }

    private void ConvertArcheryTeamAdditionalInfo(ARCRound round, IList<int> results, IOrderedEnumerable<Document> documents)
    {
        foreach (var result in results)
        {
            var document = documents.FirstOrDefault(x => x.Name.Contains($"{result}"));
            if (document != null)
            {
                var htmlDocument = this.CreateHtmlDocument(document);
                var rows = htmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']//tr");
                var headers = rows.First().Elements("th").Select(x => x.InnerText).ToList();
                var indexes = this.OlympediaService.FindIndexes(headers);
                var title = htmlDocument.DocumentNode.SelectSingleNode("//h1").InnerText.Trim();

                string teamNocCode = null;
                foreach (var row in rows.Skip(1))
                {
                    var data = row.Elements("td").ToList();
                    var nocCode = this.OlympediaService.FindNOCCode(row.OuterHtml);
                    if (nocCode != null)
                    {
                        var team = round.Teams.FirstOrDefault(x => x.NOCCode == nocCode);
                        teamNocCode = team.NOCCode;

                        team.Points30Meters ??= title.EndsWith("30 m") ? (indexes.TryGetValue(ConverterConstants.INDEX_POINTS, out int value) ? this.RegExpService.MatchInt(data[value].InnerText) : null) : null;
                        team.Points50Meters ??= title.EndsWith("50 m") ? (indexes.TryGetValue(ConverterConstants.INDEX_POINTS, out int value1) ? this.RegExpService.MatchInt(data[value1].InnerText) : null) : null;
                        team.Points70Meters ??= title.EndsWith("70 m") ? (indexes.TryGetValue(ConverterConstants.INDEX_POINTS, out int value2) ? this.RegExpService.MatchInt(data[value2].InnerText) : null) : null;
                        team.Points90Meters ??= title.EndsWith("90 m") ? (indexes.TryGetValue(ConverterConstants.INDEX_POINTS, out int value3) ? this.RegExpService.MatchInt(data[value3].InnerText) : null) : null;
                        team.Part1Points ??= title.EndsWith("Part #1") ? (indexes.TryGetValue(ConverterConstants.INDEX_POINTS, out int value4) ? this.RegExpService.MatchInt(data[value4].InnerText) : null) : null;
                        team.Part2Points ??= title.EndsWith("Part #2") ? (indexes.TryGetValue(ConverterConstants.INDEX_POINTS, out int value5) ? this.RegExpService.MatchInt(data[value5].InnerText) : null) : null;
                    }
                    else
                    {
                        var team = round.Teams.FirstOrDefault(x => x.NOCCode == teamNocCode);
                        var athleteModel = this.OlympediaService.FindAthlete(row.OuterHtml);
                        var archer = team.Archers.FirstOrDefault(x => x.AthleteNumber == athleteModel.Number);

                        if (archer != null)
                        {
                            archer.Points30Meters ??= title.EndsWith("30 m") ? (indexes.TryGetValue(ConverterConstants.INDEX_POINTS, out int value) ? this.RegExpService.MatchInt(data[value].InnerText) : null) : null;
                            archer.Points50Meters ??= title.EndsWith("50 m") ? (indexes.TryGetValue(ConverterConstants.INDEX_POINTS, out int value1) ? this.RegExpService.MatchInt(data[value1].InnerText) : null) : null;
                            archer.Points70Meters ??= title.EndsWith("70 m") ? (indexes.TryGetValue(ConverterConstants.INDEX_POINTS, out int value2) ? this.RegExpService.MatchInt(data[value2].InnerText) : null) : null;
                            archer.Points90Meters ??= title.EndsWith("90 m") ? (indexes.TryGetValue(ConverterConstants.INDEX_POINTS, out int value3) ? this.RegExpService.MatchInt(data[value3].InnerText) : null) : null;
                            archer.Part1Points ??= title.EndsWith("Part #1") ? (indexes.TryGetValue(ConverterConstants.INDEX_POINTS, out int value4) ? this.RegExpService.MatchInt(data[value4].InnerText) : null) : null;
                            archer.Part2Points ??= title.EndsWith("Part #2") ? (indexes.TryGetValue(ConverterConstants.INDEX_POINTS, out int value5) ? this.RegExpService.MatchInt(data[value5].InnerText) : null) : null;
                        }
                    }
                }
            }
        }
    }

    private async Task SetARCTeamMatchesAsync(ARCRound round, TableModel table, EventCacheModel eventCache, IOrderedEnumerable<Document> documents)
    {
        var rows = table.HtmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']//tr");
        var headers = rows.First().Elements("th").Select(x => x.InnerText).ToList();
        var indexes = this.OlympediaService.FindIndexes(headers);

        foreach (var row in rows.Skip(1))
        {
            var data = row.Elements("td").ToList();
            var team1Name = data[2].InnerText;
            var team1NOCCode = this.OlympediaService.FindNOCCode(data[3].OuterHtml);
            var team1NOCCacheModel = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == team1NOCCode);
            var team1 = await this.teamsService.GetAsync(team1NOCCacheModel.Id, eventCache.Id);

            var matchNumber = this.OlympediaService.FindMatchNumber(data[0].InnerText);
            var matchType = this.OlympediaService.FindMatchType(table.Round, data[0].InnerText);
            var matchInfo = this.OlympediaService.FindMatchInfo(data[0].InnerText);
            var matchResult = this.OlympediaService.GetMatchResult(data[4].InnerText, MatchResultType.Points);
            var decision = this.OlympediaService.FindDecision(row.OuterHtml);

            var match = new ARCTeamMatch
            {
                MatchNumber = matchNumber,
                Round = round.Round,
                RoundInfo = table.RoundInfo,
                MatchType = matchType,
                MatchInfo = matchInfo,
                Decision = decision,
                Team1 = new ARCMatchTeam
                {
                    Name = team1Name,
                    TeamId = team1.Id,
                    NOCCode = team1NOCCode
                }
            };

            if (matchResult != null)
            {
                var team2Name = data[5].InnerText;
                var team2NOCCode = this.OlympediaService.FindNOCCode(data[6].OuterHtml);
                var team2NOCCacheModel = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == team2NOCCode);
                var team2 = await this.teamsService.GetAsync(team2NOCCacheModel.Id, eventCache.Id);

                var resultId = this.OlympediaService.FindResultNumber(data[0].OuterHtml);
                match.ResultId = resultId;
                match.Decision = DecisionType.None;
                match.Team1.Result = matchResult.Result1;
                match.Team1.Points = matchResult.Points1;
                match.Team2 = new ARCMatchTeam
                {
                    Name = team2Name,
                    TeamId = team2.Id,
                    NOCCode = team2NOCCode,
                    Result = matchResult.Result2,
                    Points = matchResult.Points2
                };

                var matchDocument = documents.FirstOrDefault(x => x.Url.EndsWith($"{resultId}"));
                if (matchDocument != null)
                {
                    var htmlDocument = this.CreateHtmlDocument(matchDocument);
                    var lineJudge = this.RegExpService.MatchFirstGroup(htmlDocument.ParsedText, @"<th>Line Judge<\/th>(?:.*?)\/athletes\/(\d+)");
                    var targetJudge = this.RegExpService.MatchFirstGroup(htmlDocument.ParsedText, @"<th>Target Judge<\/th>(?:.*?)\/athletes\/(\d+)");
                    var dateString = this.RegExpService.MatchFirstGroup(htmlDocument.DocumentNode.OuterHtml, @"<th>\s*Date\s*<\/th>\s*<td>(.*?)<\/td>");
                    var dateModel = this.dateService.ParseDate(dateString);
                    match.Date = dateModel.From;
                    match.LineJudgeId = await this.ExtractRefereeAsync(lineJudge);
                    match.TargetJudgeId = await this.ExtractRefereeAsync(targetJudge);

                    var infoTables = htmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']");
                    this.ConvertArcheryTeamResults(match, infoTables[0].OuterHtml);

                    await this.ConvertArcheryTeamArchersAsync(match.Team1, eventCache, infoTables[1].OuterHtml);
                    await this.ConvertArcheryTeamArchersAsync(match.Team2, eventCache, infoTables[2].OuterHtml);
                }
            }

            round.TeamMatches.Add(match);
        }
    }

    private ARCRound CreateArcheryRound(DateTime? date, string format, RoundType roundType, string eventName)
    {
        return new ARCRound
        {
            Date = date,
            Format = format,
            Round = roundType,
            EventName = eventName
        };
    }

    private async Task SetARCTeamsAsync(ARCRound round, TableModel table, EventCacheModel eventCache)
    {
        var rows = table.HtmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']//tr");
        var headers = rows.First().Elements("th").Select(x => x.InnerText).ToList();
        var indexes = this.OlympediaService.FindIndexes(headers);

        foreach (var row in rows.Skip(1))
        {
            var data = row.Elements("td").ToList();
            var name = data[indexes[ConverterConstants.INDEX_NAME]].InnerText;
            var nocCode = this.OlympediaService.FindNOCCode(data[indexes[ConverterConstants.INDEX_NOC]].OuterHtml);
            if (nocCode != null)
            {
                var nocCodeCacheModel = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == nocCode);
                var team = await this.teamsService.GetAsync(name, nocCodeCacheModel.Id, eventCache.Id);

                round.Teams.Add(new ARCTeam
                {
                    Name = name,
                    TeamId = team.Id,
                    NOCCode = nocCode,
                    Points = indexes.TryGetValue(ConverterConstants.INDEX_POINTS, out int value1) ? this.RegExpService.MatchInt(data[value1].InnerText) : null,
                    TargetsHit = indexes.TryGetValue(ConverterConstants.INDEX_TH, out int value2) ? this.RegExpService.MatchInt(data[value2].InnerText) : null,
                    Score10s = indexes.TryGetValue(ConverterConstants.INDEX_10S, out int value3) ? this.RegExpService.MatchInt(data[value3].InnerText) : null,
                    Points30Meters = indexes.TryGetValue(ConverterConstants.INDEX_30_METERS_POINTS, out int value4) ? this.RegExpService.MatchInt(data[value4].InnerText) : null,
                    Points50Meters = indexes.TryGetValue(ConverterConstants.INDEX_50_METERS_POINTS, out int value5) ? this.RegExpService.MatchInt(data[value5].InnerText) : null,
                    Points70Meters = indexes.TryGetValue(ConverterConstants.INDEX_70_METERS_POINTS, out int value6) ? this.RegExpService.MatchInt(data[value6].InnerText) : null,
                    Points90Meters = indexes.TryGetValue(ConverterConstants.INDEX_90_METERS_POINTS, out int value7) ? this.RegExpService.MatchInt(data[value7].InnerText) : null,
                    Score9s = indexes.TryGetValue(ConverterConstants.INDEX_9S, out int value8) ? this.RegExpService.MatchInt(data[value8].InnerText) : null,
                    Qualification = this.OlympediaService.FindQualification(row.OuterHtml),
                    Record = this.OlympediaService.FindRecord(row.OuterHtml),
                    Archers = new List<ARCArcher>()
                });
            }
            else
            {
                var team = round.Teams.LastOrDefault();

                var athleteModel = this.OlympediaService.FindAthlete(data[indexes[ConverterConstants.INDEX_NAME]].OuterHtml);
                var participant = await this.participantsService.GetAsync(athleteModel.Number, eventCache.Id);
                var archer = new ARCArcher
                {
                    ParticipantId = participant.Id,
                    AthleteNumber = athleteModel.Number,
                    Name = athleteModel.Name,
                    NOCCode = team.NOCCode,
                    Points = indexes.TryGetValue(ConverterConstants.INDEX_POINTS, out int value1) ? this.RegExpService.MatchInt(data[value1].InnerText) : null,
                    TargetsHit = indexes.TryGetValue(ConverterConstants.INDEX_TH, out int value2) ? this.RegExpService.MatchInt(data[value2].InnerText) : null,
                    Score10s = indexes.TryGetValue(ConverterConstants.INDEX_10S, out int value3) ? this.RegExpService.MatchInt(data[value3].InnerText) : null,
                    Points30Meters = indexes.TryGetValue(ConverterConstants.INDEX_30_METERS_POINTS, out int value4) ? this.RegExpService.MatchInt(data[value4].InnerText) : null,
                    Points50Meters = indexes.TryGetValue(ConverterConstants.INDEX_50_METERS_POINTS, out int value5) ? this.RegExpService.MatchInt(data[value5].InnerText) : null,
                    Points70Meters = indexes.TryGetValue(ConverterConstants.INDEX_70_METERS_POINTS, out int value6) ? this.RegExpService.MatchInt(data[value6].InnerText) : null,
                    Points90Meters = indexes.TryGetValue(ConverterConstants.INDEX_90_METERS_POINTS, out int value7) ? this.RegExpService.MatchInt(data[value7].InnerText) : null,
                    Score9s = indexes.TryGetValue(ConverterConstants.INDEX_9S, out int value8) ? this.RegExpService.MatchInt(data[value8].InnerText) : null,
                    Qualification = this.OlympediaService.FindQualification(row.OuterHtml),
                    Record = this.OlympediaService.FindRecord(row.OuterHtml),
                };

                archer.Points ??= indexes.TryGetValue(ConverterConstants.INDEX_INDIVIDUAL_POINTS, out int value9) ? this.RegExpService.MatchInt(data[value9].InnerText) : null;

                team.Archers.Add(archer);
            }
        }
    }

    private async Task SetARCMatchesAsync(ARCRound round, TableModel table, EventCacheModel eventCache, IOrderedEnumerable<Document> documents)
    {
        var rows = table.HtmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']//tr");
        foreach (var row in rows.Skip(1))
        {
            var data = row.Elements("td").ToList();
            var archer1AthleteModel = this.OlympediaService.FindAthlete(data[2].OuterHtml);
            var archer2AthleteModel = this.OlympediaService.FindAthlete(data[5].OuterHtml);
            var archer1 = await this.participantsService.GetAsync(archer1AthleteModel.Number, eventCache.Id);
            var archer2 = await this.participantsService.GetAsync(archer2AthleteModel.Number, eventCache.Id);
            var archer1NOCCode = this.OlympediaService.FindNOCCode(data[3].OuterHtml);
            var archer2NOCCode = this.OlympediaService.FindNOCCode(data[6].OuterHtml);
            var matchResult = this.OlympediaService.GetMatchResult(data[4].InnerText, MatchResultType.Points);

            var match = new ARCMatch
            {
                MatchNumber = this.OlympediaService.FindMatchNumber(data[0].InnerText),
                Round = table.Round,
                RoundInfo = table.RoundInfo,
                MatchType = this.OlympediaService.FindMatchType(table.Round, data[0].InnerText),
                MatchInfo = this.OlympediaService.FindMatchInfo(data[0].InnerText),
                ResultId = this.OlympediaService.FindResultNumber(data[0].OuterHtml),
                Decision = this.OlympediaService.FindDecision(row.OuterHtml),
                Archer1 = new ARCMatchArcher
                {
                    AthleteNumber = archer1AthleteModel.Number,
                    NOCCode = archer2NOCCode,
                    Name = archer1AthleteModel.Name,
                    ParticipantId = archer1.Id,
                    Points = matchResult.Points1,
                    Result = matchResult.Result1,

                },
                Archer2 = new ARCMatchArcher
                {
                    AthleteNumber = archer2AthleteModel.Number,
                    NOCCode = archer2NOCCode,
                    Name = archer2AthleteModel.Name,
                    ParticipantId = archer2.Id,
                    Points = matchResult.Points2,
                    Result = matchResult.Result2,
                }
            };

            var matchDocument = documents.FirstOrDefault(x => x.Url.EndsWith($"{match.ResultId}"));
            if (matchDocument != null)
            {
                var htmlDocument = this.CreateHtmlDocument(matchDocument);
                var lineJudge = this.RegExpService.MatchFirstGroup(htmlDocument.ParsedText, @"<th>Line Judge<\/th>(?:.*?)\/athletes\/(\d+)");
                var targetJudge = this.RegExpService.MatchFirstGroup(htmlDocument.ParsedText, @"<th>Target Judge<\/th>(?:.*?)\/athletes\/(\d+)");
                var dateString = this.RegExpService.MatchFirstGroup(htmlDocument.DocumentNode.OuterHtml, @"<th>\s*Date\s*<\/th>\s*<td>(.*?)<\/td>");
                var dateModel = this.dateService.ParseDate(dateString);
                match.Date = dateModel.From;
                match.LineJudgeId = await this.ExtractRefereeAsync(lineJudge);
                match.TargetJudgeId = await this.ExtractRefereeAsync(targetJudge);

                var resultTables = htmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']");
                var firstResultTable = resultTables[0];
                var firstTableDocument = new HtmlDocument();
                firstTableDocument.LoadHtml(firstResultTable.OuterHtml);
                var firstTableRows = firstTableDocument.DocumentNode.SelectNodes("//tr");
                var firstTableHeaders = firstTableRows.First().Elements("th").Select(x => x.InnerText).ToList();
                var firstTableIndexes = this.OlympediaService.FindIndexes(firstTableHeaders);

                for (int i = 1; i < firstTableRows.Count; i++)
                {
                    var firstData = firstTableRows[i].Elements("td").ToList();
                    if (i == 1)
                    {
                        match.Archer1.Record = this.OlympediaService.FindRecord(firstTableRows[i].OuterHtml);
                        match.Archer1.Target = firstTableIndexes.TryGetValue(ConverterConstants.INDEX_TARGET, out int value15) ? firstData[value15].InnerText : null;
                        match.Archer1.Set1Points = firstTableIndexes.TryGetValue(ConverterConstants.INDEX_SET_1, out int value2) ? this.RegExpService.MatchInt(firstData[value2].InnerText) : null;
                        match.Archer1.Set2Points = firstTableIndexes.TryGetValue(ConverterConstants.INDEX_SET_2, out int value3) ? this.RegExpService.MatchInt(firstData[value3].InnerText) : null;
                        match.Archer1.Set3Points = firstTableIndexes.TryGetValue(ConverterConstants.INDEX_SET_3, out int value4) ? this.RegExpService.MatchInt(firstData[value4].InnerText) : null;
                        match.Archer1.Set4Points = firstTableIndexes.TryGetValue(ConverterConstants.INDEX_SET_4, out int value5) ? this.RegExpService.MatchInt(firstData[value5].InnerText) : null;
                        match.Archer1.Set5Points = firstTableIndexes.TryGetValue(ConverterConstants.INDEX_SET_5, out int value6) ? this.RegExpService.MatchInt(firstData[value6].InnerText) : null;
                    }
                    else
                    {
                        match.Archer2.Record = this.OlympediaService.FindRecord(firstTableRows[i].OuterHtml);
                        match.Archer2.Target = firstTableIndexes.TryGetValue(ConverterConstants.INDEX_TARGET, out int value1) ? firstData[value1].InnerText : null;
                        match.Archer2.Set1Points = firstTableIndexes.TryGetValue(ConverterConstants.INDEX_SET_1, out int value2) ? this.RegExpService.MatchInt(firstData[value2].InnerText) : null;
                        match.Archer2.Set2Points = firstTableIndexes.TryGetValue(ConverterConstants.INDEX_SET_2, out int value3) ? this.RegExpService.MatchInt(firstData[value3].InnerText) : null;
                        match.Archer2.Set3Points = firstTableIndexes.TryGetValue(ConverterConstants.INDEX_SET_3, out int value4) ? this.RegExpService.MatchInt(firstData[value4].InnerText) : null;
                        match.Archer2.Set4Points = firstTableIndexes.TryGetValue(ConverterConstants.INDEX_SET_4, out int value5) ? this.RegExpService.MatchInt(firstData[value5].InnerText) : null;
                        match.Archer2.Set5Points = firstTableIndexes.TryGetValue(ConverterConstants.INDEX_SET_5, out int value6) ? this.RegExpService.MatchInt(firstData[value6].InnerText) : null;
                    }
                }

                var secondResultTable = resultTables[1];
                var secondTableDocument = new HtmlDocument();
                secondTableDocument.LoadHtml(secondResultTable.OuterHtml);
                var secondTableRows = secondTableDocument.DocumentNode.SelectNodes("//tr");

                foreach (var secondRow in secondTableRows.Skip(1))
                {
                    var secondHeader = secondRow.Element("th").InnerText.Trim();
                    var secondData = secondRow.Elements("td").ToList();
                    if (secondHeader == "10s")
                    {
                        match.Archer1.Score10s = this.RegExpService.MatchInt(secondData[0].InnerText);
                        match.Archer2.Score10s = this.RegExpService.MatchInt(secondData[1].InnerText);
                    }
                    else if (secondHeader == "Xs")
                    {
                        match.Archer1.ScoreXs = this.RegExpService.MatchInt(secondData[0].InnerText);
                        match.Archer2.ScoreXs = this.RegExpService.MatchInt(secondData[1].InnerText);
                    }
                    else if (secondHeader == "Shoot-Off Points")
                    {
                        match.Archer1.ShootOffPoints = this.RegExpService.MatchInt(secondData[0].InnerText);
                        match.Archer2.ShootOffPoints = this.RegExpService.MatchInt(secondData[1].InnerText);
                    }
                    else if (secondHeader == "Arrow 1")
                    {
                        match.Archer1.Arrow1 = secondData[0].InnerText;
                        match.Archer2.Arrow1 = secondData[1].InnerText;
                    }
                    else if (secondHeader == "Arrow 2")
                    {
                        match.Archer1.Arrow2 = secondData[0].InnerText;
                        match.Archer2.Arrow2 = secondData[1].InnerText;
                    }
                    else if (secondHeader == "Arrow 3")
                    {
                        match.Archer1.Arrow3 = secondData[0].InnerText;
                        match.Archer2.Arrow3 = secondData[1].InnerText;
                    }
                    else if (secondHeader == "Arrow 4")
                    {
                        match.Archer1.Arrow4 = secondData[0].InnerText;
                        match.Archer2.Arrow4 = secondData[1].InnerText;
                    }
                    else if (secondHeader == "Arrow 5")
                    {
                        match.Archer1.Arrow5 = secondData[0].InnerText;
                        match.Archer2.Arrow5 = secondData[1].InnerText;
                    }
                    else if (secondHeader == "Arrow 6")
                    {
                        match.Archer1.Arrow6 = secondData[0].InnerText;
                        match.Archer2.Arrow6 = secondData[1].InnerText;
                    }
                    else if (secondHeader == "Arrow 7")
                    {
                        match.Archer1.Arrow7 = secondData[0].InnerText;
                        match.Archer2.Arrow7 = secondData[1].InnerText;
                    }
                    else if (secondHeader == "Arrow 8")
                    {
                        match.Archer1.Arrow8 = secondData[0].InnerText;
                        match.Archer2.Arrow8 = secondData[1].InnerText;
                    }
                    else if (secondHeader == "Arrow 9")
                    {
                        match.Archer1.Arrow9 = secondData[0].InnerText;
                        match.Archer2.Arrow9 = secondData[1].InnerText;
                    }
                    else if (secondHeader == "Arrow 10")
                    {
                        match.Archer1.Arrow10 = secondData[0].InnerText;
                        match.Archer2.Arrow10 = secondData[1].InnerText;
                    }
                    else if (secondHeader == "Arrow 11")
                    {
                        match.Archer1.Arrow11 = secondData[0].InnerText;
                        match.Archer2.Arrow11 = secondData[1].InnerText;
                    }
                    else if (secondHeader == "Arrow 12")
                    {
                        match.Archer1.Arrow12 = secondData[0].InnerText;
                        match.Archer2.Arrow12 = secondData[1].InnerText;
                    }
                    else if (secondHeader == "Arrow 13")
                    {
                        match.Archer1.Arrow13 = secondData[0].InnerText;
                        match.Archer2.Arrow13 = secondData[1].InnerText;
                    }
                    else if (secondHeader == "Arrow 14")
                    {
                        match.Archer1.Arrow14 = secondData[0].InnerText;
                        match.Archer2.Arrow14 = secondData[1].InnerText;
                    }
                    else if (secondHeader == "Arrow 15")
                    {
                        match.Archer1.Arrow15 = secondData[0].InnerText;
                        match.Archer2.Arrow15 = secondData[1].InnerText;
                    }
                    else if (secondHeader == "Arrow 16")
                    {
                        match.Archer1.Arrow16 = secondData[0].InnerText;
                        match.Archer2.Arrow16 = secondData[1].InnerText;
                    }
                    else if (secondHeader == "Tiebreak 1")
                    {
                        match.Archer1.Tiebreak1 = this.RegExpService.MatchInt(secondData[0].InnerText);
                        match.Archer2.Tiebreak1 = this.RegExpService.MatchInt(secondData[1].InnerText);
                    }
                    else if (secondHeader == "Tiebreak 2")
                    {
                        match.Archer1.Tiebreak2 = this.RegExpService.MatchInt(secondData[0].InnerText);
                        match.Archer2.Tiebreak2 = this.RegExpService.MatchInt(secondData[1].InnerText);
                    }
                }
            }

            round.Matches.Add(match);
        }
    }

    private async Task SetARCArchersAsync(ARCRound round, TableModel table, EventCacheModel eventCache)
    {
        var rows = table.HtmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']//tr");
        var headers = rows.First().Elements("th").Select(x => x.InnerText).ToList();
        var indexes = this.OlympediaService.FindIndexes(headers);

        foreach (var row in rows.Skip(1))
        {
            var data = row.Elements("td").ToList();
            var athleteModel = this.OlympediaService.FindAthlete(data[indexes[ConverterConstants.INDEX_NAME]].OuterHtml);
            var nocCode = this.OlympediaService.FindNOCCode(data[indexes[ConverterConstants.INDEX_NOC]].OuterHtml);
            var nocCacheModel = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == nocCode);
            var participant = await this.participantsService.GetAsync(athleteModel.Number, eventCache.Id, nocCacheModel.Id);

            if (participant != null)
            {
                var archer = new ARCArcher
                {
                    Name = athleteModel.Name,
                    NOCCode = nocCacheModel.Code,
                    ParticipantId = participant.Id,
                    AthleteNumber = athleteModel.Number,
                    Points = indexes.TryGetValue(ConverterConstants.INDEX_POINTS, out int value1) ? this.RegExpService.MatchDouble(data[value1].InnerText) : null,
                    TargetsHit = indexes.TryGetValue(ConverterConstants.INDEX_TH, out int value2) ? this.RegExpService.MatchInt(data[value2].InnerText) : null,
                    Score = indexes.TryGetValue(ConverterConstants.INDEX_SCORE, out int value3) ? this.RegExpService.MatchInt(data[value3].InnerText) : null,
                    Yards40 = indexes.TryGetValue(ConverterConstants.INDEX_40_YARDS, out int value4) ? this.RegExpService.MatchInt(data[value4].InnerText) : null,
                    Yards60 = indexes.TryGetValue(ConverterConstants.INDEX_60_YARDS, out int value5) ? this.RegExpService.MatchInt(data[value5].InnerText) : null,
                    Points30Meters = indexes.TryGetValue(ConverterConstants.INDEX_30_METERS_POINTS, out int value6) ? this.RegExpService.MatchInt(data[value6].InnerText) : null,
                    Points50Meters = indexes.TryGetValue(ConverterConstants.INDEX_50_METERS_POINTS, out int value7) ? this.RegExpService.MatchInt(data[value7].InnerText) : null,
                    Points70Meters = indexes.TryGetValue(ConverterConstants.INDEX_70_METERS_POINTS, out int value8) ? this.RegExpService.MatchInt(data[value8].InnerText) : null,
                    Points90Meters = indexes.TryGetValue(ConverterConstants.INDEX_90_METERS_POINTS, out int value9) ? this.RegExpService.MatchInt(data[value9].InnerText) : null,
                    Golds = indexes.TryGetValue(ConverterConstants.INDEX_GOLDS, out int value10) ? this.RegExpService.MatchInt(data[value10].InnerText) : null,
                    Score10s = indexes.TryGetValue(ConverterConstants.INDEX_10S, out int value11) ? this.RegExpService.MatchInt(data[value11].InnerText) : null,
                    Score9s = indexes.TryGetValue(ConverterConstants.INDEX_9S, out int value12) ? this.RegExpService.MatchInt(data[value12].InnerText) : null,
                    ScoreXs = indexes.TryGetValue(ConverterConstants.INDEX_XS, out int value13) ? this.RegExpService.MatchInt(data[value13].InnerText) : null,
                    ShootOff = indexes.TryGetValue(ConverterConstants.INDEX_SHOOT_OFF, out int value14) ? this.RegExpService.MatchInt(data[value14].InnerText) : null,
                    Target = indexes.TryGetValue(ConverterConstants.INDEX_TARGET, out int value15) ? data[value15].InnerText : null,
                    Record = this.OlympediaService.FindRecord(row.OuterHtml),
                    Qualification = this.OlympediaService.FindQualification(row.OuterHtml)
                };

                round.Archers.Add(archer);
            }
        }
    }

    private void ConvertArcheryRanking(ARCRound round, IList<int> results, IOrderedEnumerable<Document> documents)
    {
        foreach (var result in results)
        {
            var document = documents.FirstOrDefault(x => x.Name.Contains($"{result}"));
            if (document != null)
            {
                var htmlDocument = this.CreateHtmlDocument(document);
                var rows = htmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']//tr");
                var headers = rows.First().Elements("th").Select(x => x.InnerText).ToList();
                var indexes = this.OlympediaService.FindIndexes(headers);
                var title = htmlDocument.DocumentNode.SelectSingleNode("//h1").InnerText.Trim();

                foreach (var row in rows.Skip(1))
                {
                    var data = row.Elements("td").ToList();
                    var athleteModel = this.OlympediaService.FindAthlete(data[indexes[ConverterConstants.INDEX_NAME]].OuterHtml);
                    var participant = round.Archers.FirstOrDefault(x => x.AthleteNumber == athleteModel.Number);
                    if (participant != null)
                    {
                        participant.Points30Meters = title.EndsWith("30 m") ? (indexes.TryGetValue(ConverterConstants.INDEX_POINTS, out int value) ? this.RegExpService.MatchInt(data[value].InnerText) : null) : null;
                        participant.Points50Meters = title.EndsWith("50 m") ? (indexes.TryGetValue(ConverterConstants.INDEX_POINTS, out int value1) ? this.RegExpService.MatchInt(data[value1].InnerText) : null) : null;
                        participant.Points70Meters = title.EndsWith("70 m") ? (indexes.TryGetValue(ConverterConstants.INDEX_POINTS, out int value2) ? this.RegExpService.MatchInt(data[value2].InnerText) : null) : null;
                        participant.Points90Meters = title.EndsWith("90 m") ? (indexes.TryGetValue(ConverterConstants.INDEX_POINTS, out int value3) ? this.RegExpService.MatchInt(data[value3].InnerText) : null) : null;
                        participant.Part1Points = title.EndsWith("Part #1") ? (indexes.TryGetValue(ConverterConstants.INDEX_POINTS, out int value4) ? this.RegExpService.MatchInt(data[value4].InnerText) : null) : null;
                        participant.Part2Points = title.EndsWith("Part #2") ? (indexes.TryGetValue(ConverterConstants.INDEX_POINTS, out int value5) ? this.RegExpService.MatchInt(data[value5].InnerText) : null) : null;
                    }
                }
            }
        }
    }

    private void ConvertArcheryTeamResults(ARCTeamMatch match, string html)
    {
        var document = new HtmlDocument();
        document.LoadHtml(html);
        var rows = document.DocumentNode.SelectNodes("//tr");
        var headers = rows.First().Elements("th").Select(x => x.InnerText).ToList();
        var indexes = this.OlympediaService.FindIndexes(headers);

        for (int i = 1; i < rows.Count; i++)
        {
            var data = rows[i].Elements("td").ToList();
            if (i == 1)
            {
                match.Team1.Target = indexes.TryGetValue(ConverterConstants.INDEX_TARGET, out int value1) ? data[value1].InnerText : null;
                match.Team1.Set1Points = indexes.TryGetValue(ConverterConstants.INDEX_SET_1, out int value2) ? this.RegExpService.MatchInt(data[value2].InnerText) : null;
                match.Team1.Set2Points = indexes.TryGetValue(ConverterConstants.INDEX_SET_2, out int value3) ? this.RegExpService.MatchInt(data[value3].InnerText) : null;
                match.Team1.Set3Points = indexes.TryGetValue(ConverterConstants.INDEX_SET_3, out int value4) ? this.RegExpService.MatchInt(data[value4].InnerText) : null;
                match.Team1.Set4Points = indexes.TryGetValue(ConverterConstants.INDEX_SET_4, out int value5) ? this.RegExpService.MatchInt(data[value5].InnerText) : null;
                match.Team1.Set5Points = indexes.TryGetValue(ConverterConstants.INDEX_SET_5, out int value6) ? this.RegExpService.MatchInt(data[value6].InnerText) : null;
            }
            else
            {
                match.Team2.Target = indexes.TryGetValue(ConverterConstants.INDEX_TARGET, out int value1) ? data[value1].InnerText : null;
                match.Team2.Set1Points = indexes.TryGetValue(ConverterConstants.INDEX_SET_1, out int value2) ? this.RegExpService.MatchInt(data[value2].InnerText) : null;
                match.Team2.Set2Points = indexes.TryGetValue(ConverterConstants.INDEX_SET_2, out int value3) ? this.RegExpService.MatchInt(data[value3].InnerText) : null;
                match.Team2.Set3Points = indexes.TryGetValue(ConverterConstants.INDEX_SET_3, out int value4) ? this.RegExpService.MatchInt(data[value4].InnerText) : null;
                match.Team2.Set4Points = indexes.TryGetValue(ConverterConstants.INDEX_SET_4, out int value5) ? this.RegExpService.MatchInt(data[value5].InnerText) : null;
                match.Team2.Set5Points = indexes.TryGetValue(ConverterConstants.INDEX_SET_5, out int value6) ? this.RegExpService.MatchInt(data[value6].InnerText) : null;
            }
        }
    }

    private async Task ConvertArcheryTeamArchersAsync(ARCMatchTeam team, EventCacheModel eventCache, string html)
    {
        var document = new HtmlDocument();
        document.LoadHtml(html);

        var rows = document.DocumentNode.SelectNodes("//tr");
        var headers = rows.First().Elements("th").Select(x => x.InnerText).ToList();
        var indexes = this.OlympediaService.FindIndexes(headers);

        for (int i = 1; i < rows.Count - 1; i++)
        {
            var data = rows[i].Elements("td").ToList();
            var athleteModel = this.OlympediaService.FindAthlete(data[2].OuterHtml);
            var participant = await this.participantsService.GetAsync(athleteModel.Number, eventCache.Id);
            var archer = new ARCMatchArcher
            {
                Name = athleteModel.Name,
                AthleteNumber = athleteModel.Number,
                ParticipantId = participant.Id,
                NOCCode = team.NOCCode,
                Arrow1 = indexes.TryGetValue(ConverterConstants.INDEX_ARROW_1, out int value1) ? data[value1].InnerText : null,
                Arrow2 = indexes.TryGetValue(ConverterConstants.INDEX_ARROW_2, out int value2) ? data[value2].InnerText : null,
                Arrow3 = indexes.TryGetValue(ConverterConstants.INDEX_ARROW_3, out int value3) ? data[value3].InnerText : null,
                Arrow4 = indexes.TryGetValue(ConverterConstants.INDEX_ARROW_4, out int value4) ? data[value4].InnerText : null,
                Arrow5 = indexes.TryGetValue(ConverterConstants.INDEX_ARROW_5, out int value5) ? data[value5].InnerText : null,
                Arrow6 = indexes.TryGetValue(ConverterConstants.INDEX_ARROW_6, out int value6) ? data[value6].InnerText : null,
                Arrow7 = indexes.TryGetValue(ConverterConstants.INDEX_ARROW_6, out int value7) ? data[value7].InnerText : null,
                Arrow8 = indexes.TryGetValue(ConverterConstants.INDEX_ARROW_8, out int value8) ? data[value8].InnerText : null,
                Arrow9 = indexes.TryGetValue(ConverterConstants.INDEX_ARROW_9, out int value9) ? data[value9].InnerText : null,
                Arrow10 = indexes.TryGetValue(ConverterConstants.INDEX_ARROW_10, out int value10) ? data[value10].InnerText : null,
            };

            team.Archers.Add(archer);
        }
    }
    #endregion ARCHERY

    #region ALPINE SKIING
    private async Task ProcessAlpineSkiingAsync(ConvertOptions options)
    {
        var eventRound = this.CreateEventRound<ALPRound>(options.HtmlDocument, options.Event.Name);
        var slope = await this.SetALPSlopeAsync(options.HtmlDocument);

        if (options.Event.IsTeamEvent)
        {
            foreach (var table in options.Tables)
            {
                var round = this.CreateAlpineSkiingRound(eventRound.Dates.From, eventRound.Format, table.Round, eventRound.EventName);
                await this.SetALPTeamMatchesAsync(round, table, options.Event, options.Documents);
                eventRound.Rounds.Add(round);
            }
        }
        else
        {
            if (eventRound.EventName == "Slalom" && (options.Game.Year == 1964 || options.Game.Year == 1968 || options.Game.Year == 1972))
            {
                foreach (var table in options.Tables)
                {
                    slope = await this.SetALPSlopeAsync(table.HtmlDocument);
                    var dateString = this.RegExpService.MatchFirstGroup(table.HtmlDocument.DocumentNode.OuterHtml, @"<th>\s*Date\s*<\/th>\s*<td>(.*?)<\/td>");
                    var dateModel = this.dateService.ParseDate(dateString);
                    var format = this.RegExpService.MatchFirstGroup(table.HtmlDocument.DocumentNode.OuterHtml, @"<th>Format<\/th>\s*<td(?:.*?)>(.*?)<\/td>");
                    format = this.RegExpService.CutHtml(format);

                    if (options.Game.Year == 1964)
                    {
                        var round = this.CreateAlpineSkiingRound(dateModel.From, format, table.Round, eventRound.EventName);
                        round.Slope = slope;
                        await this.SetALPSkiersAsync(round, table, options.Event);
                        eventRound.Rounds.Add(round);
                    }
                    else if (options.Game.Year == 1968)
                    {
                        var round = this.CreateAlpineSkiingRound(dateModel.From, format, table.Round, eventRound.EventName);
                        var heats = this.SplitHeats(table);
                        foreach (var heat in heats)
                        {
                            var currentTable = new TableModel { Html = heat.Html, Title = heat.Title };
                            await this.SetALPSkiersAsync(round, currentTable, options.Event);
                        }
                        eventRound.Rounds.Add(round);
                    }
                    else if (options.Game.Year == 1972)
                    {
                        var round = this.CreateAlpineSkiingRound(dateModel.From, format, table.Round, eventRound.EventName);
                        var groups = this.SplitGroups(table.Html);
                        foreach (var group in groups)
                        {
                            var currentTable = new TableModel { Html = group.Html, Title = group.Title };
                            await this.SetALPSkiersAsync(round, currentTable, options.Event);
                        }
                        eventRound.Rounds.Add(round);
                    }

                    if (table.Round == RoundType.FinalRound)
                    {
                        var round = this.CreateAlpineSkiingRound(dateModel.From, format, table.Round, eventRound.EventName);
                        round.Slope = slope;
                        await this.SetALPSkiersAsync(round, table, options.Event);
                        eventRound.Rounds.Add(round);

                        var results = this.OlympediaService.FindResults(table.HtmlDocument.DocumentNode.OuterHtml);
                        foreach (var resultNumber in results)
                        {
                            var document = options.Documents.FirstOrDefault(x => x.Name.Contains($"{resultNumber}"));
                            if (document != null)
                            {
                                await this.SetALPDocumentsAsync(eventRound, document, options.Event);
                            }
                        }
                    }
                }
            }
            else
            {
                var finalRound = this.CreateAlpineSkiingRound(eventRound.Dates.From, eventRound.Format, RoundType.FinalRound, eventRound.EventName);
                finalRound.Slope = slope;
                await this.SetALPSkiersAsync(finalRound, options.StandingTable, options.Event);
                eventRound.Rounds.Add(finalRound);

                if (!options.Tables.Any() && options.Documents.Any())
                {
                    foreach (var document in options.Documents)
                    {
                        await this.SetALPDocumentsAsync(eventRound, document, options.Event);
                    }
                }
            }
        }

        var resultJson = this.CreateResult<ALPRound>(options.Event, options.Discipline, options.Game);
        resultJson.Rounds = eventRound.Rounds;
        var json = JsonSerializer.Serialize(resultJson);
        var result = new Result
        {
            EventId = options.Event.Id,
            Json = json
        };

        await this.resultsService.AddOrUpdateAsync(result);
    }

    private async Task SetALPTeamMatchesAsync(ALPRound round, TableModel table, EventCacheModel eventCache, IOrderedEnumerable<Document> documents)
    {
        var rows = table.HtmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']//tr");
        var headers = rows.First().Elements("th").Select(x => x.InnerText).ToList();
        var indexes = this.OlympediaService.FindIndexes(headers);

        foreach (var row in rows.Skip(1))
        {
            var data = row.Elements("td").ToList();
            var team1Name = data[3].InnerText;
            var team1NOCCode = this.OlympediaService.FindNOCCode(data[4].OuterHtml);
            var team1NOCCacheModel = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == team1NOCCode);
            var team1 = await this.teamsService.GetAsync(team1NOCCacheModel.Id, eventCache.Id);

            var matchNumber = this.OlympediaService.FindMatchNumber(data[0].InnerText);
            var matchType = this.OlympediaService.FindMatchType(table.Round, data[0].InnerText);
            var matchInfo = this.OlympediaService.FindMatchInfo(data[0].InnerText);
            var matchResult = this.OlympediaService.GetMatchResult(data[5].InnerText, MatchResultType.Points);
            var decision = this.OlympediaService.FindDecision(row.OuterHtml);

            var match = new ALPTeamMatch
            {
                MatchNumber = matchNumber,
                Round = round.Round,
                RoundInfo = table.RoundInfo,
                MatchType = matchType,
                MatchInfo = matchInfo,
                Decision = decision,
                Team1 = new ALPTeam
                {
                    Name = team1Name,
                    TeamId = team1.Id,
                    NOCCode = team1NOCCode
                }
            };

            if (matchResult != null)
            {
                var team2Name = data[6].InnerText;
                var team2NOCCode = this.OlympediaService.FindNOCCode(data[7].OuterHtml);
                var team2NOCCacheModel = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == team2NOCCode);
                var team2 = await this.teamsService.GetAsync(team2NOCCacheModel.Id, eventCache.Id);

                var resultId = this.OlympediaService.FindResultNumber(data[0].OuterHtml);
                match.ResultId = resultId;
                match.Decision = DecisionType.None;
                match.Team1.Result = matchResult.Result1;
                match.Team1.Points = matchResult.Points1;
                match.Team2 = new ALPTeam
                {
                    Name = team2Name,
                    TeamId = team2.Id,
                    NOCCode = team2NOCCode,
                    Result = matchResult.Result2,
                    Points = matchResult.Points2
                };

                var matchDocument = documents.FirstOrDefault(x => x.Url.EndsWith($"{resultId}"));
                if (matchDocument != null)
                {
                    var htmlDocument = this.CreateHtmlDocument(matchDocument);
                    var dateString = this.RegExpService.MatchFirstGroup(htmlDocument.DocumentNode.OuterHtml, @"<th>\s*Date\s*<\/th>\s*<td>(.*?)<\/td>");
                    var dateModel = this.dateService.ParseDate(dateString);
                    match.Date = dateModel.From;

                    await this.ConvertAlpineSkiingTeamSkiersAsync(match, eventCache, htmlDocument);
                }
            }

            round.TeamMatches.Add(match);
        }
    }

    private async Task ConvertAlpineSkiingTeamSkiersAsync(ALPTeamMatch match, EventCacheModel eventCache, HtmlDocument htmlDocument)
    {
        var table = htmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']").Last();
        var document = new HtmlDocument();
        document.LoadHtml(table.OuterHtml);

        var rows = document.DocumentNode.SelectNodes("//table[@class='table table-striped']//tr");
        var headers = rows.First().Elements("th").Select(x => x.InnerText).ToList();
        var indexes = this.OlympediaService.FindIndexes(headers);

        foreach (var row in rows.Skip(1))
        {
            var data = row.Elements("td").ToList();

            var race = int.Parse(data[0].InnerText.Replace("Race #", string.Empty).Trim());
            var athleteModel1 = this.OlympediaService.FindAthlete(data[3].OuterHtml);
            var skier1NOCCode = this.OlympediaService.FindNOCCode(data[4].OuterHtml);
            var skier1NOCCacheModel = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == skier1NOCCode);
            var skier1Participant = await this.participantsService.GetAsync(athleteModel1.Number, eventCache.Id, skier1NOCCacheModel.Id);

            var athleteModel2 = this.OlympediaService.FindAthlete(data[6].OuterHtml);
            var skier2NOCCode = this.OlympediaService.FindNOCCode(data[7].OuterHtml);
            var skier2NOCCacheModel = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == skier2NOCCode);
            var skier2Participant = await this.participantsService.GetAsync(athleteModel2.Number, eventCache.Id, skier2NOCCacheModel.Id);

            var matchResult = this.OlympediaService.GetMatchResult(data[5].OuterHtml, MatchResultType.Points);

            var skier1 = new ALPSkier
            {
                Name = athleteModel1.Name,
                AthleteNumber = athleteModel1.Number,
                NOCCode = skier1NOCCode,
                ParticipantId = skier1Participant.Id,
                Time = matchResult.Time1,
                Race = race
            };

            var skier2 = new ALPSkier
            {
                Name = athleteModel2.Name,
                AthleteNumber = athleteModel2.Number,
                NOCCode = skier2NOCCode,
                ParticipantId = skier2Participant.Id,
                Time = matchResult.Time2,
                Race = race
            };

            if (match.Team1.NOCCode == skier1NOCCode)
            {
                match.Team1.Skiers.Add(skier1);
                match.Team2.Skiers.Add(skier2);
            }
            else
            {
                match.Team1.Skiers.Add(skier2);
                match.Team2.Skiers.Add(skier1);
            }
        }
    }

    private async Task SetALPDocumentsAsync(EventRoundModel<ALPRound> eventRound, Document document, EventCacheModel eventCache)
    {
        var htmlDocument = this.CreateHtmlDocument(document);
        var slope = await this.SetALPSlopeAsync(htmlDocument);
        var table = this.GetStandingTable(htmlDocument, eventCache);
        var title = htmlDocument.DocumentNode.SelectSingleNode("//h1").InnerText;
        title = title.Replace(eventCache.OriginalName, string.Empty).Replace("–", string.Empty).Trim();
        var parts = title.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
        var roundType = this.NormalizeService.MapRoundType(parts.FirstOrDefault());
        var dateString = this.RegExpService.MatchFirstGroup(htmlDocument.DocumentNode.OuterHtml, @"<th>\s*Date\s*<\/th>\s*<td>(.*?)<\/td>");
        var dateModel = this.dateService.ParseDate(dateString);
        var format = this.RegExpService.MatchFirstGroup(htmlDocument.DocumentNode.OuterHtml, @"<th>Format<\/th>\s*<td(?:.*?)>(.*?)<\/td>");

        var round = this.CreateAlpineSkiingRound(dateModel.From, format, roundType, eventRound.EventName);
        round.Slope = slope;
        await this.SetALPSkiersAsync(round, table, eventCache);
        eventRound.Rounds.Add(round);
    }

    private async Task SetALPSkiersAsync(ALPRound round, TableModel table, EventCacheModel eventCache)
    {
        var rows = table.HtmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']//tr");
        var headers = rows.First().Elements("th").Select(x => x.InnerText).ToList();
        var indexes = this.OlympediaService.FindIndexes(headers);

        foreach (var row in rows.Skip(1))
        {
            var data = row.Elements("td").ToList();
            var athleteModel = this.OlympediaService.FindAthlete(row.OuterHtml);
            var nocCode = this.OlympediaService.FindNOCCode(row.OuterHtml);
            var nocCacheModel = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == nocCode);
            var participant = await this.participantsService.GetAsync(athleteModel.Number, eventCache.Id, nocCacheModel.Id);

            if (participant != null)
            {
                var skier = new ALPSkier
                {
                    Name = athleteModel.Name,
                    NOCCode = nocCacheModel.Code,
                    ParticipantId = participant.Id,
                    AthleteNumber = athleteModel.Number,
                    FinishStatus = this.OlympediaService.FindStatus(row.OuterHtml),
                    Qualification = this.OlympediaService.FindQualification(row.OuterHtml),
                    Number = indexes.TryGetValue(ConverterConstants.INDEX_NR, out int value1) ? this.RegExpService.MatchInt(data[value1].InnerText) : null,
                    Points = indexes.TryGetValue(ConverterConstants.INDEX_POINTS, out int value2) ? this.RegExpService.MatchDouble(data[value2].InnerText) : null,
                    Time = indexes.TryGetValue(ConverterConstants.INDEX_TIME, out int value3) ? this.dateService.ParseTime(data[value3].InnerText) : null,
                    Downhill = indexes.TryGetValue(ConverterConstants.INDEX_DOWNHILL, out int value4) ? this.dateService.ParseTime(data[value4].InnerText) : null,
                    Slalom = indexes.TryGetValue(ConverterConstants.INDEX_SLALOM, out int value5) ? this.dateService.ParseTime(data[value5].InnerText) : null,
                    PenaltyTime = indexes.TryGetValue(ConverterConstants.INDEX_TIME_PENALTY, out int value6) ? this.dateService.ParseTime(data[value6].InnerText) : null,
                    Run1Time = indexes.TryGetValue(ConverterConstants.INDEX_RUN1, out int value7) ? this.dateService.ParseTime(data[value7].InnerText) : null,
                    Run2Time = indexes.TryGetValue(ConverterConstants.INDEX_RUN2, out int value8) ? this.dateService.ParseTime(data[value8].InnerText) : null,
                    Heat = this.NormalizeService.MapHeats(table.Title),
                    Group = this.NormalizeService.MapGroupType(table.Title)
                };

                round.Skiers.Add(skier);
            }
        }
    }

    private async Task<ALPSlope> SetALPSlopeAsync(HtmlDocument htmlDocument)
    {
        var courseSetterMatch = this.RegExpService.Match(htmlDocument.DocumentNode.OuterHtml, @"<th>\s*Course Setter\s*<\/th>\s*<td(?:.*?)>(.*?)<\/td>");
        var gatesMatch = this.RegExpService.MatchFirstGroup(htmlDocument.DocumentNode.OuterHtml, @"Gates:(.*?)<br>");
        var lengthMatch = this.RegExpService.MatchFirstGroup(htmlDocument.DocumentNode.OuterHtml, @"Length:(.*?)<br>");
        var startAltitudeMatch = this.RegExpService.MatchFirstGroup(htmlDocument.DocumentNode.OuterHtml, @"Start Altitude:(.*?)<br>");
        var verticalDropMatch = this.RegExpService.MatchFirstGroup(htmlDocument.DocumentNode.OuterHtml, @"Vertical Drop:(.*?)<\/td>");
        var athleteModel = courseSetterMatch != null ? this.OlympediaService.FindAthlete(courseSetterMatch.Groups[1].Value) : null;
        var courseSetter = athleteModel != null ? await this.athletesService.GetAsync(athleteModel.Number) : null;

        var gates = this.RegExpService.MatchInt(gatesMatch);
        var length = this.RegExpService.MatchInt(lengthMatch);
        var startAltitude = this.RegExpService.MatchInt(startAltitudeMatch);
        var verticalDrop = this.RegExpService.MatchInt(verticalDropMatch);

        return new ALPSlope
        {
            Gates = gates,
            Length = length,
            StartAltitude = startAltitude,
            VerticalDrop = verticalDrop,
            Setter = courseSetter?.Id
        };
    }

    private ALPRound CreateAlpineSkiingRound(DateTime? date, string format, RoundType roundType, string eventName)
    {
        return new ALPRound
        {
            Date = date,
            Format = format,
            Round = roundType,
            EventName = eventName
        };
    }
    #endregion ALPINE SKIING

    #region BASKETBALL
    private async Task ProcessBasketballAsync(ConvertOptions options)
    {
        var eventRound = this.CreateEventRound<BKBRound>(options.HtmlDocument, options.Event.Name);

        foreach (var table in options.Tables)
        {
            var round = this.CreateBasketballRound(eventRound.Dates.From, eventRound.Format, table.Round, eventRound.EventName);
            var rows = table.HtmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']//tr");

            foreach (var row in rows.Where(x => this.OlympediaService.IsMatchNumber(x.InnerText)))
            {
                var data = row.Elements("td").ToList();

                var team1NOCCode = DisciplineConstants.BASKETBALL == options.Discipline.Name && options.Game.Year <= 2016
                    ? this.OlympediaService.FindNOCCode(data[2].OuterHtml)
                    : this.OlympediaService.FindNOCCode(data[3].OuterHtml);

                var team1NOCCacheModel = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == team1NOCCode);
                var team1 = await this.teamsService.GetAsync(team1NOCCacheModel.Id, options.Event.Id);

                var match = new BKBMatch
                {
                    MatchNumber = this.OlympediaService.FindMatchNumber(data[0].InnerText),
                    Round = table.Round,
                    RoundInfo = table.RoundInfo,
                    MatchType = this.OlympediaService.FindMatchType(table.Round, data[0].InnerText),
                    MatchInfo = this.OlympediaService.FindMatchInfo(data[0].InnerText),
                    Date = this.dateService.ParseDate(data[1].InnerText, options.Game.Year).From,
                    ResultId = this.OlympediaService.FindResultNumber(data[0].OuterHtml),
                    Decision = this.OlympediaService.FindDecision(row.OuterHtml),
                    Team1 = new BKBTeam
                    {
                        Name = team1.Name,
                        TeamId = team1.Id,
                        NOCCode = team1NOCCode
                    }
                };

                if (match.Decision == DecisionType.None)
                {
                    var matchResult = DisciplineConstants.BASKETBALL == options.Discipline.Name && options.Game.Year <= 2016
                        ? this.OlympediaService.GetMatchResult(data[3].InnerText, MatchResultType.Points)
                        : this.OlympediaService.GetMatchResult(data[4].InnerText, MatchResultType.Points);

                    match.Team1.Result = matchResult.Result1;
                    match.Team1.Points = matchResult.Points1;

                    var team2NOCCode = DisciplineConstants.BASKETBALL == options.Discipline.Name && options.Game.Year <= 2016
                        ? this.OlympediaService.FindNOCCode(data[4].OuterHtml)
                        : this.OlympediaService.FindNOCCode(data[6].OuterHtml);

                    var team2NOCCacheModel = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == team2NOCCode);
                    var team2 = await this.teamsService.GetAsync(team2NOCCacheModel.Id, options.Event.Id);

                    match.Team2 = new BKBTeam
                    {
                        Name = team2.Name,
                        TeamId = team2.Id,
                        NOCCode = team2NOCCode,
                        Result = matchResult.Result2,
                        Points = matchResult.Points2
                    };

                    var document = options.Documents.FirstOrDefault(x => x.Url.EndsWith($"{match.ResultId}"));
                    if (document != null)
                    {
                        await this.SetBasketballPlayersStatisticsAsync(match, document, options.Game, options.Event, options.Discipline);
                    }
                }

                round.Matches.Add(match);
            }

            eventRound.Rounds.Add(round);
        }

        var resultJson = this.CreateResult<BKBRound>(options.Event, options.Discipline, options.Game);
        resultJson.Rounds = eventRound.Rounds;
        var json = JsonSerializer.Serialize(resultJson);
        var result = new Result
        {
            EventId = options.Event.Id,
            Json = json
        };

        await this.resultsService.AddOrUpdateAsync(result);
    }

    private async Task SetBasketballPlayersStatisticsAsync(BKBMatch match, Document document, GameCacheModel gameCache, EventCacheModel eventCache, DisciplineCacheModel disciplineCache)
    {
        var htmlDocument = this.CreateHtmlDocument(document);

        var tablesHtml = new List<string>();
        if (disciplineCache.Name == DisciplineConstants.BASKETBALL)
        {
            var attendanceMatch = this.RegExpService.Match(htmlDocument.ParsedText, @"<th>Attendance<\/th><td>(.*?)<\/td>");
            var attendance = this.RegExpService.MatchInt(attendanceMatch?.Groups[1]?.Value);
            var referees = this.RegExpService.Matches(htmlDocument.ParsedText, @"<th>Referee<\/th>(?:.*?)\/athletes\/(\d+)");
            var refereeIds = referees.Select(x => x.Groups[1].Value).ToList();

            if (gameCache.Year >= 2012)
            {
                var crewChief = this.RegExpService.Match(htmlDocument.ParsedText, @"<th>Crew Chief<\/th>(?:.*?)\/athletes\/(\d+)");
                var firstUmpire = this.RegExpService.Match(htmlDocument.ParsedText, @"<th>Umpire 1<\/th>(?:.*?)\/athletes\/(\d+)");
                var secondUmpire = this.RegExpService.Match(htmlDocument.ParsedText, @"<th>Umpire 2<\/th>(?:.*?)\/athletes\/(\d+)");

                refereeIds.AddRange(new List<string> { crewChief?.Groups[1]?.Value, firstUmpire?.Groups[1]?.Value, secondUmpire?.Groups[1]?.Value });
            }
            refereeIds = refereeIds.Where(x => !string.IsNullOrEmpty(x)).ToList();

            match.Attendance = attendance;
            match.FirstRefereeId = refereeIds.Count >= 1 ? await this.ExtractRefereeAsync(refereeIds[0]) : null;
            match.SecondRefereeId = refereeIds.Count >= 2 ? await this.ExtractRefereeAsync(refereeIds[1]) : null;
            match.ThirdRefereeId = refereeIds.Count >= 3 ? await this.ExtractRefereeAsync(refereeIds[2]) : null;

            tablesHtml = htmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']").Select(x => x.OuterHtml).ToList();
            if (gameCache.Year >= 2020)
            {
                tablesHtml = htmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']").Skip(1).Select(x => x.OuterHtml).ToList();
            }

            await this.SetBasketballBKBPlayersAsync(match.Team1, tablesHtml[0], eventCache, gameCache);
            await this.SetBasketballBKBPlayersAsync(match.Team2, tablesHtml[1], eventCache, gameCache);
        }
        else
        {
            var firstReferee = this.RegExpService.MatchFirstGroup(htmlDocument.ParsedText, @"<th>Referee #1<\/th>(?:.*?)\/athletes\/(\d+)");
            var secondReferee = this.RegExpService.MatchFirstGroup(htmlDocument.ParsedText, @"<th>Referee #2<\/th>(?:.*?)\/athletes\/(\d+)");
            match.FirstRefereeId = await this.ExtractRefereeAsync(firstReferee);
            match.SecondRefereeId = await this.ExtractRefereeAsync(secondReferee);

            tablesHtml = htmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']").Skip(1).Select(x => x.OuterHtml).ToList();

            await this.SetBasketball3x3BKBPlayersAsync(match.Team1, tablesHtml[0], eventCache, gameCache);
            await this.SetBasketball3x3BKBPlayersAsync(match.Team2, tablesHtml[1], eventCache, gameCache);
        }

        match.Team1.Statistic = new BKBStatistic
        {
            Assists = match.Team1.Players.Sum(x => x.Statistic.Assists),
            BlockedShots = match.Team1.Players.Sum(x => x.Statistic.BlockedShots),
            DefensiveRebounds = match.Team1.Players.Sum(x => x.Statistic.DefensiveRebounds),
            DisqualifyingFouls = match.Team1.Players.Sum(x => x.Statistic.DisqualifyingFouls),
            FreeThrowsAttempts = match.Team1.Players.Sum(x => x.Statistic.FreeThrowsAttempts),
            FreeThrowsGoals = match.Team1.Players.Sum(x => x.Statistic.FreeThrowsGoals),
            OffensiveRebounds = match.Team1.Players.Sum(x => x.Statistic.OffensiveRebounds),
            OnePointsAttempts = match.Team1.Players.Sum(x => x.Statistic.OnePointsAttempts),
            OnePointsGoals = match.Team1.Players.Sum(x => x.Statistic.OnePointsGoals),
            PersonalFouls = match.Team1.Players.Sum(x => x.Statistic.PersonalFouls),
            Steals = match.Team1.Players.Sum(x => x.Statistic.Steals),
            ThreePointsAttempts = match.Team1.Players.Sum(x => x.Statistic.ThreePointsAttempts),
            ThreePointsGoals = match.Team1.Players.Sum(x => x.Statistic.ThreePointsGoals),
            TotalFieldGoals = match.Team1.Players.Sum(x => x.Statistic.TotalFieldGoals),
            TotalFieldGoalsAttempts = match.Team1.Players.Sum(x => x.Statistic.TotalFieldGoalsAttempts),
            TotalRebounds = match.Team1.Players.Sum(x => x.Statistic.DefensiveRebounds) + match.Team1.Players.Sum(x => x.Statistic.OffensiveRebounds),
            Turnovers = match.Team1.Players.Sum(x => x.Statistic.Turnovers),
            TwoPointsAttempts = match.Team1.Players.Sum(x => x.Statistic.TwoPointsAttempts),
            TwoPointsGoals = match.Team1.Players.Sum(x => x.Statistic.TwoPointsGoals),
        };

        match.Team2.Statistic = new BKBStatistic
        {
            Assists = match.Team2.Players.Sum(x => x.Statistic.Assists),
            BlockedShots = match.Team2.Players.Sum(x => x.Statistic.BlockedShots),
            DefensiveRebounds = match.Team2.Players.Sum(x => x.Statistic.DefensiveRebounds),
            DisqualifyingFouls = match.Team2.Players.Sum(x => x.Statistic.DisqualifyingFouls),
            FreeThrowsAttempts = match.Team2.Players.Sum(x => x.Statistic.FreeThrowsAttempts),
            FreeThrowsGoals = match.Team2.Players.Sum(x => x.Statistic.FreeThrowsGoals),
            OffensiveRebounds = match.Team2.Players.Sum(x => x.Statistic.OffensiveRebounds),
            OnePointsAttempts = match.Team2.Players.Sum(x => x.Statistic.OnePointsAttempts),
            OnePointsGoals = match.Team2.Players.Sum(x => x.Statistic.OnePointsGoals),
            PersonalFouls = match.Team2.Players.Sum(x => x.Statistic.PersonalFouls),
            Steals = match.Team2.Players.Sum(x => x.Statistic.Steals),
            ThreePointsAttempts = match.Team2.Players.Sum(x => x.Statistic.ThreePointsAttempts),
            ThreePointsGoals = match.Team2.Players.Sum(x => x.Statistic.ThreePointsGoals),
            TotalFieldGoals = match.Team2.Players.Sum(x => x.Statistic.TotalFieldGoals),
            TotalFieldGoalsAttempts = match.Team2.Players.Sum(x => x.Statistic.TotalFieldGoalsAttempts),
            TotalRebounds = match.Team2.Players.Sum(x => x.Statistic.DefensiveRebounds) + match.Team2.Players.Sum(x => x.Statistic.OffensiveRebounds),
            Turnovers = match.Team2.Players.Sum(x => x.Statistic.Turnovers),
            TwoPointsAttempts = match.Team2.Players.Sum(x => x.Statistic.TwoPointsAttempts),
            TwoPointsGoals = match.Team2.Players.Sum(x => x.Statistic.TwoPointsGoals),
        };

        if (disciplineCache.Name == DisciplineConstants.BASKETBALL)
        {
            var html = this.RegExpService.MatchFirstGroup(htmlDocument.ParsedText, @"Score<\/h2><table class=""biodata"">(.*?)<\/table>");
            if (!string.IsNullOrEmpty(html))
            {
                var scoreTableDocument = new HtmlDocument();
                scoreTableDocument.LoadHtml(html);

                var scoreTableTrs = scoreTableDocument.DocumentNode.SelectNodes("//tr").Skip(1).ToList();
                var homeTeamScore = scoreTableTrs[0].Elements("td").ToList();
                var awayTeamScore = scoreTableTrs[1].Elements("td").ToList();

                match.Team1.Statistic.FirstHalfPoints = this.RegExpService.MatchInt(homeTeamScore[2].InnerText);
                match.Team1.Statistic.SecondHalfPoints = this.RegExpService.MatchInt(homeTeamScore[3].InnerText);
                match.Team2.Statistic.FirstHalfPoints = this.RegExpService.MatchInt(awayTeamScore[2].InnerText);
                match.Team2.Statistic.SecondHalfPoints = this.RegExpService.MatchInt(awayTeamScore[3].InnerText);
            }

            if (gameCache.Year >= 2020)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(tablesHtml[0]);
                var rows = doc.DocumentNode.SelectNodes("//tr").ToList();
                var scoreTableDocument = new HtmlDocument();
                scoreTableDocument.LoadHtml(rows[rows.Count - 2].OuterHtml);
                var homeTeamTds = scoreTableDocument.DocumentNode.SelectNodes("//td");
                match.Team1.Statistic.FirstHalfPoints = this.RegExpService.MatchInt(homeTeamTds[2].InnerText);
                match.Team1.Statistic.SecondHalfPoints = this.RegExpService.MatchInt(homeTeamTds[3].InnerText);

                doc.LoadHtml(tablesHtml[1]);
                rows = doc.DocumentNode.SelectNodes("//tr").ToList();
                scoreTableDocument = new HtmlDocument();
                scoreTableDocument.LoadHtml(rows[rows.Count - 2].OuterHtml);
                var awayTeamTds = scoreTableDocument.DocumentNode.SelectNodes("//td");
                match.Team2.Statistic.FirstHalfPoints = this.RegExpService.MatchInt(awayTeamTds[2].InnerText);
                match.Team2.Statistic.SecondHalfPoints = this.RegExpService.MatchInt(awayTeamTds[3].InnerText);
            }
        }
    }

    private async Task SetBasketball3x3BKBPlayersAsync(BKBTeam team, string html, EventCacheModel eventCache, GameCacheModel gameCache)
    {
        var document = new HtmlDocument();
        document.LoadHtml(html);
        var rows = document.DocumentNode.SelectNodes("//tr");

        for (int i = 1; i < rows.Count - 1; i++)
        {
            var data = rows[i].Elements("td").ToList();

            var athleteModel = this.OlympediaService.FindAthlete(data[2].OuterHtml);
            var participant = await this.participantsService.GetAsync(athleteModel.Number, eventCache.Id);
            var onePointMatch = this.RegExpService.Match(data[9].InnerText, @"(\d+)\/(\d+)");
            var twoPointMatch = this.RegExpService.Match(data[11].InnerText, @"(\d+)\/(\d+)");
            var freeThrowPointMatch = this.RegExpService.Match(data[15].InnerText, @"(\d+)\/(\d+)");

            var player = new BKBPlayer
            {
                Name = athleteModel.Name,
                AthleteNumber = athleteModel.Number,
                NOCCode = team.NOCCode,
                ParticipantId = participant.Id,
                Position = data[0]?.InnerText.Trim(),
                Number = this.RegExpService.MatchInt(data[1]?.InnerText),
                Points = this.RegExpService.MatchInt(data[4]?.InnerText),
                TimePlayed = this.dateService.ParseTime(data[5]?.InnerText),
                PlayerValue = this.RegExpService.MatchDouble(data[6].InnerText),
                Statistic = new BKBStatistic
                {
                    PlusMinus = this.RegExpService.MatchInt(data[7]?.InnerText.Replace("+", string.Empty)),
                    ShootingEfficiency = this.RegExpService.MatchDouble(data[8]?.InnerText),
                    OnePointsGoals = this.RegExpService.MatchInt(onePointMatch?.Groups[1].Value),
                    OnePointsAttempts = this.RegExpService.MatchInt(onePointMatch?.Groups[2].Value),
                    TwoPointsGoals = this.RegExpService.MatchInt(twoPointMatch?.Groups[1].Value),
                    TwoPointsAttempts = this.RegExpService.MatchInt(twoPointMatch?.Groups[2].Value),
                    FreeThrowsGoals = this.RegExpService.MatchInt(freeThrowPointMatch?.Groups[1].Value),
                    FreeThrowsAttempts = this.RegExpService.MatchInt(freeThrowPointMatch?.Groups[2].Value),
                    OffensiveRebounds = this.RegExpService.MatchInt(data[18]?.InnerText),
                    DefensiveRebounds = this.RegExpService.MatchInt(data[19]?.InnerText),
                    BlockedShots = this.RegExpService.MatchInt(data[20]?.InnerText),
                    Turnovers = this.RegExpService.MatchInt(data[21]?.InnerText)
                }
            };

            player.Statistic.TotalFieldGoals = player.Statistic.OnePointsGoals + player.Statistic.TwoPointsGoals;
            player.Statistic.TotalFieldGoalsAttempts = player.Statistic.OnePointsAttempts + player.Statistic.TwoPointsAttempts;
            player.Statistic.TotalRebounds = player.Statistic.OffensiveRebounds + player.Statistic.DefensiveRebounds;

            team.Players.Add(player);
        }
    }

    private BKBRound CreateBasketballRound(DateTime? date, string format, RoundType roundType, string eventName)
    {
        return new BKBRound
        {
            Date = date,
            Format = format,
            EventName = eventName,
            Round = roundType
        };
    }

    private async Task SetBasketballBKBPlayersAsync(BKBTeam team, string html, EventCacheModel eventCache, GameCacheModel gameCache)
    {
        var document = new HtmlDocument();
        document.LoadHtml(html);
        var rows = document.DocumentNode.SelectNodes("//tr");

        for (int i = 1; i < rows.Count - 3; i++)
        {
            var data = rows[i].Elements("td").ToList();

            var athleteModel = this.OlympediaService.FindAthlete(data[2].OuterHtml);
            var participant = await this.participantsService.GetAsync(athleteModel.Number, eventCache.Id);

            var player = new BKBPlayer
            {
                Name = athleteModel.Name,
                AthleteNumber = athleteModel.Number,
                ParticipantId = participant.Id,
                NOCCode = team.NOCCode
            };

            if (gameCache.Year >= 2020)
            {
                var freeThrowPointMatch = this.RegExpService.Match(data[16].InnerText, @"(\d+)\/(\d+)");

                player.Position = data[0]?.InnerText.Trim();
                player.Number = this.RegExpService.MatchInt(data[1]?.InnerText);
                player.Points = this.RegExpService.MatchInt(data[4]?.InnerText);
                player.TimePlayed = this.dateService.ParseTime(data[7]?.InnerText);
                player.Statistic = new BKBStatistic
                {
                    TwoPointsGoals = this.RegExpService.MatchInt(data[8]?.InnerText),
                    TwoPointsAttempts = this.RegExpService.MatchInt(data[9]?.InnerText),
                    ThreePointsGoals = this.RegExpService.MatchInt(data[11]?.InnerText),
                    ThreePointsAttempts = this.RegExpService.MatchInt(data[12]?.InnerText),
                    FreeThrowsGoals = this.RegExpService.MatchInt(freeThrowPointMatch?.Groups[1].Value),
                    FreeThrowsAttempts = this.RegExpService.MatchInt(freeThrowPointMatch?.Groups[2].Value),
                    OffensiveRebounds = this.RegExpService.MatchInt(data[18]?.InnerText),
                    DefensiveRebounds = this.RegExpService.MatchInt(data[19]?.InnerText),
                    Assists = this.RegExpService.MatchInt(data[21]?.InnerText),
                    Steals = this.RegExpService.MatchInt(data[22]?.InnerText),
                    Turnovers = this.RegExpService.MatchInt(data[23]?.InnerText),
                    BlockedShots = this.RegExpService.MatchInt(data[24]?.InnerText),
                    PersonalFouls = this.RegExpService.MatchInt(data[25]?.InnerText),
                    DisqualifyingFouls = this.RegExpService.MatchInt(data[26]?.InnerText)
                };
            }
            else
            {
                var twoPointMatch = this.RegExpService.Match(data[5].InnerText, @"(\d+)\/(\d+)");
                var threePointMatch = this.RegExpService.Match(data[7].InnerText, @"(\d+)\/(\d+)");
                var freeThrowPointMatch = this.RegExpService.Match(data[11].InnerText, @"(\d+)\/(\d+)");

                player.Position = data[0]?.InnerText.Trim();
                player.Number = this.RegExpService.MatchInt(data[1]?.InnerText);
                player.Points = this.RegExpService.MatchInt(data[3]?.InnerText);
                player.TimePlayed = this.dateService.ParseTime(data[4]?.InnerText);
                player.Statistic = new BKBStatistic
                {
                    TwoPointsGoals = this.RegExpService.MatchInt(twoPointMatch?.Groups[1].Value),
                    TwoPointsAttempts = this.RegExpService.MatchInt(twoPointMatch?.Groups[2].Value),
                    ThreePointsGoals = this.RegExpService.MatchInt(threePointMatch?.Groups[1].Value),
                    ThreePointsAttempts = this.RegExpService.MatchInt(threePointMatch?.Groups[2].Value),
                    FreeThrowsGoals = this.RegExpService.MatchInt(freeThrowPointMatch?.Groups[1].Value),
                    FreeThrowsAttempts = this.RegExpService.MatchInt(freeThrowPointMatch?.Groups[2].Value),
                    OffensiveRebounds = this.RegExpService.MatchInt(data[13]?.InnerText),
                    DefensiveRebounds = this.RegExpService.MatchInt(data[14]?.InnerText),
                    Assists = this.RegExpService.MatchInt(data[16]?.InnerText),
                    Steals = this.RegExpService.MatchInt(data[17]?.InnerText),
                    BlockedShots = this.RegExpService.MatchInt(data[18]?.InnerText),
                    Turnovers = this.RegExpService.MatchInt(data[19]?.InnerText),
                    PersonalFouls = this.RegExpService.MatchInt(data[20]?.InnerText),
                    DisqualifyingFouls = this.RegExpService.MatchInt(data[21]?.InnerText),
                };
            }

            player.Statistic.TotalFieldGoals = player.Statistic.TwoPointsGoals + player.Statistic.ThreePointsGoals;
            player.Statistic.TotalFieldGoalsAttempts = player.Statistic.TwoPointsAttempts + player.Statistic.ThreePointsAttempts;
            player.Statistic.TotalRebounds = player.Statistic.OffensiveRebounds + player.Statistic.DefensiveRebounds;

            team.Players.Add(player);
        }
    }
    #endregion BASKETBALL
}