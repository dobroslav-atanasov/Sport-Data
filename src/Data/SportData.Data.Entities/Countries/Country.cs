namespace SportData.Data.Entities.Countries;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using SportData.Data.Common.Interfaces;
using SportData.Data.Common.Models;
using SportData.Data.Entities.OlympicGames;

[Table("Countries", Schema = "countries")]
public class Country : BaseDeletableEntity<int>, ICheckableEntity, IEquatable<Country>
{
    public Country()
    {
        this.Cities = new HashSet<City>();
        this.NOCs = new HashSet<NOC>();
    }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [Required]
    [MaxLength(100)]
    public string OfficialName { get; set; }

    [Required]
    public bool IsIndependent { get; set; } = false;

    [StringLength(2)]
    public string TwoDigitsCode { get; set; }

    [Required]
    [StringLength(10)]
    public string Code { get; set; }

    [MaxLength(100)]
    public string Capital { get; set; }

    [MaxLength(50)]
    public string Continent { get; set; }

    [MaxLength(200)]
    public string MemberOf { get; set; }

    [Required]
    public int Population { get; set; }

    [Required]
    public int TotalArea { get; set; }

    [MaxLength(500)]
    public string HighestPointPlace { get; set; }

    public int? HighestPoint { get; set; }

    [MaxLength(500)]
    public string LowestPointPlace { get; set; }

    public int? LowestPoint { get; set; }

    public byte[] Flag { get; set; }

    public virtual ICollection<City> Cities { get; set; }

    public virtual ICollection<NOC> NOCs { get; set; }

    public bool Equals(Country other)
    {
        if (other == null)
        {
            return false;
        }

        return this.Name == other.Name
            && this.OfficialName == other.OfficialName
            && this.IsDeleted == other.IsDeleted
            && this.TwoDigitsCode == other.TwoDigitsCode
            && this.Code == other.Code
            && this.Capital == other.Capital
            && this.Continent == other.Continent
            && this.MemberOf == other.MemberOf
            && this.Population == other.Population
            && this.TotalArea == other.TotalArea
            && this.HighestPoint == other.HighestPoint
            && this.HighestPointPlace == other.HighestPointPlace
            && this.LowestPointPlace == other.LowestPointPlace
            && this.LowestPoint == other.LowestPoint
            && this.Flag.Length == other.Flag.Length;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as Country);
    }

    public override int GetHashCode()
    {
        return $"{this.Name}-{this.TwoDigitsCode}-{this.Code}".GetHashCode();
    }
}