namespace SportData.Data.Entities.OlympicGames;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using SportData.Data.Common.Interfaces;
using SportData.Data.Common.Models;
using SportData.Data.Entities.OlympicGames.Enumerations;

[Table("Sports", Schema = "dbo")]
public class Sport : BaseDeletableEntity<int>, IUpdatable<Sport>
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

    public virtual ICollection<Discipline> Disciplines { get; set; }

    public bool IsUpdated(Sport other)
    {
        var isUpdated = false;

        if (this.Code != other.Code)
        {
            this.Code = other.Code;
            isUpdated = true;
        }

        if (this.Type != other.Type)
        {
            this.Type = other.Type;
            isUpdated = true;
        }

        return isUpdated;
    }
}