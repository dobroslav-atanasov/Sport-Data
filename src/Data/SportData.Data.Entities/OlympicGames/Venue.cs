namespace SportData.Data.Entities.OlympicGames;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using SportData.Data.Common.Interfaces;
using SportData.Data.Common.Models;

[Table("Venues", Schema = "og")]
public class Venue : BaseDeletableEntity<int>, IUpdatable<Venue>
{
    public Venue()
    {
        this.EventVenues = new HashSet<EventVenue>();
    }

    [Required]
    public int Number { get; set; }

    [Required]
    [MaxLength(500)]
    public string Name { get; set; }

    [Required]
    [MaxLength(500)]
    public string CityName { get; set; }

    [MaxLength(500)]
    public string EnglishName { get; set; }

    [MaxLength(1000)]
    public string FullName { get; set; }

    public double? LatitudeCoordinate { get; set; }

    public double? LongitudeCoordinate { get; set; }

    public int? OpenedYear { get; set; }

    public int? DemolishedYear { get; set; }

    public string Capacity { get; set; }

    public virtual ICollection<EventVenue> EventVenues { get; set; }

    public bool IsUpdated(Venue other)
    {
        var isUpdated = false;

        if (this.Name != other.Name)
        {
            this.Name = other.Name;
            isUpdated = true;
        }

        if (this.CityName != other.CityName)
        {
            this.CityName = other.CityName;
            isUpdated = true;
        }

        if (this.EnglishName != other.EnglishName)
        {
            this.EnglishName = other.EnglishName;
            isUpdated = true;
        }

        if (this.FullName != other.FullName)
        {
            this.FullName = other.FullName;
            isUpdated = true;
        }

        if (this.LatitudeCoordinate != other.LatitudeCoordinate)
        {
            this.LatitudeCoordinate = other.LatitudeCoordinate;
            isUpdated = true;
        }

        if (this.LongitudeCoordinate != other.LongitudeCoordinate)
        {
            this.LongitudeCoordinate = other.LongitudeCoordinate;
            isUpdated = true;
        }

        if (this.OpenedYear != other.OpenedYear)
        {
            this.OpenedYear = other.OpenedYear;
            isUpdated = true;
        }

        if (this.DemolishedYear != other.DemolishedYear)
        {
            this.DemolishedYear = other.DemolishedYear;
            isUpdated = true;
        }

        if (this.Capacity != other.Capacity)
        {
            this.Capacity = other.Capacity;
            isUpdated = true;
        }

        return isUpdated;
    }
}