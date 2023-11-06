namespace SportData.Data.Entities.SportData;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using global::SportData.Data.Common.Interfaces;
using global::SportData.Data.Common.Models;

[Table("Countries", Schema = "dbo")]
public class Country : BaseDeletableEntity<int>, IUpdatable<Country>
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

    public bool IsUpdated(Country other)
    {
        var isUpdated = false;

        if (Name != other.Name)
        {
            Name = other.Name;
            isUpdated = true;
        }

        if (OfficialName != other.OfficialName)
        {
            OfficialName = other.OfficialName;
            isUpdated = true;
        }

        if (IsIndependent != other.IsIndependent)
        {
            IsIndependent = other.IsIndependent;
            isUpdated = true;
        }

        if (Capital != other.Capital)
        {
            Capital = other.Capital;
            isUpdated = true;
        }

        if (Continent != other.Continent)
        {
            Continent = other.Continent;
            isUpdated = true;
        }

        if (MemberOf != other.MemberOf)
        {
            MemberOf = other.MemberOf;
            isUpdated = true;
        }

        if (Population != other.Population)
        {
            Population = other.Population;
            isUpdated = true;
        }

        if (TotalArea != other.TotalArea)
        {
            TotalArea = other.TotalArea;
            isUpdated = true;
        }

        if (HighestPointPlace != other.HighestPointPlace)
        {
            HighestPointPlace = other.HighestPointPlace;
            isUpdated = true;
        }

        if (HighestPoint != other.HighestPoint)
        {
            HighestPoint = other.HighestPoint;
            isUpdated = true;
        }

        if (LowestPointPlace != other.LowestPointPlace)
        {
            LowestPointPlace = other.LowestPointPlace;
            isUpdated = true;
        }

        if (LowestPoint != other.LowestPoint)
        {
            LowestPoint = other.LowestPoint;
            isUpdated = true;
        }

        if (Flag != other.Flag)
        {
            Flag = other.Flag;
            isUpdated = true;
        }

        return isUpdated;
    }
}