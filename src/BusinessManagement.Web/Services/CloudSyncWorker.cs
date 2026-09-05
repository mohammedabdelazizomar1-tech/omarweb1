using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using BusinessManagement.Persistence;
using BusinessManagement.Domain.Entities;
using Npgsql;

namespace BusinessManagement.Web.Services;

public class CloudSyncWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CloudSyncWorker> _logger;
    
    private int _failureCount = 0;
    private DateTime _lastCleanupTime = DateTime.MinValue;
    
    private static readonly JsonSerializerOptions ResilientJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true
    };

    // System properties to ignore during sync (protecting identity columns and timestamps)
    private static readonly HashSet<string> ExcludedProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "Id", "CreatedAt", "CreatedBy", "RowVersion", "xmin", "SequenceId", "RetryCount", "Status", "ProcessedAt", "LastError", "NextRetryAt"
    };

    public CloudSyncWorker(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<CloudSyncWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Enterprise Cloud Sync Background Worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var cloudConnectionString = _configuration.GetConnectionString("CloudConnection");
                if (string.IsNullOrEmpty(cloudConnectionString))
                {
                    await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
                    continue;
                }

                int batchSize = _configuration.GetValue<int>("Sync:BatchSize", 50);
                bool processedAny = await ProcessSyncQueueAsync(cloudConnectionString, batchSize, stoppingToken);
                
                if (processedAny)
                {
                    _failureCount = 0;
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken); // Fast processing
                }
                else
                {
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Enterprise Cloud Sync Worker is stopping gracefully.");
                break;
            }
            catch (Exception ex)
            {
                _failureCount++;
                var delay = GetNextDelay();
                CategorizeAndLogError(ex, delay);
                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }
    }

    private TimeSpan GetNextDelay()
    {
        double seconds = 10 * Math.Pow(2, Math.Min(_failureCount - 1, 7));
        return TimeSpan.FromSeconds(Math.Min(seconds, 900)); // Cap backoff at 15 minutes
    }

    private void CategorizeAndLogError(Exception ex, TimeSpan nextDelay)
    {
        if (ex is NpgsqlException nex && nex.SqlState == "28P01")
        {
            _logger.LogError("CRITICAL: Cloud Database Authentication Failed (Invalid Credentials). Retrying in {Delay}.", nextDelay);
        }
        else if (ex is TimeoutException || ex is System.Net.Sockets.SocketException || ex is System.IO.IOException)
        {
            _logger.LogWarning("Network/Connection Timeout detected. Cloud Database is offline. Retrying in {Delay}.", nextDelay);
        }
        else
        {
            _logger.LogError(ex, "Unexpected sync worker failure. Retrying in {Delay}.", nextDelay);
        }
    }

    private async Task<bool> ProcessSyncQueueAsync(string cloudConnectionString, int batchSize, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var localDbContext = scope.ServiceProvider.GetRequiredService<BusinessManagementDbContext>();

        // Lock messages using database-level FOR UPDATE SKIP LOCKED to avoid multiple processes picking same rows
        using var localTransaction = await localDbContext.Database.BeginTransactionAsync(cancellationToken);
        
        var messages = await localDbContext.OutboxMessages
            .FromSqlRaw(
                "SELECT * FROM \"outbox_messages\" " +
                "WHERE \"status\" IN ('Pending', 'Failed') AND (\"next_retry_at\" IS NULL OR \"next_retry_at\" <= NOW()) " +
                "ORDER BY \"sequence_id\" LIMIT {0} FOR UPDATE SKIP LOCKED", batchSize)
            .ToListAsync(cancellationToken);

        if (!messages.Any())
        {
            await localTransaction.RollbackAsync(cancellationToken);
            return false;
        }

        foreach (var message in messages)
        {
            message.Status = "Processing";
        }
        await localDbContext.SaveChangesAsync(cancellationToken);
        await localTransaction.CommitAsync(cancellationToken);

        // Connect to Cloud Database (using BusinessManagementDbContext correctly!)
        var optionsBuilder = new DbContextOptionsBuilder<BusinessManagementDbContext>();
        optionsBuilder.UseNpgsql(cloudConnectionString);

        using var cloudDbContext = new BusinessManagementDbContext(optionsBuilder.Options);
        
        // Logging metrics for observability
        _logger.LogInformation("Processing batch of {Count} sync events. Metrics: active sync running.", messages.Count);

        // Daily cleanup tasks
        if (DateTime.UtcNow - _lastCleanupTime >= TimeSpan.FromDays(1))
        {
            await CleanupOldProcessedEventsAsync(cloudDbContext, cancellationToken);
            await CleanupLocalOutboxAsync(localDbContext, cancellationToken);
            _lastCleanupTime = DateTime.UtcNow;
        }

        // Apply events one-by-one with isolated transaction context to prevent batch failures
        foreach (var message in messages)
        {
            using var cloudTransaction = await cloudDbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // 1. Idempotency Check: check if event was already processed on cloud
                bool alreadyProcessed = await cloudDbContext.Set<ProcessedEvent>()
                    .AnyAsync(pe => pe.EventId == message.EventId, cancellationToken);

                if (alreadyProcessed)
                {
                    message.Status = "Completed";
                    message.ProcessedAt = DateTime.UtcNow;
                    await cloudTransaction.CommitAsync(cancellationToken);
                    continue;
                }

                var entityType = cloudDbContext.Model.GetEntityTypes()
                    .FirstOrDefault(e => e.ClrType.Name == message.AggregateType);

                if (entityType == null)
                {
                    message.Status = "Failed";
                    message.LastError = $"Unknown Entity Type: {message.AggregateType}";
                    await cloudTransaction.RollbackAsync(cancellationToken);
                    continue;
                }

                // Extract 'Payload' segment from Event Contract
                using var jsonDoc = JsonDocument.Parse(message.Payload);
                if (!jsonDoc.RootElement.TryGetProperty("Payload", out var payloadElement))
                {
                    message.Status = "Failed";
                    message.LastError = "Missing 'Payload' inside event contract.";
                    await cloudTransaction.RollbackAsync(cancellationToken);
                    continue;
                }

                var entity = JsonSerializer.Deserialize(payloadElement.GetRawText(), entityType.ClrType, ResilientJsonOptions);
                if (entity == null) continue;

                // Load existing entity dynamically using the non-generic FindAsync
                var existing = await cloudDbContext.FindAsync(entityType.ClrType, message.AggregateId);

                if (message.EventType.EndsWith("Created"))
                {
                    if (existing == null)
                    {
                        cloudDbContext.Add(entity);
                    }
                    else
                    {
                        ApplyValuesLWW(existing, entity, entityType, cloudDbContext);
                    }
                }
                else if (message.EventType.EndsWith("Updated"))
                {
                    if (existing == null)
                    {
                        cloudDbContext.Add(entity);
                    }
                    else
                    {
                        ApplyValuesLWW(existing, entity, entityType, cloudDbContext);
                    }
                }
                else if (message.EventType.EndsWith("Deleted"))
                {
                    if (existing != null)
                    {
                        cloudDbContext.Remove(existing);
                    }
                }

                // Log EventId as processed in the cloud database
                cloudDbContext.Set<ProcessedEvent>().Add(new ProcessedEvent
                {
                    EventId = message.EventId,
                    ProcessedAt = DateTime.UtcNow,
                    DeviceId = message.DeviceId
                });

                await cloudDbContext.SaveChangesAsync(cancellationToken);
                await cloudTransaction.CommitAsync(cancellationToken);

                message.Status = "Completed";
                message.ProcessedAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                await cloudTransaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Failed to apply message {EventId} to cloud DB. Marking as failed.", message.EventId);
                
                message.RetryCount++;
                message.LastError = ex.Message.Length > 500 ? ex.Message[..500] : ex.Message;
                
                if (message.RetryCount >= 10)
                {
                    message.Status = "DeadLetter";
                    message.NextRetryAt = null;
                }
                else
                {
                    message.Status = "Failed";
                    // Retry delay backoff: 30s * 2^(retryCount-1)
                    message.NextRetryAt = DateTime.UtcNow.AddSeconds(30 * Math.Pow(2, message.RetryCount - 1));
                }
            }
        }

        // Commit status changes locally (Completed / Failed states)
        await localDbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private void ApplyValuesLWW(object existing, object source, Microsoft.EntityFrameworkCore.Metadata.IEntityType entityType, DbContext context)
    {
        var entry = context.Entry(existing);

        // Last Write Wins (LWW) check based on UpdatedAt column
        var updatedAtProp = entityType.FindProperty("UpdatedAt");
        if (updatedAtProp != null && updatedAtProp.PropertyInfo != null)
        {
            var existingTime = (DateTime?)updatedAtProp.PropertyInfo.GetValue(existing);
            var incomingTime = (DateTime?)updatedAtProp.PropertyInfo.GetValue(source);

            if (existingTime.HasValue && incomingTime.HasValue && incomingTime.Value <= existingTime.Value)
            {
                _logger.LogDebug("LWW Concurrency check failed. Cloud has newer/equal data. Skipping overwrite.");
                return;
            }
        }

        foreach (var prop in entityType.GetProperties())
        {
            // Ignore primary keys and database-managed properties (like CreatedAt, SequenceId, xmin)
            if (ExcludedProperties.Contains(prop.Name)) continue;

            if (prop.ValueGenerated == Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.OnAddOrUpdate)
            {
                continue;
            }

            if (prop.PropertyInfo != null)
            {
                var value = prop.PropertyInfo.GetValue(source);
                entry.Property(prop.Name).CurrentValue = value;
            }
        }
    }

    private async Task CleanupOldProcessedEventsAsync(DbContext cloudDbContext, CancellationToken cancellationToken)
    {
        try
        {
            var cutoff = DateTime.UtcNow.AddDays(-90); // 90 days retention period
            await cloudDbContext.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM \"processed_events\" WHERE \"processed_at\" < {cutoff}", cancellationToken);
            _logger.LogInformation("Retention cleanup complete: removed processed_events older than 90 days.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute cloud processed_events cleanup job.");
        }
    }

    private async Task CleanupLocalOutboxAsync(BusinessManagementDbContext localDbContext, CancellationToken cancellationToken)
    {
        try
        {
            var cutoff = DateTime.UtcNow.AddDays(-30); // 30 days retention period for completed outbox messages
            await localDbContext.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM \"outbox_messages\" WHERE \"status\" = 'Completed' AND \"processed_at\" < {cutoff}", cancellationToken);
            _logger.LogInformation("Retention cleanup complete: removed completed local outbox messages older than 30 days.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute local outbox cleanup job.");
        }
    }
}
