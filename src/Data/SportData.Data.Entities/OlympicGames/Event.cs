namespace SportData.Data.Entities.OlympicGames;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using SportData.Data.Common.Interfaces;
using SportData.Data.Common.Models;

[Table("Events", Schema = "og")]
public class Event : BaseEntity<int>, IDeletableEntity, IEquatable<Event>
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; }

    [Required]
    [MaxLength(200)]
    public string OriginalName { get; set; }

    [Required]
    [MaxLength(200)]
    public string NormalizedName { get; set; }

    public int DisciplineId { get; set; }
    public virtual Discipline Discipline { get; set; }

    public int GameId { get; set; }
    public virtual Game Game { get; set; }

    [Column(TypeName = "Date")]
    public DateTime? StartDate { get; set; }

    [Column(TypeName = "Date")]
    public DateTime? EndDate { get; set; }

    [Required]
    public bool IsTeamEvent { get; set; } = false;

    [MaxLength(200)]
    public string AdditionalInfo { get; set; }

    public int Athletes { get; set; }

    public int NOCs { get; set; }

    public string Format { get; set; }

    public string Description { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedOn { get; set; }

    public bool Equals(Event other)
    {
        if (other == null)
        {
            return false;
        }

        return this.Name == other.Name
            && this.OriginalName == other.OriginalName
            && this.NormalizedName == other.NormalizedName
            && this.DisciplineId == other.DisciplineId
            && this.GameId == other.GameId
            && this.StartDate == other.StartDate
            && this.EndDate == other.EndDate
            && this.IsTeamEvent == other.IsTeamEvent
            && this.AdditionalInfo == other.AdditionalInfo
            && this.Athletes == other.Athletes
            && this.NOCs == other.NOCs
            && this.Format == other.Format
            && this.Description == other.Description;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as Event);
    }

    public override int GetHashCode()
    {
        return $"{this.Name}-{this.DisciplineId}-{this.GameId}".GetHashCode();
    }
}