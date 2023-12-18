using Avalonia.Controls.Notifications;

namespace AutoSpex.Client.Services;

public interface INotificationService
{
    void Show(Notification notification);
}