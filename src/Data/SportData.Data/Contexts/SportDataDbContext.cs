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

    public virtual DbSet<City> Cities { get; set; }

    public virtual DbSet<Discipline> Disciplines { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<Game> Games { get; set; }

    public virtual DbSet<Host> Hosts { get; set; }

    public virtual DbSet<NOC> NOCs { get; set; }

    public virtual DbSet<Sport> Sports { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Host>()
            .HasKey(h => new { h.CityId, h.GameId });

        builder.Entity<Host>()
            .HasOne(h => h.City)
            .WithMany(c => c.Hosts)
            .HasForeignKey(h => h.CityId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.Entity<Host>()
            .HasOne(h => h.Game)
            .WithMany(g => g.Hosts)
            .HasForeignKey(h => h.GameId)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}