namespace KayraExport.Application.Products.Events;

public sealed record ProductCreatedEvent(
    Guid EventId,
    DateTime OccurredAt,
    Guid ProductId,
    string Name,
    string? Description,
    decimal Price,
    int Stock);

public sealed record ProductUpdatedEvent(
    Guid EventId,
    DateTime OccurredAt,
    Guid ProductId,
    string Name,
    string? Description,
    decimal Price,
    int Stock);