namespace SportData.Data.Entities.OlympicGames;

using System.ComponentModel.DataAnnotations.Schema;

using SportData.Data.Common.Interfaces;
using SportData.Data.Common.Models;
using SportData.Data.Entities.Enumerations;

[Table("Participants", Schema = "og")]
public class Participant : BaseDeletableEntity<Guid>, IUpdatable<Participant>
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

    public virtual ICollection<Squad> Squads { get; set; }

    public bool IsUpdated(Participant other)
    {
        var isUpdated = false;

        if (this.AgeYears != other.AgeYears)
        {
            this.AgeYears = other.AgeYears;
            isUpdated = true;
        }

        if (this.AgeDays != other.AgeDays)
        {
            this.AgeDays = other.AgeDays;
            isUpdated = true;
        }

        if (this.Medal != other.Medal)
        {
            this.Medal = other.Medal;
            isUpdated = true;
        }

        if (this.FinishStatus != other.FinishStatus)
        {
            this.FinishStatus = other.FinishStatus;
            isUpdated = true;
        }

        if (this.IsCoach != other.IsCoach)
        {
            this.IsCoach = other.IsCoach;
            isUpdated = true;
        }

        return isUpdated;
    }
}