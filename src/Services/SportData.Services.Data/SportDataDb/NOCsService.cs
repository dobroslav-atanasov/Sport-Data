namespace SportData.Services.Data.SportDataDb;

using System.Collections.Generic;
using System.Threading.Tasks;

using SportData.Data.Entities.OlympicGames;
using SportData.Data.Models.Cache;
using SportData.Data.Repositories;
using SportData.Services.Data.SportDataDb.Interfaces;
using SportData.Services.Mapper.Extensions;

public class NOCsService : INOCsService
{
    private readonly SportDataRepository<NOC> repository;

    public NOCsService(SportDataRepository<NOC> repository)
    {
        this.repository = repository;
    }

    public async Task<NOC> AddOrUpdateAsync(NOC noc)
    {
        var dbNoc = await repository.GetAsync(x => x.Code == noc.Code);
        if (dbNoc == null)
        {
            await repository.AddAsync(noc);
            await repository.SaveChangesAsync();
        }
        else
        {
            var isUpdated = dbNoc.IsUpdated(noc);
            if (isUpdated)
            {
                repository.Update(dbNoc);
                await repository.SaveChangesAsync();
            }

            noc = dbNoc;
        }

        return noc;
    }

    public ICollection<NOCCacheModel> GetNOCCacheModels()
    {
        var models = this.repository
            .AllAsNoTracking()
            .To<NOCCacheModel>()
            .ToList();

        return models;
    }
}