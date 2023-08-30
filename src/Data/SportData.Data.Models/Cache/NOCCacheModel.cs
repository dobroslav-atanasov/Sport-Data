namespace SportData.Data.Models.Cache;

using SportData.Data.Entities.OlympicGames;
using SportData.Services.Mapper.Interfaces;

public class NOCCacheModel : IMapFrom<NOC>
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Code { get; set; }
}