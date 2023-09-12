namespace SportData.Services.Data.SportDataDb;

using System.Threading.Tasks;

using SportData.Data.Entities.OlympicGames;
using SportData.Data.Repositories;
using SportData.Services.Data.SportDataDb.Interfaces;

public class EventVenueService : IEventVenueService
{
    private readonly SportDataRepository<EventVenue> repository;

    public EventVenueService(SportDataRepository<EventVenue> repository)
    {
        this.repository = repository;
    }

    public async Task<EventVenue> AddOrUpdateAsync(EventVenue eventVenue)
    {
        var dbEventVenue = await repository.GetAsync(x => x.EventId == eventVenue.EventId && x.VenueId == eventVenue.VenueId);
        if (dbEventVenue == null)
        {
            await repository.AddAsync(eventVenue);
            await repository.SaveChangesAsync();
        }
        else
        {
            eventVenue = dbEventVenue;
        }

        return eventVenue;
    }
}