namespace SportData.Data.Entities.OlympicGames;

using System.ComponentModel.DataAnnotations.Schema;

using SportData.Data.Common.Interfaces;
using SportData.Data.Common.Models;

[Table("Cities", Schema = "og")]
public class City : BaseDeletableEntity<int>, IUpdatable<City>
{
    public City()
    {
        this.Hosts = new HashSet<Host>();
    }

    public string Name { get; set; }

    public int NOCId { get; set; }
    public virtual NOC NOC { get; set; }

    public virtual ICollection<Host> Hosts { get; set; }

    public bool IsUpdated(City other)
    {
        var isUpdated = false;

        if (this.NOCId != other.NOCId)
        {
            this.NOCId = other.NOCId;
            isUpdated = true;
        }

        return isUpdated;
    }
}