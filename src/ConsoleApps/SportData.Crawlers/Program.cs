namespace SportData.Crawlers;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SportData.Common.Constants;
using SportData.Services;
using SportData.Services.Interfaces;

public class Program
{
    public static async Task Main(string[] args)
    {
        var services = ConfigureServices();
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
        //var crawlerStorageOptions = new DbContextOptionsBuilder<CrawlerStorageDbContext>()
        //    .UseLazyLoadingProxies(true)
        //    .UseSqlServer(configuration.GetConnectionString(AppGlobalConstants.CRAWLER_STORAGE_CONNECTION_STRING))
        //    .Options;

        services.AddScoped<IHttpService, HttpService>();

        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider;
    }
}