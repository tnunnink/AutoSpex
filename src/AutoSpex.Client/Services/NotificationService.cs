using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;

namespace AutoSpex.Client.Services;

public class NotificationService : INotificationService
{
    private readonly Func<TopLevel> _factory;
    private readonly List<INotification> _cache = new();

    public NotificationService(Func<TopLevel> factory)
    {
        _factory = factory;
    }

    public void Show(INotification notification)
    {
        _cache.Add(notification);

        var window = _factory();
        var manager = new WindowNotificationManager(window);
        manager.Show(notification);
    }

    public IEnumerable<INotification> Notifications() => _cache;

    public void Clear() => _cache.Clear();
}