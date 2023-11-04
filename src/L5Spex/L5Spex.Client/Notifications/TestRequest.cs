using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using MediatR;
using INotification = MediatR.INotification;

namespace L5Spex.Client.Notifications;

public static class TestRequest
{
    public record Message(string Title, string Description) : INotification;
    
    public class Handler : INotificationHandler<Message>
    {
        private readonly INotificationManager _notificationManager;

        public Handler(INotificationManager notificationManager)
        {
            _notificationManager = notificationManager;
        }
        
        public Task Handle(Message message, CancellationToken cancellationToken)
        {
            var notification = new Notification(message.Title, message.Description, NotificationType.Success);
            _notificationManager.Show(notification);
            return Task.CompletedTask;
        }
    }
}