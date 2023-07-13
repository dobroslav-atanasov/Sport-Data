namespace SportData.Data.Entities.OlympicGames;

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using SportData.Data.Common.Interfaces;
using SportData.Data.Common.Models;

[Table("Disciplines", Schema = "og")]
public class Discipline : BaseEntity<int>, IDeletableEntity, IEquatable<Discipline>
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [Required]
    [StringLength(3)]
    public string Code { get; set; }

    public int SportId { get; set; }
    public virtual Sport Sport { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedOn { get; set; }

    public bool Equals(Discipline other)
    {
        if (other == null)
        {
            return false;
        }

        return this.Name == other.Name
            && this.Code == other.Code
            && this.SportId == other.SportId;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as Discipline);
    }

    public override int GetHashCode()
    {
        return $"{this.Name}-{this.Code}-{this.SportId}".GetHashCode();
    }
}