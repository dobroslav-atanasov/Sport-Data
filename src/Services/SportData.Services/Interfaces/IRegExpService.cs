namespace SportData.Services.Interfaces;

using System.Text.RegularExpressions;

public interface IRegExpService
{
    string MatchFirstGroup(string text, string pattern);

    Match Match(string text, string pattern);

    string Replace(string text, string pattern, string replacement);

    string CutHtml(string input);

    string CutHtml(string input, string pattern);

    MatchCollection Matches(string text, string pattern);

    bool IsMatch(string text, string pattern);

    int? MatchInt(string text);

    double? MatchDouble(string text);

    decimal? MatchDecimal(string text);
}