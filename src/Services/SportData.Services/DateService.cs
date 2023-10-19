namespace SportData.Services;

using System.Globalization;

using SportData.Common.Extensions;
using SportData.Data.Models.Dates;
using SportData.Services.Interfaces;

public class DateService : IDateService
{
    private readonly IRegExpService regExpService;

    public DateService(IRegExpService regExpService)
    {
        this.regExpService = regExpService;
    }

    public DateTime? MatchDate(string text)
    {
        var match = this.regExpService.Match(text, @"(\d+)\s*(January|February|March|April|May|June|July|August|September|October|November|December)\s*(\d{4})");
        if (match != null)
        {
            var day = int.Parse(match.Groups[1].Value);
            var month = match.Groups[2].Value.GetMonthNumber();
            var year = int.Parse(match.Groups[3].Value);
            return new DateTime(year, month, day);
        }

        return null;
    }

    public DateTime? MatchDate(string text, int year)
    {
        if (year == 2020)
        {
            year += 1;
        }

        var match = this.regExpService.Match(text, @"(\d+)\s+(\w+)\s*(\d+)?(?::)?(\d+)?");
        if (match != null)
        {
            var day = int.Parse(match.Groups[1].Value.Trim());
            var month = match.Groups[2].Value.Trim().GetMonthNumber();
            var hour = string.IsNullOrEmpty(match.Groups[3].Value) ? 0 : int.Parse(match.Groups[3].Value.Trim());
            var minutes = string.IsNullOrEmpty(match.Groups[4].Value) ? 0 : int.Parse(match.Groups[4].Value.Trim());

            return new DateTime(year, month, day, hour, minutes, 0);
        }

        match = this.regExpService.Match(text, @"(\d+)\s*(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)\s*(\d+)?(?::)?(\d+)");
        if (match != null)
        {
            var day = int.Parse(match.Groups[1].Value.Trim());
            var month = match.Groups[2].Value.Trim().GetMonthNumber();
            var hour = string.IsNullOrEmpty(match.Groups[3].Value) ? 0 : int.Parse(match.Groups[3].Value.Trim());
            var minutes = string.IsNullOrEmpty(match.Groups[4].Value) ? 0 : int.Parse(match.Groups[4].Value.Trim());

            return new DateTime(year, month, day, hour, minutes, 0);
        }

        return null;
    }

    public DateTime? MatchDateTime(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        var match = this.regExpService.Match(text, @"(\d+)\s*(January|February|March|April|May|June|July|August|September|October|November|December)\s*(\d{4})\s*(?:-|—)\s*(\d+)\s*:\s*(\d+)");
        if (match != null)
        {
            return new DateTime(int.Parse(match.Groups[3].Value), match.Groups[2].Value.ToString().GetMonthNumber(), int.Parse(match.Groups[1].Value), int.Parse(match.Groups[4].Value), int.Parse(match.Groups[5].Value), 0);
        }

        return null;
    }

    public Tuple<DateTime?, DateTime?> MatchStartAndEndDate(string text)
    {
        text = text.Replace("–", string.Empty);
        var match = this.regExpService.Match(text, @"(\d+)\s*([A-z]+)?\s*(\d+)?\s*([A-z]+)?\s*(\d{4})");
        if (match != null)
        {
            DateTime? startDate = null;
            DateTime? endDate = null;
            if (match.Groups[1].Value.Length > 0 && match.Groups[2].Value.Length > 0 && match.Groups[3].Value.Length > 0 && match.Groups[4].Value.Length > 0)
            {
                startDate = DateTime.ParseExact($"{match.Groups[1].Value}-{match.Groups[2].Value.GetMonthNumber()}-{match.Groups[5].Value}", "d-M-yyyy", null);
                endDate = DateTime.ParseExact($"{match.Groups[3].Value}-{match.Groups[4].Value.GetMonthNumber()}-{match.Groups[5].Value}", "d-M-yyyy", null);
            }
            else if (match.Groups[1].Value.Length > 0 && match.Groups[3].Value.Length > 0 && match.Groups[4].Value.Length > 0)
            {
                startDate = DateTime.ParseExact($"{match.Groups[1].Value}-{match.Groups[4].Value.GetMonthNumber()}-{match.Groups[5].Value}", "d-M-yyyy", null);
                endDate = DateTime.ParseExact($"{match.Groups[3].Value}-{match.Groups[4].Value.GetMonthNumber()}-{match.Groups[5].Value}", "d-M-yyyy", null);
            }
            else if (match.Groups[1].Value.Length > 0 && match.Groups[2].Value.Length > 0)
            {
                startDate = DateTime.ParseExact($"{match.Groups[1].Value}-{match.Groups[2].Value.GetMonthNumber()}-{match.Groups[5].Value}", "d-M-yyyy", null);
            }

            return new Tuple<DateTime?, DateTime?>(startDate, endDate);
        }

        return null;
    }

