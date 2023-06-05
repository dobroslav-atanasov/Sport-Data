namespace SportData.Data.Contexts;

using Microsoft.EntityFrameworkCore;

using SportData.Data.Entities.Crawlers;

public class CrawlerStorageDbContext : DbContext
{
    public CrawlerStorageDbContext(DbContextOptions<CrawlerStorageDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Crawler> Crawlers { get; set; }

    public virtual DbSet<Document> Documents { get; set; }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<Log> Logs { get; set; }

    public virtual DbSet<Operation> Operations { get; set; }

    public virtual DbSet<DataJson> DataJsons { get; set; }
}