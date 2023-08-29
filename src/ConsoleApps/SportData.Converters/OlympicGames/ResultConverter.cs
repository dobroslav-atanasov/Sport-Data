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
using SportData.Data.Models.OlympicGames.AlpineSkiing;
using SportData.Data.Models.OlympicGames.Archery;
using SportData.Data.Models.OlympicGames.ArtisticGymnastics;
using SportData.Data.Models.OlympicGames.ArtisticGymnastics.Individual;
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
                    var tables = this.GetTables(htmlDocument, eventCacheModel);

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
                        case DisciplineConstants.ARTISTIC_GYMNASTICS:
                            await this.ProcessArtisticGymnasticsAsync(options);
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

    #region ARTISTIC GYMNASTICS
    private async Task ProcessArtisticGymnasticsAsync(ConvertOptions options)
    {
        // JUDGES ?????????????????????????????????????????????///
        var dateString = this.RegExpService.MatchFirstGroup(options.HtmlDocument.DocumentNode.OuterHtml, @"<th>\s*Date\s*<\/th>\s*<td>(.*?)<\/td>");
        var dateModel = this.dateService.ParseDate(dateString);
        var eventType = this.NormalizeService.MapArtisticGymnasticsEvent(options.Event.Name);

        if (eventType == GAREventType.Vault)
        {
            //await Console.Out.WriteLineAsync($"{options.Game.Year}");
            //var standingHeaders = options.StandingTable.HtmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']//th").Where(x => !string.IsNullOrEmpty(x.InnerText)).Select(x => x.InnerText).ToList();
            //foreach (var item in standingHeaders)
            //{
            //    Console.WriteLine(item);
            //}

            //foreach (var table in options.Tables)
            //{
            //    var headers = table.HtmlDocument.DocumentNode.SelectNodes("//table[@class='table table-striped']//th").Where(x => !string.IsNullOrEmpty(x.InnerText)).Select(x => x.InnerText).ToList();
            //    foreach (var item in headers)
            //    {
            //        Console.WriteLine(item);
            //    }
            //}
            ;
        }

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
            //    //var result = new Result
            //    //{
            //    //    EventId = options.Event.Id,
            //    //    Json = json
            //    //};

            //    //await this.resultsService.AddOrUpdateAsync(result);
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