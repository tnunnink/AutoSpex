using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Serilog;

namespace AutoSpex.Client.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        Log.Debug("Processing request {@requestName}", typeof(TRequest).Name);
        var result = await next();
        Log.Debug("Completed request {@requestName}", typeof(TRequest).Name);
        return result;
    }
}