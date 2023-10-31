namespace SportData.Data.Models.OlympicGames.Aquatics.ArtisticSwimming;

using SportData.Data.Models.OlympicGames.Base;

public class SWARound : BaseRound
{
    public SWARound()
    {
        Solos = new List<SWASolo>();
        Duets = new List<SWADuet>();
        Teams = new List<SWATeam>();
    }

    public List<SWASolo> Solos { get; set; }

    public List<SWADuet> Duets { get; set; }

    public List<SWATeam> Teams { get; set; }
}