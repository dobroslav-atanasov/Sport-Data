namespace SportData.Data.Models.Cache;

using SportData.Data.Entities.OlympicGames;
using SportData.Services.Mapper.Interfaces;

public class CityCacheModel : IMapFrom<City>
{
    public int Id { get; set; }

    public string Name { get; set; }
}