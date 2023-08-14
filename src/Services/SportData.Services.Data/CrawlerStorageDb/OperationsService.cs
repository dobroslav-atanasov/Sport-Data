namespace SportData.Services.Data.CrawlerStorageDb;

using SportData.Data.Entities.Crawlers;
using SportData.Data.Factories.Interfaces;
using SportData.Data.Repositories;
using SportData.Services.Data.CrawlerStorageDb.Interfaces;

public class OperationsService : IOperationsService
{
    private readonly IDbContextFactory dbContextFactory;

    //private readonly CrawlerStorageRepository<Operation> repository;

    public OperationsService(CrawlerStorageRepository<Operation> repository, IDbContextFactory dbContextFactory)
    {
        this.dbContextFactory = dbContextFactory;
        //this.repository = repository;
    }

    public async Task AddOperationAsync(string operationName)
    {
        using var context = this.dbContextFactory.CreateCrawlerStorageDbContext();

        var operation = new Operation { Name = operationName };
        //await this.repository.AddAsync(operation);
        //await this.repository.SaveChangesAsync();

        await context.AddAsync(operation);
        await context.SaveChangesAsync();
    }

    public bool IsOperationTableFull()
    {
        using var context = this.dbContextFactory.CreateCrawlerStorageDbContext();
        return context.Operations.Any();

        //return this.repository.All().Any();
    }
}