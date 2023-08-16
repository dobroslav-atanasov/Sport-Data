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
using SportData.Data.Models.OlympicGames.AlpineSkiing;
using SportData.Data.Models.OlympicGames.Archery;
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
                        //case DisciplineConstants.BASKETBALL_3X3:
                        //    await this.ProcessBasketball3x3Async(document, documents, gameCacheModel, disciplineCacheModel, eventCacheModel, standingTable, tables);
                        //    break;
                        //case DisciplineConstants.ALPINE_SKIING:
                        //    await this.ProcessAlpineSkiingAsync(document, documents, gameCacheModel, disciplineCacheModel, eventCacheModel, standingTable, tables);
                        //    break;
                        case DisciplineConstants.ARCHERY:
                            Console.WriteLine(group.Identifier);
                            //await this.ProcessArcheryAsync(document, documents, gameCacheModel, disciplineCacheModel, eventCacheModel, standingTable, tables);
                            break;
                            //case DisciplineConstants.BASKETBALL:
                            //    await this.ProcessBasketballAsync(document, documents, gameCacheModel, disciplineCacheModel, eventCacheModel, standingTable, tables);
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

    #region ARCHERY
    private async Task ProcessArcheryAsync(HtmlDocument document, IOrderedEnumerable<Document> documents, GameCacheModel gameCacheModel, DisciplineCacheModel disciplineCacheModel, EventCacheModel eventCacheModel, TableModel standingTable, IList<TableModel> tables)
    {
        var format = this.RegExpService.MatchFirstGroup(document.DocumentNode.OuterHtml, @"<th>Format<\/th>\s*<td(?:.*?)>(.*?)<\/td>");
        var dateString = this.RegExpService.MatchFirstGroup(document.DocumentNode.OuterHtml, @"<th>\s*Date\s*<\/th>\s*<td>(.*?)<\/td>");
        var dateModel = this.dateService.ParseDate(dateString);

        if (!eventCacheModel.IsTeamEvent)
        {
            var archeryEvent = new ArcheryEvent
            {
                Format = format,
                StartDate = dateModel.StartDateTime,
                EndDate = dateModel.EndDateTime,
                Participants = new List<ArcheryParticipant>()
            };

            if (gameCacheModel.Year <= 1920)
            {
                var rows = standingTable.HtmlDocument.DocumentNode.SelectNodes("//tr");
                var headers = rows.First().Elements("th").Select(x => x.InnerText).ToList();
                var indexes = this.OlympediaService.FindIndexes(headers);
                foreach (var row in rows.Skip(1))
                {
                    var data = row.Elements("td").ToList();
                    var athleteNumber = this.OlympediaService.FindAthleteNumber(data[indexes[ConverterConstants.INDEX_NAME]].OuterHtml);
                    var nocCode = this.OlympediaService.FindCountryCode(data[indexes[ConverterConstants.INDEX_NOC]].OuterHtml);
                    var nocCacheModel = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == nocCode);
                    var participant = await this.participantsService.GetAsync(athleteNumber, eventCacheModel.Id, nocCacheModel.Id);

                    if (participant != null)
                    {
                        archeryEvent.Participants.Add(new ArcheryParticipant
                        {
                            ParticipantId = participant.Id,
                            Points = indexes.ContainsKey(ConverterConstants.INDEX_POINTS) ? this.RegExpService.MatchDouble(data[indexes[ConverterConstants.INDEX_POINTS]].InnerText) : null,
                            TargetsHit = indexes.ContainsKey(ConverterConstants.INDEX_TH) ? this.RegExpService.MatchInt(data[indexes[ConverterConstants.INDEX_TH]].InnerText) : null,
                            Score = indexes.ContainsKey(ConverterConstants.INDEX_SCORE) ? this.RegExpService.MatchInt(data[indexes[ConverterConstants.INDEX_SCORE]].InnerText) : null,
                            Yards40 = indexes.ContainsKey(ConverterConstants.INDEX_40_YARDS) ? this.RegExpService.MatchInt(data[indexes[ConverterConstants.INDEX_40_YARDS]].InnerText) : null,
                            Yards60 = indexes.ContainsKey(ConverterConstants.INDEX_60_YARDS) ? this.RegExpService.MatchInt(data[indexes[ConverterConstants.INDEX_60_YARDS]].InnerText) : null,
                            Points30Meters = indexes.ContainsKey(ConverterConstants.INDEX_30_YARDS) ? this.RegExpService.MatchInt(data[indexes[ConverterConstants.INDEX_30_YARDS]].InnerText) : null,
                            Points50Meters = indexes.ContainsKey(ConverterConstants.INDEX_50_YARDS) ? this.RegExpService.MatchInt(data[indexes[ConverterConstants.INDEX_50_YARDS]].InnerText) : null,
                            Points70Meters = indexes.ContainsKey(ConverterConstants.INDEX_80_YARDS) ? this.RegExpService.MatchInt(data[indexes[ConverterConstants.INDEX_80_YARDS]].InnerText) : null,
                            Points90Meters = indexes.ContainsKey(ConverterConstants.INDEX_100_YARDS) ? this.RegExpService.MatchInt(data[indexes[ConverterConstants.INDEX_100_YARDS]].InnerText) : null,
                            Golds = indexes.ContainsKey(ConverterConstants.INDEX_GOLDS) ? this.RegExpService.MatchInt(data[indexes[ConverterConstants.INDEX_GOLDS]].InnerText) : null,
                        });
                    }
                }
            }
            else if (gameCacheModel.Year >= 1972 && gameCacheModel.Year <= 1988)
            {

            }
            else
            {

            }

            var json = JsonSerializer.Serialize(archeryEvent);
            //var result = new Result
            //{
            //    EventId = eventCacheModel.Id,
            //    Json = json
            //};

            //await this.resultsService.AddOrUpdateAsync(result);
        }
        else
        {

        }
    }

    //private async Task<List<ArcheryParticipant>> ExtractArcheryParticipantsAsync(HtmlDocument document, EventCacheModel eventCacheModel, GameCacheModel gameCacheModel)
    //{

    //}
    #endregion ARCHERY

    #region ALPINE SKIING
    private async Task ProcessAlpineSkiingAsync(HtmlDocument document, IOrderedEnumerable<Document> documents, GameCacheModel gameCacheModel, DisciplineCacheModel disciplineCacheModel, EventCacheModel eventCacheModel, TableModel standingTable, IList<TableModel> tables)
    {
        var format = this.RegExpService.MatchFirstGroup(document.DocumentNode.OuterHtml, @"<th>Format<\/th>\s*<td(?:.*?)>(.*?)<\/td>");
        var dateString = this.RegExpService.MatchFirstGroup(document.DocumentNode.OuterHtml, @"<th>\s*Date\s*<\/th>\s*<td>(.*?)<\/td>");
        var courseSetterString = this.RegExpService.MatchFirstGroup(document.DocumentNode.OuterHtml, @"<th>\s*Course Setter\s*<\/th>\s*<td(.*?)<\/td>");
        var gatesMatch = this.RegExpService.MatchFirstGroup(document.DocumentNode.OuterHtml, @"Gates:(.*?)<br>");
        var lengthMatch = this.RegExpService.MatchFirstGroup(document.DocumentNode.OuterHtml, @"Length:(.*?)<br>");
        var startAltitudeMatch = this.RegExpService.MatchFirstGroup(document.DocumentNode.OuterHtml, @"Start Altitude:(.*?)<br>");
        var verticalDropMatch = this.RegExpService.MatchFirstGroup(document.DocumentNode.OuterHtml, @"Vertical Drop:(.*?)<\/td>");

        var date = this.dateService.ParseDate(dateString);
        var courseSetter = await this.athletesService.GetAsync(this.OlympediaService.FindAthleteNumber(courseSetterString));
        var gates = this.RegExpService.MatchInt(gatesMatch);
        var length = this.RegExpService.MatchInt(lengthMatch);
        var startAltitude = this.RegExpService.MatchInt(startAltitudeMatch);
        var verticalDrop = this.RegExpService.MatchInt(verticalDropMatch);

        if (!eventCacheModel.IsTeamEvent)
        {
            var alpineSkiingEvent = new AlpineSkiingEvent
            {
                Format = format,
                Participants = new List<AlpineSkiingParticipant>(),
                Run1Date = date.StartDateTime,
                Run1SlopeInformation = new Slope
                {
                    Gates = gates,
                    Length = length,
                    StartAltitude = startAltitude,
                    VerticalDrop = verticalDrop,
                    Setter = courseSetter?.Id
                }
            };

            if (documents.Any())
            {
                foreach (var doc in documents)
                {
                    var htmlDocument = this.CreateHtmlDocument(doc);
                    var title = htmlDocument.DocumentNode.SelectSingleNode("//h1");
                    dateString = this.RegExpService.MatchFirstGroup(htmlDocument.DocumentNode.OuterHtml, @"<th>\s*Date\s*<\/th>\s*<td>(.*?)<\/td>");
                    courseSetterString = this.RegExpService.MatchFirstGroup(htmlDocument.DocumentNode.OuterHtml, @"<th>\s*Course Setter\s*<\/th>\s*<td(.*?)<\/td>");
                    gatesMatch = this.RegExpService.MatchFirstGroup(htmlDocument.DocumentNode.OuterHtml, @"Gates:(.*?)<br>");
                    lengthMatch = this.RegExpService.MatchFirstGroup(htmlDocument.DocumentNode.OuterHtml, @"Length:(.*?)<br>");
                    startAltitudeMatch = this.RegExpService.MatchFirstGroup(htmlDocument.DocumentNode.OuterHtml, @"Start Altitude:(.*?)<br>");
                    verticalDropMatch = this.RegExpService.MatchFirstGroup(htmlDocument.DocumentNode.OuterHtml, @"Vertical Drop:(.*?)<\/td>");

                    date = this.dateService.ParseDate(dateString);
                    courseSetter = await this.athletesService.GetAsync(this.OlympediaService.FindAthleteNumber(courseSetterString));
                    gates = this.RegExpService.MatchInt(gatesMatch);
                    length = this.RegExpService.MatchInt(lengthMatch);
                    startAltitude = this.RegExpService.MatchInt(startAltitudeMatch);
                    verticalDrop = this.RegExpService.MatchInt(verticalDropMatch);

                    if ((title.InnerText.Contains("Combined") && title.InnerText.Contains("Slalom")) || title.InnerText.Contains("Run 2"))
                    {
                        alpineSkiingEvent.Run2Date = date.StartDateTime;
                        alpineSkiingEvent.Run2SlopeInformation = new Slope
                        {
                            Gates = gates,
                            Length = length,
                            StartAltitude = startAltitude,
                            VerticalDrop = verticalDrop,
                            Setter = courseSetter?.Id
                        };
                    }
                    else
                    {
                        alpineSkiingEvent.Run1Date = date.StartDateTime;
                        alpineSkiingEvent.Run1SlopeInformation = new Slope
                        {
                            Gates = gates,
                            Length = length,
                            StartAltitude = startAltitude,
                            VerticalDrop = verticalDrop,
                            Setter = courseSetter?.Id
                        };
                    }

                    await this.ExtractAlpineSkiingParticipantsAsync(htmlDocument, eventCacheModel, gameCacheModel, alpineSkiingEvent);
                }
            }
            else
            {
                await this.ExtractAlpineSkiingParticipantsAsync(document, eventCacheModel, gameCacheModel, alpineSkiingEvent);
            }

            var json = JsonSerializer.Serialize(alpineSkiingEvent);
            var result = new Result
            {
                EventId = eventCacheModel.Id,
                Json = json
            };

            await this.resultsService.AddOrUpdateAsync(result);
        }
        else
        {
            var alpineSkiingEvent = new AlpineSkiingEvent
            {
                Format = format,
                Matches = new List<AlpineSkiingMatch>(),
                Run1Date = this.dateService.ParseDate(dateString).StartDateTime,
                Run1SlopeInformation = new Slope
                {
                    Gates = gates,
                    Length = length,
                    StartAltitude = startAltitude,
                    VerticalDrop = verticalDrop,
                    Setter = courseSetter?.Id
                }
            };

            var matches = new List<AlpineSkiingMatch>();
            foreach (var table in tables)
            {
                var rows = table.HtmlDocument.DocumentNode.SelectNodes("//tr").Where(x => this.OlympediaService.IsMatchNumber(x.InnerText)).ToList();
                foreach (var row in rows)
                {
                    var tdNodes = row.Elements("td").ToList();
                    var homeTeamName = tdNodes[3].InnerText;
                    var homeTeamCode = tdNodes[4].OuterHtml;
                    var resultString = tdNodes[5].InnerText;
                    var awayTeamName = tdNodes[6].InnerText;
                    var awayTeamCode = tdNodes[7].OuterHtml;
                    var resultModel = this.OlympediaService.GetResult(resultString);
                    var homeNOCCacheModel = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == this.OlympediaService.FindCountryCode(homeTeamCode));
                    var homeTeam = await this.teamsService.GetAsync(homeNOCCacheModel.Id, eventCacheModel.Id);
                    var awayNOCCacheModel = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == this.OlympediaService.FindCountryCode(awayTeamCode));
                    var awayTeam = await this.teamsService.GetAsync(awayNOCCacheModel.Id, eventCacheModel.Id);

                    var match = new AlpineSkiingMatch
                    {
                        MatchNumber = this.OlympediaService.FindMatchNumber(tdNodes[0].InnerText),
                        Round = table.Round,
                        RoundInfo = table.RoundInfo,
                        MatchType = this.OlympediaService.FindMatchType(table.Round, tdNodes[0].InnerText),
                        MatchInfo = this.OlympediaService.FindMatchInfo(tdNodes[0].InnerText),
                        Date = this.dateService.MatchDate(tdNodes[1].InnerText, gameCacheModel.Year),
                        ResultId = this.OlympediaService.FindResultNumber(tdNodes[0].OuterHtml),
                        Decision = resultModel.Decision,
                        HomeTeam = new AlpineSkiingTeam
                        {
                            Name = homeTeamName,
                            TeamId = homeTeam.Id,
                            Result = resultModel.HomeTeamResult,
                            Points = resultModel.HomeTeamPoints,
                            Participants = new List<AlpineSkiingParticipant>()
                        },
                        AwayTeam = new AlpineSkiingTeam
                        {
                            Name = awayTeamName,
                            TeamId = awayTeam.Id,
                            Result = resultModel.AwayTeamResult,
                            Points = resultModel.AwayTeamPoints,
                            Participants = new List<AlpineSkiingParticipant>()
                        }
                    };

                    var matchDocument = documents.FirstOrDefault(x => x.Url.EndsWith($"{match.ResultId}"));
                    if (matchDocument != null)
                    {
                        var htmlDocument = this.CreateHtmlDocument(matchDocument);
                        var raceTable = htmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']").Last();
                        var tableRows = raceTable.Elements("tr");

                        foreach (var tableRow in tableRows)
                        {
                            var trDocument = new HtmlDocument();
                            trDocument.LoadHtml(tableRow.OuterHtml);
                            var nodes = trDocument.DocumentNode.SelectNodes("//td");

                            var race = nodes[0].InnerText.Replace("Race #", string.Empty).Trim();
                            var firstParticipantNumber = this.OlympediaService.FindAthleteNumber(nodes[3].OuterHtml);
                            var firstParticipantNOCCode = this.OlympediaService.FindCountryCode(nodes[4].OuterHtml);
                            var firstParticipant = await this.participantsService.GetAsync(firstParticipantNumber, eventCacheModel.Id);
                            var secondParticipantNumber = this.OlympediaService.FindAthleteNumber(nodes[6].OuterHtml);
                            var secondParticipantNOCCode = this.OlympediaService.FindCountryCode(nodes[7].OuterHtml);
                            var secondParticipant = await this.participantsService.GetAsync(secondParticipantNumber, eventCacheModel.Id);
                            var raceResultModel = this.OlympediaService.GetResult(nodes[5].InnerHtml);

                            var firstAlpineSkiingParticipant = new AlpineSkiingParticipant
                            {
                                Race = race,
                                ParticipantId = firstParticipant.Id,
                                Run1Time = raceResultModel.HomeTeamTime,
                                Result = raceResultModel.HomeTeamResult
                            };

                            var secondAlpineSkiingParticipant = new AlpineSkiingParticipant
                            {
                                Race = race,
                                ParticipantId = secondParticipant.Id,
                                Run1Time = raceResultModel.AwayTeamTime,
                                Result = raceResultModel.AwayTeamResult
                            };

                            if (firstParticipantNOCCode == homeNOCCacheModel.Code)
                            {
                                match.HomeTeam.Participants.Add(firstAlpineSkiingParticipant);
                                match.AwayTeam.Participants.Add(secondAlpineSkiingParticipant);
                            }
                            else
                            {
                                match.HomeTeam.Participants.Add(secondAlpineSkiingParticipant);
                                match.AwayTeam.Participants.Add(firstAlpineSkiingParticipant);
                            }
                        }

                        if (match.HomeTeam.Result == ResultType.Draw)
                        {
                            match.HomeTeam.Time = new DateTime(match.HomeTeam.Participants.Where(x => x.Result == ResultType.Win).Sum(x => x.Run1Time.Value.Ticks));
                            match.AwayTeam.Time = new DateTime(match.AwayTeam.Participants.Where(x => x.Result == ResultType.Win).Sum(x => x.Run1Time.Value.Ticks));

                            if (match.HomeTeam.Time < match.AwayTeam.Time)
                            {
                                match.HomeTeam.Result = ResultType.Win;
                                match.AwayTeam.Result = ResultType.Loss;
                            }
                            else
                            {
                                match.HomeTeam.Result = ResultType.Loss;
                                match.AwayTeam.Result = ResultType.Win;
                            }
                        }
                    }

                    alpineSkiingEvent.Matches.Add(match);
                }
            }

            var json = JsonSerializer.Serialize(alpineSkiingEvent);
            var result = new Result
            {
                EventId = eventCacheModel.Id,
                Json = json
            };

            await this.resultsService.AddOrUpdateAsync(result);
        }
    }

    private async Task ExtractAlpineSkiingParticipantsAsync(HtmlDocument document, EventCacheModel eventCacheModel, GameCacheModel gameCacheModel, AlpineSkiingEvent alpineSkiingEvent)
    {
        var alpineSkiingParticipants = new List<AlpineSkiingParticipant>();
        var rows = document.DocumentNode.SelectNodes("//table[@class='table table-striped']//tr");
        var headers = rows.First().Elements("th").Select(x => x.InnerText).ToList();
        var indexes = this.OlympediaService.FindIndexes(headers);
        var title = document.DocumentNode.SelectSingleNode("//h1");

        foreach (var row in rows.Skip(1))
        {
            var data = row.Elements("td").ToList();
            var athleteNumber = this.OlympediaService.FindAthleteNumber(data[indexes[ConverterConstants.INDEX_NAME]].OuterHtml);
            var nocCode = this.OlympediaService.FindCountryCode(data[indexes[ConverterConstants.INDEX_NOC]].OuterHtml);
            var nocCacheModel = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == nocCode);
            var participant = await this.participantsService.GetAsync(athleteNumber, eventCacheModel.Id, nocCacheModel.Id);

            var alpineSkiingParticipant = new AlpineSkiingParticipant();
            if (alpineSkiingEvent.Participants.Any(x => x.ParticipantId == participant.Id))
            {
                alpineSkiingParticipant = alpineSkiingEvent.Participants.FirstOrDefault(x => x.ParticipantId == participant.Id);
            }
            else
            {
                alpineSkiingEvent.Participants.Add(alpineSkiingParticipant);
                alpineSkiingParticipant.ParticipantId = participant.Id;
            }

            if (title.InnerText.Contains("Combined") && title.InnerText.Contains("Downhill"))
            {
                alpineSkiingParticipant.Run3FinishStatus = this.OlympediaService.FindStatus(row.OuterHtml);
                alpineSkiingParticipant.Run3Time = this.dateService.ParseTime(data[indexes[ConverterConstants.INDEX_TIME]].InnerText);
                alpineSkiingParticipant.Run3Points = indexes.ContainsKey(ConverterConstants.INDEX_POINTS) ? this.RegExpService.MatchDouble(data[indexes[ConverterConstants.INDEX_POINTS]].InnerText) : null;
                alpineSkiingParticipant.Run3Number = this.RegExpService.MatchInt(data[indexes[ConverterConstants.INDEX_NR]].InnerText);
            }
            else if (title.InnerText.Contains("Combined") && title.InnerText.Contains("Slalom") && gameCacheModel.Year <= 2006)
            {
                alpineSkiingParticipant.Run1Points = indexes.ContainsKey(ConverterConstants.INDEX_POINTS) ? this.RegExpService.MatchDouble(data[indexes[ConverterConstants.INDEX_POINTS]].InnerText) : null;
                alpineSkiingParticipant.Run1FinishStatus = this.OlympediaService.FindStatus(data[indexes[ConverterConstants.INDEX_RUN1]].OuterHtml);
                alpineSkiingParticipant.Run1Time = this.dateService.ParseTime(data[indexes[ConverterConstants.INDEX_RUN1]].InnerText);
                alpineSkiingParticipant.Run1Number = this.RegExpService.MatchInt(data[indexes[ConverterConstants.INDEX_NR]].InnerText);
                alpineSkiingParticipant.Run2FinishStatus = this.OlympediaService.FindStatus(data[indexes[ConverterConstants.INDEX_RUN2]].OuterHtml);
                alpineSkiingParticipant.Run2Time = this.dateService.ParseTime(data[indexes[ConverterConstants.INDEX_RUN2]].InnerText);
                alpineSkiingParticipant.Run2Number = alpineSkiingParticipant.Run1Number;
            }
            else if (title.InnerText.Contains("Combined") && title.InnerText.Contains("Slalom") && gameCacheModel.Year >= 2010)
            {
                alpineSkiingParticipant.Run1Time = this.dateService.ParseTime(data[indexes[ConverterConstants.INDEX_TIME]].InnerText);
                alpineSkiingParticipant.Run1FinishStatus = this.OlympediaService.FindStatus(row.OuterHtml);
                alpineSkiingParticipant.Run1Number = this.RegExpService.MatchInt(data[indexes[ConverterConstants.INDEX_NR]].InnerText);
            }
            else if (title.InnerText.Contains("Run 2"))
            {
                alpineSkiingParticipant.Run2Time = this.dateService.ParseTime(data[indexes[ConverterConstants.INDEX_TIME]].InnerText);
                alpineSkiingParticipant.Run2FinishStatus = this.OlympediaService.FindStatus(row.OuterHtml);
                alpineSkiingParticipant.Run2Number = this.RegExpService.MatchInt(data[indexes[ConverterConstants.INDEX_NR]].InnerText);
            }
            else
            {
                alpineSkiingParticipant.Run1Time = this.dateService.ParseTime(data[indexes[ConverterConstants.INDEX_TIME]].InnerText);
                alpineSkiingParticipant.Run1FinishStatus = this.OlympediaService.FindStatus(row.OuterHtml);
                alpineSkiingParticipant.Run1Number = this.RegExpService.MatchInt(data[indexes[ConverterConstants.INDEX_NR]].InnerText);
            }
        }
    }
    #endregion ALPINE SKIING

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
                    Round = table.Round,
                    RoundInfo = table.RoundInfo,
                    MatchType = this.OlympediaService.FindMatchType(table.Round, tdNodes[0].InnerText),
                    MatchInfo = this.OlympediaService.FindMatchInfo(tdNodes[0].InnerText),
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
                        BlockedShots = homeTeamParticipants.Sum(x => x.BlockedShots),
                        DefensiveRebounds = homeTeamParticipants.Sum(x => x.DefensiveRebounds),
                        FreeThrowsAttempts = homeTeamParticipants.Sum(x => x.FreeThrowsAttempts),
                        FreeThrowsGoals = homeTeamParticipants.Sum(x => x.FreeThrowsGoals),
                        OffensiveRebounds = homeTeamParticipants.Sum(x => x.OffensiveRebounds),
                        PlusMinus = match.HomeTeam.Points - match.AwayTeam.Points,
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
                        BlockedShots = awayTeamParticipants.Sum(x => x.BlockedShots),
                        DefensiveRebounds = awayTeamParticipants.Sum(x => x.DefensiveRebounds),
                        FreeThrowsAttempts = awayTeamParticipants.Sum(x => x.FreeThrowsAttempts),
                        FreeThrowsGoals = awayTeamParticipants.Sum(x => x.FreeThrowsGoals),
                        OffensiveRebounds = awayTeamParticipants.Sum(x => x.OffensiveRebounds),
                        PlusMinus = match.AwayTeam.Points - match.HomeTeam.Points,
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
                TimePlayed = this.dateService.ParseTime(tdNodes[5]?.InnerText),
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
                    Round = table.Round,
                    RoundInfo = table.RoundInfo,
                    MatchType = this.OlympediaService.FindMatchType(table.Round, tdNodes[0].InnerText),
                    MatchInfo = this.OlympediaService.FindMatchInfo(tdNodes[0].InnerText),
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

                        var attendanceMatch = this.RegExpService.Match(htmlDocument.ParsedText, @"<th>Attendance<\/th><td>(.*?)<\/td>");
                        var attendance = this.RegExpService.MatchInt(attendanceMatch?.Groups[1]?.Value);
                        match.Attendance = attendance;

                        var referees = this.RegExpService.Matches(htmlDocument.ParsedText, @"<th>Referee<\/th>(?:.*?)\/athletes\/(\d+)");
                        var refereeIds = referees.Select(x => x.Groups[1].Value).ToList();

                        if (gameCacheModel.Year >= 2012)
                        {
                            var crewChief = this.RegExpService.Match(htmlDocument.ParsedText, @"<th>Crew Chief<\/th>(?:.*?)\/athletes\/(\d+)");
                            var firstUmpire = this.RegExpService.Match(htmlDocument.ParsedText, @"<th>Umpire 1<\/th>(?:.*?)\/athletes\/(\d+)");
                            var secondUmpire = this.RegExpService.Match(htmlDocument.ParsedText, @"<th>Umpire 2<\/th>(?:.*?)\/athletes\/(\d+)");

                            refereeIds.AddRange(new List<string> { crewChief?.Groups[1]?.Value, firstUmpire?.Groups[1]?.Value, secondUmpire?.Groups[1]?.Value });
                        }
                        refereeIds = refereeIds.Where(x => !string.IsNullOrEmpty(x)).ToList();
                        match.FirstRefereeId = refereeIds.Count >= 1 ? await this.ExtractRefereeAsync(refereeIds[0]) : null;
                        match.SecondRefereeId = refereeIds.Count >= 2 ? await this.ExtractRefereeAsync(refereeIds[1]) : null;
                        match.ThirdRefereeId = refereeIds.Count >= 3 ? await this.ExtractRefereeAsync(refereeIds[2]) : null;

                        var teamTables = htmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']").ToList();
                        if (gameCacheModel.Year >= 2020)
                        {
                            teamTables = htmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']").Skip(1).ToList();
                        }

                        var homeTeamParticipants = await this.ExtractBasketballParticipantsAsync(teamTables[0].OuterHtml, eventCacheModel, gameCacheModel);
                        var awayTeamParticipants = await this.ExtractBasketballParticipantsAsync(teamTables[1].OuterHtml, eventCacheModel, gameCacheModel);

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

                        var scoreTableHtml = this.RegExpService.MatchFirstGroup(htmlDocument.ParsedText, @"Score<\/h2><table class=""biodata"">(.*?)<\/table>");
                        if (!string.IsNullOrEmpty(scoreTableHtml))
                        {
                            var scoreTableDocument = new HtmlDocument();
                            scoreTableDocument.LoadHtml(scoreTableHtml);

                            var scoreTableTrs = scoreTableDocument.DocumentNode.SelectNodes("//tr").Skip(1).ToList();
                            var homeTeamScore = scoreTableTrs[0].Elements("td").ToList();
                            var awayTeamScore = scoreTableTrs[1].Elements("td").ToList();

                            match.HomeTeam.Statistic.FirstHalfPoints = this.RegExpService.MatchInt(homeTeamScore[2].InnerText);
                            match.HomeTeam.Statistic.SecondHalfPoints = this.RegExpService.MatchInt(homeTeamScore[3].InnerText);
                            match.AwayTeam.Statistic.FirstHalfPoints = this.RegExpService.MatchInt(awayTeamScore[2].InnerText);
                            match.AwayTeam.Statistic.SecondHalfPoints = this.RegExpService.MatchInt(awayTeamScore[3].InnerText);
                        }

                        if (gameCacheModel.Year >= 2020)
                        {
                            var trNodes = teamTables[0].Elements("tr").ToList();
                            var scoreTableDocument = new HtmlDocument();
                            scoreTableDocument.LoadHtml(trNodes[trNodes.Count - 2].OuterHtml);
                            var homeTeamTds = scoreTableDocument.DocumentNode.SelectNodes("//td");
                            match.HomeTeam.Statistic.FirstHalfPoints = this.RegExpService.MatchInt(homeTeamTds[2].InnerText);
                            match.HomeTeam.Statistic.SecondHalfPoints = this.RegExpService.MatchInt(homeTeamTds[3].InnerText);

                            trNodes = teamTables[1].Elements("tr").ToList();
                            scoreTableDocument = new HtmlDocument();
                            scoreTableDocument.LoadHtml(trNodes[trNodes.Count - 2].OuterHtml);
                            var awayTeamTds = scoreTableDocument.DocumentNode.SelectNodes("//td");
                            match.AwayTeam.Statistic.FirstHalfPoints = this.RegExpService.MatchInt(awayTeamTds[2].InnerText);
                            match.AwayTeam.Statistic.SecondHalfPoints = this.RegExpService.MatchInt(awayTeamTds[3].InnerText);
                        }
                    }
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

    private async Task<List<BasketballParticipant>> ExtractBasketballParticipantsAsync(string html, EventCacheModel eventCacheModel, GameCacheModel gameCacheModel)
    {
        var document = new HtmlDocument();
        document.LoadHtml(html);
        var trNodes = document.DocumentNode.SelectNodes("//tr");

        var participants = new List<BasketballParticipant>();
        for (int i = 1; i < trNodes.Count - 3; i++)
        {
            var tdDocument = new HtmlDocument();
            tdDocument.LoadHtml(trNodes[i].OuterHtml);
            var tdNodes = tdDocument.DocumentNode.SelectNodes("//td").ToList();

            var olympediaNumber = this.OlympediaService.FindAthleteNumber(tdNodes[2].OuterHtml);
            var dbParticipant = await this.participantsService.GetAsync(olympediaNumber, eventCacheModel.Id);

            var participant = new BasketballParticipant();

            if (gameCacheModel.Year >= 2020)
            {
                var freeThrowPointMatch = this.RegExpService.Match(tdNodes[16].InnerText, @"(\d+)\/(\d+)");

                participant.ParticipantId = dbParticipant.Id;
                participant.Position = tdNodes[0]?.InnerText.Trim();
                participant.Number = this.RegExpService.MatchInt(tdNodes[1]?.InnerText);
                participant.Points = this.RegExpService.MatchInt(tdNodes[4]?.InnerText);
                participant.TimePlayed = this.dateService.ParseTime(tdNodes[7]?.InnerText);
                participant.TwoPointsGoals = this.RegExpService.MatchInt(tdNodes[8]?.InnerText);
                participant.TwoPointsAttempts = this.RegExpService.MatchInt(tdNodes[9]?.InnerText);
                participant.ThreePointsGoals = this.RegExpService.MatchInt(tdNodes[11]?.InnerText);
                participant.ThreePointsAttempts = this.RegExpService.MatchInt(tdNodes[12]?.InnerText);
                participant.FreeThrowsGoals = this.RegExpService.MatchInt(freeThrowPointMatch?.Groups[1].Value);
                participant.FreeThrowsAttempts = this.RegExpService.MatchInt(freeThrowPointMatch?.Groups[2].Value);
                participant.OffensiveRebounds = this.RegExpService.MatchInt(tdNodes[18]?.InnerText);
                participant.DefensiveRebounds = this.RegExpService.MatchInt(tdNodes[19]?.InnerText);
                participant.Assists = this.RegExpService.MatchInt(tdNodes[21]?.InnerText);
                participant.Steals = this.RegExpService.MatchInt(tdNodes[22]?.InnerText);
                participant.Turnovers = this.RegExpService.MatchInt(tdNodes[23]?.InnerText);
                participant.BlockedShots = this.RegExpService.MatchInt(tdNodes[24]?.InnerText);
                participant.PersonalFouls = this.RegExpService.MatchInt(tdNodes[25]?.InnerText);
                participant.DisqualifyingFouls = this.RegExpService.MatchInt(tdNodes[26]?.InnerText);
            }
            else
            {
                var twoPointMatch = this.RegExpService.Match(tdNodes[5].InnerText, @"(\d+)\/(\d+)");
                var threePointMatch = this.RegExpService.Match(tdNodes[7].InnerText, @"(\d+)\/(\d+)");
                var freeThrowPointMatch = this.RegExpService.Match(tdNodes[11].InnerText, @"(\d+)\/(\d+)");

                participant.ParticipantId = dbParticipant.Id;
                participant.Position = tdNodes[0]?.InnerText.Trim();
                participant.Number = this.RegExpService.MatchInt(tdNodes[1]?.InnerText);
                participant.Points = this.RegExpService.MatchInt(tdNodes[3]?.InnerText);
                participant.TimePlayed = this.dateService.ParseTime(tdNodes[4]?.InnerText);
                participant.TwoPointsGoals = this.RegExpService.MatchInt(twoPointMatch?.Groups[1].Value);
                participant.TwoPointsAttempts = this.RegExpService.MatchInt(twoPointMatch?.Groups[2].Value);
                participant.ThreePointsGoals = this.RegExpService.MatchInt(threePointMatch?.Groups[1].Value);
                participant.ThreePointsAttempts = this.RegExpService.MatchInt(threePointMatch?.Groups[2].Value);
                participant.FreeThrowsGoals = this.RegExpService.MatchInt(freeThrowPointMatch?.Groups[1].Value);
                participant.FreeThrowsAttempts = this.RegExpService.MatchInt(freeThrowPointMatch?.Groups[2].Value);
                participant.OffensiveRebounds = this.RegExpService.MatchInt(tdNodes[13]?.InnerText);
                participant.DefensiveRebounds = this.RegExpService.MatchInt(tdNodes[14]?.InnerText);
                participant.Assists = this.RegExpService.MatchInt(tdNodes[16]?.InnerText);
                participant.Steals = this.RegExpService.MatchInt(tdNodes[17]?.InnerText);
                participant.BlockedShots = this.RegExpService.MatchInt(tdNodes[18]?.InnerText);
                participant.Turnovers = this.RegExpService.MatchInt(tdNodes[19]?.InnerText);
                participant.PersonalFouls = this.RegExpService.MatchInt(tdNodes[20]?.InnerText);
                participant.DisqualifyingFouls = this.RegExpService.MatchInt(tdNodes[21]?.InnerText);
            }

            participant.TotalFieldGoals = participant.TwoPointsGoals + participant.ThreePointsGoals;
            participant.TotalFieldGoalsAttempts = participant.TwoPointsAttempts + participant.ThreePointsAttempts;
            participant.TotalRebounds = participant.OffensiveRebounds + participant.DefensiveRebounds;

            participants.Add(participant);
        }

        return participants;
    }
    #endregion BASKETBALL
}