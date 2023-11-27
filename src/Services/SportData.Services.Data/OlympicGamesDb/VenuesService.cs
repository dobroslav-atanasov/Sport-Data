namespace SportData.Services.Data.OlympicGamesDb;

using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using SportData.Data.Entities.OlympicGames;
using SportData.Data.Factories.Interfaces;
using SportData.Data.Models.Cache;
using SportData.Services.Data.OlympicGamesDb.Interfaces;
using SportData.Services.Mapper.Extensions;

public class VenuesService : IVenuesService
{
    private readonly IDbContextFactory dbContextFactory;

    public VenuesService(IDbContextFactory dbContextFactory)
    {
        this.dbContextFactory = dbContextFactory;
    }

    public async Task<Venue> AddOrUpdateAsync(Venue venue)
    {
        using var context = dbContextFactory.CreateOlympicGamesDbContext();

        var dbVenue = await context.Venues.FirstOrDefaultAsync(x => x.Number == venue.Number);
        if (dbVenue == null)
        {
            await context.AddAsync(venue);
            await context.SaveChangesAsync();
        }
        else
        {
            var isUpdated = dbVenue.IsUpdated(venue);
            if (isUpdated)
            {
                context.Update(dbVenue);
                await context.SaveChangesAsync();
            }

            venue = dbVenue;
        }

        return venue;
    }

    public ICollection<VenueCacheModel> GetVenueCacheModels()
    {
        using var context = dbContextFactory.CreateOlympicGamesDbContext();

        var models = context
            .Venues
            .AsNoTracking()
            .To<VenueCacheModel>()
            .ToList();

        return models;
    }
}