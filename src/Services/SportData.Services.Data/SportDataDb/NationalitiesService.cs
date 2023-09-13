namespace SportData.Services.Data.SportDataDb;

using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using SportData.Data.Entities.OlympicGames;
using SportData.Data.Factories.Interfaces;
using SportData.Data.Repositories;
using SportData.Services.Data.SportDataDb.Interfaces;

public class NationalitiesService : INationalitiesService
{
    private readonly IDbContextFactory dbContextFactory;

    //private readonly SportDataRepository<Nationality> repository;

    public NationalitiesService(SportDataRepository<Nationality> repository, IDbContextFactory dbContextFactory)
    {
        this.dbContextFactory = dbContextFactory;
        //this.repository = repository;
    }

    public async Task<Nationality> AddOrUpdateAsync(Nationality nationality)
    {
        using var context = this.dbContextFactory.CreateSportDataDbContext();
        var dbNationality = await context.Nationalities.FirstOrDefaultAsync(x => x.AthleteId == nationality.AthleteId && x.NOCId == nationality.NOCId);
        if (dbNationality == null)
        {
            await context.AddAsync(nationality);
            await context.SaveChangesAsync();
        }
        else
        {
            nationality = dbNationality;
        }

        return nationality;

        //var dbNationality = await repository.GetAsync(x => x.AthleteId == nationality.AthleteId && x.NOCId == nationality.NOCId);
        //if (dbNationality == null)
        //{
        //    await repository.AddAsync(nationality);
        //    await repository.SaveChangesAsync();
        //}
        //else
        //{
        //    nationality = dbNationality;
        //}

        //return nationality;
    }
}