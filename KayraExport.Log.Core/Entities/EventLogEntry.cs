namespace KayraExport.Log.Core.Entities;

public sealed class EventLogEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string ServiceName { get; set; } = string.Empty;

    public string EventType { get; set; } = string.Empty;

    public string RoutingKey { get; set; } = string.Empty;

    public string Level { get; set; } = "Information";

    public string Payload { get; set; } = string.Empty;

    public DateTime OccurredAt { get; set; }

    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}