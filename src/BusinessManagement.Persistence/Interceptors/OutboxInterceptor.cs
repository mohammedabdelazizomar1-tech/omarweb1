using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using BusinessManagement.Domain.Entities;

namespace BusinessManagement.Persistence.Interceptors;

public class OutboxInterceptor : SaveChangesInterceptor
{
    private static readonly string DeviceIdDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
        "BusinessManagementERP"
    );
    private static readonly string DeviceIdPath = Path.Combine(DeviceIdDir, "device.json");
    private static readonly string DeviceId = GetOrCreateDeviceId();

    private static string GetOrCreateDeviceId()
    {
        try
        {
            if (File.Exists(DeviceIdPath))
            {
                var content = File.ReadAllText(DeviceIdPath);
                var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("DeviceId", out var prop))
                {
                    return prop.GetString() ?? Guid.NewGuid().ToString("N");
                }
            }
        }
        catch { }

        string newId = Guid.NewGuid().ToString("N");
        try
        {
            Directory.CreateDirectory(DeviceIdDir);
            File.WriteAllText(DeviceIdPath, JsonSerializer.Serialize(new { DeviceId = newId }));
        }
        catch { }
        return newId;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context != null)
        {
            CaptureOutboxEvents(context);
        }
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void CaptureOutboxEvents(DbContext context)
    {
        var outboxEntries = new List<OutboxMessage>();
        string? correlationId = System.Diagnostics.Activity.Current?.TraceId.ToString();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is OutboxMessage || 
                entry.Entity is ProcessedEvent ||
                entry.State == EntityState.Detached || 
                entry.State == EntityState.Unchanged)
            {
                continue;
            }

            string? eventAction = entry.State switch
            {
                EntityState.Added => "Created",
                EntityState.Modified => "Updated",
                EntityState.Deleted => "Deleted",
                _ => null
            };

            if (eventAction == null) continue;

            var entityType = entry.Entity.GetType();
            string aggregateType = entityType.Name;
            if (aggregateType.Contains("_") || aggregateType.Contains("Proxy"))
            {
                aggregateType = entityType.BaseType?.Name ?? aggregateType;
            }

            string eventType = $"{aggregateType}{eventAction}";

            Guid aggregateId = Guid.Empty;
            var idProp = entry.Metadata.FindProperty("Id");
            if (idProp != null && entry.Property("Id").CurrentValue is Guid gId)
            {
                aggregateId = gId;
            }

            // Serialize scalar database columns only
            var values = new Dictionary<string, object?>();
            foreach (var property in entry.CurrentValues.Properties)
            {
                if (property.Name.Equals("xmin", StringComparison.OrdinalIgnoreCase)) continue;
                values[property.Name] = entry.CurrentValues[property];
            }

            // Wrap payload in structured Event Contract
            var eventContract = new
            {
                EventId = Guid.NewGuid(),
                EventVersion = 1,
                AggregateType = aggregateType,
                AggregateId = aggregateId,
                Payload = values
            };

            var payloadJson = JsonSerializer.Serialize(eventContract);

            outboxEntries.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                EventId = eventContract.EventId,
                EventVersion = eventContract.EventVersion,
                AggregateId = aggregateId,
                AggregateType = aggregateType,
                EventType = eventType,
                Payload = payloadJson,
                Status = "Pending",
                RetryCount = 0,
                CreatedAt = DateTime.UtcNow,
                DeviceId = DeviceId,
                CorrelationId = correlationId
            });
        }

        if (outboxEntries.Any())
        {
            context.Set<OutboxMessage>().AddRange(outboxEntries);
        }
    }
}
