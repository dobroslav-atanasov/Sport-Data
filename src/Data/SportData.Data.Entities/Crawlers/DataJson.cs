namespace SportData.Data.Entities.Crawlers;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("DataJsons", Schema = "dbo")]
public class DataJson
{
    [Key]
    public int Id { get; set; }

    [Column(TypeName = "UNIQUEIDENTIFIER")]
    public Guid Identifier { get; set; }

    [Required]
    public string Converter { get; set; }

    [Required]
    public string Json { get; set; }
}