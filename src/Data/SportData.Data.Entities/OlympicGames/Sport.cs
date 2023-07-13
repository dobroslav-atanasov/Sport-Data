namespace SportData.Data.Entities.OlympicGames;

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using SportData.Data.Common.Interfaces;
using SportData.Data.Common.Models;
using SportData.Data.Entities.Enumerations;

[Table("Sports", Schema = "og")]
public class Sport : BaseEntity<int>, IDeletableEntity, IEquatable<Sport>
{
    public Sport()
    {
        this.Disciplines = new HashSet<Discipline>();
    }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; }

    [Required]
    [StringLength(2)]
    public string Code { get; set; }

    [Required]
    public OlympicGameType Type { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedOn { get; set; }

    public virtual ICollection<Discipline> Disciplines { get; set; }

    public bool Equals(Sport other)
    {
        if (other == null)
        {
            return false;
        }

        return this.Name == other.Name
            && this.Code == other.Code
            && this.Type == other.Type;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as Sport);
    }

    public override int GetHashCode()
    {
        return $"{this.Name}-{this.Code}-{this.Type}".GetHashCode();
    }
}