namespace SportData.Data.Models.Converters;

using SportData.Data.Entities.Enumerations;

public class RoundModel
{
    public string Name { get; set; }

    public RoundStatus Status { get; set; }

    public RoundStatus SubStatus { get; set; }

    public int Group { get; set; }

    public string Description { get; set; }
}