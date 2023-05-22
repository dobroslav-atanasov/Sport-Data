namespace SportData.Data.Contexts;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using SportData.Data.Entities;

public class SportDataDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public SportDataDbContext(DbContextOptions<SportDataDbContext> options)
        : base(options)
    {
    }
}