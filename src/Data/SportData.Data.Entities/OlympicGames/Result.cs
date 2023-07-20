namespace SportData.Data.Entities.OlympicGames;

using SportData.Data.Common.Interfaces;
using SportData.Data.Common.Models;

public class Result : BaseEntity<Guid>, IDeletableEntity, IEquatable<Result>
{
    public int EventId { get; set; }
    public virtual Event Event { get; set; }

    public Guid ParticipantId { get; set; }
    public virtual Participant Participant { get; set; }

    public Guid TeamId { get; set; }
    public virtual Team Team { get; set; }

    public string Json { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedOn { get; set; }

    public bool Equals(Result other)
    {
        if (other == null)
        {
            return false;
        }

        return this.EventId == other.EventId
            && this.ParticipantId == other.ParticipantId
            && this.TeamId == other.TeamId
            && this.Json == other.Json;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as Result);
    }

    public override int GetHashCode()
    {
        return $"{this.EventId}-{(this.ParticipantId != Guid.Empty ? this.ParticipantId : this.TeamId)}".GetHashCode();
    }
}