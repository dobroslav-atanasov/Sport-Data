namespace SportData.Data.Entities.SportData;

using global::SportData.Data.Common.Interfaces;

using Microsoft.AspNetCore.Identity;

public class ApplicationRole : IdentityRole, ICheckableEntity, IDeletableEntity
{
    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedOn { get; set; }
}