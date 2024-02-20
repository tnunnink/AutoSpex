using System.Collections.Generic;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;

namespace AutoSpex.Client.Services;

[UsedImplicitly]
[PublicAPI]
public sealed class Notifier(Shell shell)
{
    private INotificationManager _manager = new WindowNotificationManager(shell);
    public List<INotification> Notifications { get; } = [];

    public void Notify(INotification notification)
    {
        Notifications.Add(notification);
        _manager.Show(notification);
    }

    public record NotificationMessage(INotification Notification);
}