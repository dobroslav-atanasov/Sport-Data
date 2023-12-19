namespace SportData.Services.Interfaces;

using SportData.Data.Entities.Enumerations;
using SportData.Data.Entities.OlympicGames.Enumerations;
using SportData.Data.Models.Converters;
using SportData.Data.Models.OlympicGames;

public interface IOlympediaService
{
    List<AthleteModel> FindAthletes(string text);

    IList<string> FindNOCCodes(string text);

    List<int> FindVenues(string text);

    AthleteModel FindAthlete(string text);

    string FindNOCCode(string text);

    MedalType FindMedal(string text);

    MedalType FindMedal(string text, RoundType round);

    FinishStatus FindStatus(string text);

    int FindMatchNumber(string text);

    int FindResultNumber(string text);

    string FindLocation(string html);

    //MatchResultOld GetMatchResult(string text, MatchResultType type);

    string FindMatchInfo(string text);

    RecordType FindRecord(string text);

    QualificationType FindQualification(string text);

    IList<int> FindResults(string text);

    DecisionType FindDecision(string text);

    int FindSeedNumber(string text);

    void SetWinAndLose(MatchModel match);

    Horse FindHorse(string text);

    Dictionary<string, int> GetIndexes(List<string> headers);

    Dictionary<string, int> FindIndexes(List<string> headers);
}