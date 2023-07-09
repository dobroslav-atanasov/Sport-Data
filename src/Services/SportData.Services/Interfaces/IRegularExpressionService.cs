namespace SportData.Services.Interfaces;

using System.Text.RegularExpressions;

public interface IRegularExpressionService
{
    string MatchFirstGroup(string text, string pattern);

    Match Match(string text, string pattern);

    string Replace(string text, string pattern, string replacement);

    string CutHtml(string input);

    MatchCollection Matches(string text, string pattern);

    bool IsMatch(string text, string pattern);
}