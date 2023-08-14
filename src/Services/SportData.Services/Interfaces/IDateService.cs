namespace SportData.Services.Interfaces;

using SportData.Data.Models.Dates;

public interface IDateService
{
    Tuple<DateTime?, DateTime?> MatchStartAndEndDate(string text);

    DateTime? MatchDate(string text);

    DateTime? MatchDate(string text, int year);

    DateTime? MatchDateTime(string text);

    DateTime? MatchTime(string text);

    DateModel FindDateTime(string text, DateOptions options);
}