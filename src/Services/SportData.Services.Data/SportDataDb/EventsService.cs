namespace SportData.Services.Data.SportDataDb;

using System.Collections.Generic;
using System.Threading.Tasks;

using SportData.Data.Entities.OlympicGames;
using SportData.Data.Models.Cache;
using SportData.Data.Repositories;
using SportData.Services.Data.SportDataDb.Interfaces;
using SportData.Services.Mapper.Extensions;

public class EventsService : IEventsService
{
    private readonly SportDataRepository<Event> repository;

    public EventsService(SportDataRepository<Event> repository)
    {
        this.repository = repository;
    }

    public async Task<Event> AddOrUpdateAsync(Event @event)
    {
        var dbEvent = await repository.GetAsync(x => x.OriginalName == @event.OriginalName && x.DisciplineId == @event.DisciplineId && x.GameId == @event.GameId);
        if (dbEvent == null)
        {
            await repository.AddAsync(@event);
            await repository.SaveChangesAsync();
        }
        else
        {
            var isUpdated = dbEvent.IsUpdated(@event);
            if (isUpdated)
            {
                repository.Update(dbEvent);
                await repository.SaveChangesAsync();
            }

            @event = dbEvent;
        }

        return @event;
    }

    public ICollection<EventCacheModel> GetEventCacheModels()
    {
        var models = this.repository
            .AllAsNoTracking()
            .To<EventCacheModel>()
            .ToList();

        return models;
    }
}