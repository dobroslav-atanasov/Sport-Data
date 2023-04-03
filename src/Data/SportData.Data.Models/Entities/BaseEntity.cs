namespace SportData.Data.Models.Entities;

using System;
using System.ComponentModel.DataAnnotations;

using SportData.Data.Models.Entities.Interfaces;

public abstract class BaseEntity<TKey> : IAuditEntity
{
    [Key]
    public TKey Id { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }
}