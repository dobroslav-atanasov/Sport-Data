namespace SportData.Crawlers;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using SportData.Services.Interfaces;

public abstract class BaseCrawler
{
    public BaseCrawler(ILogger<BaseCrawler> logger, IConfiguration configuration, IHttpService httpService)
    {
        Logger = logger;
        Configuration = configuration;
        HttpService = httpService;
    }

    protected ILogger<BaseCrawler> Logger { get; }

    protected IConfiguration Configuration { get; }

    protected IHttpService HttpService { get; }

    public abstract Task StartAsync();
}