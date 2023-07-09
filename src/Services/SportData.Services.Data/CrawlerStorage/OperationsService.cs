namespace SportData.Services.Data.CrawlerStorage;

using SportData.Data.Contexts;
using SportData.Data.Entities.Crawlers;
using SportData.Services.Data.CrawlerStorage.Interfaces;

public class OperationsService : BaseService, IOperationsService
{
    public OperationsService(CrawlerStorageDbContext context)
        : base(context)
    {
    }

    public async Task AddOperationAsync(string operationName)
    {
        using var context = this.Context;
        var operation = new Operation { Name = operationName };
        await context.AddAsync(operation);
        await context.SaveChangesAsync();
    }

    public bool IsOperationTableFull()
    {
        using var context = this.Context;

        return context.Operations.Any();
    }
}