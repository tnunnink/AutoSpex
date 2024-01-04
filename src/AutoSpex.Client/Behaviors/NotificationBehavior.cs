using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Services;
using Avalonia.Controls.Notifications;
using MediatR;

namespace AutoSpex.Client.Behaviors;

public interface INotifiableRequest<TResponse> : IRequest<TResponse>
{
    Notification? BuildNotification(TResponse result);
}

public class NotificationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : INotifiableRequest<TResponse>
{
    private readonly INotificationService _notifier;

    public NotificationBehavior(INotificationService notifier)
    {
        _notifier = notifier;
    }
    
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var result = await next();

        var notification = request.BuildNotification(result);

        if (notification is not null)
        {
            _notifier.Show(notification);    
        }

        return result;
    }
}