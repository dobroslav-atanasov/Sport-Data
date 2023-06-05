namespace SportData.Data.Entities.Crawlers;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Logs", Schema = "dbo")]
public class Log
{
    [Key]
    public int Id { get; set; }

    [Required]
    [Column(TypeName = "DATETIME2")]
    public DateTime LogDate { get; set; }

    [Required]
    [Column(TypeName = "UNIQUEIDENTIFIER")]
    public Guid Identifier { get; set; }

    [Required]
    public int Operation { get; set; }

    [Required]
    public int CrawlerId { get; set; }
    public virtual Crawler Crawler { get; set; }

    [MaxLength(5000)]
    public string Message { get; set; }
}