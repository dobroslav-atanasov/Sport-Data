namespace SportData.Services.Data;

using SportData.Data.Contexts;

public abstract class BaseService
{
    public BaseService(SportDataDbContext context)
    {
        this.Context = context;
    }

    protected SportDataDbContext Context { get; }
}