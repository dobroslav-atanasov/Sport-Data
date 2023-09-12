namespace SportData.Services.Data.SportDataDb;

using System.Collections.Generic;
using System.Threading.Tasks;

using SportData.Data.Entities.OlympicGames;
using SportData.Data.Models.Cache;
using SportData.Data.Repositories;
using SportData.Services.Data.SportDataDb.Interfaces;
using SportData.Services.Mapper.Extensions;

public class DisciplinesService : IDisciplinesService
{
    private readonly SportDataRepository<Discipline> repository;

    public DisciplinesService(SportDataRepository<Discipline> repository)
    {
        this.repository = repository;
    }

    public async Task<Discipline> AddOrUpdateAsync(Discipline discipline)
    {
        var dbDiscipline = await repository.GetAsync(x => x.Name == discipline.Name);
        if (dbDiscipline == null)
        {
            await repository.AddAsync(discipline);
            await repository.SaveChangesAsync();
        }
        else
        {
            var isUpdated = dbDiscipline.IsUpdated(discipline);
            if (isUpdated)
            {
                repository.Update(dbDiscipline);
                await repository.SaveChangesAsync();
            }

            discipline = dbDiscipline;
        }

        return discipline;
    }

    public ICollection<DisciplineCacheModel> GetDisciplineCacheModels()
    {
        var models = this.repository
            .AllAsNoTracking()
            .To<DisciplineCacheModel>()
            .ToList();

        return models;
    }
}