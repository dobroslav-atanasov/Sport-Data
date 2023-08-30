namespace SportData.Data.Models.Cache;

using SportData.Data.Entities.OlympicGames;
using SportData.Services.Mapper.Interfaces;

public class VenueCacheModel : IMapFrom<Venue>
{
    public int Id { get; set; }

    public int Number { get; set; }
}