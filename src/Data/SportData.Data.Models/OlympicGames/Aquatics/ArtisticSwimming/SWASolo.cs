namespace SportData.Data.Models.OlympicGames.Aquatics.ArtisticSwimming;

using SportData.Data.Entities.Enumerations;
using SportData.Data.Models.OlympicGames.Base;

public class SWASolo : BaseAthlete
{
    public double? Points { get; set; }

    public double? MusicalRoutinePoints { get; set; }

    public double? FigurePoints { get; set; }

    public QualificationType Qualification { get; set; }
}