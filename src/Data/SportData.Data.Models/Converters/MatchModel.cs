namespace SportData.Data.Models.Converters;

using SportData.Data.Entities.Enumerations;
using SportData.Data.Entities.OlympicGames.Enumerations;

public class MatchModel
{
    public int Number { get; set; }

    public DateTime? Date { get; set; }

    public string Location { get; set; }

    public DecisionType Decision { get; set; } = DecisionType.None;

    public int ResultId { get; set; }

    public MedalType Medal { get; set; }

    public string Info { get; set; }

    public MatchTeamModel Team1 { get; set; } = new();

    public MatchTeamModel Team2 { get; set; } = new();
}

public class MatchTeamModel
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string NOC { get; set; }

    public int Code { get; set; }

    public MatchResultType MatchResult { get; set; } = MatchResultType.None;

    public int Points { get; set; }

    public TimeSpan? Time { get; set; }

    public List<MatchPartModel> Parts { get; set; } = new();
}

public class MatchPartModel
{
    public int Number { get; set; }

    public int Points { get; set; }
}