namespace SportData.Data.Models.Converters;

using SportData.Data.Entities.OlympicGames.Enumerations;

public class AthleteModel
{
    public int Code { get; set; }

    public FinishStatus FinishStatus { get; set; }

    public string Name { get; set; }
}