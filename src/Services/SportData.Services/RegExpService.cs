namespace SportData.Services;

using System;
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

        text = text.Replace(",", string.Empty);

        var match = Regex.Match(text, @"(\d+)\.(\d+)\.(\d+)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (match.Success)
        {
            return double.Parse($"{match.Groups[1].Value}{match.Groups[2].Value},{match.Groups[3].Value}");
        }

        match = Regex.Match(text, @"([.\d]+)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (match.Success)
        {
            var number = match.Groups[1].Value.Replace(".", ",");
            return double.Parse(number);
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

    public DateTime? MatchTime(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        var match = Regex.Match(text, @"(\d+)\s*:\s*(\d+)\.(\d+)", RegexOptions.Singleline);
        if (match.Success)
        {
            var minutes = int.Parse(match.Groups[1].Value.Trim());
            var seconds = int.Parse(match.Groups[2].Value.Trim());
            var milisecondsString = match.Groups[3].Value.Trim();
            var miliseconds = int.Parse(milisecondsString);
            if (milisecondsString.Length == 1)
            {
                miliseconds *= 100;
            }
            else if (milisecondsString.Length == 2)
            {
                miliseconds *= 10;
            }

            return new DateTime(1, 1, 1, 0, minutes, seconds, miliseconds);
        }

        match = Regex.Match(text, @"(\d+)\s*:\s*(\d+)", RegexOptions.Singleline);
        if (match.Success)
        {
            return new DateTime(1, 1, 1, 1, int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value));
        }

        match = Regex.Match(text, @"(\d+)\.(\d+)", RegexOptions.Singleline);
        if (match.Success)
        {
            var seconds = int.Parse(match.Groups[1].Value.Trim());
            var milisecondsString = match.Groups[2].Value.Trim();
            var miliseconds = int.Parse(milisecondsString);
            if (milisecondsString.Length == 1)
            {
                miliseconds *= 100;
            }
            else if (milisecondsString.Length == 2)
            {
                miliseconds *= 10;
            }

            return new DateTime(1, 1, 1, 0, 0, seconds, miliseconds);
        }

        return null;
    }

    public string Replace(string text, string pattern, string replacement)
    {
        return Regex.Replace(text, pattern, replacement);
    }
}