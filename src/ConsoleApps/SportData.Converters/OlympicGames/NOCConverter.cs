namespace SportData.Converters.OlympicGames;

using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using SportData.Data.Entities.Crawlers;
using SportData.Services.Data.CrawlerStorage.Interfaces;
using SportData.Services.Interfaces;

public class NOCConverter : BaseConverter
{
    public NOCConverter(ILogger<BaseConverter> logger, ICrawlersService crawlersService, ILogsService logsService, IGroupsService groupsService, IZipService zipService,
        IRegExpService regExpService)
        : base(logger, crawlersService, logsService, groupsService, zipService, regExpService)
    {
    }

    protected override async Task ProcessGroupAsync(Group group)
    {
        throw new NotImplementedException();
    }
}