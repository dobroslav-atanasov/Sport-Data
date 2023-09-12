namespace SportData.Services.Data.SportDataDb;

using System.Collections.Generic;

using SportData.Data.Models.Cache;
using SportData.Services.Data.SportDataDb.Interfaces;

public class DataCacheService : IDataCacheService
{
    private readonly Lazy<ICollection<NOCCacheModel>> nocCacheModels;
    private readonly Lazy<ICollection<GameCacheModel>> gameCacheModels;
    private readonly Lazy<ICollection<DisciplineCacheModel>> disciplineCacheModels;
    private readonly Lazy<ICollection<VenueCacheModel>> venueCacheModels;
    private readonly Lazy<ICollection<EventCacheModel>> eventCacheModels;
    private readonly INOCsService nocsService;
    private readonly IGamesService gamesService;
    private readonly IDisciplinesService disciplinesService;
    private readonly IVenuesService venuesService;
    private readonly IEventsService eventsService;

    public DataCacheService(INOCsService nocsService, IGamesService gamesService, IDisciplinesService disciplinesService, IVenuesService venuesService, IEventsService eventsService)
    {
        this.nocCacheModels = new Lazy<ICollection<NOCCacheModel>>(() => this.nocsService.GetNOCCacheModels());
        this.gameCacheModels = new Lazy<ICollection<GameCacheModel>>(() => this.gamesService.GetGameCacheModels());
        this.disciplineCacheModels = new Lazy<ICollection<DisciplineCacheModel>>(() => this.disciplinesService.GetDisciplineCacheModels());
        this.venueCacheModels = new Lazy<ICollection<VenueCacheModel>>(() => this.venuesService.GetVenueCacheModels());
        this.eventCacheModels = new Lazy<ICollection<EventCacheModel>>(() => this.eventsService.GetEventCacheModels());
        this.nocsService = nocsService;
        this.gamesService = gamesService;
        this.disciplinesService = disciplinesService;
        this.venuesService = venuesService;
        this.eventsService = eventsService;
    }

    public ICollection<NOCCacheModel> NOCCacheModels => this.nocCacheModels.Value;

    public ICollection<GameCacheModel> GameCacheModels => this.gameCacheModels.Value;

    public ICollection<DisciplineCacheModel> DisciplineCacheModels => this.disciplineCacheModels.Value;

    public ICollection<VenueCacheModel> VenueCacheModels => this.venueCacheModels.Value;

    public ICollection<EventCacheModel> EventCacheModels => this.eventCacheModels.Value;
}