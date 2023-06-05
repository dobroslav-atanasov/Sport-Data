namespace SportData.Data.Entities.Crawlers;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

[Table("Crawlers", Schema = "dbo")]
public class Crawler
{
    public Crawler()
    {
        this.Logs = new HashSet<Log>();
        this.Groups = new HashSet<Group>();
    }

    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(250)]
    public string Name { get; set; }

    public virtual ICollection<Log> Logs { get; set; }

    public virtual ICollection<Group> Groups { get; set; }
}