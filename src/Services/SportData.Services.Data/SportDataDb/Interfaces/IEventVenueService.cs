namespace SportData.Services.Data.SportDataDb.Interfaces;

using SportData.Data.Entities.OlympicGames;

public interface IEventVenueService
{
    Task<EventVenue> AddOrUpdateAsync(EventVenue eventVenue);
}