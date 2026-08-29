using KayraExport.Log.Application.Interfaces;
using KayraExport.Log.Core.Entities;
using KayraExport.Log.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KayraExport.Log.Infrastructure.Repositories;

public sealed class EventLogRepository : IEventLogRepository
{
    private readonly LogDbContext _dbContext;

    public EventLogRepository(LogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        EventLogEntry logEntry,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.EventLogs.AddAsync(
            logEntry,
            cancellationToken);
    }

    public async Task<IReadOnlyList<EventLogEntry>> GetRecentAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.EventLogs
            .AsNoTracking()
            .OrderByDescending(logEntry => logEntry.ReceivedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}