using System;
using Avalonia.Controls.Notifications;

namespace AutoSpex.Client.Services;

public class MockNotificationService : INotificationService
{
    public void Show(INotification notification)
    {
        Console.WriteLine(notification.Message);
    }
}