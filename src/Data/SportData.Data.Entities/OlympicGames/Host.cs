namespace SportData.Data.Entities.OlympicGames;

using System.ComponentModel.DataAnnotations.Schema;

using SportData.Data.Common.Interfaces;

[Table("Hosts", Schema = "og")]
public class Host : ICheckableEntity, IDeletableEntity, IEquatable<Host>
{
    public int CityId { get; set; }
    public virtual City City { get; set; }

    public int GameId { get; set; }
    public virtual Game Game { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedOn { get; set; }

    public bool Equals(Host other)
    {
        if (other == null)
        {
            return false;
        }

        return this.CityId == other.CityId
            && this.GameId == other.GameId;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as Host);
    }

    public override int GetHashCode()
    {
        return $"{this.CityId}-{this.GameId}".GetHashCode();
    }
}