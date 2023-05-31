namespace SportData.Data.Contexts;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using SportData.Data.Entities;
using SportData.Data.Entities.Countries;

public class SportDataDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public SportDataDbContext(DbContextOptions<SportDataDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Country> Countries { get; set; }
}