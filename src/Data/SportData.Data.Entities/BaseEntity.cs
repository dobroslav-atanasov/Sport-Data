namespace SportData.Data.Entities;

using System.ComponentModel.DataAnnotations;

public abstract class BaseEntity<TKey>
{
    [Key]
    public TKey Id { get; set; }
}