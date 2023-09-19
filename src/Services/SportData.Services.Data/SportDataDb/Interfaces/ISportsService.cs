namespace SportData.Services.Data.SportDataDb.Interfaces;

using SportData.Data.Entities.OlympicGames;

public interface ISportsService
{
    Task<Sport> AddOrUpdateAsync(Sport sport);
}