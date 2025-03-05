using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutoSpex.Persistence;

[UsedImplicitly]
public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        logger.LogInformation("Starting request: {RequestName} at {DateTime}", requestName, DateTime.UtcNow);

        try
        {
            var response = await next();
            logger.LogInformation("Completed request: {RequestName} at {DateTime}", requestName, DateTime.UtcNow);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Request {RequestName} failed at {DateTime}", requestName, DateTime.UtcNow);
            throw;
        }
    }
}