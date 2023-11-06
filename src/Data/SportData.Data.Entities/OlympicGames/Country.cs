namespace SportData.Data.Entities.OlympicGames;

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

    [StringLength(2)]
    public string TwoDigitsCode { get; set; }

    [Required]
    [StringLength(10)]
    public string Code { get; set; }

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

        if (Flag != other.Flag)
        {
            Flag = other.Flag;
            isUpdated = true;
        }

        return isUpdated;
    }
}