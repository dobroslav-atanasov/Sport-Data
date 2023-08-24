namespace SportData.Data.Entities.OlympicGames;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using SportData.Data.Common.Interfaces;
using SportData.Data.Common.Models;

[Table("Results", Schema = "og")]
public class Result : BaseDeletableEntity<Guid>, IUpdatable<Result>
{
    public int EventId { get; set; }
    public virtual Event Event { get; set; }

    [Required]
    public string Json { get; set; }

    public bool IsUpdated(Result other)
    {
        var isUpdated = false;

        if (this.Json != other.Json)
        {
            this.Json = other.Json;
            isUpdated = true;
        }

        return isUpdated;
    }
}