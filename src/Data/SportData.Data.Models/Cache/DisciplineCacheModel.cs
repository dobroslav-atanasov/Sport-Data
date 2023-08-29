namespace SportData.Data.Models.Cache;

using SportData.Data.Entities.OlympicGames;
using SportData.Services.Mapper.Interfaces;

public class DisciplineCacheModel : IMapFrom<Discipline>
{
    public int Id { get; set; }

    public string Name { get; set; }
}