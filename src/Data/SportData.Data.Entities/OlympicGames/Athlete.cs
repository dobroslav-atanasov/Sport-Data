namespace SportData.Data.Entities.OlympicGames;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using SportData.Data.Common.Interfaces;
using SportData.Data.Common.Models;
using SportData.Data.Entities.Enumerations;

[Table("Athletes", Schema = "og")]
public class Athlete : BaseEntity<Guid>, IDeletableEntity, IEquatable<Athlete>
{
    public Athlete()
    {
        this.Nationalities = new HashSet<Nationality>();
        this.Participants = new HashSet<Participant>();
        this.Teams = new HashSet<Team>();
    }

    [Required]
    public int Number { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; }

    [Required]
    [MaxLength(200)]
    public string EnglishName { get; set; }

    [MaxLength(200)]
    public string FullName { get; set; }

    public GenderType Gender { get; set; }

    public AthleteType Type { get; set; }

    [MaxLength(100)]
    public string Nationality { get; set; }

    [Column(TypeName = "Date")]
    public DateTime? BirthDate { get; set; }

    [Column(TypeName = "Date")]
    public DateTime? DiedDate { get; set; }

    [MaxLength(100)]
    public string BirthPlace { get; set; }

    [MaxLength(100)]
    public string DiedPlace { get; set; }

    public int? Height { get; set; }

    public int? Weight { get; set; }

    [MaxLength(200)]
    public string Association { get; set; }

    public string Description { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedOn { get; set; }

    public virtual ICollection<Nationality> Nationalities { get; set; }

    public virtual ICollection<Participant> Participants { get; set; }

    public virtual ICollection<Team> Teams { get; set; }

    public bool Equals(Athlete other)
    {
        if (other == null)
        {
            return false;
        }

        return this.Name == other.Name
            && this.Number == other.Number
            && this.EnglishName == other.EnglishName
            && this.FullName == other.FullName
            && this.Gender == other.Gender
            && this.Type == other.Type
            && this.Nationality == other.Nationality
            && this.BirthDate == other.BirthDate
            && this.DiedDate == other.DiedDate
            && this.BirthPlace == other.BirthPlace
            && this.DiedPlace == other.DiedPlace
            && this.Height == other.Height
            && this.Weight == other.Weight
            && this.Association == other.Association
            && this.Description == other.Description;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as Athlete);
    }

    public override int GetHashCode()
    {
        return $"{this.Name}-{this.Number}-{this.EnglishName}".GetHashCode();
    }
}