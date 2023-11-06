namespace SportData.Data.Entities.OlympicGames;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using global::SportData.Data.Common.Interfaces;
using global::SportData.Data.Common.Models;

[Table("NOCs", Schema = "dbo")]
public class NOC : BaseDeletableEntity<int>, IUpdatable<NOC>
{
    public NOC()
    {
        this.Nationalities = new HashSet<Nationality>();
        this.Participants = new HashSet<Participant>();
        this.Teams = new HashSet<Team>();
        this.Cities = new HashSet<City>();
    }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; }

    [Required]
    [StringLength(3)]
    public string Code { get; set; }

    public int? CountryId { get; set; }
    public virtual Country Country { get; set; }

    [MaxLength(500)]
    public string Title { get; set; }

    [StringLength(20)]
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

    public virtual ICollection<Nationality> Nationalities { get; set; }

    public virtual ICollection<Participant> Participants { get; set; }

    public virtual ICollection<Team> Teams { get; set; }

    public virtual ICollection<City> Cities { get; set; }

    public bool IsUpdated(NOC other)
    {
        var isUpdated = false;

        if (this.Name != other.Name)
        {
            this.Name = other.Name;
            isUpdated = true;
        }

        if (this.Title != other.Title)
        {
            this.Title = other.Title;
            isUpdated = true;
        }

        if (this.Abbreviation != other.Abbreviation)
        {
            this.Abbreviation = other.Abbreviation;
            isUpdated = true;
        }

        if (this.FoundedYear != other.FoundedYear)
        {
            this.FoundedYear = other.FoundedYear;
            isUpdated = true;
        }

        if (this.RecognationYear != other.RecognationYear)
        {
            this.RecognationYear = other.RecognationYear;
            isUpdated = true;
        }

        if (this.DisbandedYear != other.DisbandedYear)
        {
            this.DisbandedYear = other.DisbandedYear;
            isUpdated = true;
        }

        if (this.RelatedNOCCode != other.RelatedNOCCode)
        {
            this.RelatedNOCCode = other.RelatedNOCCode;
            isUpdated = true;
        }

        if (this.CountryDescription != other.CountryDescription)
        {
            this.CountryDescription = other.CountryDescription;
            isUpdated = true;
        }

        if (this.NOCDescription != other.NOCDescription)
        {
            this.NOCDescription = other.NOCDescription;
            isUpdated = true;
        }

        if (this.CountryFlag != other.CountryFlag)
        {
            this.CountryFlag = other.CountryFlag;
            isUpdated = true;
        }

        if (this.NOCFlag != other.NOCFlag)
        {
            this.NOCFlag = other.NOCFlag;
            isUpdated = true;
        }

        return isUpdated;
    }
}