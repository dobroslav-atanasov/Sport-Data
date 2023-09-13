namespace SportData.Services.Data.SportDataDb.Interfaces;

using SportData.Data.Entities.OlympicGames;
using SportData.Data.Models.Cache;

public interface IEventsService
{
    Task<Event> AddOrUpdateAsync(Event @event);

    ICollection<EventCacheModel> GetEventCacheModels();
}