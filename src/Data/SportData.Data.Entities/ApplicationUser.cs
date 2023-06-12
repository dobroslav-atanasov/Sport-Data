namespace SportData.Data.Entities;

using Microsoft.AspNetCore.Identity;

using SportData.Data.Common.Interfaces;

public class ApplicationUser : IdentityUser, ICheckableEntity, IDeletableEntity
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Nickname { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string ImageUrl { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedOn { get; set; }
}