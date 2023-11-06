namespace SportData.Data.Entities.OlympicGames;

using System.ComponentModel.DataAnnotations.Schema;

using SportData.Data.Common.Interfaces;

[Table("Nationalities", Schema = "dbo")]
public class Nationality : ICheckableEntity, IDeletableEntity
{
    public Guid AthleteId { get; set; }
    public virtual Athlete Athlete { get; set; }

    public int NOCId { get; set; }
    public virtual NOC NOC { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedOn { get; set; }
}