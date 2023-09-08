namespace SportData.Services.Data.SportDataDb;

using System.Collections.Generic;
using System.Threading.Tasks;

using SportData.Data.Entities.OlympicGames;
using SportData.Data.Models.Cache;
using SportData.Data.Repositories;
using SportData.Services.Data.SportDataDb.Interfaces;
using SportData.Services.Mapper.Extensions;

public class VenuesService : IVenuesService
{
    private readonly SportDataRepository<Venue> repository;

    public VenuesService(SportDataRepository<Venue> repository)
    {
        this.repository = repository;
    }

    public async Task<Venue> AddOrUpdateAsync(Venue venue)
    {
        var dbVenue = await repository.GetAsync(x => x.Number == venue.Number);
        if (dbVenue == null)
        {
            await repository.AddAsync(venue);
            await repository.SaveChangesAsync();
        }
        else
        {
            var isUpdated = dbVenue.IsUpdated(venue);
            if (isUpdated)
            {
                repository.Update(dbVenue);
                await repository.SaveChangesAsync();
            }

            venue = dbVenue;
        }

        return venue;
    }

    public ICollection<VenueCacheModel> GetVenueCacheModels()
    {
        var models = this.repository
            .AllAsNoTracking()
            .To<VenueCacheModel>()
            .ToList();

        return models;
    }
}