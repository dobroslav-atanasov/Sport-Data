namespace SportData.Data.Contexts;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using SportData.Data.Common.Interfaces;
using SportData.Data.Entities;
using SportData.Data.Entities.Countries;

public class SportDataDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public SportDataDbContext(DbContextOptions<SportDataDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Country> Countries { get; set; }

    public override int SaveChanges() => this.SaveChanges(true);

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        this.ApplyCheckRules();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => this.SaveChangesAsync(true, cancellationToken);

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        this.ApplyCheckRules();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyCheckRules()
    {
        var changedEntries = this.ChangeTracker
            .Entries()
            .Where(e =>
                e.Entity is ICheckableEntity &&
                (e.State == EntityState.Added || e.State == EntityState.Modified));

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
}