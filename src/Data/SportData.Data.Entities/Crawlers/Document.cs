namespace SportData.Data.Entities.Crawlers;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Documents", Schema = "dbo")]
public class Document
{
    [Key]
    public int Id { get; set; }

    [Required]
    [Column(TypeName = "UNIQUEIDENTIFIER")]
    public Guid Identifier { get; set; }

    [Required]
    [MaxLength(250)]
    public string Name { get; set; }

    [Required]
    [MaxLength(250)]
    public string Format { get; set; }

    [Required]
    public int Order { get; set; }

    [Required]
    [MaxLength(500)]
    public string Url { get; set; }

    [Required]
    public int Operation { get; set; }

    [Required]
    [MaxLength(50)]
    public string Encoding { get; set; }

    [Required]
    [MaxLength(50)]
    public string MD5 { get; set; }

    public int GroupId { get; set; }
    public virtual Group Group { get; set; }

    [NotMapped]
    public byte[] Content { get; set; }
}