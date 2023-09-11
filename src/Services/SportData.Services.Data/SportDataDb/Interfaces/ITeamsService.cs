namespace SportData.Services.Data.SportDataDb.Interfaces;

using SportData.Data.Entities.OlympicGames;

public interface ITeamsService
{
    Task<Team> AddOrUpdateAsync(Team team);

    Task<Team> GetAsync(int nocId, int eventId);

    Task<Team> GetAsync(string name, int nocId, int eventId);
}