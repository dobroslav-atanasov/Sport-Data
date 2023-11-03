namespace SportData.Data.Models.OlympicGames.Aquatics.Diving;

using SportData.Data.Models.OlympicGames.Base;

public class DIVRound : BaseRound
{
    public DIVRound()
    {
        this.Judges = new List<BaseJudge>();
        this.Divers = new List<DIVDiver>();
        this.Pairs = new List<DIVPair>();
    }

    public List<BaseJudge> Judges { get; set; }

    public List<DIVDiver> Divers { get; set; }

    public List<DIVPair> Pairs { get; set; }
}