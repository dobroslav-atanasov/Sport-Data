namespace SportData.Data.Entities.OlympicGames;

using System.ComponentModel.DataAnnotations.Schema;

using SportData.Data.Common.Interfaces;

[Table("EventsVenues", Schema = "og")]
public class EventVenue : ICheckableEntity, IDeletableEntity
{
    public int EventId { get; set; }
    public virtual Event Event { get; set; }

    public int VenueId { get; set; }
    public virtual Venue Venue { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedOn { get; set; }
}