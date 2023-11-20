namespace SportData.Data.Models.OlympicGames.Aquatics.Diving;

using SportData.Data.Entities.OlympicGames.Enumerations;
using SportData.Data.Models.OlympicGames.Base;

public class DIVPair : BaseTeam
{
    public DIVPair()
    {
        this.Divers = new List<DIVDiver>();
        this.Dives = new List<DIVDive>();
    }

    public decimal? Points { get; set; }

    public FinishStatus FinishStatus { get; set; }

    public List<DIVDiver> Divers { get; set; }

    public List<DIVDive> Dives { get; set; }
}