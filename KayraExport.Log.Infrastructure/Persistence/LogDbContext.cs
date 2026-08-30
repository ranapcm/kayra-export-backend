using KayraExport.Log.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace KayraExport.Log.Infrastructure.Persistence;

public sealed class LogDbContext : DbContext
{
    public LogDbContext(DbContextOptions<LogDbContext> options)
        : base(options)
    {
    }

    public DbSet<EventLogEntry> EventLogs =>
        Set<EventLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<EventLogEntry>(entity =>
        {
            entity.HasKey(logEntry => logEntry.Id);

            entity.Property(logEntry => logEntry.ServiceName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(logEntry => logEntry.EventType)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(logEntry => logEntry.RoutingKey)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(logEntry => logEntry.Level)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Information");

            entity.Property(logEntry => logEntry.Payload)
                .IsRequired()
                .HasColumnType("jsonb");

            entity.HasIndex(logEntry => logEntry.ReceivedAt);
            entity.HasIndex(logEntry => logEntry.RoutingKey);
            entity.HasIndex(logEntry => logEntry.Level);
        });
    }
}