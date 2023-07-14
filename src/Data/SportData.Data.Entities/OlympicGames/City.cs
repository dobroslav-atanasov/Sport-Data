namespace SportData.Data.Entities.OlympicGames;

using System.ComponentModel.DataAnnotations.Schema;

using SportData.Data.Common.Interfaces;
using SportData.Data.Common.Models;
using SportData.Data.Entities.Countries;

[Table("Cities", Schema = "og")]
public class City : BaseEntity<int>, IDeletableEntity, IEquatable<City>
{
    public City()
    {
        this.Hosts = new HashSet<Host>();
    }

    public string Name { get; set; }

    public int CountryId { get; set; }
    public virtual Country Country { get; set; }

    public virtual ICollection<Host> Hosts { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedOn { get; set; }

    public bool Equals(City other)
    {
        if (other == null)
        {
            return false;
        }

        return this.Name == other.Name
            && this.CountryId == other.CountryId;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as City);
    }

    public override int GetHashCode()
    {
        return $"{this.Name}-{this.CountryId}".GetHashCode();
    }
}