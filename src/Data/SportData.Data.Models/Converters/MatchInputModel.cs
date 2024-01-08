namespace SportData.Data.Models.Converters;

using SportData.Data.Entities.OlympicGames.Enumerations;

public class MatchInputModel
{
    public string Row { get; set; }

    public string Number { get; set; }

    public string Date { get; set; }

    public string Location { get; set; }

    public int Year { get; set; }

    public int EventId { get; set; }

    public bool IsTeam { get; set; }

    public string HomeName { get; set; }

    public string HomeNOC { get; set; }

    public string AwayName { get; set; }

    public string AwayNOC { get; set; }

    public string Result { get; set; }

    public bool AnyParts { get; set; }

    public RoundType Round { get; set; }
}