    public DateTime? MatchTime(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        var match = this.regExpService.Match(text, @"(\d+)\s*:\s*(\d+)\.(\d+)");
        if (match != null)
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

        match = this.regExpService.Match(text, @"(\d+)\s*:\s*(\d+)");
        if (match != null)
        {
            return new DateTime(1, 1, 1, 0, int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value));
        }

        match = this.regExpService.Match(text, @"(\d+)\.(\d+)");
        if (match != null)
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

    public DateTimeModel ParseDate(string text, int year = 0)
    {
        var dateModel = new DateTimeModel();
        if (string.IsNullOrEmpty(text))
        {
            return dateModel;
        }

        if (year == 2020)
        {
            year = 2021;
        }

        var patternsDictionary = new Dictionary<int, string>
        {
            { 1, @"(\d+)\s*(January|February|March|April|May|June|July|August|September|October|November|December)\s*–\s*(\d+)\s*(January|February|March|April|May|June|July|August|September|October|November|December)\s*(\d{4})" },
            { 2, @"(\d+)\s*–\s*(\d+)\s*(January|February|March|April|May|June|July|August|September|October|November|December)\s*(\d{4})\s*(?:-|—|—)\s*(\d+)\s*:\s*(\d+)\s*(?:-|—|—)\s*(\d+)\s*:\s*(\d+)" },
            { 3, @"(\d+)\s*–\s*(\d+)\s*(January|February|March|April|May|June|July|August|September|October|November|December)\s*(\d{4})\s*(?:-|—|—)\s*(\d+)\s*:\s*(\d+)" },
            { 4, @"(\d+)\s*–\s*(\d+)\s*(January|February|March|April|May|June|July|August|September|October|November|December)\s*(\d{4})" },
            { 5, @"(\d+)\s*(January|February|March|April|May|June|July|August|September|October|November|December)\s*(\d{4})\s*(?:-|—|—)\s*(\d+)\s*:\s*(\d+)" },
            { 6, @"(\d+)\s*(January|February|March|April|May|June|July|August|September|October|November|December)\s*(\d{4})" },
            { 7, @"(\d+)\s*(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)\s*(\d+)(?::)(\d+)" },
            { 8, @"(\d+)\s*(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)" },
        };

        var formats = new string[] { "dd-MM-yyyy", "dd-M-yyyy", "d-MM-yyyy", "d-M-yyyy", "dd-MM-yyyy HH:mm", "dd-M-yyyy HH:mm", "d-MM-yyyy HH:mm", "d-M-yyyy HH:mm",
            "dd-MM-yyyy H:mm", "dd-M-yyyy H:mm", "d-MM-yyyy H:mm", "d-M-yyyy H:mm"};
        foreach (var kvp in patternsDictionary)
        {
            var match = this.regExpService.Match(text, kvp.Value);
            if (match != null)
            {
                var startDate = string.Empty;
                var endDate = string.Empty;
                switch (kvp.Key)
                {
                    case 1:
                        startDate = $"{match.Groups[1].Value}-{match.Groups[2].Value.GetMonthNumber()}-{match.Groups[5].Value}";
                        endDate = $"{match.Groups[3].Value}-{match.Groups[4].Value.GetMonthNumber()}-{match.Groups[5].Value}";
                        break;
                    case 2:
                        startDate = $"{match.Groups[1].Value}-{match.Groups[3].Value.GetMonthNumber()}-{match.Groups[4].Value} {match.Groups[5].Value}:{match.Groups[6].Value}";
                        endDate = $"{match.Groups[2].Value}-{match.Groups[3].Value.GetMonthNumber()}-{match.Groups[4].Value} {match.Groups[7].Value}:{match.Groups[8].Value}";
                        break;
                    case 3:
                        startDate = $"{match.Groups[1].Value}-{match.Groups[3].Value.GetMonthNumber()}-{match.Groups[4].Value} {match.Groups[5].Value}:{match.Groups[6].Value}";
                        endDate = $"{match.Groups[2].Value}-{match.Groups[3].Value.GetMonthNumber()}-{match.Groups[4].Value}";
                        break;
                    case 4:
                        startDate = $"{match.Groups[1].Value}-{match.Groups[3].Value.GetMonthNumber()}-{match.Groups[4].Value}";
                        endDate = $"{match.Groups[2].Value}-{match.Groups[3].Value.GetMonthNumber()}-{match.Groups[4].Value}";
                        break;
                    case 5:
                        startDate = $"{match.Groups[1].Value}-{match.Groups[2].Value.GetMonthNumber()}-{match.Groups[3].Value} {match.Groups[4].Value}:{match.Groups[5].Value}";
                        break;
                    case 6:
                        startDate = $"{match.Groups[1].Value}-{match.Groups[2].Value.GetMonthNumber()}-{match.Groups[3].Value}";
                        break;
                    case 7:
                        startDate = $"{match.Groups[1].Value}-{match.Groups[2].Value.GetMonthNumber()}-{year} {match.Groups[3].Value}:{match.Groups[4].Value}";
                        break;
                    case 8:
                        startDate = $"{match.Groups[1].Value}-{match.Groups[2].Value.GetMonthNumber()}-{year}";
                        break;
                }

                if (DateTime.TryParseExact(startDate, formats, null, DateTimeStyles.None, out DateTime startDateResult)
                    && DateTime.TryParseExact(endDate, formats, null, DateTimeStyles.None, out DateTime endDateResult))
                {
                    dateModel.From = startDateResult;
                    dateModel.To = endDateResult;
                    break;
                }

                if (DateTime.TryParseExact(startDate, formats, null, DateTimeStyles.None, out DateTime startDateTimeResult))
                {
                    dateModel.From = startDateTimeResult;
                    break;
                }
            }
        }

        return dateModel;
    }

