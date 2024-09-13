using System;
using System.Collections.Generic;
using Avalonia.Controls.Notifications;
using FluentResults;
using JetBrains.Annotations;

namespace AutoSpex.Client.Services;

[UsedImplicitly]
[PublicAPI]
public sealed class Notifier(Shell shell)
{
    private WindowNotificationManager _manager = new(shell);
    public List<INotification> Notifications { get; } = [];

    public void Notify(string title, string message, Action? onClick = null)
    {
        var notification = new Notification(title, message, onClick: onClick);
        Notifications.Add(notification);
        _manager.Show(notification);
    }

    public void ShowSuccess(string title, string message, Action? onClick = null)
    {
        var notification = new Notification(title, message, NotificationType.Success, onClick: onClick);
        Notifications.Add(notification);
        _manager.Show(notification);
    }

    public void ShowError(string title, string message, Action? onClick = null)
    {
        var notification = new Notification(title, message, NotificationType.Error, onClick: onClick);
        Notifications.Add(notification);
        _manager.Show(notification);
    }

    public void ShowWarning(string title, string message, Action? onClick = null)
    {
        var notification = new Notification(title, message, NotificationType.Warning, onClick: onClick);
        Notifications.Add(notification);
        _manager.Show(notification);
    }

    /// <summary>
    /// Checks the provided result, notifies the user if failed, and returns true/false depending on the state of the result.
    /// </summary>
    /// <param name="result">The result of the request.</param>
    /// <param name="message">The optional message of the notification to show th user if the request failed.</param>
    /// <param name="title">The optional title of the notification to show the user if the request failed.</param>
    /// <returns><c>true</c> if the result is failed; oterhwise, <c>true</c>.</returns>
    /// <remarks>
    /// This is to simplify control flow for requests that return results, since we can notify and break out
    /// of the method in a single line.
    /// </remarks>
    public bool ShowIfFailed(IResultBase result, string? message = null, string? title = null)
    {
        if (!result.IsFailed) return false;

        title ??= "Request failed";
        message ??= "An error was encountered while processing the previous request. See notifications for details.";
        ShowError(title, message);

        return true;
    }

    public record NotificationMessage(INotification Notification);

    #region CustomMessages

    public void NofityExportFailed(string name, IEnumerable<string> errors)
    {
        const string title = "Export Failed";
        var message = $"Failed to generate or save a valid export package for {name}. Click to view error details.";

        ShowError(title, message);
    }

    #endregion
}