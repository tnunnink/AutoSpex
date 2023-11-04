using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace L5Spex.Client.Pipelines;

public class LoggingPipeline<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        Console.WriteLine("Test Pre");
        var result = await next();
        Console.WriteLine("Test Post");
        return result;
    }
}