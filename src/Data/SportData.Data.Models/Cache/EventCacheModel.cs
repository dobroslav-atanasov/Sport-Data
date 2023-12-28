namespace SportData.Data.Models.Cache;

using SportData.Data.Entities.Enumerations;

public class EventCacheModel
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