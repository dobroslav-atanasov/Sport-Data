namespace SportData.Crawlers;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SportData.Common.Constants;
using SportData.Crawlers.Countries;
using SportData.Data.Contexts;
using SportData.Services;
using SportData.Services.Data.CrawlerStorage;
using SportData.Services.Data.CrawlerStorage.Interfaces;
using SportData.Services.Interfaces;

public class Program
{
    public static async Task Main(string[] args)
    {
        var services = ConfigureServices();
        await StartCrawlersAsync(services);
    }

    private static async Task StartCrawlersAsync(ServiceProvider services)
    {
        await services.GetService<CountryDataCrawler>().StartAsync();

    }

    private static ServiceProvider ConfigureServices()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(AppGlobalConstants.APP_SETTINGS_FILE, false, true)
            .Build();

        var services = new ServiceCollection();

        // CONFIGURATION
        services.AddSingleton<IConfiguration>(configuration);

        // LOGGING
        services.AddLogging(options =>
        {
            options.AddConfiguration(configuration.GetSection(AppGlobalConstants.LOGGING));
            options.AddConsole();
            options.AddLog4Net(configuration.GetSection(AppGlobalConstants.LOG4NET_CORE).Get<Log4NetProviderOptions>());
        });

        // DATABASE
        services.AddDbContext<CrawlerStorageDbContext>(options =>
        {
            options.UseLazyLoadingProxies();
            options.UseSqlServer(configuration.GetConnectionString(AppGlobalConstants.CRAWLER_STORAGE_CONNECTION_STRING));
        });
        //var crawlerStorageOptions = new DbContextOptionsBuilder<CrawlerStorageDbContext>()
        //    .UseLazyLoadingProxies(true)
        //    .UseSqlServer(configuration.GetConnectionString(AppGlobalConstants.CRAWLER_STORAGE_CONNECTION_STRING))
        //    .Options;

        services.AddScoped<IHttpService, HttpService>();
        services.AddScoped<IMD5Hash, MD5Hash>();
        services.AddScoped<IZipService, ZipService>();

        services.AddScoped<ICrawlersService, CrawlersService>();
        services.AddScoped<IGroupsService, GroupsService>();
        services.AddScoped<ILogsService, LogsService>();
        services.AddScoped<IOperationsService, OperationsService>();

        services.AddTransient<CountryDataCrawler>();

        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider;
    }
}