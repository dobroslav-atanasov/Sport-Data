namespace SportData.Services;

using System.Text.RegularExpressions;

using SportData.Common.Extensions;
using SportData.Services.Interfaces;

public class RegExpService : IRegExpService
{
    public string CutHtml(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return null;
        }

        return Regex.Replace(input, "<.*?>", string.Empty);
    }

    public string CutHtml(string input, string pattern)
    {
        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(pattern))
        {
            return null;
        }

        return Regex.Replace(input, pattern, string.Empty);
    }

    public bool IsMatch(string text, string pattern)
    {
        return Regex.IsMatch(text, pattern);
    }

    public Match Match(string text, string pattern)
    {
        var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (match.Success)
        {
            return match;
        }

        return null;
    }

    public double? MatchDouble(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        text = Regex.Replace(text, @"\(.*?\)", string.Empty);
        if (string.IsNullOrEmpty(text) || !text.Any(char.IsDigit))
        {
            return null;
        }

        text = text.Replace(",", ".").Replace("+", string.Empty).Trim();

        var match = Regex.Match(text, @"(\d+)\.(\d+)\.(\d+)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (match.Success)
        {
            return double.Parse($"{match.Groups[1].Value}{match.Groups[2].Value},{match.Groups[3].Value}");
        }

        match = Regex.Match(text, @"([-.\d]+)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (match.Success)
        {
            var number = match.Groups[1].Value.Replace(".", ",");
            return double.Parse(number);
        }

        return null;
    }

    public decimal? MatchDecimal(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        text = Regex.Replace(text, @"\(.*?\)", string.Empty);
        if (string.IsNullOrEmpty(text) || !text.Any(char.IsDigit))
        {
            return null;
        }

        text = text.Replace(",", ".").Replace("+", string.Empty).Trim();

        var match = Regex.Match(text, @"(\d+)\.(\d+)\.(\d+)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (match.Success)
        {
            return decimal.Parse($"{match.Groups[1].Value}{match.Groups[2].Value},{match.Groups[3].Value}");
        }

        match = Regex.Match(text, @"([-.\d]+)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (match.Success)
        {
            var number = match.Groups[1].Value.Replace(".", ",");
            return decimal.Parse(number);
        }

        return null;
    }

    public MatchCollection Matches(string text, string pattern)
    {
        return Regex.Matches(text, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
    }

    public string MatchFirstGroup(string text, string pattern)
    {
        var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (match.Success)
        {
            var result = match.Groups[1].Value.Trim().Decode();
            return result;
        }

        return null;
    }

    public int? MatchInt(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        text = text.Replace(",", string.Empty).Replace(".", string.Empty);

        var match = Regex.Match(text, @"([.\d]+)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (match.Success)
        {
            return int.Parse(match.Groups[1].Value);
        }

        return null;
    }

    public string Replace(string text, string pattern, string replacement)
    {
        return Regex.Replace(text, pattern, replacement);
    }
}