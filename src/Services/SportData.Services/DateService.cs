namespace SportData.Services;

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

    public DateModel FindDateTime(string text, DateOptions options)
    {
        var dateModel = new DateModel();
        if (string.IsNullOrEmpty(text))
        {
            return dateModel;
        }

        if (options.IsTimeOnly)
        {
            text = "2:12.3";
            var formats = new string[] { "m\\:ss\\.ff", "m\\:ss\\.f" };
            var asd = TimeSpan.ParseExact(text, formats, null);


            var timeMatch = this.regExpService.Match(text, @"^(\d+)\s*:\s*(\d+)\.(\d+)$");
            if (timeMatch != null)
            {
                var minutes = int.Parse(timeMatch.Groups[1].Value.Trim());
                var seconds = int.Parse(timeMatch.Groups[2].Value.Trim());
                var milisecondsString = timeMatch.Groups[3].Value.Trim();
                var miliseconds = int.Parse(milisecondsString);
                if (milisecondsString.Length == 1)
                {
                    miliseconds *= 100;
                }
                else if (milisecondsString.Length == 2)
                {
                    miliseconds *= 10;
                }

                dateModel.Time = new TimeSpan(0, 0, minutes, seconds, miliseconds);
            }

            timeMatch = this.regExpService.Match(text, @"^(\d+)\.(\d+)$");
            if (timeMatch != null)
            {
                var seconds = int.Parse(timeMatch.Groups[1].Value.Trim());
                var milisecondsString = timeMatch.Groups[2].Value.Trim();
                var miliseconds = int.Parse(milisecondsString);
                if (milisecondsString.Length == 1)
                {
                    miliseconds *= 100;
                }
                else if (milisecondsString.Length == 2)
                {
                    miliseconds *= 10;
                }

                dateModel.Time = new TimeSpan(0, 0, 0, seconds, miliseconds);
            }
        }

        return dateModel;
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
}