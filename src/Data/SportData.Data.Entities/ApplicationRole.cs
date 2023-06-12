namespace SportData.Data.Entities;

using Microsoft.AspNetCore.Identity;

using SportData.Data.Common.Interfaces;

public class ApplicationRole : IdentityRole, ICheckableEntity, IDeletableEntity
{
    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedOn { get; set; }
}