using System;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;

namespace AutoSpex.Client.Services;

public class NotificationService : INotificationService
{
    private readonly Func<TopLevel> _factory;

    public NotificationService(Func<TopLevel> factory)
    {
        _factory = factory;
    }

    private INotificationManager Notifier => new WindowNotificationManager(_factory());

    public void Show(Notification notification) => Notifier.Show(notification);
}