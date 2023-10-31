namespace SportData.Data.Models.OlympicGames.Base;

public class BaseAthlete
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public int Code { get; set; }

    public string NOC { get; set; }
}