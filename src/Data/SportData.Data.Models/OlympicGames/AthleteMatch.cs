namespace SportData.Data.Models.OlympicGames;

public class AthleteMatch<TModel> : BaseMatch
{
    public TModel Athlete1 { get; set; }

    public TModel Athlete2 { get; set; }
}