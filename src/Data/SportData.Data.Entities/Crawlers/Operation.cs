namespace SportData.Data.Entities.Crawlers;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Operations", Schema = "dbo")]
public class Operation
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(10)]
    public string Name { get; set; }
}