namespace SportData.Services.Interfaces;

using SportData.Data.Entities.Enumerations;
using SportData.Data.Models.Converters;
using SportData.Data.Models.Enumerations;

public interface IOlympediaService
{
    List<AthleteModel> FindAthletes(string text);

    IList<string> FindCountryCodes(string text);

    List<int> FindVenues(string text);

    AthleteModel FindAthlete(string text);

    string FindNOCCode(string text);

    Dictionary<string, int> FindIndexes(List<string> headers);

    MedalType FindMedal(string text);

    FinishStatus FindStatus(string text);

    IList<AthleteModel> GetAthletes(string text);

    bool IsMatchNumber(string text);

    bool IsAthleteNumber(string text);

    int FindMatchNumber(string text);

    int FindResultNumber(string text);

    MatchResult GetMatchResult(string text, MatchResultType type);

    MatchType FindMatchType(RoundType round, string text);

    string FindMatchInfo(string text);

    RecordType FindRecord(string text);

    QualificationType FindQualification(string text);

    IList<int> FindResults(string text);

    DecisionType FindDecision(string text);

    int FindSeedNumber(string text);
}