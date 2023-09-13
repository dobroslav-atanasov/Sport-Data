namespace SportData.Services.Data.SportDataDb;

using System.Collections.Generic;
using System.Threading.Tasks;

using SportData.Data.Entities.OlympicGames;
using SportData.Data.Models.Cache;
using SportData.Data.Repositories;
using SportData.Services.Data.SportDataDb.Interfaces;
using SportData.Services.Mapper.Extensions;

public class GamesService : IGamesService
{
    private readonly SportDataRepository<Game> repository;

    public GamesService(SportDataRepository<Game> repository)
    {
        this.repository = repository;
    }

    public async Task<Game> AddOrUpdateAsync(Game game)
    {
        var dbGame = await repository.GetAsync(x => x.Year == game.Year && x.Type == game.Type);
        if (dbGame == null)
        {
            await repository.AddAsync(game);
            await repository.SaveChangesAsync();
        }
        else
        {
            var isUpdated = dbGame.IsUpdated(game);
            if (isUpdated)
            {
                repository.Update(dbGame);
                await repository.SaveChangesAsync();
            }

            game = dbGame;
        }

        return game;
    }

    public ICollection<GameCacheModel> GetGameCacheModels()
    {
        var models = this.repository
            .AllAsNoTracking()
            .To<GameCacheModel>()
            .ToList();

        return models;
    }
}