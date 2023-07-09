namespace SportData.Services.Data.CrawlerStorage;

using SportData.Data.Contexts;

public abstract class BaseService
{
    public BaseService(CrawlerStorageDbContext context)
    {
        this.Context = context;
    }

    protected CrawlerStorageDbContext Context { get; }
}