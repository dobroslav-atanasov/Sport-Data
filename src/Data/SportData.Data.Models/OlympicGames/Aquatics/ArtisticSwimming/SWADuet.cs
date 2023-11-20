namespace SportData.Data.Models.OlympicGames.Aquatics.ArtisticSwimming;

using SportData.Data.Entities.OlympicGames.Enumerations;
using SportData.Data.Models.OlympicGames.Base;

public class SWADuet : BaseTeam
{
    public SWADuet()
    {
        this.Swimmers = new List<BaseAthlete>();
    }

    public List<BaseAthlete> Swimmers { get; set; }

    public double? Points { get; set; }

    public double? MusicalRoutinePoints { get; set; }

    public double? FigurePoints { get; set; }

    public QualificationType Qualification { get; set; }

    public SWATechnicalRoutine TechnicalRoutine { get; set; }

    public SWAFreeRoutine FreeRoutine { get; set; }
}