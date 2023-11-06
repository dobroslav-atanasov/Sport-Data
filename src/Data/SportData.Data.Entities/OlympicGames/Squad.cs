namespace SportData.Data.Entities.OlympicGames;

using System.ComponentModel.DataAnnotations.Schema;

using global::SportData.Data.Common.Interfaces;

[Table("Squads", Schema = "dbo")]
public class Squad : ICheckableEntity, IDeletableEntity
{
    public Guid ParticipantId { get; set; }
    public virtual Participant Participant { get; set; }

    public Guid TeamId { get; set; }
    public virtual Team Team { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedOn { get; set; }
}