    public TimeSpan? ParseTime(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        var patterns = new List<string> { @"(\d+)-(\d+)\s*:\s*(\d+)\.(\d+)", @"(\d+)-(\d+)\s*:\s*(\d+)", @"(\d+)\s*:\s*(\d+)\.(\d+)", @"(\d+)\s*:\s*(\d+)", @"(\d+)\.(\d+)", @"(\d+)" };
        foreach (var pattern in patterns)
        {
            var match = this.regExpService.Match(text, pattern);
            if (match != null)
            {
                var formats = new string[] {
                    "h\\-mm\\:ss\\.fff",
                    "h\\-mm\\:ss\\.ff",
                    "h\\-mm\\:ss\\.f",
                    "h\\-mm\\:ss",
                    "h\\-mm\\:s\\.fff",
                    "h\\-mm\\:s\\.ff",
                    "h\\-mm\\:s\\.f",
                    "h\\-mm\\:s",
                    "h\\-m\\:ss\\.fff",
                    "h\\-m\\:ss\\.ff",
                    "h\\-m\\:ss\\.f",
                    "h\\-m\\:ss",
                    "h\\-m\\:s\\.fff",
                    "h\\-m\\:s\\.ff",
                    "h\\-m\\:s\\.f",
                    "h\\-m\\:s",
                    "mm\\:ss", "mm\\:s", "m\\:ss", "m\\:s", "mm\\:ss\\.fff", "mm\\:ss\\.ff", "mm\\:ss\\.f", "m\\:ss\\.fff",
                    "m\\:ss\\.f", "m\\:ss\\.f", "m\\:s\\.fff", "m\\:s\\.ff", "m\\:s\\.f", "ss\\.fff", "ss\\.ff", "ss\\.f", "s\\.fff", "s\\.ff", "s\\.f", "ss", "s" };
                if (TimeSpan.TryParseExact(match.Groups[0].Value, formats, null, out TimeSpan timeResult))
                {
                    return timeResult;
                }
            }
        }

        return null;
    }
}