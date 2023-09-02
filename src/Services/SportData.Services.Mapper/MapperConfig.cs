namespace SportData.Services.Mapper;

using System.Reflection;

using AutoMapper;

using SportData.Services.Mapper.Interfaces;
using SportData.Services.Mapper.Models;

public static class MapperConfig
{
    private static bool initialized;

    public static IMapper Mapper { get; set; }

    public static void RegisterMapper(params Assembly[] assemblies)
    {
        if (initialized)
        {
            return;
        }

        initialized = true;

        var types = assemblies.SelectMany(a => a.GetExportedTypes()).ToList();

        var config = new MapperConfigurationExpression();
        config.CreateProfile(
            "ReflectionProfile",
            configuration =>
            {
                foreach (var map in GetFromMaps(types))
                {
                    configuration.CreateMap(map.Source, map.Destination);
                }

                foreach (var map in GetToMaps(types))
                {
                    configuration.CreateMap(map.Source, map.Destination);
                }

                foreach (var map in GetCustomMappings(types))
                {
                    map.CreateMap(configuration);
                }
            });

        Mapper = new Mapper(new MapperConfiguration(config));
    }

    private static IEnumerable<TypesMap> GetFromMaps(IEnumerable<Type> types)
    {
        var fromMaps = from t in types
                       from i in t.GetTypeInfo().GetInterfaces()
                       where i.GetTypeInfo().IsGenericType &&
                             i.GetGenericTypeDefinition() == typeof(IMapFrom<>) &&
                             !t.GetTypeInfo().IsAbstract &&
                             !t.GetTypeInfo().IsInterface
                       select new TypesMap
                       {
                           Source = i.GetTypeInfo().GetGenericArguments()[0],
                           Destination = t,
                       };

        return fromMaps;
    }

    private static IEnumerable<TypesMap> GetToMaps(IEnumerable<Type> types)
    {
        var toMaps = from t in types
                     from i in t.GetTypeInfo().GetInterfaces()
                     where i.GetTypeInfo().IsGenericType &&
                           i.GetTypeInfo().GetGenericTypeDefinition() == typeof(IMapTo<>) &&
                           !t.GetTypeInfo().IsAbstract &&
                           !t.GetTypeInfo().IsInterface
                     select new TypesMap
                     {
                         Source = t,
                         Destination = i.GetTypeInfo().GetGenericArguments()[0],
                     };

        return toMaps;
    }

    private static IEnumerable<ICustomMap> GetCustomMappings(IEnumerable<Type> types)
    {
        var customMap = from t in types
                        from i in t.GetTypeInfo().GetInterfaces()
                        where typeof(ICustomMap).GetTypeInfo().IsAssignableFrom(t) &&
                              !t.GetTypeInfo().IsAbstract &&
                              !t.GetTypeInfo().IsInterface
                        select (ICustomMap)Activator.CreateInstance(t);

        return customMap;
    }
}