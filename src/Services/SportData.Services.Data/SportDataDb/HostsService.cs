namespace SportData.Services.Data.SportDataDb;

using System.Threading.Tasks;

using SportData.Data.Entities.OlympicGames;
using SportData.Data.Repositories;
using SportData.Services.Data.SportDataDb.Interfaces;

public class HostsService : IHostsService
{
    private readonly SportDataRepository<Host> repository;

    public HostsService(SportDataRepository<Host> repository)
    {
        this.repository = repository;
    }

    public async Task<Host> AddOrUpdateAsync(Host host)
    {
        var dbHost = await repository.GetAsync(x => x.CityId == host.CityId && x.GameId == host.GameId);
        if (dbHost == null)
        {
            await repository.AddAsync(host);
            await repository.SaveChangesAsync();
        }
        else
        {
            host = dbHost;
        }

        return host;
    }
}