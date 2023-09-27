namespace SportData.Data.Models.Cache;

using SportData.Data.Entities.Enumerations;
using SportData.Data.Entities.OlympicGames;
using SportData.Services.Mapper.Interfaces;

public class EventCacheModel : IMapFrom<Event>
{
    public int Id { get; set; }

    public string OriginalName { get; set; }

    public string Name { get; set; }

    public string NormalizedName { get; set; }

    public GenderType Gender { get; set; }

    public bool IsTeamEvent { get; set; }

    public int DisciplineId { get; set; }

    public int GameId { get; set; }
}