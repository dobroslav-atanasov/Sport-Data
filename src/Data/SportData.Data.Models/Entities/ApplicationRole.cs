namespace SportData.Data.Models.Entities;

using System;

using Microsoft.AspNetCore.Identity;

using SportData.Data.Models.Entities.Interfaces;

public class ApplicationRole : IdentityRole, IAuditEntity, IDeletableEntity
{
    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public bool IsDeletable { get; set; }

    public DateTime? DeletedOn { get; set; }
}