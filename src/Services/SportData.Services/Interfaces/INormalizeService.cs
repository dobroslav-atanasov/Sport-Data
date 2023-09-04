namespace SportData.Services.Interfaces;

using SportData.Data.Entities.Enumerations;
using SportData.Data.Models.OlympicGames.ArtisticGymnastics;
using SportData.Data.Models.OlympicGames.ArtisticSwimming;

public interface INormalizeService
{
    string MapOlympicGamesCountriesAndWorldCountries(string code);

    string NormalizeHostCityName(string hostCity);

    string NormalizeEventName(string name, int gameYear, string disciplineName);

    string ReplaceNonEnglishLetters(string name);

    AthleteType MapAthleteType(string text);

    string MapCityNameAndYearToNOCCode(string cityName, int year);

    RoundType MapRoundType(string text);

    GAREventType MapArtisticGymnasticsEvent(string text);

    SWAEventType MapArtisticSwimmingEvent(string name);
}