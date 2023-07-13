namespace SportData.Data.Contexts;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using SportData.Data.Entities;
using SportData.Data.Entities.Countries;
using SportData.Data.Entities.OlympicGames;

public class SportDataDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public SportDataDbContext(DbContextOptions<SportDataDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<Sport> Sports { get; set; }

    public virtual DbSet<Discipline> Disciplines { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<NOC> NOCs { get; set; }

    public virtual DbSet<Game> Games { get; set; }
}