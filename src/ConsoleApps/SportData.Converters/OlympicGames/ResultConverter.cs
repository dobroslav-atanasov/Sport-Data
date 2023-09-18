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
using SportData.Data.Models.OlympicGames;
using SportData.Data.Models.OlympicGames.AlpineSkiing;
using SportData.Data.Models.OlympicGames.Archery;
using SportData.Data.Models.OlympicGames.ArtisticGymnastics;
using SportData.Data.Models.OlympicGames.ArtisticGymnastics.Individual;
using SportData.Data.Models.OlympicGames.ArtisticGymnastics.Team;
using SportData.Data.Models.OlympicGames.ArtisticSwimming;
using SportData.Data.Models.OlympicGames.Athletics;
using SportData.Data.Models.OlympicGames.Athletics.Combined;
using SportData.Data.Models.OlympicGames.Athletics.CrossCountry;
using SportData.Data.Models.OlympicGames.Athletics.Field;
using SportData.Data.Models.OlympicGames.Athletics.Road;
using SportData.Data.Models.OlympicGames.Athletics.Track;
using SportData.Data.Models.OlympicGames.Basketball;
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
                        //    await this.ProcessBasketball3x3Async(options);
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
                        case DisciplineConstants.ATHLETICS:
                            await this.ProcessAthleticsAsync(options);
                            break;
                            //case DisciplineConstants.BASKETBALL:
                            //    await this.ProcessBasketballAsync(options);
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
                        table.FromDate = dates.StartDateTime;
                        table.ToDate = dates.EndDateTime;
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

    #region ATHLETICS
    private async Task ProcessAthleticsAsync(ConvertOptions options)
    {
        var eventModel = this.NormalizeService.MapAthleticsEvent(options.Event.Name);
        var dateString = this.RegExpService.MatchFirstGroup(options.HtmlDocument.DocumentNode.OuterHtml, @"<th>\s*Date\s*<\/th>\s*<td>(.*?)<\/td>");
        var dateModel = this.dateService.ParseDate(dateString);
        var format = this.RegExpService.MatchFirstGroup(options.HtmlDocument.DocumentNode.OuterHtml, @"<th>Format<\/th>\s*<td(?:.*?)>(.*?)<\/td>");
        var rounds = new List<ATHRound>();

        if (eventModel.GroupEventType == ATHGroupEventType.TrackEvents)
        {
            if (options.Tables.Any())
            {
                foreach (var table in options.Tables.Where(x => x.Round != RoundType.None))
                {
                    format = this.RegExpService.MatchFirstGroup(table.HtmlDocument.DocumentNode.OuterHtml, @"<i>(.*?)<\/i>");
                    var round = new ATHRound
                    {
                        EventType = eventModel.EventType,
                        GroupEventType = eventModel.GroupEventType,
                        Round = table.Round,
                        Date = table.FromDate,
                        Format = format,
                        GenderType = eventModel.Gender,

                        Track = new ATHTrack()
                    };

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

                    rounds.Add(round);
                }
            }
            else
            {
                var round = new ATHRound
                {
                    EventType = eventModel.EventType,
                    GroupEventType = eventModel.GroupEventType,
                    Round = RoundType.Final,
                    Date = dateModel.StartDateTime,
                    Format = format,
                    GenderType = eventModel.Gender,

                    Track = new ATHTrack()
                };

                this.SetAthleticsSlipts(round, options.HtmlDocument, HeatType.None);
                await this.SetATHTrackAthletesAsync(round, options.StandingTable.HtmlDocument, options.Event, HeatType.None, null, false);

                rounds.Add(round);
            }
        }
        else if (eventModel.GroupEventType == ATHGroupEventType.RoadEvents)
        {
            var round = new ATHRound
            {
                EventType = eventModel.EventType,
                GroupEventType = eventModel.GroupEventType,
                Round = RoundType.Final,
                Date = dateModel.StartDateTime,
                Format = format,
                GenderType = eventModel.Gender,
                Road = new ATHRoad()
            };

            await this.SetATHRoadAthletesAsync(round, options.StandingTable.HtmlDocument, options.Event);

            rounds.Add(round);
        }
        else if (eventModel.GroupEventType == ATHGroupEventType.CrossCountryEvents)
        {
            var round = new ATHRound
            {
                EventType = eventModel.EventType,
                GroupEventType = eventModel.GroupEventType,
                Round = RoundType.Final,
                Date = dateModel.StartDateTime,
                Format = format,
                GenderType = eventModel.Gender,
                CrossCountry = new ATHCrossCountry()
            };

            await this.SetATHCrossCountryAsync(round, options.StandingTable.HtmlDocument, options.Event);

            rounds.Add(round);
        }
        else if (eventModel.GroupEventType == ATHGroupEventType.FieldEvents)
        {
            if (!options.Tables.Any() && !options.Documents.Any())
            {
                var round = new ATHRound
                {
                    EventType = eventModel.EventType,
                    GroupEventType = eventModel.GroupEventType,
                    Round = RoundType.Final,
                    Format = format,
                    GenderType = eventModel.Gender,
                    Date = dateModel.StartDateTime,
                    Field = new ATHField()
                };

                await this.SetATHFieldAthletesAsync(round, options.StandingTable.HtmlDocument, options.Event);

                rounds.Add(round);
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
                    format = this.RegExpService.MatchFirstGroup(table.HtmlDocument.DocumentNode.OuterHtml, @"<i>(.*?)<\/i>");

                    var round = new ATHRound
                    {
                        EventType = eventModel.EventType,
                        GroupEventType = eventModel.GroupEventType,
                        Round = table.Round,
                        GroupType = table.GroupType,
                        Format = format,
                        GenderType = eventModel.Gender,
                        Date = table.FromDate,
                        Field = new ATHField()
                    };

                    await this.SetATHFieldAthletesAsync(round, table.HtmlDocument, options.Event);

                    rounds.Add(round);
                }
            }
        }
        else if (eventModel.GroupEventType == ATHGroupEventType.CombinedEvents)
        {
            var round = new ATHRound
            {
                EventType = eventModel.EventType,
                GroupEventType = eventModel.GroupEventType,
                Round = RoundType.Final,
                Format = format,
                GenderType = eventModel.Gender,
                Date = dateModel.StartDateTime,
                Combined = new ATHCombined()
            };

            await this.SetATHCombinedAthletesAsync(round, options.StandingTable.HtmlDocument, options.Event);
            this.SetATHCombinedResults(round, options.Documents, options.Event, options.Discipline, options.Game);

            rounds.Add(round);
        }

        var json = JsonSerializer.Serialize(rounds);
        ;
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
                var athleticsEventModel = this.NormalizeService.MapAthleticsEvent(title);
                var dateString = this.RegExpService.MatchFirstGroup(htmlDocument.DocumentNode.OuterHtml, @"<th>\s*Date\s*<\/th>\s*<td>(.*?)<\/td>");
                var dateModel = this.dateService.ParseDate(dateString);
                round.Combined.Events.Add(new ATHCombinedEvent
                {
                    Date = dateModel.StartDateTime,
                    EventType = athleticsEventModel.EventType,
                    Order = order
                });

                order++;

                this.AddATHCombinedResult(round, standingTable.HtmlDocument, HeatType.None, GroupType.None, athleticsEventModel, true, null);
                var groups = this.SplitGroups(htmlDocument.DocumentNode.OuterHtml);
                if (groups.Any())
                {
                    foreach (var group in groups)
                    {
                        var heatType = HeatType.None;
                        var groupType = GroupType.None;
                        if (athleticsEventModel.GroupEventType == ATHGroupEventType.TrackEvents)
                        {
                            heatType = this.NormalizeService.MapHeats(group.Title);
                        }
                        else if (athleticsEventModel.GroupEventType == ATHGroupEventType.FieldEvents)
                        {
                            groupType = this.NormalizeService.MapGroupType(group.Title);
                        }

                        this.AddATHCombinedResult(round, group.HtmlDocument, heatType, groupType, athleticsEventModel, false, group.Wind);
                    }
                }
            }
        }
    }

    private void AddATHCombinedResult(ATHRound round, HtmlDocument htmlDocument, HeatType heatType, GroupType groupType, AthleticsEventModel athleticsEventModel, bool isStandingTable, double? wind)
    {
        var rows = htmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']//tr");
        var headers = rows.First().Elements("th").Select(x => x.InnerText).ToList();
        var indexes = this.OlympediaService.FindIndexes(headers);

        foreach (var row in rows.Skip(1))
        {
            var data = row.Elements("td").ToList();
            var number = this.OlympediaService.FindAthleteNumber(row.OuterHtml);
            var athlete = round.Combined.Athletes.FirstOrDefault(x => x.ParticipantNumber == number);

            if (isStandingTable)
            {
                var measurement = indexes.TryGetValue(ConverterConstants.INDEX_HEIGHT, out int value1) ? this.RegExpService.MatchDouble(data[value1].InnerText) : null;
                measurement ??= indexes.TryGetValue(ConverterConstants.INDEX_DISTANCE, out int value2) ? this.RegExpService.MatchDouble(data[value2].InnerText) : null;

                athlete.Results.Add(new ATHCombinedResult
                {
                    EventType = athleticsEventModel.EventType,
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
                var result = athlete.Results.FirstOrDefault(x => x.EventType == athleticsEventModel.EventType);
                if (result != null)
                {
                    if (athleticsEventModel.GroupEventType == ATHGroupEventType.TrackEvents)
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

                        if (athleticsEventModel.EventType == ATHEventType.HighJump || athleticsEventModel.EventType == ATHEventType.PoleVault)
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
                                        Tries = this.MapATHTries(data[i].InnerText, round.EventType),
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
                                    Tries = this.MapATHTries(data[value10].InnerText.Trim(), round.EventType),
                                    AttemptOrder = 1,
                                    Record = this.OlympediaService.FindRecord(row.OuterHtml),
                                });
                            }
                            if (indexes.ContainsKey(ConverterConstants.INDEX_ATH_ROUND_2))
                            {
                                result.Attempts.Add(new ATHFieldAttempt
                                {
                                    Measurement = indexes.TryGetValue(ConverterConstants.INDEX_ATH_ROUND_2, out int value11) ? this.RegExpService.MatchDouble(data[value11].InnerText) : null,
                                    Tries = this.MapATHTries(data[value11].InnerText.Trim(), round.EventType),
                                    AttemptOrder = 2,
                                    Record = this.OlympediaService.FindRecord(row.OuterHtml),
                                });
                            }
                            if (indexes.ContainsKey(ConverterConstants.INDEX_ATH_ROUND_3))
                            {
                                result.Attempts.Add(new ATHFieldAttempt
                                {
                                    Measurement = indexes.TryGetValue(ConverterConstants.INDEX_ATH_ROUND_3, out int value12) ? this.RegExpService.MatchDouble(data[value12].InnerText) : null,
                                    Tries = this.MapATHTries(data[value12].InnerText.Trim(), round.EventType),
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
            var nocCode = this.OlympediaService.FindCountryCode(row.OuterHtml);
            var nocCache = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == nocCode);
            var name = data[indexes[ConverterConstants.INDEX_NAME]].InnerText;
            var number = this.OlympediaService.FindAthleteNumber(data[indexes[ConverterConstants.INDEX_NAME]].OuterHtml);
            var participant = await this.participantsService.GetAsync(number, eventCache.Id, nocCache.Id);
            if (participant != null)
            {
                var athlete = new ATHCombinedAthlete
                {
                    Name = name,
                    NOCCode = nocCode,
                    ParticipantId = participant.Id,
                    ParticipantNumber = number,
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
            var nocCode = this.OlympediaService.FindCountryCode(row.OuterHtml);
            var nocCache = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == nocCode);
            var name = data[indexes[ConverterConstants.INDEX_NAME]].InnerText;
            var number = this.OlympediaService.FindAthleteNumber(data[indexes[ConverterConstants.INDEX_NAME]].OuterHtml);
            var participant = await this.participantsService.GetAsync(number, eventCache.Id, nocCache.Id);
            if (participant is not null)
            {
                var measurement = indexes.TryGetValue(ConverterConstants.INDEX_HEIGHT, out int value1) ? this.RegExpService.MatchDouble(data[value1].InnerText) : null;
                measurement ??= indexes.TryGetValue(ConverterConstants.INDEX_DISTANCE, out int value2) ? this.RegExpService.MatchDouble(data[value2].InnerText) : null;

                var athlete = new ATHFieldAthlete
                {
                    Name = name,
                    NOCCode = nocCode,
                    ParticipantId = participant.Id,
                    ParticipantNumber = number,
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
                if (round.EventType == ATHEventType.HighJump || round.EventType == ATHEventType.StandingHighJump || round.EventType == ATHEventType.PoleVault)
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
                                Tries = this.MapATHTries(data[i].InnerText, round.EventType),
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
                            Tries = this.MapATHTries(data[value10].InnerText.Trim(), round.EventType),
                            AttemptOrder = 1,
                            Record = this.OlympediaService.FindRecord(row.OuterHtml),
                        });
                    }
                    if (indexes.ContainsKey(ConverterConstants.INDEX_ATH_ROUND_2))
                    {
                        athlete.Attempts.Add(new ATHFieldAttempt
                        {
                            Measurement = indexes.TryGetValue(ConverterConstants.INDEX_ATH_ROUND_2, out int value11) ? this.RegExpService.MatchDouble(data[value11].InnerText) : null,
                            Tries = this.MapATHTries(data[value11].InnerText.Trim(), round.EventType),
                            AttemptOrder = 2,
                            Record = this.OlympediaService.FindRecord(row.OuterHtml),
                        });
                    }
                    if (indexes.ContainsKey(ConverterConstants.INDEX_ATH_ROUND_3))
                    {
                        athlete.Attempts.Add(new ATHFieldAttempt
                        {
                            Measurement = indexes.TryGetValue(ConverterConstants.INDEX_ATH_ROUND_3, out int value12) ? this.RegExpService.MatchDouble(data[value12].InnerText) : null,
                            Tries = this.MapATHTries(data[value12].InnerText.Trim(), round.EventType),
                            AttemptOrder = 3,
                            Record = this.OlympediaService.FindRecord(row.OuterHtml),
                        });
                    }
                    if (indexes.ContainsKey(ConverterConstants.INDEX_ATH_ROUND_4))
                    {
                        athlete.Attempts.Add(new ATHFieldAttempt
                        {
                            Measurement = indexes.TryGetValue(ConverterConstants.INDEX_ATH_ROUND_4, out int value13) ? this.RegExpService.MatchDouble(data[value13].InnerText) : null,
                            Tries = this.MapATHTries(data[value13].InnerText.Trim(), round.EventType),
                            AttemptOrder = 4,
                            Record = this.OlympediaService.FindRecord(row.OuterHtml),
                        });
                    }
                    if (indexes.ContainsKey(ConverterConstants.INDEX_ATH_ROUND_5))
                    {
                        athlete.Attempts.Add(new ATHFieldAttempt
                        {
                            Measurement = indexes.TryGetValue(ConverterConstants.INDEX_ATH_ROUND_5, out int value14) ? this.RegExpService.MatchDouble(data[value14].InnerText) : null,
                            Tries = this.MapATHTries(data[value14].InnerText.Trim(), round.EventType),
                            AttemptOrder = 5,
                            Record = this.OlympediaService.FindRecord(row.OuterHtml),
                        });
                    }
                    if (indexes.ContainsKey(ConverterConstants.INDEX_ATH_ROUND_6))
                    {
                        athlete.Attempts.Add(new ATHFieldAttempt
                        {
                            Measurement = indexes.TryGetValue(ConverterConstants.INDEX_ATH_ROUND_6, out int value15) ? this.RegExpService.MatchDouble(data[value15].InnerText) : null,
                            Tries = this.MapATHTries(data[value15].InnerText.Trim(), round.EventType),
                            AttemptOrder = 6,
                            Record = this.OlympediaService.FindRecord(row.OuterHtml),
                        });
                    }
                }

                round.Field.Athletes.Add(athlete);
            }
        }
    }

    private List<ATHFieldTry> MapATHTries(string symbols, ATHEventType eventType)
    {
        var result = new List<ATHFieldTry>();
        if (string.IsNullOrEmpty(symbols))
        {
            return result;
        }

        if (eventType == ATHEventType.HighJump || eventType == ATHEventType.StandingHighJump || eventType == ATHEventType.PoleVault)
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
            var nocCode = this.OlympediaService.FindCountryCode(row.OuterHtml);
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
                var number = this.OlympediaService.FindAthleteNumber(data[indexes[ConverterConstants.INDEX_NAME]].OuterHtml);
                var participant = await this.participantsService.GetAsync(number, eventCache.Id, nocCache.Id);
                if (participant != null)
                {
                    var athlete = new ATHCrossCountryAthlete
                    {
                        ParticipantNumber = number,
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
            var nocCode = this.OlympediaService.FindCountryCode(row.OuterHtml);
            var nocCache = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == nocCode);
            var name = data[indexes[ConverterConstants.INDEX_NAME]].InnerText;
            var number = this.OlympediaService.FindAthleteNumber(row.OuterHtml);
            var participant = await this.participantsService.GetAsync(number, eventCache.Id, nocCache.Id);

            if (participant is not null)
            {
                var athlete = new ATHRoadAthlete
                {
                    Name = name,
                    ParticipantId = participant.Id,
                    ParticipantNumber = number,
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
                var nocCode = this.OlympediaService.FindCountryCode(row.OuterHtml);
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
                    var numbers = this.OlympediaService.FindAthleteNumbers(data[indexes[ConverterConstants.INDEX_NAME]].OuterHtml);
                    foreach (var number in numbers)
                    {
                        var participant = await this.participantsService.GetAsync(number, eventCache.Id, nocCache.Id);
                        if (participant != null)
                        {
                            if (numbers.Count > 1)
                            {
                                round.Track.Teams.Last().Athletes.Add(new ATHTrackAthlete
                                {
                                    ParticipantNumber = number,
                                    ParticipantId = participant.Id,
                                    NOCCode = nocCode
                                });
                            }
                            else
                            {
                                var athlete = new ATHTrackAthlete
                                {
                                    ParticipantNumber = number,
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
                                Number = this.OlympediaService.FindAthleteNumber(row.OuterHtml),
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
                                Number = this.OlympediaService.FindAthleteNumber(row.OuterHtml),
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
    #endregion ATHLETICS

    #region ARTISTIC SWIMMING
    private async Task ProcessArtisticSwimmingAsync(ConvertOptions options)
    {
        var eventType = this.NormalizeService.MapArtisticSwimmingEvent(options.Event.Name);
        var rounds = new List<SWARound>();

        if (eventType == SWAEventType.Team)
        {
            var dateString = this.RegExpService.MatchFirstGroup(options.HtmlDocument.DocumentNode.OuterHtml, @"<th>\s*Date\s*<\/th>\s*<td>(.*?)<\/td>");
            var dateModel = this.dateService.ParseDate(dateString);
            var format = this.RegExpService.MatchFirstGroup(options.HtmlDocument.DocumentNode.OuterHtml, @"<th>Format<\/th>\s*<td(?:.*?)>(.*?)<\/td>");

            var round = new SWARound
            {
                Date = dateModel.StartDateTime,
                Format = this.RegExpService.CutHtml(format),
                EventType = SWAEventType.Team,
                Round = RoundType.Final
            };

            await this.SetSWAEventResultsAsync(round, options.StandingTable, options.Event, null);
            rounds.Add(round);

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

                var @event = new SWARound
                {
                    Date = dateModel.StartDateTime,
                    Format = this.RegExpService.CutHtml(format),
                    EventType = eventType,
                    Round = this.NormalizeService.MapRoundType(table.Title)
                };

                await this.SetSWAEventResultsAsync(@event, table, options.Event, null);
                rounds.Add(@event);
            }

            foreach (var document in options.Documents)
            {
                var htmlDocument = this.CreateHtmlDocument(document);
                var table = this.GetStandingTable(htmlDocument, options.Event);
                var info = htmlDocument.DocumentNode.SelectSingleNode("//h1").InnerText;
                info = info.Replace(options.Event.OriginalName, string.Empty).Replace("–", string.Empty).Trim();
                var rowsCount = table.HtmlDocument.DocumentNode.SelectNodes("//tr").Skip(1).Count();
                var currentEvent = rounds.Where(x => x.Duets.Count == rowsCount).FirstOrDefault();

                await this.SetSWAEventResultsAsync(currentEvent, table, options.Event, info);
            }
        }

        var json = JsonSerializer.Serialize(rounds);
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
            var nocCode = this.OlympediaService.FindCountryCode(row.OuterHtml);
            var noc = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == nocCode);
            var athleteNumbers = this.OlympediaService.FindAthleteNumbers(row.OuterHtml);

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

                    if (round.EventType == SWAEventType.Duet)
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

                    if (round.EventType == SWAEventType.Duet)
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

            if (round.EventType == SWAEventType.Solo)
            {
                var participant = await this.participantsService.GetAsync(athleteNumbers[0], eventCache.Id, noc.Id);

                var solo = new SWASolo
                {
                    Name = data[indexes[ConverterConstants.INDEX_NAME]].InnerText,
                    ParticipantId = participant.Id,
                    ParticipantNumber = athleteNumbers[0],
                    Points = indexes.TryGetValue(ConverterConstants.INDEX_POINTS, out int value1) ? this.RegExpService.MatchDouble(data[value1].InnerText) : null,
                    FigurePoints = indexes.TryGetValue(ConverterConstants.INDEX_SWA_FIGURE_POINTS, out int value2) ? this.RegExpService.MatchDouble(data[value2].InnerText) : null,
                    MusicalRoutinePoints = indexes.TryGetValue(ConverterConstants.INDEX_SWA_MUSICAL_ROUTINE_POINTS, out int value3) ? this.RegExpService.MatchDouble(data[value3].InnerText) : null,
                    Qualification = this.OlympediaService.FindQualification(row.OuterHtml)
                };

                round.Solos.Add(solo);
            }
            else if (round.EventType == SWAEventType.Duet)
            {
                var teamName = data[indexes[ConverterConstants.INDEX_NAME]].InnerText;
                var team = await this.teamsService.GetAsync(teamName, noc.Id, eventCache.Id);
                team ??= await this.teamsService.GetAsync(noc.Id, eventCache.Id);

                var swimmers = new List<BaseIndividual>();
                foreach (var athleteNumber in athleteNumbers)
                {
                    var participant = await this.participantsService.GetAsync(athleteNumber, eventCache.Id, noc.Id);
                    if (participant is not null)
                    {
                        swimmers.Add(new BaseIndividual { ParticipantId = participant.Id, ParticipantNumber = athleteNumber });
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
            else if (round.EventType == SWAEventType.Team)
            {
                if (athleteNumbers.Any())
                {
                    var currentTeam = round.Teams.Last();
                    var currentNoc = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == currentTeam.NOCCode);
                    foreach (var athleteNumber in athleteNumbers)
                    {
                        var participant = await this.participantsService.GetAsync(athleteNumber, eventCache.Id, currentNoc.Id);
                        if (participant is not null)
                        {
                            round.Teams.Last().Swimmers.Add(new BaseIndividual { ParticipantId = participant.Id, ParticipantNumber = athleteNumber });
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
    #endregion ARTISITC SWIMMING

    #region ARTISTIC GYMNASTICS
    private async Task ProcessArtisticGymnasticsAsync(ConvertOptions options)
    {
        var dateString = this.RegExpService.MatchFirstGroup(options.HtmlDocument.DocumentNode.OuterHtml, @"<th>\s*Date\s*<\/th>\s*<td>(.*?)<\/td>");
        var dateModel = this.dateService.ParseDate(dateString);
        var eventType = this.NormalizeService.MapArtisticGymnasticsEvent(options.Event.Name);

        if (!options.Event.IsTeamEvent)
        {
            var @event = new GARIndividualEvent { EventType = eventType };

            if (options.Tables.Count == 0)
            {
                @event.FinalStartDate = dateModel.StartDateTime;
                @event.FinalEndDate = dateModel.EndDateTime;
                await this.ConvertGARIndividualAsync(@event, options.StandingTable, RoundType.Final, options.Event, eventType, false, null);
            }
            else
            {
                if (@event.EventType != GAREventType.Individual)
                {
                    foreach (var table in options.Tables)
                    {
                        string info = null;
                        if (@event.EventType == GAREventType.Triathlon)
                        {
                            table.Round = RoundType.Final;
                            info = table.Title;
                        }

                        this.SetGAREventDates(table, @event);

                        await this.ConvertGARIndividualAsync(@event, table, table.Round, options.Event, eventType, false, info);
                    }

                    if (options.Game.Year >= 2012 && @event.EventType == GAREventType.Vault)
                    {
                        foreach (var document in options.Documents)
                        {
                            await this.ConvertGARIndividualDocumentsAsync(document, options.Event, @event);
                        }
                    }
                }
                else
                {
                    if (options.Documents.Any())
                    {
                        foreach (var table in options.Tables)
                        {
                            this.SetGAREventDates(table, @event);
                        }

                        foreach (var document in options.Documents)
                        {
                            await this.ConvertGARIndividualDocumentsAsync(document, options.Event, @event);
                        }
                    }
                    else
                    {
                        @event.FinalStartDate = dateModel.StartDateTime;
                        @event.FinalEndDate = dateModel.EndDateTime;

                        foreach (var table in options.Tables)
                        {
                            this.SetGAREventDates(table, @event);
                            var currentEventType = this.NormalizeService.MapArtisticGymnasticsEvent(table.Title);
                            await this.ConvertGARIndividualAsync(@event, table, table.Round, options.Event, currentEventType, false, table.Title);
                        }
                    }
                }

                if (options.Game.Year >= 2012)
                {
                    await this.ConvertGARIndividualAsync(@event, options.StandingTable, options.StandingTable.Round, options.Event, eventType, true, null);
                }
            }

            this.CalculateGARIndividualTotalPoints(@event);

            var json = JsonSerializer.Serialize(@event);
            var result = new Result
            {
                EventId = options.Event.Id,
                Json = json
            };

            await this.resultsService.AddOrUpdateAsync(result);
        }
        else
        {
            var @event = new GARTeamEvent { EventType = eventType };

            await this.SetGARTeamsAsync(options.StandingTable, @event, options.Event.Id);

            if (!options.Tables.Any() || options.Game.Year == 1924)
            {
                @event.FinalStartDate = dateModel.StartDateTime;
                @event.FinalEndDate = dateModel.EndDateTime;
                this.SetGARTeamResults(@event, options.StandingTable, eventType, RoundType.Final, options.Game.Year, options.Event);
            }
            else if (options.Tables.Any() && options.Game.Year <= 1996)
            {
                @event.FinalStartDate = dateModel.StartDateTime;
                @event.FinalEndDate = dateModel.EndDateTime;
                this.SetGARTeamResults(@event, options.StandingTable, eventType, RoundType.Final, options.Game.Year, options.Event);

                foreach (var table in options.Tables)
                {
                    var currentEventType = this.NormalizeService.MapArtisticGymnasticsEvent(table.Title);
                    if (currentEventType != GAREventType.None)
                    {
                        this.SetGARTeamResults(@event, table, currentEventType, RoundType.Final, options.Game.Year, options.Event);
                    }
                }
            }
            else if (options.Game.Year == 2000 || options.Game.Year == 2004)
            {
                foreach (var table in options.Tables)
                {
                    if (table.Round == RoundType.Final)
                    {
                        @event.FinalStartDate = table.FromDate;
                        @event.FinalEndDate = table.ToDate;
                    }
                    else if (table.Round == RoundType.Qualification)
                    {
                        @event.QualificationStartDate = table.FromDate;
                        @event.QualificationEndDate = table.ToDate;
                    }

                    this.SetGARTeamResults(@event, table, GAREventType.Team, table.Round, options.Game.Year, options.Event);
                }

                foreach (var document in options.Documents)
                {
                    var htmlDocument = this.CreateHtmlDocument(document);
                    var table = this.GetStandingTable(htmlDocument, options.Event);
                    var title = htmlDocument.DocumentNode.SelectSingleNode("//h1").InnerText;
                    title = title.Replace(options.Event.OriginalName, string.Empty).Replace("–", string.Empty).Trim();
                    var parts = title.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    var round = this.NormalizeService.MapRoundType(parts.FirstOrDefault());
                    var currentEventType = this.NormalizeService.MapArtisticGymnasticsEvent(parts.LastOrDefault());

                    if (@event.Teams.Count >= table.HtmlDocument.DocumentNode.SelectNodes("//tr").Skip(1).Count())
                    {
                        this.SetGARTeamResults(@event, table, currentEventType, round, options.Game.Year, options.Event);
                    }
                }
            }
            else if (options.Game.Year >= 2008)
            {
                foreach (var table in options.Tables)
                {
                    if (table.Round == RoundType.Final)
                    {
                        @event.FinalStartDate = table.FromDate;
                        @event.FinalEndDate = table.ToDate;
                    }
                    else if (table.Round == RoundType.Qualification)
                    {
                        @event.QualificationStartDate = table.FromDate;
                        @event.QualificationEndDate = table.ToDate;
                    }

                    this.SetGARTeamResults(@event, table, GAREventType.Team, table.Round, options.Game.Year, options.Event);
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
                        var round = this.NormalizeService.MapRoundType(parts.FirstOrDefault());
                        var currentEventType = this.NormalizeService.MapArtisticGymnasticsEvent(parts.LastOrDefault());

                        this.SetGARTeamResults(@event, table, currentEventType, round, options.Game.Year, options.Event);
                    }
                }
            }

            var json = JsonSerializer.Serialize(@event);
            var result = new Result
            {
                EventId = options.Event.Id,
                Json = json
            };

            await this.resultsService.AddOrUpdateAsync(result);
        }
    }

    private void SetGARTeamResults(GARTeamEvent @event, TableModel table, GAREventType eventType, RoundType round, int year, EventCacheModel eventCache)
    {
        var rows = table.HtmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']//tr");
        var headers = rows.First().Elements("th").Select(x => x.InnerText).ToList();
        var indexes = this.OlympediaService.FindIndexes(headers);

        GARTeam team = null;
        var isMainTeam = true;
        foreach (var row in rows.Skip(1))
        {
            var data = row.Elements("td").ToList();
            var nocCode = this.OlympediaService.FindCountryCode(row.OuterHtml);
            var isAthleteNumber = this.OlympediaService.IsAthleteNumber(row.OuterHtml);
            if (nocCode != null && !isAthleteNumber)
            {
                isMainTeam = false;
                var teamName = data[indexes[ConverterConstants.INDEX_NAME]].InnerHtml;
                team = @event.Teams.FirstOrDefault(x => x.Name == teamName && x.NOCCode == nocCode);

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
                            new GARTeamScore { Points = floorExercise, EventType = GAREventType.FloorExercise, Round = round },
                            new GARTeamScore { Points = vault, EventType = GAREventType.Vault, Round = round },
                            new GARTeamScore { Points = parallelBars, EventType = GAREventType.ParallelBars, Round = round },
                            new GARTeamScore { Points = horizontalBar, EventType = GAREventType.HorizontalBar, Round = round },
                            new GARTeamScore { Points = rings, EventType = GAREventType.Rings, Round = round },
                            new GARTeamScore { Points = pommelHorse, EventType = GAREventType.PommelHorse, Round = round }
                        };
                    }
                    else
                    {
                        team.Scores = new List<GARTeamScore>
                        {
                            new GARTeamScore { Points = floorExercise, EventType = GAREventType.FloorExercise, Round = round },
                            new GARTeamScore { Points = vault, EventType = GAREventType.Vault, Round = round },
                            new GARTeamScore { Points = unevenBars, EventType = GAREventType.UnevenBars, Round = round },
                            new GARTeamScore { Points = balanceBeam, EventType = GAREventType.BalanceBeam, Round = round },
                        };
                    }
                }
                else
                {
                    team.Scores.Add(new GARTeamScore
                    {
                        Round = round,
                        EventType = eventType,
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
                        Qualification = this.OlympediaService.FindQualification(row.OuterHtml),
                    });
                }
            }
            else
            {
                if (isMainTeam)
                {
                    team = @event.Teams.FirstOrDefault(x => x.NOCCode == nocCode);
                }

                var athleteNumbers = this.OlympediaService.FindAthleteNumbers(row.OuterHtml);
                foreach (var number in athleteNumbers)
                {
                    var gymnast = team.Gymnasts.FirstOrDefault(x => x.ParticipantNumber == number);
                    if (gymnast != null && athleteNumbers.Count == 1)
                    {
                        gymnast.Round = round;
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
                                gymnast.Scores = new List<GARIndividualScore>
                                {
                                    new GARIndividualScore { Points = floorExercise, EventType = GAREventType.FloorExercise },
                                    new GARIndividualScore { Points = vault, EventType = GAREventType.Vault },
                                    new GARIndividualScore { Points = parallelBars, EventType = GAREventType.ParallelBars },
                                    new GARIndividualScore { Points = horizontalBar, EventType = GAREventType.HorizontalBar },
                                    new GARIndividualScore { Points = rings, EventType = GAREventType.Rings },
                                    new GARIndividualScore { Points = pommelHorse, EventType = GAREventType.PommelHorse }
                                };
                            }
                            else
                            {
                                gymnast.Scores = new List<GARIndividualScore>
                                {
                                    new GARIndividualScore { Points = floorExercise, EventType = GAREventType.FloorExercise },
                                    new GARIndividualScore { Points = vault, EventType = GAREventType.Vault },
                                    new GARIndividualScore { Points = unevenBars, EventType = GAREventType.UnevenBars },
                                    new GARIndividualScore { Points = balanceBeam, EventType = GAREventType.BalanceBeam },
                                };
                            }
                        }
                        else
                        {
                            var score = new GARIndividualScore
                            {
                                EventType = eventType,
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
                                Qualification = this.OlympediaService.FindQualification(row.OuterHtml),
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

    private async Task SetGARTeamsAsync(TableModel table, GARTeamEvent @event, int eventId)
    {
        var rows = table.HtmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']//tr");
        var headers = rows.First().Elements("th").Select(x => x.InnerText).ToList();
        var indexes = this.OlympediaService.FindIndexes(headers);

        //GARTeam eventTeam = null;
        foreach (var row in rows.Skip(1))
        {
            var data = row.Elements("td").ToList();
            var nocCode = this.OlympediaService.FindCountryCode(row.OuterHtml);
            if (nocCode != null)
            {
                var teamName = data[indexes[ConverterConstants.INDEX_NAME]].InnerHtml;
                var noc = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == nocCode);
                var team = await this.teamsService.GetAsync(teamName, noc.Id, eventId);

                var eventTeam = new GARTeam
                {
                    Name = teamName,
                    TeamId = team.Id,
                    NOCCode = nocCode,
                };

                @event.Teams.Add(eventTeam);
            }
            else
            {
                var athleteNumbers = this.OlympediaService.FindAthleteNumbers(row.OuterHtml);
                foreach (var number in athleteNumbers)
                {
                    var participant = await this.participantsService.GetAsync(number, eventId);
                    if (participant != null)
                    {
                        @event.Teams.Last().Gymnasts.Add(new GARIndividual
                        {
                            ParticipantNumber = number,
                            ParticipantId = participant.Id,
                            Name = this.OlympediaService.FindAthleteName(row.OuterHtml, number)
                        });
                    }
                }
            }
        }
    }

    private void SetGAREventDates(TableModel table, GARIndividualEvent @event)
    {
        if (table.Round == RoundType.Final)
        {
            @event.FinalStartDate = table.FromDate;
            @event.FinalEndDate = table.ToDate;
        }
        else if (table.Round == RoundType.Qualification)
        {
            @event.QualificationStartDate = table.FromDate;
            @event.QualificationEndDate = table.ToDate;
        }
    }

    private async Task ConvertGARIndividualDocumentsAsync(Document document, EventCacheModel eventCache, GARIndividualEvent @event)
    {
        var htmlDocument = this.CreateHtmlDocument(document);
        var table = this.GetStandingTable(htmlDocument, eventCache);
        var title = htmlDocument.DocumentNode.SelectSingleNode("//h1").InnerText;
        title = title.Replace(eventCache.OriginalName, string.Empty).Replace("–", string.Empty).Trim();
        var parts = title.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
        var round = this.NormalizeService.MapRoundType(parts.FirstOrDefault());
        var eventType = this.NormalizeService.MapArtisticGymnasticsEvent(parts.LastOrDefault());

        await this.ConvertGARIndividualAsync(@event, table, round, eventCache, eventType, false, parts.LastOrDefault().Trim());
    }

    private void CalculateGARIndividualTotalPoints(GARIndividualEvent @event)
    {
        @event.Gymnasts
            .ForEach(x =>
            {
                x.Points = x.Scores.Sum(x => x.Points);
            });
    }

    private async Task ConvertGARIndividualAsync(GARIndividualEvent @event, TableModel table, RoundType round, EventCacheModel eventCache, GAREventType eventType, bool isOnlyNumber, string info)
    {
        var rows = table.HtmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']//tr");
        var headers = rows.First().Elements("th").Select(x => x.InnerText).ToList();
        var indexes = this.OlympediaService.FindIndexes(headers);
        foreach (var row in rows.Skip(1))
        {
            var data = row.Elements("td").ToList();
            var athleteNumber = this.OlympediaService.FindAthleteNumber(data[indexes[ConverterConstants.INDEX_NAME]].OuterHtml);
            if (isOnlyNumber)
            {
                @event.Gymnasts
                    .Where(x => x.ParticipantNumber == athleteNumber)
                    .ToList()
                    .ForEach(x =>
                    {
                        x.Number = indexes.TryGetValue(ConverterConstants.INDEX_NR, out int value1) ? this.RegExpService.MatchInt(data[value1].InnerText) : null;
                    });

                continue;
            }

            var score = new GARIndividualScore
            {
                EventType = eventType,
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
                Qualification = this.OlympediaService.FindQualification(row.OuterHtml),
            };

            switch (eventType)
            {
                case GAREventType.Triathlon:
                    score.Info = table.Title;
                    break;
            }

            var gymnast = @event.Gymnasts.FirstOrDefault(x => x.ParticipantNumber == athleteNumber && x.Round == round);
            if (gymnast is null)
            {
                var nocCode = this.OlympediaService.FindCountryCode(data[indexes[ConverterConstants.INDEX_NOC]].OuterHtml);
                var nocCacheModel = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == nocCode);
                if (nocCacheModel is null)
                {
                    continue;
                }

                var participant = await this.participantsService.GetAsync(athleteNumber, eventCache.Id, nocCacheModel.Id);
                if (participant == null)
                {
                    continue;
                }

                gymnast = new GARIndividual
                {
                    ParticipantId = participant.Id,
                    ParticipantNumber = athleteNumber,
                    Name = data[indexes[ConverterConstants.INDEX_NAME]].InnerText,
                    Round = round,
                    Scores = new List<GARIndividualScore> { score }
                };

                @event.Gymnasts.Add(gymnast);
            }
            else
            {
                gymnast.Scores.Add(score);
            }

            if (eventType == GAREventType.RopeClimbing && @event.FinalStartDate.HasValue && @event.FinalStartDate.Value.Year == 1932)
            {
                gymnast.Scores.Add(new GARIndividualScore
                {
                    Time = indexes.TryGetValue(ConverterConstants.INDEX_TRIAL_TIME_1, out int value30) ? this.RegExpService.MatchDouble(data[value30].InnerText) : null,
                    Info = "TT1"
                });

                gymnast.Scores.Add(new GARIndividualScore
                {
                    Time = indexes.TryGetValue(ConverterConstants.INDEX_TRIAL_TIME_2, out int value31) ? this.RegExpService.MatchDouble(data[value31].InnerText) : null,
                    Info = "TT2"
                });

                gymnast.Scores.Add(new GARIndividualScore
                {
                    Time = indexes.TryGetValue(ConverterConstants.INDEX_TRIAL_TIME_3, out int value32) ? this.RegExpService.MatchDouble(data[value32].InnerText) : null,
                    Info = "TT3"
                });
            }
            else if (eventType == GAREventType.Individual && @event.FinalStartDate.HasValue && @event.FinalStartDate.Value.Year == 2008)
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
                    gymnast.Scores = new List<GARIndividualScore>
                    {
                        new GARIndividualScore { Points = floorExercise, EventType = GAREventType.FloorExercise },
                        new GARIndividualScore { Points = vault, EventType = GAREventType.Vault },
                        new GARIndividualScore { Points = parallelBars, EventType = GAREventType.ParallelBars },
                        new GARIndividualScore { Points = horizontalBar, EventType = GAREventType.HorizontalBar },
                        new GARIndividualScore { Points = rings, EventType = GAREventType.Rings },
                        new GARIndividualScore { Points = pommelHorse, EventType = GAREventType.PommelHorse }
                    };
                }
                else
                {
                    gymnast.Scores = new List<GARIndividualScore>
                    {
                        new GARIndividualScore { Points = floorExercise, EventType = GAREventType.FloorExercise },
                        new GARIndividualScore { Points = vault, EventType = GAREventType.Vault },
                        new GARIndividualScore { Points = unevenBars, EventType = GAREventType.UnevenBars },
                        new GARIndividualScore { Points = balanceBeam, EventType = GAREventType.BalanceBeam },
                    };
                }
            }
        }
    }
    #endregion

    #region ARCHERY
    private async Task ProcessArcheryAsync(ConvertOptions options)
    {
        var format = this.RegExpService.MatchFirstGroup(options.HtmlDocument.DocumentNode.OuterHtml, @"<th>Format<\/th>\s*<td(?:.*?)>(.*?)<\/td>");
        var dateString = this.RegExpService.MatchFirstGroup(options.HtmlDocument.DocumentNode.OuterHtml, @"<th>\s*Date\s*<\/th>\s*<td>(.*?)<\/td>");
        var dateModel = this.dateService.ParseDate(dateString);

        if (!options.Event.IsTeamEvent)
        {
            var archeryEvent = new ArcheryIndividualEvent
            {
                Format = format,
                StartDate = dateModel.StartDateTime,
                EndDate = dateModel.EndDateTime,
                Archers = new List<ArcheryIndividualRanking>()
            };

            if (options.Game.Year <= 1920)
            {
                var rows = options.StandingTable.HtmlDocument.DocumentNode.SelectNodes("//tr");
                var archers = await this.ConvertArcheryIndividualRankingAsync(rows, options.Event, options.Game, null, RoundType.None);
                archeryEvent.Archers = archers;
            }
            else if (options.Game.Year >= 1972 && options.Game.Year <= 1984)
            {
                foreach (var document in options.Documents)
                {
                    var htmlDocument = this.CreateHtmlDocument(document);
                    var title = htmlDocument.DocumentNode.SelectSingleNode("//h1").InnerText.Trim();

                    if (title.EndsWith("Part #1") || title.EndsWith("Part #2"))
                    {
                        var table = htmlDocument.DocumentNode.SelectSingleNode("//table[@class='table table-striped']");
                        var tableDocument = new HtmlDocument();
                        tableDocument.LoadHtml(table.OuterHtml);

                        var rows = tableDocument.DocumentNode.SelectNodes("//tr");
                        var archers = await this.ConvertArcheryIndividualRankingAsync(rows, options.Event, options.Game, title, RoundType.None);
                        archeryEvent.Archers.AddRange(archers);
                    }
                }
            }
            else if (options.Game.Year == 1988)
            {
                foreach (var table in options.Tables)
                {
                    var rows = table.HtmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']//tr");
                    var archers = await this.ConvertArcheryIndividualRankingAsync(rows, options.Event, options.Game, null, table.Round);
                    archeryEvent.Archers.AddRange(archers);
                }
            }
            else
            {
                var rankingRoundTable = options.Tables.FirstOrDefault();
                var rankingRoundRows = rankingRoundTable.HtmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']//tr");
                var archers = await this.ConvertArcheryIndividualRankingAsync(rankingRoundRows, options.Event, options.Game, null, rankingRoundTable.Round);
                archeryEvent.Archers.AddRange(archers);

                this.ConvertArcheryIndividualRankingAdditionalInfo(archeryEvent, rankingRoundTable, options.Documents);

                var matches = await this.ConvertArcheryIndividualMatchesAsync(options.Tables, options.Event, options.Game, options.Documents);
                archeryEvent.Matches = matches;
            }

            var json = JsonSerializer.Serialize(archeryEvent);
            var result = new Result
            {
                EventId = options.Event.Id,
                Json = json
            };

            await this.resultsService.AddOrUpdateAsync(result);
        }
        else
        {
            var archeryTeamEvent = new ArcheryTeamEvent
            {
                Format = format,
                StartDate = dateModel.StartDateTime,
                EndDate = dateModel.EndDateTime,
                Teams = new List<ArcheryTeamRanking>()
            };

            if (options.Game.Year <= 1920)
            {
                var rows = options.StandingTable.HtmlDocument.DocumentNode.SelectNodes("//tr");
                var headers = rows.First().Elements("th").Select(x => x.InnerText).ToList();
                var indexes = this.OlympediaService.FindIndexes(headers);
                await this.ConvertArcheryTeamRankingAsync(rows, indexes, options.Event, archeryTeamEvent, RoundType.None);
            }
            else if (options.Game.Year == 1988)
            {
                foreach (var table in options.Tables)
                {
                    var rows = table.HtmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']//tr");
                    var headers = rows.First().Elements("th").Select(x => x.InnerText).ToList();
                    var indexes = this.OlympediaService.FindIndexes(headers);

                    await this.ConvertArcheryTeamRankingAsync(rows, indexes, options.Event, archeryTeamEvent, table.Round);
                }
            }
            else
            {
                var rankingTable = options.Tables.FirstOrDefault();
                var rankingTableRows = rankingTable.HtmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']//tr");
                var tableRankingHeaders = rankingTableRows.First().Elements("th").Select(x => x.InnerText).ToList();
                var tableRankingIndexes = this.OlympediaService.FindIndexes(tableRankingHeaders);

                await this.ConvertArcheryTeamRankingAsync(rankingTableRows, tableRankingIndexes, options.Event, archeryTeamEvent, rankingTable.Round);

                var matches = await this.ConvertArcheryTeamMatchesAsync(options.Tables, options.Event, options.Game, options.Documents);
                archeryTeamEvent.Matches = matches;
            }

            var json = JsonSerializer.Serialize(archeryTeamEvent);
            var result = new Result
            {
                EventId = options.Event.Id,
                Json = json
            };

            await this.resultsService.AddOrUpdateAsync(result);
        }
    }

    private async Task<List<ArcheryTeamMatch>> ConvertArcheryTeamMatchesAsync(IList<TableModel> tables, EventCacheModel eventCacheModel, GameCacheModel gameCacheModel, IOrderedEnumerable<Document> documents)
    {
        var matches = new List<ArcheryTeamMatch>();

        foreach (var table in tables.Skip(1))
        {
            var rows = table.HtmlDocument.DocumentNode.SelectNodes("//tr").Where(x => this.OlympediaService.IsMatchNumber(x.InnerText)).ToList();
            foreach (var row in rows)
            {
                var tdNodes = row.Elements("td").ToList();
                var resultModel = this.OlympediaService.GetResult(tdNodes[4].InnerText);
                var homeNOCCacheModel = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == this.OlympediaService.FindCountryCode(tdNodes[3].OuterHtml));
                var homeTeam = await this.teamsService.GetAsync(homeNOCCacheModel.Id, eventCacheModel.Id);

                var match = new ArcheryTeamMatch
                {
                    MatchNumber = this.OlympediaService.FindMatchNumber(tdNodes[0].InnerText),
                    Round = table.Round,
                    RoundInfo = table.RoundInfo,
                    MatchType = this.OlympediaService.FindMatchType(table.Round, tdNodes[0].InnerText),
                    MatchInfo = this.OlympediaService.FindMatchInfo(tdNodes[0].InnerText),
                    Date = this.dateService.ParseDate(tdNodes[1].InnerText, gameCacheModel.Year).StartDateTime,
                    Decision = DecisionType.Buy,
                    HomeTeam = new ArcheryTeam
                    {
                        Name = homeTeam.Name,
                        TeamId = homeTeam.Id,
                        Result = ResultType.Draw
                    }
                };

                if (resultModel != null)
                {
                    var awayNOCCacheModel = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == this.OlympediaService.FindCountryCode(tdNodes[6].OuterHtml));
                    var awayTeam = await this.teamsService.GetAsync(awayNOCCacheModel.Id, eventCacheModel.Id);

                    match.ResultId = this.OlympediaService.FindResultNumber(tdNodes[0].OuterHtml);
                    match.Decision = DecisionType.None;
                    match.HomeTeam.Result = resultModel.HomeResult;
                    match.HomeTeam.Points = resultModel.HomePoints;
                    match.AwayTeam = new ArcheryTeam
                    {
                        Name = awayTeam.Name,
                        TeamId = awayTeam.Id,
                        Result = resultModel.AwayResult,
                        Points = resultModel.AwayPoints
                    };

                    var matchDocument = documents.FirstOrDefault(x => x.Url.EndsWith($"{match.ResultId}"));
                    if (matchDocument != null)
                    {
                        var htmlDocument = this.CreateHtmlDocument(matchDocument);
                        var lineJudge = this.RegExpService.MatchFirstGroup(htmlDocument.ParsedText, @"<th>Line Judge<\/th>(?:.*?)\/athletes\/(\d+)");
                        var targetJudge = this.RegExpService.MatchFirstGroup(htmlDocument.ParsedText, @"<th>Target Judge<\/th>(?:.*?)\/athletes\/(\d+)");
                        match.LineJudgeId = await this.ExtractRefereeAsync(lineJudge);
                        match.TargetJudgeId = await this.ExtractRefereeAsync(targetJudge);

                        var infoTables = htmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']");
                        var teamTable = infoTables[0];
                        this.ProcessArcheryTeamAdditionalInfo(teamTable, match);

                        var homeArchers = await this.ConvertArcheryTeamArchersAsync(infoTables[1], eventCacheModel);
                        match.HomeTeam.Archers = homeArchers;
                        var awayArchers = await this.ConvertArcheryTeamArchersAsync(infoTables[2], eventCacheModel);
                        match.AwayTeam.Archers = awayArchers;
                    }
                }

                matches.Add(match);
            }
        }

        return matches;
    }

    private async Task<List<ArcheryIndividualMatch>> ConvertArcheryIndividualMatchesAsync(IList<TableModel> tables, EventCacheModel eventCacheModel, GameCacheModel gameCacheModel, IOrderedEnumerable<Document> documents)
    {
        var matches = new List<ArcheryIndividualMatch>();

        foreach (var table in tables.Skip(1))
        {
            var rows = table.HtmlDocument.DocumentNode.SelectNodes("//tr").Where(x => this.OlympediaService.IsMatchNumber(x.InnerText)).ToList();
            foreach (var row in rows)
            {
                var tdNodes = row.Elements("td").ToList();
                var homeParticipantNumber = this.OlympediaService.FindAthleteNumber(tdNodes[2].OuterHtml);
                var awayParticipantNumber = this.OlympediaService.FindAthleteNumber(tdNodes[5].OuterHtml);
                var homeParticipant = await this.participantsService.GetAsync(homeParticipantNumber, eventCacheModel.Id);
                var awayParticipant = await this.participantsService.GetAsync(awayParticipantNumber, eventCacheModel.Id);
                var resultModel = this.OlympediaService.GetResult(tdNodes[4].InnerText);

                var match = new ArcheryIndividualMatch
                {
                    MatchNumber = this.OlympediaService.FindMatchNumber(tdNodes[0].InnerText),
                    Round = table.Round,
                    RoundInfo = table.RoundInfo,
                    MatchType = this.OlympediaService.FindMatchType(table.Round, tdNodes[0].InnerText),
                    MatchInfo = this.OlympediaService.FindMatchInfo(tdNodes[0].InnerText),
                    Date = this.dateService.ParseDate(tdNodes[1].InnerText, gameCacheModel.Year).StartDateTime,
                    ResultId = this.OlympediaService.FindResultNumber(tdNodes[0].OuterHtml),
                    Decision = resultModel.Decision,
                    HomeArcher = new ArcheryIndividual
                    {
                        Name = tdNodes[2].InnerText,
                        ParticipantId = homeParticipant.Id,
                        ParticipantNumber = homeParticipantNumber,
                        Points = resultModel.HomePoints,
                        Result = resultModel.HomeResult
                    },
                    AwayArcher = new ArcheryIndividual
                    {
                        Name = tdNodes[5].InnerText,
                        ParticipantId = awayParticipant.Id,
                        ParticipantNumber = awayParticipantNumber,
                        Points = resultModel.AwayPoints,
                        Result = resultModel.AwayResult
                    }
                };

                var matchDocument = documents.FirstOrDefault(x => x.Url.EndsWith($"{match.ResultId}"));
                if (matchDocument != null)
                {
                    var htmlDocument = this.CreateHtmlDocument(matchDocument);
                    var lineJudge = this.RegExpService.MatchFirstGroup(htmlDocument.ParsedText, @"<th>Line Judge<\/th>(?:.*?)\/athletes\/(\d+)");
                    var targetJudge = this.RegExpService.MatchFirstGroup(htmlDocument.ParsedText, @"<th>Target Judge<\/th>(?:.*?)\/athletes\/(\d+)");
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
                            match.HomeArcher.Target = firstTableIndexes.TryGetValue(ConverterConstants.INDEX_TARGET, out int value15) ? firstData[value15].InnerText : null;
                            match.HomeArcher.Set1Points = firstTableIndexes.TryGetValue(ConverterConstants.INDEX_SET_1, out int value2) ? this.RegExpService.MatchInt(firstData[value2].InnerText) : null;
                            match.HomeArcher.Set2Points = firstTableIndexes.TryGetValue(ConverterConstants.INDEX_SET_2, out int value3) ? this.RegExpService.MatchInt(firstData[value3].InnerText) : null;
                            match.HomeArcher.Set3Points = firstTableIndexes.TryGetValue(ConverterConstants.INDEX_SET_3, out int value4) ? this.RegExpService.MatchInt(firstData[value4].InnerText) : null;
                            match.HomeArcher.Set4Points = firstTableIndexes.TryGetValue(ConverterConstants.INDEX_SET_4, out int value5) ? this.RegExpService.MatchInt(firstData[value5].InnerText) : null;
                            match.HomeArcher.Set5Points = firstTableIndexes.TryGetValue(ConverterConstants.INDEX_SET_5, out int value6) ? this.RegExpService.MatchInt(firstData[value6].InnerText) : null;
                        }
                        else
                        {
                            match.AwayArcher.Target = firstTableIndexes.TryGetValue(ConverterConstants.INDEX_TARGET, out int value1) ? firstData[value1].InnerText : null;
                            match.AwayArcher.Set1Points = firstTableIndexes.TryGetValue(ConverterConstants.INDEX_SET_1, out int value2) ? this.RegExpService.MatchInt(firstData[value2].InnerText) : null;
                            match.AwayArcher.Set2Points = firstTableIndexes.TryGetValue(ConverterConstants.INDEX_SET_2, out int value3) ? this.RegExpService.MatchInt(firstData[value3].InnerText) : null;
                            match.AwayArcher.Set3Points = firstTableIndexes.TryGetValue(ConverterConstants.INDEX_SET_3, out int value4) ? this.RegExpService.MatchInt(firstData[value4].InnerText) : null;
                            match.AwayArcher.Set4Points = firstTableIndexes.TryGetValue(ConverterConstants.INDEX_SET_4, out int value5) ? this.RegExpService.MatchInt(firstData[value5].InnerText) : null;
                            match.AwayArcher.Set5Points = firstTableIndexes.TryGetValue(ConverterConstants.INDEX_SET_5, out int value6) ? this.RegExpService.MatchInt(firstData[value6].InnerText) : null;
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
                            match.HomeArcher.Score10s = this.RegExpService.MatchInt(secondData[0].InnerText);
                            match.AwayArcher.Score10s = this.RegExpService.MatchInt(secondData[1].InnerText);
                        }
                        else if (secondHeader == "Xs")
                        {
                            match.HomeArcher.ScoreXs = this.RegExpService.MatchInt(secondData[0].InnerText);
                            match.AwayArcher.ScoreXs = this.RegExpService.MatchInt(secondData[1].InnerText);
                        }
                        else if (secondHeader == "Shoot-Off Points")
                        {
                            match.HomeArcher.ShootOffPoints = this.RegExpService.MatchInt(secondData[0].InnerText);
                            match.AwayArcher.ShootOffPoints = this.RegExpService.MatchInt(secondData[1].InnerText);
                        }
                        else if (secondHeader == "Arrow 1")
                        {
                            match.HomeArcher.Arrow1 = secondData[0].InnerText;
                            match.AwayArcher.Arrow1 = secondData[1].InnerText;
                        }
                        else if (secondHeader == "Arrow 2")
                        {
                            match.HomeArcher.Arrow2 = secondData[0].InnerText;
                            match.AwayArcher.Arrow2 = secondData[1].InnerText;
                        }
                        else if (secondHeader == "Arrow 3")
                        {
                            match.HomeArcher.Arrow3 = secondData[0].InnerText;
                            match.AwayArcher.Arrow3 = secondData[1].InnerText;
                        }
                        else if (secondHeader == "Arrow 4")
                        {
                            match.HomeArcher.Arrow4 = secondData[0].InnerText;
                            match.AwayArcher.Arrow4 = secondData[1].InnerText;
                        }
                        else if (secondHeader == "Arrow 5")
                        {
                            match.HomeArcher.Arrow5 = secondData[0].InnerText;
                            match.AwayArcher.Arrow5 = secondData[1].InnerText;
                        }
                        else if (secondHeader == "Arrow 6")
                        {
                            match.HomeArcher.Arrow6 = secondData[0].InnerText;
                            match.AwayArcher.Arrow6 = secondData[1].InnerText;
                        }
                        else if (secondHeader == "Arrow 7")
                        {
                            match.HomeArcher.Arrow7 = secondData[0].InnerText;
                            match.AwayArcher.Arrow7 = secondData[1].InnerText;
                        }
                        else if (secondHeader == "Arrow 8")
                        {
                            match.HomeArcher.Arrow8 = secondData[0].InnerText;
                            match.AwayArcher.Arrow8 = secondData[1].InnerText;
                        }
                        else if (secondHeader == "Arrow 9")
                        {
                            match.HomeArcher.Arrow9 = secondData[0].InnerText;
                            match.AwayArcher.Arrow9 = secondData[1].InnerText;
                        }
                        else if (secondHeader == "Arrow 10")
                        {
                            match.HomeArcher.Arrow10 = secondData[0].InnerText;
                            match.AwayArcher.Arrow10 = secondData[1].InnerText;
                        }
                        else if (secondHeader == "Arrow 11")
                        {
                            match.HomeArcher.Arrow11 = secondData[0].InnerText;
                            match.AwayArcher.Arrow11 = secondData[1].InnerText;
                        }
                        else if (secondHeader == "Arrow 12")
                        {
                            match.HomeArcher.Arrow12 = secondData[0].InnerText;
                            match.AwayArcher.Arrow12 = secondData[1].InnerText;
                        }
                        else if (secondHeader == "Arrow 13")
                        {
                            match.HomeArcher.Arrow13 = secondData[0].InnerText;
                            match.AwayArcher.Arrow13 = secondData[1].InnerText;
                        }
                        else if (secondHeader == "Arrow 14")
                        {
                            match.HomeArcher.Arrow14 = secondData[0].InnerText;
                            match.AwayArcher.Arrow14 = secondData[1].InnerText;
                        }
                        else if (secondHeader == "Arrow 15")
                        {
                            match.HomeArcher.Arrow15 = secondData[0].InnerText;
                            match.AwayArcher.Arrow15 = secondData[1].InnerText;
                        }
                        else if (secondHeader == "Arrow 16")
                        {
                            match.HomeArcher.Arrow16 = secondData[0].InnerText;
                            match.AwayArcher.Arrow16 = secondData[1].InnerText;
                        }
                        else if (secondHeader == "Tiebreak 1")
                        {
                            match.HomeArcher.Tiebreak1 = this.RegExpService.MatchInt(secondData[0].InnerText);
                            match.AwayArcher.Tiebreak1 = this.RegExpService.MatchInt(secondData[1].InnerText);
                        }
                        else if (secondHeader == "Tiebreak 2")
                        {
                            match.HomeArcher.Tiebreak2 = this.RegExpService.MatchInt(secondData[0].InnerText);
                            match.AwayArcher.Tiebreak2 = this.RegExpService.MatchInt(secondData[1].InnerText);
                        }
                    }
                }

                matches.Add(match);
            }
        }

        return matches;
    }

    private void ConvertArcheryIndividualRankingAdditionalInfo(ArcheryIndividualEvent archeryEvent, TableModel table, IOrderedEnumerable<Document> documents)
    {
        var resultNumbers = this.OlympediaService.FindResults(table.HtmlDocument.DocumentNode.OuterHtml);
        foreach (var resultNumber in resultNumbers)
        {
            var document = documents.FirstOrDefault(x => x.Name.Contains($"{resultNumber}"));
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
                    var athleteNumber = this.OlympediaService.FindAthleteNumber(data[indexes[ConverterConstants.INDEX_NAME]].OuterHtml);
                    var participant = archeryEvent.Archers.FirstOrDefault(x => x.ParticipantNumber == athleteNumber);
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

    private async Task<List<ArcheryIndividualRanking>> ConvertArcheryIndividualRankingAsync(HtmlNodeCollection rows, EventCacheModel eventCacheModel, GameCacheModel gameCacheModel, string title, RoundType round)
    {
        var archers = new List<ArcheryIndividualRanking>();

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
                var archer = new ArcheryIndividualRanking
                {
                    ParticipantId = participant.Id,
                    ParticipantNumber = athleteNumber,
                    Round = round,
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
                    Record = this.OlympediaService.FindRecord(row.OuterHtml),
                    Qualification = this.OlympediaService.FindQualification(row.OuterHtml)
                };

                if (!string.IsNullOrEmpty(title) && (gameCacheModel.Year >= 1972 && gameCacheModel.Year <= 1984) && title.EndsWith("Part #1"))
                {
                    archer.Round = RoundType.RoundOne;
                    archer.Part1Points = indexes.TryGetValue(ConverterConstants.INDEX_POINTS, out int value) ? this.RegExpService.MatchInt(data[value].InnerText) : null;
                }
                else
                {
                    archer.Round = RoundType.RoundTwo;
                    archer.Part2Points = indexes.TryGetValue(ConverterConstants.INDEX_POINTS, out int value) ? this.RegExpService.MatchInt(data[value].InnerText) : null;
                }

                archers.Add(archer);
            }
        }

        return archers;
    }

    private void ProcessArcheryTeamAdditionalInfo(HtmlNode htmlNode, ArcheryTeamMatch match)
    {
        var document = new HtmlDocument();
        document.LoadHtml(htmlNode.OuterHtml);
        var rows = document.DocumentNode.SelectNodes("//tr");
        var headers = rows.First().Elements("th").Select(x => x.InnerText).ToList();
        var indexes = this.OlympediaService.FindIndexes(headers);

        for (int i = 1; i < rows.Count; i++)
        {
            var data = rows[i].Elements("td").ToList();
            if (i == 1)
            {
                match.HomeTeam.Target = indexes.TryGetValue(ConverterConstants.INDEX_TARGET, out int value1) ? data[value1].InnerText : null;
                match.HomeTeam.Set1Points = indexes.TryGetValue(ConverterConstants.INDEX_SET_1, out int value2) ? this.RegExpService.MatchInt(data[value2].InnerText) : null;
                match.HomeTeam.Set2Points = indexes.TryGetValue(ConverterConstants.INDEX_SET_2, out int value3) ? this.RegExpService.MatchInt(data[value3].InnerText) : null;
                match.HomeTeam.Set3Points = indexes.TryGetValue(ConverterConstants.INDEX_SET_3, out int value4) ? this.RegExpService.MatchInt(data[value4].InnerText) : null;
                match.HomeTeam.Set4Points = indexes.TryGetValue(ConverterConstants.INDEX_SET_4, out int value5) ? this.RegExpService.MatchInt(data[value5].InnerText) : null;
                match.HomeTeam.Set5Points = indexes.TryGetValue(ConverterConstants.INDEX_SET_5, out int value6) ? this.RegExpService.MatchInt(data[value6].InnerText) : null;
            }
            else
            {
                match.AwayTeam.Target = indexes.TryGetValue(ConverterConstants.INDEX_TARGET, out int value1) ? data[value1].InnerText : null;
                match.AwayTeam.Set1Points = indexes.TryGetValue(ConverterConstants.INDEX_SET_1, out int value2) ? this.RegExpService.MatchInt(data[value2].InnerText) : null;
                match.AwayTeam.Set2Points = indexes.TryGetValue(ConverterConstants.INDEX_SET_2, out int value3) ? this.RegExpService.MatchInt(data[value3].InnerText) : null;
                match.AwayTeam.Set3Points = indexes.TryGetValue(ConverterConstants.INDEX_SET_3, out int value4) ? this.RegExpService.MatchInt(data[value4].InnerText) : null;
                match.AwayTeam.Set4Points = indexes.TryGetValue(ConverterConstants.INDEX_SET_4, out int value5) ? this.RegExpService.MatchInt(data[value5].InnerText) : null;
                match.AwayTeam.Set5Points = indexes.TryGetValue(ConverterConstants.INDEX_SET_5, out int value6) ? this.RegExpService.MatchInt(data[value6].InnerText) : null;
            }
        }
    }

    private async Task<List<ArcheryIndividual>> ConvertArcheryTeamArchersAsync(HtmlNode htmlNode, EventCacheModel eventCacheModel)
    {
        var archers = new List<ArcheryIndividual>();

        var document = new HtmlDocument();
        document.LoadHtml(htmlNode.OuterHtml);
        var rows = document.DocumentNode.SelectNodes("//tr");
        var headers = rows.First().Elements("th").Select(x => x.InnerText).ToList();
        var indexes = this.OlympediaService.FindIndexes(headers);

        for (int i = 1; i < rows.Count - 1; i++)
        {
            var data = rows[i].Elements("td").ToList();
            var archerNumber = this.OlympediaService.FindAthleteNumber(data[2].OuterHtml);
            var archerName = data[2].InnerText;
            var archerParticipant = await this.participantsService.GetAsync(archerNumber, eventCacheModel.Id);
            var archer = new ArcheryIndividual
            {
                Name = archerName,
                ParticipantId = archerParticipant.Id,
                ParticipantNumber = archerNumber,
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

            archers.Add(archer);
        }

        return archers;
    }

    private async Task ConvertArcheryTeamRankingAsync(HtmlNodeCollection rows, Dictionary<string, int> indexes, EventCacheModel eventCacheModel, ArcheryTeamEvent archeryTeamEvent, RoundType round)
    {
        foreach (var row in rows.Skip(1))
        {
            var data = row.Elements("td").ToList();
            var name = data[indexes[ConverterConstants.INDEX_NAME]].InnerText;
            var nocCode = this.OlympediaService.FindCountryCode(data[indexes[ConverterConstants.INDEX_NOC]].OuterHtml);
            if (nocCode != null)
            {
                var nocCodeCacheModel = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == nocCode);
                var team = await this.teamsService.GetAsync(name, nocCodeCacheModel.Id, eventCacheModel.Id);

                archeryTeamEvent.Teams.Add(new ArcheryTeamRanking
                {
                    Name = name,
                    TeamId = team.Id,
                    Round = round,
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
                    Archers = new List<ArcheryIndividualRanking>()
                });
            }
            else
            {
                var athleteNumber = this.OlympediaService.FindAthleteNumber(data[indexes[ConverterConstants.INDEX_NAME]].OuterHtml);
                var participant = await this.participantsService.GetAsync(athleteNumber, eventCacheModel.Id);
                var archer = new ArcheryIndividualRanking
                {
                    ParticipantId = participant.Id,
                    ParticipantNumber = athleteNumber,
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

                if (archer.Points == null)
                {
                    archer.Points = indexes.TryGetValue(ConverterConstants.INDEX_INDIVIDUAL_POINTS, out int value9) ? this.RegExpService.MatchInt(data[value9].InnerText) : null;
                }

                archeryTeamEvent.Teams.LastOrDefault().Archers.Add(archer);
            }
        }
    }
    #endregion ARCHERY

    #region ALPINE SKIING
    private async Task ProcessAlpineSkiingAsync(ConvertOptions options)
    {
        var format = this.RegExpService.MatchFirstGroup(options.HtmlDocument.DocumentNode.OuterHtml, @"<th>Format<\/th>\s*<td(?:.*?)>(.*?)<\/td>");
        var dateString = this.RegExpService.MatchFirstGroup(options.HtmlDocument.DocumentNode.OuterHtml, @"<th>\s*Date\s*<\/th>\s*<td>(.*?)<\/td>");
        var courseSetterString = this.RegExpService.MatchFirstGroup(options.HtmlDocument.DocumentNode.OuterHtml, @"<th>\s*Course Setter\s*<\/th>\s*<td(.*?)<\/td>");
        var gatesMatch = this.RegExpService.MatchFirstGroup(options.HtmlDocument.DocumentNode.OuterHtml, @"Gates:(.*?)<br>");
        var lengthMatch = this.RegExpService.MatchFirstGroup(options.HtmlDocument.DocumentNode.OuterHtml, @"Length:(.*?)<br>");
        var startAltitudeMatch = this.RegExpService.MatchFirstGroup(options.HtmlDocument.DocumentNode.OuterHtml, @"Start Altitude:(.*?)<br>");
        var verticalDropMatch = this.RegExpService.MatchFirstGroup(options.HtmlDocument.DocumentNode.OuterHtml, @"Vertical Drop:(.*?)<\/td>");

        var date = this.dateService.ParseDate(dateString);
        var courseSetter = await this.athletesService.GetAsync(this.OlympediaService.FindAthleteNumber(courseSetterString));
        var gates = this.RegExpService.MatchInt(gatesMatch);
        var length = this.RegExpService.MatchInt(lengthMatch);
        var startAltitude = this.RegExpService.MatchInt(startAltitudeMatch);
        var verticalDrop = this.RegExpService.MatchInt(verticalDropMatch);

        if (!options.Event.IsTeamEvent)
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

            if (options.Documents.Any())
            {
                foreach (var doc in options.Documents)
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

                    await this.ExtractAlpineSkiingParticipantsAsync(htmlDocument, options.Event, options.Game, alpineSkiingEvent);
                }
            }
            else
            {
                await this.ExtractAlpineSkiingParticipantsAsync(options.HtmlDocument, options.Event, options.Game, alpineSkiingEvent);
            }

            var json = JsonSerializer.Serialize(alpineSkiingEvent);
            var result = new Result
            {
                EventId = options.Event.Id,
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
            foreach (var table in options.Tables)
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
                    var homeTeam = await this.teamsService.GetAsync(homeNOCCacheModel.Id, options.Event.Id);
                    var awayNOCCacheModel = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == this.OlympediaService.FindCountryCode(awayTeamCode));
                    var awayTeam = await this.teamsService.GetAsync(awayNOCCacheModel.Id, options.Event.Id);

                    var match = new AlpineSkiingMatch
                    {
                        MatchNumber = this.OlympediaService.FindMatchNumber(tdNodes[0].InnerText),
                        Round = table.Round,
                        RoundInfo = table.RoundInfo,
                        MatchType = this.OlympediaService.FindMatchType(table.Round, tdNodes[0].InnerText),
                        MatchInfo = this.OlympediaService.FindMatchInfo(tdNodes[0].InnerText),
                        Date = this.dateService.ParseDate(tdNodes[1].InnerText, options.Game.Year).StartDateTime,
                        ResultId = this.OlympediaService.FindResultNumber(tdNodes[0].OuterHtml),
                        Decision = resultModel.Decision,
                        HomeTeam = new AlpineSkiingTeam
                        {
                            Name = homeTeamName,
                            TeamId = homeTeam.Id,
                            Result = resultModel.HomeResult,
                            Points = resultModel.HomePoints,
                            Participants = new List<AlpineSkiingParticipant>()
                        },
                        AwayTeam = new AlpineSkiingTeam
                        {
                            Name = awayTeamName,
                            TeamId = awayTeam.Id,
                            Result = resultModel.AwayResult,
                            Points = resultModel.AwayPoints,
                            Participants = new List<AlpineSkiingParticipant>()
                        }
                    };

                    var matchDocument = options.Documents.FirstOrDefault(x => x.Url.EndsWith($"{match.ResultId}"));
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
                            var firstParticipant = await this.participantsService.GetAsync(firstParticipantNumber, options.Event.Id);
                            var secondParticipantNumber = this.OlympediaService.FindAthleteNumber(nodes[6].OuterHtml);
                            var secondParticipantNOCCode = this.OlympediaService.FindCountryCode(nodes[7].OuterHtml);
                            var secondParticipant = await this.participantsService.GetAsync(secondParticipantNumber, options.Event.Id);
                            var raceResultModel = this.OlympediaService.GetResult(nodes[5].InnerHtml);

                            var firstAlpineSkiingParticipant = new AlpineSkiingParticipant
                            {
                                Race = race,
                                ParticipantId = firstParticipant.Id,
                                Run1Time = raceResultModel.HomeTime,
                                Result = raceResultModel.HomeResult
                            };

                            var secondAlpineSkiingParticipant = new AlpineSkiingParticipant
                            {
                                Race = race,
                                ParticipantId = secondParticipant.Id,
                                Run1Time = raceResultModel.AwayTime,
                                Result = raceResultModel.AwayResult
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
                EventId = options.Event.Id,
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
    private async Task ProcessBasketball3x3Async(ConvertOptions options)
    {
        var matches = new List<BasketballMatch>();
        foreach (var table in options.Tables)
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
                var homeTeam = await this.teamsService.GetAsync(homeNOCCacheModel.Id, options.Event.Id);
                var awayNOCCacheModel = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == this.OlympediaService.FindCountryCode(awayTeamCode));
                var awayTeam = await this.teamsService.GetAsync(awayNOCCacheModel.Id, options.Event.Id);

                var match = new BasketballMatch
                {
                    MatchNumber = this.OlympediaService.FindMatchNumber(tdNodes[0].InnerText),
                    Round = table.Round,
                    RoundInfo = table.RoundInfo,
                    MatchType = this.OlympediaService.FindMatchType(table.Round, tdNodes[0].InnerText),
                    MatchInfo = this.OlympediaService.FindMatchInfo(tdNodes[0].InnerText),
                    Date = this.dateService.ParseDate(tdNodes[1].InnerText, options.Game.Year).StartDateTime,
                    ResultId = this.OlympediaService.FindResultNumber(tdNodes[0].OuterHtml),
                    Decision = resultModel.Decision,
                    HomeTeam = new BasketballTeam
                    {
                        Name = homeTeam.Name,
                        TeamId = homeTeam.Id,
                        Result = resultModel.HomeResult,
                        Points = resultModel.HomePoints
                    },
                    AwayTeam = new BasketballTeam
                    {
                        Name = awayTeam.Name,
                        TeamId = awayTeam.Id,
                        Result = resultModel.AwayResult,
                        Points = resultModel.AwayPoints
                    }
                };

                var matchDocument = options.Documents.FirstOrDefault(x => x.Url.EndsWith($"{match.ResultId}"));
                if (matchDocument != null)
                {
                    var htmlDocument = this.CreateHtmlDocument(matchDocument);
                    var firstReferee = this.RegExpService.MatchFirstGroup(htmlDocument.ParsedText, @"<th>Referee #1<\/th>(?:.*?)\/athletes\/(\d+)");
                    var secondReferee = this.RegExpService.MatchFirstGroup(htmlDocument.ParsedText, @"<th>Referee #2<\/th>(?:.*?)\/athletes\/(\d+)");
                    match.FirstRefereeId = await this.ExtractRefereeAsync(firstReferee);
                    match.SecondRefereeId = await this.ExtractRefereeAsync(secondReferee);

                    var teamTables = htmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']").Skip(1).ToList();
                    var homeTeamParticipants = await this.ExtractBasketball3x3ParticipantsAsync(teamTables[0].OuterHtml, options.Event);
                    var awayTeamParticipants = await this.ExtractBasketball3x3ParticipantsAsync(teamTables[1].OuterHtml, options.Event);

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
            EventId = options.Event.Id,
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

    private async Task ProcessBasketballAsync(ConvertOptions options)
    {
        var matches = new List<BasketballMatch>();
        foreach (var table in options.Tables)
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

                if (options.Game.Year >= 2020)
                {
                    homeTeamCode = tdNodes[3].OuterHtml;
                    resultString = tdNodes[4].InnerText;
                    awayTeamCode = tdNodes[6].OuterHtml;
                }

                var resultModel = this.OlympediaService.GetResult(resultString);
                var homeNOCCacheModel = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == this.OlympediaService.FindCountryCode(homeTeamCode));
                var homeTeam = await this.teamsService.GetAsync(homeNOCCacheModel.Id, options.Event.Id);

                var match = new BasketballMatch
                {
                    MatchNumber = this.OlympediaService.FindMatchNumber(tdNodes[0].InnerText),
                    Round = table.Round,
                    RoundInfo = table.RoundInfo,
                    MatchType = this.OlympediaService.FindMatchType(table.Round, tdNodes[0].InnerText),
                    MatchInfo = this.OlympediaService.FindMatchInfo(tdNodes[0].InnerText),
                    Date = this.dateService.ParseDate(tdNodes[1].InnerText, options.Game.Year).StartDateTime,
                    ResultId = this.OlympediaService.FindResultNumber(tdNodes[0].OuterHtml),
                    Decision = resultModel.Decision,
                    HomeTeam = new BasketballTeam
                    {
                        Name = homeTeam.Name,
                        TeamId = homeTeam.Id,
                        Result = resultModel.HomeResult,
                        Points = resultModel.HomePoints
                    }
                };

                if (match.Decision == DecisionType.None)
                {
                    var awayNOCCacheModel = this.DataCacheService.NOCCacheModels.FirstOrDefault(x => x.Code == this.OlympediaService.FindCountryCode(awayTeamCode));
                    var awayTeam = await this.teamsService.GetAsync(awayNOCCacheModel.Id, options.Event.Id);

                    match.AwayTeam = new BasketballTeam
                    {
                        Name = awayTeam.Name,
                        TeamId = awayTeam.Id,
                        Result = resultModel.AwayResult,
                        Points = resultModel.AwayPoints
                    };

                    var matchDocument = options.Documents.FirstOrDefault(x => x.Url.EndsWith($"{match.ResultId}"));
                    if (matchDocument != null)
                    {
                        var htmlDocument = this.CreateHtmlDocument(matchDocument);

                        var attendanceMatch = this.RegExpService.Match(htmlDocument.ParsedText, @"<th>Attendance<\/th><td>(.*?)<\/td>");
                        var attendance = this.RegExpService.MatchInt(attendanceMatch?.Groups[1]?.Value);
                        match.Attendance = attendance;

                        var referees = this.RegExpService.Matches(htmlDocument.ParsedText, @"<th>Referee<\/th>(?:.*?)\/athletes\/(\d+)");
                        var refereeIds = referees.Select(x => x.Groups[1].Value).ToList();

                        if (options.Game.Year >= 2012)
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
                        if (options.Game.Year >= 2020)
                        {
                            teamTables = htmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']").Skip(1).ToList();
                        }

                        var homeTeamParticipants = await this.ExtractBasketballParticipantsAsync(teamTables[0].OuterHtml, options.Event, options.Game);
                        var awayTeamParticipants = await this.ExtractBasketballParticipantsAsync(teamTables[1].OuterHtml, options.Event, options.Game);

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

                        if (options.Game.Year >= 2020)
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
            EventId = options.Event.Id,
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