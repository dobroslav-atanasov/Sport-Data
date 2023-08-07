namespace SportData.Converters;

using System.Reflection;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SportData.Common.Constants;
using SportData.Converters.Countries;
using SportData.Converters.OlympicGames;
using SportData.Data.Contexts;
using SportData.Data.Factories;
using SportData.Data.Factories.Interfaces;
using SportData.Data.Repositories;
using SportData.Services;
using SportData.Services.Data.CrawlerStorageDb;
using SportData.Services.Data.CrawlerStorageDb.Interfaces;
using SportData.Services.Data.SportDataDb;
using SportData.Services.Data.SportDataDb.Interfaces;
using SportData.Services.Interfaces;
using SportData.Services.Mapper;

public class Program
{
    public static async Task Main(string[] args)
    {
        var services = ConfigureServices();
        await StartConvertersAscyn(services);
    }

    private static async Task StartConvertersAscyn(ServiceProvider services)
    {
        //await services.GetService<CountryDataConverter>().ConvertAsync(ConverterConstants.COUNTRY_CONVERTER);
        //await services.GetService<NOCConverter>().ConvertAsync(ConverterConstants.OLYMPEDIA_NOC_CONVERTER);
        //await services.GetService<GameConverter>().ConvertAsync(ConverterConstants.OLYMPEDIA_GAME_CONVERTER);
        //await services.GetService<SportDisciplineConverter>().ConvertAsync(ConverterConstants.OLYMPEDIA_SPORT_DISCIPLINE_CONVERTER);
        //await services.GetService<VenueConverter>().ConvertAsync(ConverterConstants.OLYMPEDIA_VENUE_CONVERTER);
        //await services.GetService<EventConverter>().ConvertAsync(ConverterConstants.OLYMPEDIA_RESULT_CONVERTER);
        //await services.GetService<AthleteConverter>().ConvertAsync(ConverterConstants.OLYMPEDIA_ATHELETE_CONVERTER);
        //await services.GetService<ParticipantConverter>().ConvertAsync(ConverterConstants.OLYMPEDIA_RESULT_CONVERTER);
        await services.GetService<ResultConverter>().ConvertAsync(ConverterConstants.OLYMPEDIA_RESULT_CONVERTER);
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

        // AUTOMAPPER
        MapperConfig.RegisterMapper(Assembly.Load(AppGlobalConstants.AUTOMAPPER_MODELS_ASSEMBLY));

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

        services.AddDbContext<SportDataDbContext>(options =>
        {
            options.UseLazyLoadingProxies();
            options.UseSqlServer(configuration.GetConnectionString(AppGlobalConstants.SPORT_DATA_CONNECTION_STRING));
        });

        services.AddDbContext<CrawlerStorageDbContext>(options =>
        {
            options.UseLazyLoadingProxies();
            options.UseSqlServer(configuration.GetConnectionString(AppGlobalConstants.CRAWLER_STORAGE_CONNECTION_STRING));
        });

        services.AddScoped(typeof(SportDataRepository<>));
        services.AddScoped(typeof(CrawlerStorageRepository<>));

        services.AddScoped<IZipService, ZipService>();
        services.AddScoped<IRegExpService, RegExpService>();
        services.AddScoped<IHttpService, HttpService>();
        services.AddScoped<IMD5Hash, MD5Hash>();
        services.AddScoped<INormalizeService, NormalizeService>();
        services.AddScoped<IOlympediaService, OlympediaService>();
        services.AddScoped<IDateService, DateService>();

        services.AddScoped<ICrawlersService, CrawlersService>();
        services.AddScoped<IGroupsService, GroupsService>();
        services.AddScoped<ILogsService, LogsService>();
        services.AddScoped<IDataCacheService, DataCacheService>();
        services.AddScoped<ICountriesService, CountriesService>();
        services.AddScoped<INOCsService, NOCsService>();
        services.AddScoped<ICitiesService, CitiesService>();
        services.AddScoped<IGamesService, GamesService>();
        services.AddScoped<IHostsService, HostsService>();
        services.AddScoped<ISportsService, SportsService>();
        services.AddScoped<IDisciplinesService, DisciplinesService>();
        services.AddScoped<IVenuesService, VenuesService>();
        services.AddScoped<IEventsService, EventsService>();
        services.AddScoped<IEventVenueService, EventVenueService>();
        services.AddScoped<IAthletesService, AthletesService>();
        services.AddScoped<INationalitiesService, NationalitiesService>();
        services.AddScoped<IParticipantsService, ParticipantsService>();
        services.AddScoped<ITeamsService, TeamsService>();
        services.AddScoped<ISquadsService, SquadsService>();
        services.AddScoped<IResultsService, ResultsService>();

        services.AddScoped<CountryDataConverter>();
        services.AddScoped<NOCConverter>();
        services.AddScoped<GameConverter>();
        services.AddScoped<SportDisciplineConverter>();
        services.AddScoped<VenueConverter>();
        services.AddScoped<EventConverter>();
        services.AddScoped<AthleteConverter>();
        services.AddScoped<ParticipantConverter>();
        services.AddScoped<ResultConverter>();

        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider;
    }
}