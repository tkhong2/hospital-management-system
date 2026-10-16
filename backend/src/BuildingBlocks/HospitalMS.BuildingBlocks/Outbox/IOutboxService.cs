using HospitalMS.BuildingBlocks.Common;

namespace HospitalMS.BuildingBlocks.Outbox;

public interface IOutboxService
{
    Task EnqueueAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent;

    Task<IReadOnlyList<OutboxMessage>> GetPendingMessagesAsync(int batchSize = 50, CancellationToken cancellationToken = default);

    Task MarkAsProcessedAsync(Guid messageId, CancellationToken cancellationToken = default);

    Task MarkAsFailedAsync(Guid messageId, string error, CancellationToken cancellationToken = default);
}

