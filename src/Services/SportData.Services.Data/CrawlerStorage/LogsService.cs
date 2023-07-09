namespace SportData.Services.Data.CrawlerStorage;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using SportData.Data.Contexts;
using SportData.Data.Entities.Crawlers;
using SportData.Services.Data.CrawlerStorage.Interfaces;

public class LogsService : BaseService, ILogsService
{
    public LogsService(CrawlerStorageDbContext context)
        : base(context)
    {
    }

    public async Task AddLogAsync(Log log)
    {
        using var context = this.Context;
        await context.AddAsync(log);
        await context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Guid>> GetLogIdentifiersAsync(int crawlerId)
    {
        using var context = this.Context;

        var identifiers = await context
            .Logs
            .Where(l => l.CrawlerId == crawlerId)
            .Select(l => l.Identifier)
            .ToListAsync();

        return identifiers;
    }

    public async Task UpdateLogAsync(Guid identifier, int operation)
    {
        using var context = this.Context;

        var log = await context
            .Logs
            .FirstOrDefaultAsync(l => l.Identifier == identifier);

        if (log != null)
        {
            log.Operation = operation;
            context.Entry(log).State = EntityState.Modified;
            await context.SaveChangesAsync();
        }
    }
}