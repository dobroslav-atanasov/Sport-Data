namespace SportData.Data.Entities.OlympicGames;

using System.ComponentModel.DataAnnotations.Schema;

using SportData.Data.Common.Interfaces;

[Table("Hosts", Schema = "dbo")]
public class Host : ICheckableEntity, IDeletableEntity
{
    public int CityId { get; set; }
    public virtual City City { get; set; }

    public int GameId { get; set; }
    public virtual Game Game { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedOn { get; set; }
}