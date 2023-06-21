namespace SportData.Common.Extensions;

using System.Net;

public static class StringExtensions
{
    public static string Decode(this string text)
    {
        return WebUtility.HtmlDecode(text);
    }

    public static int GetMonthNumber(this string text)
    {
        var month = 0;

        switch (text.ToLower())
        {
            case "january":
            case "jan":
                month = 1; break;
            case "february":
            case "feb":
                month = 2; break;
            case "march":
            case "mar":
                month = 3; break;
            case "april":
            case "apr":
                month = 4; break;
            case "may":
                month = 5; break;
            case "june":
            case "jun":
                month = 6; break;
            case "july":
            case "jul":
                month = 7; break;
            case "august":
            case "aug":
                month = 8; break;
            case "september":
            case "sep":
                month = 9; break;
            case "october":
            case "oct":
                month = 10; break;
            case "november":
            case "nov":
                month = 11; break;
            case "december":
            case "dec":
                month = 12; break;
        }

        return month;
    }

    public static string UpperFirstChar(this string text)
    {
        var firstChar = text.Substring(0, 1);
        var otherString = text.Substring(1);
        var newString = firstChar.ToUpper() + otherString;
        return newString;
    }
}