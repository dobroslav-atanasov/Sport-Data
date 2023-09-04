namespace SportData.Services.Data.SportDataDb;

using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using SportData.Data.Entities.OlympicGames;
using SportData.Data.Factories.Interfaces;
using SportData.Data.Repositories;
using SportData.Services.Data.SportDataDb.Interfaces;

public class AthletesService : IAthletesService
{
    //private readonly SportDataRepository<Athlete> repository;
    private readonly IDbContextFactory dbContextFactory;

    public AthletesService(SportDataRepository<Athlete> repository, IDbContextFactory dbContextFactory)
    {
        //this.repository = repository;
        this.dbContextFactory = dbContextFactory;
    }

    public async Task<Athlete> AddOrUpdateAsync(Athlete athlete)
    {
        using var context = this.dbContextFactory.CreateSportDataDbContext();

        var dbAthlete = await context.Athletes.FirstOrDefaultAsync(x => x.Number == athlete.Number);
        if (dbAthlete == null)
        {
            await context.AddAsync(athlete);
            await context.SaveChangesAsync();
        }
        else
        {
            var isUpdated = dbAthlete.IsUpdated(athlete);
            if (isUpdated)
            {
                context.Update(dbAthlete);
                await context.SaveChangesAsync();
            }

            athlete = dbAthlete;
        }

        return athlete;

        //var dbAthlete = await repository.GetAsync(x => x.Number == athlete.Number);
        //if (dbAthlete == null)
        //{
        //    await repository.AddAsync(athlete);
        //    await repository.SaveChangesAsync();
        //}
        //else
        //{
        //    var isUpdated = dbAthlete.Equals(athlete);
        //    if (isUpdated)
        //    {
        //        repository.Update(dbAthlete);
        //        await repository.SaveChangesAsync();
        //    }
        //
        //     athlete = dbAthlete;
        //}

        //return athlete;
    }

    public async Task<Athlete> GetAsync(int number)
    {
        using var context = this.dbContextFactory.CreateSportDataDbContext();
        var athlete = await context.Athletes.FirstOrDefaultAsync(x => x.Number == number);
        //var athlete = await this.repository.GetAsync(x => x.Number == number);
        return athlete;
    }
}