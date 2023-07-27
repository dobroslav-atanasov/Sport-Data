﻿namespace SportData.Converters;

using System.Reflection;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SportData.Common.Constants;
using SportData.Converters.Countries;
using SportData.Converters.OlympicGames;
using SportData.Data.Contexts;
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
        await services.GetService<CountryDataConverter>().ConvertAsync(ConverterConstants.COUNTRY_CONVERTER);
        await services.GetService<NOCConverter>().ConvertAsync(ConverterConstants.OLYMPEDIA_NOC_CONVERTER);
        await services.GetService<GameConverter>().ConvertAsync(ConverterConstants.OLYMPEDIA_GAME_CONVERTER);
        await services.GetService<SportDisciplineConverter>().ConvertAsync(ConverterConstants.OLYMPEDIA_SPORT_DISCIPLINE_CONVERTER);
        await services.GetService<VenueConverter>().ConvertAsync(ConverterConstants.OLYMPEDIA_VENUE_CONVERTER);
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
        services.AddDbContext<SportDataDbContext>(options =>
        {
            options.UseLazyLoadingProxies();
            var connectionString = configuration.GetConnectionString(AppGlobalConstants.SPORT_DATA_CONNECTION_STRING);
            options.UseSqlServer(connectionString);
        });

        services.AddDbContext<CrawlerStorageDbContext>(options =>
        {
            options.UseLazyLoadingProxies();
            var connectionString = configuration.GetConnectionString(AppGlobalConstants.CRAWLER_STORAGE_CONNECTION_STRING);
            options.UseSqlServer(connectionString);
        });

        services.AddScoped(typeof(SportDataRepository<>));
        services.AddScoped(typeof(CrawlerStorageRepository<>));

        services.AddScoped<IZipService, ZipService>();
        services.AddScoped<IRegExpService, RegExpService>();
        services.AddScoped<IHttpService, HttpService>();
        services.AddScoped<IMD5Hash, MD5Hash>();
        services.AddScoped<INormalizeService, NormalizeService>();

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

        services.AddScoped<CountryDataConverter>();
        services.AddScoped<NOCConverter>();
        services.AddScoped<GameConverter>();
        services.AddScoped<SportDisciplineConverter>();
        services.AddScoped<VenueConverter>();

        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider;
    }
}