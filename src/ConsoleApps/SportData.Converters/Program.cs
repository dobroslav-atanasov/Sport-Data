namespace SportData.Converters;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SportData.Common.Constants;
using SportData.Converters.Countries;
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
        await StartConvertersAscyn(services);
    }

    private static async Task StartConvertersAscyn(ServiceProvider services)
    {
        await services.GetService<CountryDataConverter>().ConvertAsync("");
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
        services.AddLogging(config =>
        {
            config.AddConfiguration(configuration.GetSection(AppGlobalConstants.LOGGING));
            config.AddConsole();
            config.AddLog4Net(configuration.GetSection(AppGlobalConstants.LOG4NET_CORE).Get<Log4NetProviderOptions>());
        });

        // DATABASE
        services.AddDbContext<SportDataDbContext>(options =>
        {
            options.UseLazyLoadingProxies();
            options.UseSqlServer(configuration.GetConnectionString(""));
        });

        services.AddScoped<IZipService, ZipService>();
        services.AddScoped<IRegExpService, RegExpService>();
        services.AddScoped<IHttpService, HttpService>();

        services.AddScoped<ICrawlersService, CrawlersService>();
        services.AddScoped<IGroupsService, GroupsService>();
        services.AddScoped<ILogsService, LogsService>();

        services.AddTransient<CountryDataConverter>();

        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider;
    }
}