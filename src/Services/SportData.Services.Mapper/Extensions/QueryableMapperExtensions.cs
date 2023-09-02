namespace SportData.Services.Mapper.Extensions;

using System.Linq.Expressions;

using AutoMapper.QueryableExtensions;

public static class QueryableMapperExtensions
{
    public static IQueryable<TDestination> To<TDestination>(this IQueryable source, params Expression<Func<TDestination, object>>[] membersToExpand)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        return source.ProjectTo(MapperConfig.Mapper.ConfigurationProvider, null, membersToExpand);
    }

    public static IQueryable<TDestination> To<TDestination>(this IQueryable source, object parameters)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        return source.ProjectTo<TDestination>(MapperConfig.Mapper.ConfigurationProvider, parameters);
    }
}