namespace SportData.Data.Models.Entities;

using Microsoft.AspNetCore.Identity;

using SportData.Data.Models.Entities.Interfaces;

public class ApplicationUser : IdentityUser, IAuditEntity, IDeletableEntity
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Nickname { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string ImageUrl { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public bool IsDeletable { get; set; }

    public DateTime? DeletedOn { get; set; }
}