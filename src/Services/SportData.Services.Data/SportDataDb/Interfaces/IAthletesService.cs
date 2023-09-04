namespace SportData.Services.Data.SportDataDb.Interfaces;

using SportData.Data.Entities.OlympicGames;

public interface IAthletesService
{
    Task<Athlete> AddOrUpdateAsync(Athlete athlete);

    Task<Athlete> GetAsync(int number);
}