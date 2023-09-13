namespace SportData.Services.Data.SportDataDb.Interfaces;

using SportData.Data.Entities.OlympicGames;
using SportData.Data.Models.Cache;

public interface INOCsService
{
    Task<NOC> AddOrUpdateAsync(NOC noc);

    ICollection<NOCCacheModel> GetNOCCacheModels();
}