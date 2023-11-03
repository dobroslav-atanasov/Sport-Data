namespace SportData.Data.Models.OlympicGames.Aquatics.Diving;

public class DIVDive
{
    public int Number { get; set; }

    public decimal? Points { get; set; }

    public decimal? Difficulty { get; set; }

    public string Name { get; set; }

    public decimal? Ordinals { get; set; }

    public decimal? ExecutionJudge1Score { get; set; }
    public decimal? ExecutionJudge2Score { get; set; }
    public decimal? ExecutionJudge3Score { get; set; }
    public decimal? ExecutionJudge4Score { get; set; }
    public decimal? ExecutionJudge5Score { get; set; }
    public decimal? ExecutionJudge6Score { get; set; }
    public decimal? ExecutionJudge7Score { get; set; }

    public decimal? SynchronizationJudge1Score { get; set; }
    public decimal? SynchronizationJudge2Score { get; set; }
    public decimal? SynchronizationJudge3Score { get; set; }
    public decimal? SynchronizationJudge4Score { get; set; }
    public decimal? SynchronizationJudge5Score { get; set; }
}