namespace SportData.Converters.OlympicGames;

using System.Threading.Tasks;

using HtmlAgilityPack;

using Microsoft.Extensions.Logging;

using SportData.Data.Entities.Crawlers;
using SportData.Data.Entities.OlympicGames;
using SportData.Services.Data.CrawlerStorageDb.Interfaces;
using SportData.Services.Data.OlympicGamesDb.Interfaces;
using SportData.Services.Interfaces;

public class EventConverter : BaseOlympediaConverter
{
    private readonly IDateService dateService;
    private readonly IEventsService eventsService;
    private readonly IEventVenueService eventVenueService;

    public EventConverter(ILogger<BaseConverter> logger, ICrawlersService crawlersService, ILogsService logsService, IGroupsService groupsService, IZipService zipService,
        IRegExpService regExpService, INormalizeService normalizeService, IDataCacheService dataCacheService, IOlympediaService olympediaService, IDateService dateService,
        IEventsService eventsService, IEventVenueService eventVenueService)
        : base(logger, crawlersService, logsService, groupsService, zipService, regExpService, normalizeService, dataCacheService, olympediaService)
    {
        this.dateService = dateService;
        this.eventsService = eventsService;
        this.eventVenueService = eventVenueService;
    }

    protected override async Task ProcessGroupAsync(Group group)
    {
        try
        {
            var document = this.CreateHtmlDocument(group.Documents.OrderBy(d => d.Order).FirstOrDefault());
            var originalEventName = document.DocumentNode.SelectSingleNode("//ol[@class='breadcrumb']/li[@class='active']").InnerText;
            var gameCacheModel = this.FindGame(document);
            var disciplineCacheModel = this.FindDiscipline(document);
            var eventModel = this.CreateEventModel(originalEventName, gameCacheModel, disciplineCacheModel);
            if (eventModel != null && !this.CheckForbiddenEvent(eventModel.OriginalName, disciplineCacheModel.Name, gameCacheModel.Year))
            {
                var format = this.RegExpService.MatchFirstGroup(document.DocumentNode.OuterHtml, @"<tr>\s*<th>Format<\/th>\s*<td colspan=""2"">(.*?)<\/td>\s*<\/tr>");
                var description = document.DocumentNode.SelectSingleNode("//div[@class='description']")?.OuterHtml;
                var isTeamEvent = this.IsTeamEvent(document, eventModel.NormalizedName);

                var @event = new Event
                {
                    OriginalName = eventModel.OriginalName,
                    Name = eventModel.Name,
                    NormalizedName = eventModel.NormalizedName,
                    AdditionalInfo = eventModel.AdditionalInfo,
                    DisciplineId = disciplineCacheModel.Id,
                    GameId = gameCacheModel.Id,
                    Format = format,
                    Description = description != null ? this.RegExpService.CutHtml(description) : null,
                    IsTeamEvent = isTeamEvent,
                    Gender = this.NormalizeService.MapGenderType(eventModel.Name)
                };

                var dateMatch = this.RegExpService.Match(document.DocumentNode.OuterHtml, @"<tr>\s*<th>Date<\/th>\s*<td>(.*?)<\/td>\s*<\/tr>");
                if (dateMatch != null)
                {
                    var dateModel = this.dateService.ParseDate(dateMatch.Groups[1].Value.Trim());
                    @event.StartDate = dateModel.From;
                    @event.EndDate = dateModel.To;
                }

                @event = await this.eventsService.AddOrUpdateAsync(@event);

                var locationMatch = this.RegExpService.Match(document.DocumentNode.OuterHtml, @"<tr>\s*<th>Location<\/th>\s*<td>(.*?)<\/td>\s*<\/tr>");
                if (locationMatch != null)
                {
                    var venues = this.OlympediaService.FindVenues(locationMatch.Groups[1].Value);

                    foreach (var venue in venues)
                    {
                        var venueCacheModel = this.DataCacheService.VenueCacheModels.FirstOrDefault(v => v.Number == venue);
                        if (venueCacheModel != null)
                        {
                            await this.eventVenueService.AddOrUpdateAsync(new EventVenue { EventId = @event.Id, VenueId = venueCacheModel.Id });
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, $"Failed to process group: {group.Identifier}");
        }
    }

    private bool IsTeamEvent(HtmlDocument document, string eventNormalizeName)
    {
        var table = document.DocumentNode.SelectSingleNode("//table[@class='table table-striped']");
        //var rows = table.Elements("tr");

        var athletes = this.OlympediaService.FindAthletes(table.OuterHtml);
        var codes = this.OlympediaService.FindCountryCodes(table.OuterHtml);
        var isTeamEvent = false;
        if (athletes.Count != codes.Count)
        {
            isTeamEvent = true;
        }

        if (eventNormalizeName.ToLower().Contains("individual"))
        {
            isTeamEvent = false;
        }

        return isTeamEvent;
    }
}