namespace SportData.Converters;

using System.Text;

using Dasync.Collections;

using HtmlAgilityPack;

using Microsoft.Extensions.Logging;

using SportData.Common.Extensions;
using SportData.Data.Entities.Crawlers;
using SportData.Services.Data.CrawlerStorageDb.Interfaces;
using SportData.Services.Interfaces;

public abstract class BaseConverter
{
    private readonly ICrawlersService crawlersService;
    private readonly ILogsService logsService;
    private readonly IGroupsService groupsService;
    private readonly IZipService zipService;

    public BaseConverter(ILogger<BaseConverter> logger, ICrawlersService crawlersService, ILogsService logsService, IGroupsService groupsService, IZipService zipService)
    {
        this.Logger = logger;
        this.crawlersService = crawlersService;
        this.logsService = logsService;
        this.groupsService = groupsService;
        this.zipService = zipService;
    }

    protected ILogger<BaseConverter> Logger { get; }

    protected abstract Task ProcessGroupAsync(Group group);

    public async Task ConvertAsync(string crawlerName)
    {
        this.Logger.LogInformation($"Converter: {crawlerName} start.");

        try
        {
            var crawlerId = await this.crawlersService.GetCrawlerIdAsync(crawlerName);
            var identifiers = await this.logsService.GetLogIdentifiersAsync(crawlerId);

            identifiers = new List<Guid>
            {
                Guid.Parse("3ecc281a-0f0a-4857-80ad-53cb05c3f231"),
                Guid.Parse("ebe52e4b-dc68-47d4-8eff-f847c1b9d2f4"),
                Guid.Parse("771cf26f-6d70-4c38-8320-da95764b6223"),
                Guid.Parse("c48cada5-33c2-434a-adf1-5d25dab5eb44"),
                Guid.Parse("1fc03d32-8df3-404c-ad79-a8c5733e511b"),
                Guid.Parse("c9bbb435-fb48-4226-9a2d-037be6b7db19"),
                Guid.Parse("f16b7525-5eeb-47a7-888b-5f47f59289e3"),
                Guid.Parse("edd8dcb7-5089-473f-bed9-7217d4b167da"),
                Guid.Parse("f84c4774-7846-43ae-8660-ba4d9004487f"),
                Guid.Parse("caf7190c-70d6-4024-8644-3c7d3a4475e6"),
                Guid.Parse("54ec48a2-61fa-41b5-9288-d7ba16e78135"),
                Guid.Parse("6f6354bc-9921-4246-973b-f72aa78a21be"),
                Guid.Parse("db03a1a2-fc82-4341-9017-79552750fe15"),
                Guid.Parse("0147714a-2fac-4133-a7c7-2151a7a31233"),
            };

            await identifiers.ParallelForEachAsync(async identifier =>
            {
                try
                {
                    var group = await this.groupsService.GetGroupAsync(identifier);
                    var zipModels = this.zipService.UnzipGroup(group.Content);
                    foreach (var document in group.Documents)
                    {
                        var zipModel = zipModels.First(z => z.Name == document.Name);
                        document.Content = zipModel.Content;
                    }

                    await this.ProcessGroupAsync(group);
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, $"Group was not process: {identifier};");
                }
            }, maxDegreeOfParallelism: 1);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, $"Failed to process documents from converter: {crawlerName};");
        }

        this.Logger.LogInformation($"Converter: {crawlerName} end.");
    }

    protected HtmlDocument CreateHtmlDocument(Document document)
    {
        var encoding = Encoding.GetEncoding(document.Encoding);
        var html = encoding.GetString(document.Content).Decode();
        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(html);

        return htmlDocument;
    }
}