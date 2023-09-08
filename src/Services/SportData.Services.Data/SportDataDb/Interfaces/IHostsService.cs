namespace SportData.Services.Data.SportDataDb.Interfaces;

using SportData.Data.Entities.OlympicGames;

public interface IHostsService
{
    Task<Host> AddOrUpdateAsync(Host host);
}
