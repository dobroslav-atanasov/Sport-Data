namespace SportData.Services.Interfaces;

using SportData.Data.Entities.Enumerations;
using SportData.Data.Models.Converters;

public interface IOlympediaService
{
    List<int> FindAthleteNumbers(string text);

    IList<string> FindCountryCodes(string text);

    List<int> FindVenues(string text);

    int FindAthleteNumber(string text);

    string FindCountryCode(string text);

    string FindAthleteName(string text, int number);

    Dictionary<string, int> FindIndexes(List<string> headers);

    MedalType FindMedal(string text);

    FinishStatus FindStatus(string text);

    IList<AthleteModel> GetAthletes(string text);

    bool IsMatchNumber(string text);

    bool IsAthleteNumber(string text);

    int FindMatchNumber(string text);

    int FindResultNumber(string text);

    ResultModel GetResult(string text);

    MatchType FindMatchType(RoundType round, string text);

    string FindMatchInfo(string text);

    RecordType FindRecord(string text);

    QualificationType FindQualification(string text);

    IList<int> FindResults(string text);

    DecisionType FindDecision(string text);
}