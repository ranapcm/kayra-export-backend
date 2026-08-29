using KayraExport.Log.Core.Entities;

namespace KayraExport.Log.Application.Interfaces;

public interface IEventLogRepository
{
    Task AddAsync(
        EventLogEntry logEntry,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EventLogEntry>> GetRecentAsync(
        int count,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}