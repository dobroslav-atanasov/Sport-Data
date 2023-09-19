namespace SportData.Services.Data.SportDataDb.Interfaces;

using SportData.Data.Entities.OlympicGames;

public interface ISquadsService
{
    Task<Squad> AddOrUpdateAsync(Squad squad);
}