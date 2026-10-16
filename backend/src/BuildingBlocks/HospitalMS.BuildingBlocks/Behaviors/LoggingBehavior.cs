using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HospitalMS.BuildingBlocks.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        logger.LogInformation("[START] Handling {RequestName} {@Request}", requestName, request);

        var stopwatch = Stopwatch.StartNew();
        var response = await next();
        stopwatch.Stop();

        logger.LogInformation("[END] Handled {RequestName} in {ElapsedMilliseconds}ms",
            requestName, stopwatch.ElapsedMilliseconds);

        if (stopwatch.ElapsedMilliseconds > 500)
        {
            logger.LogWarning("[PERF] Long running request: {RequestName} ({ElapsedMilliseconds}ms)",
                requestName, stopwatch.ElapsedMilliseconds);
        }

        return response;
    }
}
