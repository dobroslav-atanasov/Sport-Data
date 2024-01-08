namespace SportData.Crawlers.Olympedia;

using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using SportData.Common.Constants;
using SportData.Data.Models.Http;
using SportData.Services.Data.CrawlerStorageDb.Interfaces;
using SportData.Services.Interfaces;

public class AthleteCrawler : BaseOlympediaCrawler
{
    public AthleteCrawler(ILogger<BaseCrawler> logger, IConfiguration configuration, IHttpService httpService, ICrawlersService crawlersService, IGroupsService groupsService)
        : base(logger, configuration, httpService, crawlersService, groupsService)
    {
    }

    public override async Task StartAsync()
    {
        var list = await File.ReadAllLinesAsync("athletes.txt");
        var count = 1;
        foreach (var item in list)
        {
            var athleteUrl = $"https://www.olympedia.org/athletes/{item}";

            try
            {
                var athleteHttpModel = await this.HttpService.GetAsync(athleteUrl);
                await this.ProcessGroupAsync(athleteHttpModel);
                Console.WriteLine(count++);
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, $"Failed to process data: {athleteUrl};");
            }
        }

        //this.Logger.LogInformation($"{this.GetType().FullName} Start!");
        //var groups = await this.GroupsService.GetGroupNamesAsync(this.CrawlerId.Value);

        //try
        //{
        //    var httpModel = await this.HttpService.GetAsync(this.Configuration.GetSection(CrawlerConstants.OLYMPEDIA_GAMES_URL).Value);
        //    var gameUrls = this.ExtractGameUrls(httpModel);

        //    foreach (var gameUrl in gameUrls)
        //    {
        //        try
        //        {
        //            var gameHttpModel = await this.HttpService.GetAsync(gameUrl);
        //            var disciplineUrls = this.ExtractOlympediaDisciplineUrls(gameHttpModel);

        //            if (disciplineUrls != null && disciplineUrls.Count > 0)
        //            {
        //                foreach (var disciplineUrl in disciplineUrls)
        //                {
        //                    try
        //                    {
        //                        var disciplineModel = await this.HttpService.GetAsync(disciplineUrl);
        //                        var medalDisciplineUrls = this.GetMedalDisciplineUrls(disciplineModel);

        //                        foreach (var medalDisciplineUrl in medalDisciplineUrls)
        //                        {
        //                            try
        //                            {
        //                                var mainResultHttpModel = await this.HttpService.GetAsync(medalDisciplineUrl);
        //                                var athletetUrls = this.ExtractAthleteUrls(mainResultHttpModel);

        //                                foreach (var athleteUrl in athletetUrls)
        //                                {
        //                                    var number = Regex.Match(athleteUrl, @"athletes/(\d+)");
        //                                    if (!groups.Contains($"athletes_{number.Groups[1].Value}.zip"))
        //                                    {
        //                                        try
        //                                        {
        //                                            var athleteHttpModel = await this.HttpService.GetAsync(athleteUrl);
        //                                            groups.Add($"athletes_{number.Groups[1].Value}.zip");
        //                                            await this.ProcessGroupAsync(athleteHttpModel);
        //                                        }
        //                                        catch (Exception ex)
        //                                        {
        //                                            this.Logger.LogError(ex, $"Failed to download data: {athleteUrl};");
        //                                        }
        //                                    }
        //                                }

        //                                var resultUrls = this.ExtractResultUrls(mainResultHttpModel);
        //                                foreach (var resultUrl in resultUrls)
        //                                {
        //                                    var resultHttpModel = await this.HttpService.GetAsync(resultUrl);
        //                                    athletetUrls = this.ExtractAthleteUrls(resultHttpModel);

        //                                    if (athletetUrls != null)
        //                                    {
        //                                        foreach (var athleteUrl in athletetUrls)
        //                                        {
        //                                            var number = Regex.Match(athleteUrl, @"athletes/(\d+)");
        //                                            if (!groups.Contains($"athletes_{number.Groups[1].Value}.zip"))
        //                                            {
        //                                                try
        //                                                {
        //                                                    var athleteHttpModel = await this.HttpService.GetAsync(athleteUrl);
        //                                                    groups.Add($"athletes_{number.Groups[1].Value}.zip");
        //                                                    await this.ProcessGroupAsync(athleteHttpModel);
        //                                                }
        //                                                catch (Exception ex)
        //                                                {
        //                                                    this.Logger.LogError(ex, $"Failed to process data: {athleteUrl};");
        //                                                }
        //                                            }
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                            catch (Exception ex)
        //                            {
        //                                this.Logger.LogError(ex, $"Failed to process data: {disciplineUrl};");
        //                            }
        //                        }
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        this.Logger.LogError(ex, $"Failed to process data: {disciplineUrl};");
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            this.Logger.LogError(ex, $"Failed to process data: {gameUrl};");
        //        }
        //    }
        //}
        //catch (Exception ex)
        //{
        //    this.Logger.LogError(ex, $"Failed to process url: {this.Configuration.GetSection(CrawlerConstants.OLYMPEDIA_GAMES_URL).Value};");
        //}

        //this.Logger.LogInformation($"{this.GetType().FullName} End!");
    }

    private IReadOnlyCollection<string> ExtractAthleteUrls(HttpModel httpModel)
    {
        var urls = httpModel
            .HtmlDocument
            .DocumentNode?
            .SelectNodes("//div[@class='container']//a")?
            .Select(x => x.Attributes["href"]?.Value.Trim())
            .Where(x => x.StartsWith("/athletes/"))
            .Select(x => this.CreateUrl(x, this.Configuration.GetSection(CrawlerConstants.OLYMPEDIA_MAIN_URL).Value))
            .Distinct()
            .ToList();

        return urls;
    }
}