using HospitalMS.BuildingBlocks.Abstractions;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HospitalMS.Patient.Infrastructure.Services;

public class MassTransitEventBus(
    IPublishEndpoint publishEndpoint,
    ILogger<MassTransitEventBus> logger)
    : IEventBus
{
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        try
        {
            logger.LogInformation("[EventBus] Publishing integration event {EventType}: {@Event}",
                typeof(TEvent).Name, @event);

            await publishEndpoint.Publish(@event, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[EventBus] Failed to publish event {EventType}", typeof(TEvent).Name);
            // Non-blocking for local dev if broker is not connected
        }
    }
}

