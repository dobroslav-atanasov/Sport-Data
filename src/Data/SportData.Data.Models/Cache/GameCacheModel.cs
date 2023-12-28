namespace SportData.Data.Models.Cache;

using SportData.Data.Entities.OlympicGames.Enumerations;

public class GameCacheModel
{
    public int Id { get; set; }

    public int Year { get; set; }

    public OlympicGameType Type { get; set; }

    public DateTime? OpenDate { get; set; }

    public DateTime? StartCompetitionDate { get; set; }
}