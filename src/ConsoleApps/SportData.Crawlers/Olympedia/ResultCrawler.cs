namespace SportData.Crawlers.Olympedia;

using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using SportData.Common.Constants;
using SportData.Data.Entities.Crawlers;
using SportData.Services.Data.CrawlerStorageDb.Interfaces;
using SportData.Services.Interfaces;

public class ResultCrawler : BaseOlympediaCrawler
{
    private readonly IOlympediaService olympediaService;

    public ResultCrawler(ILogger<BaseCrawler> logger, IConfiguration configuration, IHttpService httpService, ICrawlersService crawlersService, IGroupsService groupsService,
        IOlympediaService olympediaService)
        : base(logger, configuration, httpService, crawlersService, groupsService)
    {
        this.olympediaService = olympediaService;
    }

    public override async Task StartAsync()
    {
        try
        {
            var httpModel = await this.HttpService.GetAsync(this.Configuration.GetSection(CrawlerConstants.OLYMPEDIA_GAMES_URL).Value);
            var gameUrls = this.ExtractGameUrls(httpModel);
            var list = new HashSet<int>();

            foreach (var gameUrl in gameUrls)
            {
                try
                {
                    var gameHttpModel = await this.HttpService.GetAsync(gameUrl);
                    var disciplineUrls = this.ExtractOlympediaDisciplineUrls(gameHttpModel);

                    if (disciplineUrls != null && disciplineUrls.Count > 0)
                    {
                        foreach (var disciplineUrl in disciplineUrls)
                        {
                            try
                            {
                                var disciplineModel = await this.HttpService.GetAsync(disciplineUrl);
                                var medalDisciplineUrls = this.GetMedalDisciplineUrls(disciplineModel);

                                foreach (var medalDisciplineUrl in medalDisciplineUrls)
                                {
                                    try
                                    {
                                        var mainResultHttpModel = await this.HttpService.GetAsync(medalDisciplineUrl);
                                        var resultUrls = this.ExtractResultUrls(mainResultHttpModel);
                                        var athletes = this.olympediaService.FindAthletes(mainResultHttpModel.Content);
                                        foreach (var athlete in athletes)
                                        {
                                            list.Add(athlete.Code);
                                        }

                                        var documents = new List<Document>
                                        {
                                            this.CreateDocument(mainResultHttpModel)
                                        };
                                        var order = 2;

                                        foreach (var resultUrl in resultUrls)
                                        {
                                            try
                                            {
                                                var resultHttpModel = await this.HttpService.GetAsync(resultUrl);
                                                athletes = this.olympediaService.FindAthletes(resultHttpModel.Content);
                                                foreach (var athlete in athletes)
                                                {
                                                    list.Add(athlete.Code);
                                                }

                                                var document = this.CreateDocument(resultHttpModel);
                                                document.Order = order;
                                                order++;
                                                documents.Add(document);
                                            }
                                            catch (Exception ex)
                                            {
                                                this.Logger.LogError(ex, $"Failed to process data: {resultUrl};");
                                            }
                                        }

                                        await this.ProcessGroupAsync(mainResultHttpModel, documents);
                                    }
                                    catch (Exception ex)
                                    {
                                        this.Logger.LogError(ex, $"Failed to process data: {medalDisciplineUrl};");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                this.Logger.LogError(ex, $"Failed to process data: {disciplineUrl};");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, $"Failed to process data: {gameUrl};");
                }
            }

            await File.WriteAllLinesAsync("athletes.txt", list.Select(x => x.ToString()));
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, $"Failed to process url: {this.Configuration.GetSection(CrawlerConstants.OLYMPEDIA_GAMES_URL).Value};");
        }
    }
}