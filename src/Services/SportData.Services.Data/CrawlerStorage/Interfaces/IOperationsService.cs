namespace SportData.Services.Data.CrawlerStorage.Interfaces;

public interface IOperationsService
{
    Task AddOperationAsync(string operationName);

    bool IsOperationTableFull();
}