namespace SportData.Services.Interfaces;

using SportData.Data.Entities.Enumerations;
using SportData.Data.Models.Enumerations;
using SportData.Data.Models.OlympicGames.ArtisticGymnastics;
using SportData.Data.Models.OlympicGames.Athletics;

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

    GenderType MapGenderType(string text);

    ATHEventGroup MapAthleticsEventGroup(string text);

    HeatType MapHeats(string text);

    GroupType MapGroupType(string text);

    string CleanEventName(string text);
    string MapAthleticsCombinedEvents(string text);
}