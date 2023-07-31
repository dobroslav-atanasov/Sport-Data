namespace SportData.Data.Entities.OlympicGames;

using System.ComponentModel.DataAnnotations.Schema;

using SportData.Data.Common.Interfaces;

[Table("Nationalities", Schema = "og")]
public class Nationality : ICheckableEntity, IDeletableEntity, IEquatable<Nationality>
{
    public Guid AthleteId { get; set; }
    public virtual Athlete Athlete { get; set; }

    public int NOCId { get; set; }
    public virtual NOC NOC { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedOn { get; set; }

    public bool Equals(Nationality other)
    {
        if (other == null)
        {
            return false;
        }

        return this.AthleteId == other.AthleteId
            && this.NOCId == other.NOCId;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as Nationality);
    }

    public override int GetHashCode()
    {
        return $"{this.AthleteId}-{this.NOCId}".GetHashCode();
    }
}