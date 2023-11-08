namespace SportData.Converters.OlympicGames;

using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using SportData.Common.Extensions;
using SportData.Data.Entities.Crawlers;
using SportData.Data.Entities.OlympicGames;
using SportData.Data.Entities.OlympicGames.Enumerations;
using SportData.Services.Data.CrawlerStorageDb.Interfaces;
using SportData.Services.Data.OlympicGamesDb.Interfaces;
using SportData.Services.Interfaces;

public class SportDisciplineConverter : BaseOlympediaConverter
{
    private readonly ISportsService sportsService;
    private readonly IDisciplinesService disciplinesService;

    public SportDisciplineConverter(ILogger<BaseConverter> logger, ICrawlersService crawlersService, ILogsService logsService, IGroupsService groupsService, IZipService zipService,
        IRegExpService regExpService, INormalizeService normalizeService, IDataCacheService dataCacheService, IOlympediaService olympediaService, ISportsService sportsService,
        IDisciplinesService disciplinesService)
        : base(logger, crawlersService, logsService, groupsService, zipService, regExpService, normalizeService, dataCacheService, olympediaService)
    {
        this.sportsService = sportsService;
        this.disciplinesService = disciplinesService;
    }

    protected override async Task ProcessGroupAsync(Group group)
    {
        try
        {
            var document = this.CreateHtmlDocument(group.Documents.Single());
            var lines = document.DocumentNode.SelectNodes("//table[@class='table table-striped sortable']//tr");

            foreach (var line in lines)
            {
                if (line.OuterHtml.Contains("glyphicon-ok"))
                {
                    var elements = line.Elements("td").ToList();
                    var sportName = elements[2].InnerText.Trim();
                    if (sportName != "Air Sports" && sportName != "Mountaineering and Climbing" && sportName != "Art Competitions")
                    {
                        var type = elements[3].InnerText.Trim().ToEnum<OlympicGameType>();
                        var sportCode = this.RegExpService.MatchFirstGroup(elements[2].OuterHtml, @"/sport_groups/(.*?)""");
                        var sport = new Sport
                        {
                            Name = sportName,
                            Type = type,
                            Code = sportCode
                        };

                        sport = await this.sportsService.AddOrUpdateAsync(sport);

                        var disciplineName = elements[1].InnerText.Trim();
                        var disciplineAbbreviation = elements[0].InnerText.Trim();
                        var disciplines = new List<Discipline>();
                        if (sport.Name == "Wrestling")
                        {
                            disciplines.Add(new Discipline
                            {
                                Name = "Wrestling Freestyle",
                                Code = "WRF",
                                SportId = sport != null ? sport.Id : sport.Id
                            });

                            disciplines.Add(new Discipline
                            {
                                Name = "Wrestling Greco-Roman",
                                Code = "WRG",
                                SportId = sport != null ? sport.Id : sport.Id
                            });
                        }
                        else
                        {
                            disciplines.Add(new Discipline
                            {
                                Name = disciplineName,
                                Code = disciplineAbbreviation,
                                SportId = sport != null ? sport.Id : sport.Id
                            });
                        }

                        foreach (var discipline in disciplines.Where(x => x.Name != "Canoe Marathon"))
                        {
                            await this.disciplinesService.AddOrUpdateAsync(discipline);
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
}