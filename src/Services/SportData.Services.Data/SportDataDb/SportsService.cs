namespace SportData.Services.Data.SportDataDb;

using System.Threading.Tasks;

using SportData.Data.Entities.OlympicGames;
using SportData.Data.Repositories;
using SportData.Services.Data.SportDataDb.Interfaces;

public class SportsService : ISportsService
{
    private readonly SportDataRepository<Sport> repository;

    public SportsService(SportDataRepository<Sport> repository)
    {
        this.repository = repository;
    }

    public async Task<Sport> AddOrUpdateAsync(Sport sport)
    {
        var dbSport = await repository.GetAsync(x => x.Name == sport.Name);
        if (dbSport == null)
        {
            await repository.AddAsync(sport);
            await repository.SaveChangesAsync();
        }
        else
        {
            var isUpdated = dbSport.IsUpdated(sport);
            if (isUpdated)
            {
                repository.Update(dbSport);
                await repository.SaveChangesAsync();
            }

            sport = dbSport;
        }

        return sport;
    }
}