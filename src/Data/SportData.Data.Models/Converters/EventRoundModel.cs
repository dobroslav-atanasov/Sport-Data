namespace SportData.Data.Models.Converters;

using SportData.Data.Models.Dates;

public class EventRoundModel<TModel>
{
    public EventRoundModel()
    {
        this.Rounds = new List<TModel>();
    }

    public string Format { get; set; }

    public DateTimeModel Dates { get; set; }

    public string EventName { get; set; }

    public List<TModel> Rounds { get; set; }
}