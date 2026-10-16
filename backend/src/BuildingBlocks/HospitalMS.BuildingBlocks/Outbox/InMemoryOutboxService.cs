using System.Collections.Concurrent;
using System.Text.Json;
using HospitalMS.BuildingBlocks.Common;
using Microsoft.Extensions.Logging;

namespace HospitalMS.BuildingBlocks.Outbox;

public class InMemoryOutboxService(ILogger<InMemoryOutboxService> logger) : IOutboxService
{
    private static readonly ConcurrentDictionary<Guid, OutboxMessage> Messages = new();
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };

    public Task EnqueueAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        var message = new OutboxMessage
        {
            Id = @event.EventId,
            EventType = typeof(TEvent).AssemblyQualifiedName ?? typeof(TEvent).FullName ?? @event.GetType().Name,
            Payload = JsonSerializer.Serialize(@event, @event.GetType(), JsonOptions),
            OccurredOn = @event.OccurredOn,
            RetryCount = 0
        };

        Messages[message.Id] = message;
        logger.LogInformation("Enqueued OutboxMessage {MessageId} of type {EventType}", message.Id, message.EventType);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<OutboxMessage>> GetPendingMessagesAsync(int batchSize = 50, CancellationToken cancellationToken = default)
    {
        var pending = Messages.Values
            .Where(m => !m.IsProcessed && m.RetryCount < 5)
            .OrderBy(m => m.OccurredOn)
            .Take(batchSize)
            .ToList();

        return Task.FromResult<IReadOnlyList<OutboxMessage>>(pending);
    }

    public Task MarkAsProcessedAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        if (Messages.TryGetValue(messageId, out var msg))
        {
            msg.ProcessedOn = DateTime.UtcNow;
            logger.LogInformation("OutboxMessage {MessageId} processed successfully", messageId);
        }
        return Task.CompletedTask;
    }

    public Task MarkAsFailedAsync(Guid messageId, string error, CancellationToken cancellationToken = default)
    {
        if (Messages.TryGetValue(messageId, out var msg))
        {
            msg.RetryCount++;
            msg.Error = error;
            logger.LogWarning("OutboxMessage {MessageId} failed (retry {RetryCount}): {Error}", messageId, msg.RetryCount, error);
        }
        return Task.CompletedTask;
    }
}
