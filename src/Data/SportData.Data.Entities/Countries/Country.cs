namespace SportData.Data.Entities.Countries;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using SportData.Data.Common.Interfaces;
using SportData.Data.Common.Models;

[Table("Countries", Schema = "countries")]
public class Country : BaseDeletableEntity<int>, ICheckableEntity, IUpdatable<Country>
{
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

    public bool Update(Country other)
    {
        var isUpdated = false;

        if (this.Name != other.Name)
        {
            this.Name = other.Name;
            isUpdated = true;
        }

        if (this.OfficialName != other.OfficialName)
        {
            this.OfficialName = other.OfficialName;
            isUpdated = true;
        }

        if (this.IsIndependent != other.IsIndependent)
        {
            this.IsIndependent = other.IsIndependent;
            isUpdated = true;
        }

        if (this.Capital != other.Capital)
        {
            this.Capital = other.Capital;
            isUpdated = true;
        }

        if (this.Continent != other.Continent)
        {
            this.Continent = other.Continent;
            isUpdated = true;
        }

        if (this.MemberOf != other.MemberOf)
        {
            this.MemberOf = other.MemberOf;
            isUpdated = true;
        }

        if (this.Population != other.Population)
        {
            this.Population = other.Population;
            isUpdated = true;
        }

        if (this.TotalArea != other.TotalArea)
        {
            this.TotalArea = other.TotalArea;
            isUpdated = true;
        }

        if (this.HighestPointPlace != other.HighestPointPlace)
        {
            this.HighestPointPlace = other.HighestPointPlace;
            isUpdated = true;
        }

        if (this.HighestPoint != other.HighestPoint)
        {
            this.HighestPoint = other.HighestPoint;
            isUpdated = true;
        }

        if (this.LowestPointPlace != other.LowestPointPlace)
        {
            this.LowestPointPlace = other.LowestPointPlace;
            isUpdated = true;
        }

        if (this.LowestPoint != other.LowestPoint)
        {
            this.LowestPoint = other.LowestPoint;
            isUpdated = true;
        }

        if (this.Flag != other.Flag)
        {
            this.Flag = other.Flag;
            isUpdated = true;
        }

        return isUpdated;
    }
}