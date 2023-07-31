namespace SportData.Data.Entities.OlympicGames;

using System.ComponentModel.DataAnnotations.Schema;

using SportData.Data.Common.Interfaces;
using SportData.Data.Common.Models;
using SportData.Data.Entities.Enumerations;

[Table("Participants", Schema = "og")]
public class Participant : BaseEntity<Guid>, IDeletableEntity, IEquatable<Participant>
{
    public Participant()
    {
        this.Squads = new HashSet<Squad>();
    }

    public Guid AthleteId { get; set; }
    public virtual Athlete Athlete { get; set; }

    public int EventId { get; set; }
    public virtual Event Event { get; set; }

    public int NOCId { get; set; }
    public virtual NOC NOC { get; set; }

    public int? AgeYears { get; set; }

    public int? AgeDays { get; set; }

    public MedalType Medal { get; set; } = MedalType.None;

    public FinishStatus FinishStatus { get; set; }

    public int Number { get; set; }

    public bool IsCoach { get; set; } = false;

    public bool IsDeleted { get; set; }

    public DateTime? DeletedOn { get; set; }

    public virtual ICollection<Squad> Squads { get; set; }

    public bool Equals(Participant other)
    {
        if (other == null)
        {
            return false;
        }

        return this.AthleteId == other.AthleteId
            && this.EventId == other.EventId
            && this.NOCId == other.NOCId
            && this.AgeYears == other.AgeYears
            && this.AgeDays == other.AgeDays
            && this.Medal == other.Medal
            && this.FinishStatus == other.FinishStatus
            && this.Number == other.Number
            && this.IsCoach == other.IsCoach;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as Participant);
    }

    public override int GetHashCode()
    {
        return $"{this.AthleteId}-{this.EventId}-{this.NOCId}-{this.Number}".GetHashCode();
    }
}