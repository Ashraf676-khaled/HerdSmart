using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Common.Behaviors;

public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private const int LongRunningThresholdMs = 500;

    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private readonly Stopwatch _timer = new();

    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        _timer.Restart();

        var response = await next();

        _timer.Stop();

        var elapsedMs = _timer.ElapsedMilliseconds;

        if (elapsedMs > LongRunningThresholdMs)
        {
            _logger.LogWarning(
                "Long running request: {RequestName} took {ElapsedMilliseconds} ms",
                typeof(TRequest).Name,
                elapsedMs);
        }

        return response;
    }
}
