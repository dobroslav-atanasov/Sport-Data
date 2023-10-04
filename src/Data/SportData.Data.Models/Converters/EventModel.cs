namespace SportData.Data.Models.Converters;

public class EventModel
{
    public string OriginalName { get; set; }

    public string Name { get; set; }

    public string NormalizedName { get; set; }

    public string AdditionalInfo { get; set; }

    public int GameId { get; set; }

    public int GameYear { get; set; }

    public int DisciplineId { get; set; }

    public string DisciplineName { get; set; }
}