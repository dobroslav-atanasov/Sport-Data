namespace SportData.Data.Models.Cache;

using SportData.Data.Entities.Enumerations;
using SportData.Data.Entities.OlympicGames;
using SportData.Services.Mapper.Interfaces;

public class GameCacheModel : IMapFrom<Game>
{
    public int Id { get; set; }

    public int Year { get; set; }

    public OlympicGameType Type { get; set; }

    public DateTime? OpenDate { get; set; }

    public DateTime? StartCompetitionDate { get; set; }
}