using System.Collections.Generic;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;

namespace AutoSpex.Client.Services;

[UsedImplicitly]
[PublicAPI]
public sealed class Notifier(IMessenger messenger)
{
    public List<INotification> Notifications { get; } = [];

    public void Notify(INotification notification)
    {
        Notifications.Add(notification);
        messenger.Send(new NotificationMessage(notification));
    }

    public record NotificationMessage(INotification Notification);
}