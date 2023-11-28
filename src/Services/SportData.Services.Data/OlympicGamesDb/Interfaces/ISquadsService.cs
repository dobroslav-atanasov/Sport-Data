namespace SportData.Services.Data.OlympicGamesDb.Interfaces;

using SportData.Data.Entities.OlympicGames;

public interface ISquadsService
{
    Task<Squad> AddOrUpdateAsync(Squad squad);
}