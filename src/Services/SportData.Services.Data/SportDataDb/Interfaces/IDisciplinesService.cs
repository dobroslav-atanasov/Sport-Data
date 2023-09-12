namespace SportData.Services.Data.SportDataDb.Interfaces;

using SportData.Data.Entities.OlympicGames;
using SportData.Data.Models.Cache;

public interface IDisciplinesService
{
    Task<Discipline> AddOrUpdateAsync(Discipline discipline);

    ICollection<DisciplineCacheModel> GetDisciplineCacheModels();
}