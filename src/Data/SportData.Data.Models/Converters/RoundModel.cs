namespace SportData.Data.Models.Converters;

using SportData.Data.Entities.OlympicGames.Enumerations;

public class RoundModel
{
    public string Name { get; set; }

    public RoundType Type { get; set; }

    public RoundType SubType { get; set; }

    public int Group { get; set; }

    public string Description { get; set; }
}