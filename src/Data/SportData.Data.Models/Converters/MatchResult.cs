namespace SportData.Data.Models.Converters;

using SportData.Data.Entities.Enumerations;

public class MatchResult
{
    public MatchResult()
    {
        this.Games1 = new List<int?>();
        this.Games2 = new List<int?>();
    }

    public int Points1 { get; set; }

    public TimeSpan? Time1 { get; set; }

    public ResultType Result1 { get; set; }

    public int Points2 { get; set; }

    public TimeSpan? Time2 { get; set; }

    public ResultType Result2 { get; set; }

    //public DecisionType Decision { get; set; }

    public List<int?> Games1 { get; set; }

    public List<int?> Games2 { get; set; }
}