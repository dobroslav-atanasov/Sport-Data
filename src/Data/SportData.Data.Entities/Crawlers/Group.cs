namespace SportData.Data.Entities.Crawlers;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Groups", Schema = "dbo")]
public class Group
{
    public Group()
    {
        this.Documents = new HashSet<Document>();
    }

    [Key]
    public int Id { get; set; }

    [Required]
    [Column(TypeName = "UNIQUEIDENTIFIER")]
    public Guid Identifier { get; set; }

    [Required]
    [MaxLength(250)]
    public string Name { get; set; }

    [Required]
    [Column(TypeName = "DATETIME2")]
    public DateTime Date { get; set; }

    public int CrawlerId { get; set; }
    public virtual Crawler Crawler { get; set; }

    [Required]
    public int Operation { get; set; }

    [Column(TypeName = "VARBINARY(MAX)")]
    public byte[] Content { get; set; }

    public virtual ICollection<Document> Documents { get; set; }
}