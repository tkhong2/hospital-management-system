using HospitalMS.BuildingBlocks.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HospitalMS.BuildingBlocks.Outbox;

public class OutboxProcessorBackgroundService(
    IOutboxService outboxService,
    IEventBus eventBus,
    ILogger<OutboxProcessorBackgroundService> logger) : BackgroundService
{
    private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("OutboxProcessorBackgroundService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var pendingMessages = await outboxService.GetPendingMessagesAsync(20, stoppingToken);

                foreach (var message in pendingMessages)
                {
                    try
                    {
                        var eventType = Type.GetType(message.EventType);
                        if (eventType != null)
                        {
                            var @event = System.Text.Json.JsonSerializer.Deserialize(message.Payload, eventType);
                            if (@event is Common.IntegrationEvent integrationEvent)
                            {
                                await eventBus.PublishAsync(integrationEvent, stoppingToken);
                                await outboxService.MarkAsProcessedAsync(message.Id, stoppingToken);
                                continue;
                            }
                        }

                        // Fallback if type resolution fails
                        logger.LogWarning("Could not deserialize event type {EventType} for OutboxMessage {MessageId}", message.EventType, message.Id);
                        await outboxService.MarkAsFailedAsync(message.Id, "Unresolved event type", stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to publish OutboxMessage {MessageId}", message.Id);
                        await outboxService.MarkAsFailedAsync(message.Id, ex.Message, stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while processing outbox messages");
            }

            await Task.Delay(_pollInterval, stoppingToken);
        }

        logger.LogInformation("OutboxProcessorBackgroundService stopped.");
    }
}

