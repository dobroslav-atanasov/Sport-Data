namespace SportData.Data.Models.OlympicGames.Aquatics.Diving;

using SportData.Data.Entities.Enumerations;
using SportData.Data.Entities.OlympicGames.Enumerations;
using SportData.Data.Models.OlympicGames.Base;

public class DIVDiver : BaseAthlete
{
    public DIVDiver()
    {
        this.Dives = new List<DIVDive>();
    }

    public decimal? Points { get; set; }

    public FinishStatus FinishStatus { get; set; }

    public QualificationType Qualification { get; set; }

    public GroupType Group { get; set; }

    public int? Order { get; set; }

    public decimal? CompulsoryPoints { get; set; }

    public decimal? FinalPoints { get; set; }

    public decimal? SemiFinalsPoints { get; set; }

    public decimal? QualificationPoints { get; set; }

    public decimal? Ordinals { get; set; }

    public List<DIVDive> Dives { get; set; }
}