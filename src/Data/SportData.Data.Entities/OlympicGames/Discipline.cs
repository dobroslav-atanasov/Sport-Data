namespace SportData.Data.Entities.OlympicGames;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using SportData.Data.Common.Interfaces;
using SportData.Data.Common.Models;

[Table("Disciplines", Schema = "dbo")]
public class Discipline : BaseDeletableEntity<int>, IUpdatable<Discipline>
{
    public Discipline()
    {
        this.Events = new HashSet<Event>();
    }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [Required]
    [StringLength(3)]
    public string Code { get; set; }

    public int SportId { get; set; }
    public virtual Sport Sport { get; set; }

    public virtual ICollection<Event> Events { get; set; }

    public bool IsUpdated(Discipline other)
    {
        var isUpdated = false;

        if (this.Code != other.Code)
        {
            this.Code = other.Code;
            isUpdated = true;
        }

        if (this.SportId != other.SportId)
        {
            this.SportId = other.SportId;
            isUpdated = true;
        }

        return isUpdated;
    }
}