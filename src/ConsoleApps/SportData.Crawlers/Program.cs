namespace SportData.Crawlers;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SportData.Common.Constants;
using SportData.Crawlers.Countries;
using SportData.Crawlers.Olympedia;
using SportData.Data.Contexts;
using SportData.Data.Factories;
using SportData.Data.Factories.Interfaces;
using SportData.Data.Repositories;
using SportData.Services;
using SportData.Services.Data.CrawlerStorageDb;
using SportData.Services.Data.CrawlerStorageDb.Interfaces;
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
        //await services.GetService<CountryDataCrawler>().StartAsync();
        //await services.GetService<NOCCrawler>().StartAsync();
        //await services.GetService<GameCrawler>().StartAsync();
        //await services.GetService<SportDisciplineCrawler>().StartAsync();
        await services.GetService<ResultCrawler>().StartAsync();
        //await services.GetService<AthleteCrawler>().StartAsync();
        //await services.GetService<VenueCrawler>().StartAsync();
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
        var sportDataDbOptions = new DbContextOptionsBuilder<SportDataDbContext>()
            .UseLazyLoadingProxies(true)
            .UseSqlServer(configuration.GetConnectionString(AppGlobalConstants.SPORT_DATA_CONNECTION_STRING))
            .Options;

        var crawlerStorageDbOptions = new DbContextOptionsBuilder<CrawlerStorageDbContext>()
            .UseLazyLoadingProxies(true)
            .UseSqlServer(configuration.GetConnectionString(AppGlobalConstants.CRAWLER_STORAGE_CONNECTION_STRING))
            .Options;

        var dbContextFactory = new DbContextFactory(crawlerStorageDbOptions, sportDataDbOptions);
        services.AddSingleton<IDbContextFactory>(dbContextFactory);

        services.AddDbContext<CrawlerStorageDbContext>(options =>
        {
            options.UseLazyLoadingProxies();
            options.UseSqlServer(configuration.GetConnectionString(AppGlobalConstants.CRAWLER_STORAGE_CONNECTION_STRING));
        });

        services.AddScoped(typeof(CrawlerStorageRepository<>));

        services.AddScoped<IHttpService, HttpService>();
        services.AddScoped<IMD5Hash, MD5Hash>();
        services.AddScoped<IZipService, ZipService>();
        services.AddScoped<IRegExpService, RegExpService>();

        services.AddScoped<ICrawlersService, CrawlersService>();
        services.AddScoped<IGroupsService, GroupsService>();
        services.AddScoped<ILogsService, LogsService>();
        services.AddScoped<IOperationsService, OperationsService>();

        services.AddTransient<CountryDataCrawler>();
        services.AddTransient<NOCCrawler>();
        services.AddTransient<GameCrawler>();
        services.AddTransient<SportDisciplineCrawler>();
        services.AddTransient<ResultCrawler>();
        services.AddTransient<AthleteCrawler>();
        services.AddTransient<VenueCrawler>();

        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider;
    }
}