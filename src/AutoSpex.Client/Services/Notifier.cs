using System;
using System.Collections.Generic;
using Avalonia.Controls.Notifications;
using JetBrains.Annotations;

namespace AutoSpex.Client.Services;

[UsedImplicitly]
[PublicAPI]
public sealed class Notifier(Shell shell)
{
    private INotificationManager _manager = new WindowNotificationManager(shell);
    public List<INotification> Notifications { get; } = [];

    public void Notify(string title, string message, Action? onClick = null)
    {
        var notification = new Notification(title, message, onClick: onClick);
        Notifications.Add(notification);
        _manager.Show(notification);
    }

    public void NotifySuccess(string title, string message, Action? onClick = null)
    {
        var notification = new Notification(title, message, NotificationType.Success, onClick: onClick);
        Notifications.Add(notification);
        _manager.Show(notification);
    }

    public void NotifyError(string title, string message, Action? onClick = null)
    {
        var notification = new Notification(title, message, NotificationType.Error, onClick: onClick);
        Notifications.Add(notification);
        _manager.Show(notification);
    }

    public void NotifyWarning(string title, string message, Action? onClick = null)
    {
        var notification = new Notification(title, message, NotificationType.Warning, onClick: onClick);
        Notifications.Add(notification);
        _manager.Show(notification);
    }

    public record NotificationMessage(INotification Notification);
}