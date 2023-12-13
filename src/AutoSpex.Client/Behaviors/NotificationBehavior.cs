using System.Threading;
using System.Threading.Tasks;
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
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var result = await next();

        var notification = request.BuildNotification(result);

        /*if (notification is not null)
        {
            _notificationManager.Show(notification);    
        }*/

        //todo insert notification to app store

        return result;
    }
}