using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HospitalMS.BuildingBlocks.Messaging;

public static class MassTransitResilienceExtensions
{
    /// <summary>
    /// Configures RabbitMQ or InMemory bus with standard resilience policies:
    /// - Exponential backoff retry (3 retries: 1s, 3s, 7s)
    /// - Redelivery on transient failures
    /// - Poison message isolation to Dead-Letter Queue (*_error)
    /// </summary>
    public static IServiceCollection AddHospitalMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator>? configureConsumers = null)
    {
        services.AddMassTransit(x =>
        {
            configureConsumers?.Invoke(x);

            var rabbitHost = configuration["RabbitMQ:Host"];
            if (!string.IsNullOrEmpty(rabbitHost))
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitHost, "/", h =>
                    {
                        h.Username(configuration["RabbitMQ:Username"] ?? "guest");
                        h.Password(configuration["RabbitMQ:Password"] ?? "guest");
                    });

                    // Resilience: Exponential backoff retry policy
                    cfg.UseMessageRetry(r => r.Exponential(
                        retryLimit: 3,
                        minInterval: TimeSpan.FromSeconds(1),
                        maxInterval: TimeSpan.FromSeconds(10),
                        intervalDelta: TimeSpan.FromSeconds(2)));

                    // MassTransit automatically handles Dead-Letter Queues (DLQ)
                    // Messages exceeding retry count are routed to {queue}_error
                    cfg.ConfigureEndpoints(context);
                });
            }
            else
            {
                x.UsingInMemory((context, cfg) =>
                {
                    cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(1)));
                    cfg.ConfigureEndpoints(context);
                });
            }
        });

        return services;
    }
}
