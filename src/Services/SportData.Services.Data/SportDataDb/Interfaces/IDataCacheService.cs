namespace SportData.Services.Data.SportDataDb.Interfaces;

using SportData.Data.Models.Cache;

public interface IDataCacheService
{
    ICollection<NOCCacheModel> NOCCacheModels { get; }

    ICollection<GameCacheModel> GameCacheModels { get; }

    ICollection<DisciplineCacheModel> DisciplineCacheModels { get; }

    ICollection<VenueCacheModel> VenueCacheModels { get; }

    ICollection<EventCacheModel> EventCacheModels { get; }
}