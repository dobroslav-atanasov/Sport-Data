namespace SportData.Data.Entities.OlympicGames;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using SportData.Data.Common.Interfaces;
using SportData.Data.Common.Models;
using SportData.Data.Entities.Countries;

[Table("NOCs", Schema = "og")]
public class NOC : BaseEntity<int>, IDeletableEntity, IEquatable<NOC>
{
    public NOC()
    {
        this.Nationalities = new HashSet<Nationality>();
        this.Participants = new HashSet<Participant>();
        this.Teams = new HashSet<Team>();
    }

    // check olympic.org data
    [Required]
    [MaxLength(50)]
    public string Name { get; set; }

    [Required]
    [StringLength(3)]
    public string Code { get; set; }

    public int CountryId { get; set; }
    public virtual Country Country { get; set; }

    [MaxLength(500)]
    public string Title { get; set; }

    [StringLength(3)]
    public string Abbreviation { get; set; }

    public int? FoundedYear { get; set; }

    public int? RecognationYear { get; set; }

    public int? DisbandedYear { get; set; }

    [StringLength(3)]
    public string RelatedNOCCode { get; set; }

    [MaxLength(10000)]
    public string CountryDescription { get; set; }

    [MaxLength(10000)]
    public string NOCDescription { get; set; }

    public byte[] CountryFlag { get; set; }

    public byte[] NOCFlag { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedOn { get; set; }

    public virtual ICollection<Nationality> Nationalities { get; set; }

    public virtual ICollection<Participant> Participants { get; set; }

    public virtual ICollection<Team> Teams { get; set; }

    public bool Equals(NOC other)
    {
        if (other == null)
        {
            return false;
        }

        return this.Name == other.Name
            && this.Code == other.Code
            && this.CountryId == other.CountryId
            && this.Title == other.Title
            && this.Abbreviation == other.Abbreviation
            && this.FoundedYear == other.FoundedYear
            && this.RecognationYear == other.RecognationYear
            && this.DisbandedYear == other.DisbandedYear
            && this.RelatedNOCCode == other.RelatedNOCCode
            && this.CountryDescription == other.CountryDescription
            && this.NOCDescription == other.NOCDescription
            && this.CountryFlag?.Length == other.CountryFlag?.Length
            && this.NOCFlag?.Length == other.NOCFlag?.Length;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as NOC);
    }

    public override int GetHashCode()
    {
        return $"{this.Name}-{this.CountryId}-{this.Code}-{this.Title}-{this.Abbreviation}".GetHashCode();
    }
}