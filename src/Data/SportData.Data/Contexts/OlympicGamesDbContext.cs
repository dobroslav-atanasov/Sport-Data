namespace SportData.Data.Contexts;

using Microsoft.EntityFrameworkCore;

using SportData.Data.Common.Interfaces;
using SportData.Data.Entities.OlympicGames;

public class OlympicGamesDbContext : DbContext
{
    public OlympicGamesDbContext(DbContextOptions<OlympicGamesDbContext> options)
        :base(options)
    {        
    }

    public virtual DbSet<Athlete> Athletes { get; set; }

    public virtual DbSet<City> Cities { get; set; }

    public virtual DbSet<Discipline> Disciplines { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<EventVenue> EventVenues { get; set; }

    public virtual DbSet<Game> Games { get; set; }

    public virtual DbSet<Host> Hosts { get; set; }

    public virtual DbSet<Nationality> Nationalities { get; set; }

    public virtual DbSet<NOC> NOCs { get; set; }

    public virtual DbSet<Participant> Participants { get; set; }

    public virtual DbSet<Result> Results { get; set; }

    public virtual DbSet<Sport> Sports { get; set; }

    public virtual DbSet<Squad> Squads { get; set; }

    public virtual DbSet<Team> Teams { get; set; }

    public virtual DbSet<Venue> Venues { get; set; }

    public override int SaveChanges()
    {
        return this.SaveChanges(true);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        this.ApplyCheckRules();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return this.SaveChangesAsync(true, cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        this.ApplyCheckRules();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyCheckRules()
    {
        var changedEntries = this.ChangeTracker
                .Entries()
                .Where(e => e.Entity is ICheckableEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in changedEntries)
        {
            var entity = (ICheckableEntity)entry.Entity;
            if (entry.State == EntityState.Added && entity.CreatedOn == default)
            {
                entity.CreatedOn = DateTime.UtcNow;
            }
            else
            {
                entity.ModifiedOn = DateTime.UtcNow;
            }
        }
    }

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

        builder.Entity<EventVenue>()
             .HasKey(ev => new { ev.EventId, ev.VenueId });

        builder.Entity<EventVenue>()
            .HasOne(ev => ev.Event)
            .WithMany(e => e.EventVenues)
            .HasForeignKey(ev => ev.EventId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.Entity<EventVenue>()
            .HasOne(ev => ev.Venue)
            .WithMany(v => v.EventVenues)
            .HasForeignKey(ev => ev.VenueId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.Entity<Nationality>()
            .HasKey(n => new { n.AthleteId, n.NOCId });

        builder.Entity<Nationality>()
            .HasOne(n => n.Athlete)
            .WithMany(a => a.Nationalities)
            .HasForeignKey(n => n.AthleteId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.Entity<Nationality>()
            .HasOne(n => n.NOC)
            .WithMany(noc => noc.Nationalities)
            .HasForeignKey(n => n.NOCId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.Entity<Squad>()
            .HasKey(s => new { s.ParticipantId, s.TeamId });

        builder.Entity<Squad>()
            .HasOne(s => s.Participant)
            .WithMany(p => p.Squads)
            .HasForeignKey(s => s.ParticipantId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.Entity<Squad>()
            .HasOne(s => s.Team)
            .WithMany(t => t.Squads)
            .HasForeignKey(s => s.TeamId)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}