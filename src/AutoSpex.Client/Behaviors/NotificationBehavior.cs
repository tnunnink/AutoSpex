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
    private readonly INotificationService _service;

    public NotificationBehavior(INotificationService service)
    {
        _service = service;
    }
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var result = await next();

        var notification = request.BuildNotification(result);

        if (notification is not null)
        {
            //todo service should hold notifications for us to view later.
            _service.Show(notification);    
        }

        return result;
    }
}