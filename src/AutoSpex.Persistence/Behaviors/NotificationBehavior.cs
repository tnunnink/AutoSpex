using FluentResults;
using MediatR;

namespace AutoSpex.Persistence;

public class NotificationBehavior<TRequest, TResponse>(IPublisher mediator) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IDbCommand<TResponse>
    where TResponse : IResultBase
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var result = await next();

        var notification = new CommandNotification<TResponse>(request, typeof(TRequest).Name, result);
        await mediator.Publish(notification, cancellationToken);
        
        return result;
    }
}