namespace SportData.Services.Data.CrawlerStorageDb;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using SportData.Data.Entities.Crawlers;
using SportData.Data.Factories.Interfaces;
using SportData.Data.Repositories;
using SportData.Services.Data.CrawlerStorageDb.Interfaces;

public class LogsService : ILogsService
{
    private readonly IDbContextFactory dbContextFactory;

    //private readonly CrawlerStorageRepository<Log> repository;

    public LogsService(CrawlerStorageRepository<Log> repository, IDbContextFactory dbContextFactory)
    {
        this.dbContextFactory = dbContextFactory;
        //this.repository = repository;
    }

    public async Task AddLogAsync(Log log)
    {
        using var context = this.dbContextFactory.CreateCrawlerStorageDbContext();

        //await this.repository.AddAsync(log);
        //await this.repository.SaveChangesAsync();

        await context.AddAsync(log);
        await context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Guid>> GetLogIdentifiersAsync(int crawlerId)
    {
        using var context = this.dbContextFactory.CreateCrawlerStorageDbContext();

        //var identifiers = await repository
        //    .All()
        //    .Where(l => l.CrawlerId == crawlerId)
        //    .Select(l => l.Identifier)
        //    .ToListAsync();

        var identifiers = await context
            .Logs
            .Where(l => l.CrawlerId == crawlerId)
            .Select(l => l.Identifier)
            .ToListAsync();

        return identifiers;
    }

    public async Task UpdateLogAsync(Guid identifier, int operation)
    {
        using var context = this.dbContextFactory.CreateCrawlerStorageDbContext();

        //var log = await this.repository.GetAsync(x => x.Identifier == identifier);
        var log = await context.Logs.FirstOrDefaultAsync(x => x.Identifier == identifier);

        if (log != null)
        {
            log.Operation = operation;
            //this.repository.Update(log);
            //await this.repository.SaveChangesAsync();

            context.Update(log);
            await context.SaveChangesAsync();
        }
    }
}