using System;
using AutoSpex.Client.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoSpex.Client.Pages;

public partial class InfoPageModel(Observer observer) : PageViewModel("Details")
{
    public override string Route => $"{Observer.Icon}/{Observer.Id}/{Title}";
    public override string Icon => "IconLineInfo";
    public Observer Observer { get; } = observer;

    [ObservableProperty] private string? _createdOn;

    [ObservableProperty] private string? _createdBy;

    [ObservableProperty] private string? _updatedOn;

    [ObservableProperty] private string? _updatedBy;
    

    [RelayCommand]
    private async Task CopyId()
    {
        var clipboard = Shell.Clipboard;
        if (clipboard is null) return;
        await clipboard.SetTextAsync(Observer.Id.ToString());
        Notifier.Notify($"{Observer.Name} Id Copied", "Successfully copied Id to clipboard");
    }

    /*/// <summary>
    /// Set the local created properties using the loaded changes for this entity.
    /// </summary>
    private void PopuplateCreatedMessage()
    {
        var change = Changes
            .Where(c => c.ChangeType == ChangeType.Created)
            .MinBy(c => c.ChangedOn);
        
        if (change is null) return;

        CreatedBy = change.ChangedBy;
        CreatedOn = TimeAgo(change.ChangedOn);
    }

    /// <summary>
    /// Set the local updated properties using the loaded changes for this entity.
    /// </summary>
    private void PopuplateUpdatedMessage()
    {
        var change = Changes.MaxBy(c => c.ChangedOn);
        
        if (change is null) return;

        UpdatedBy = change.ChangedBy;
        UpdatedOn = TimeAgo(change.ChangedOn);
    }*/

    /// <summary>
    /// Gets the string of how long ago the change was.
    /// </summary>
    private static string TimeAgo(DateTime dateTime)
    {
        var timeSpan = DateTime.Now - dateTime;
        if (timeSpan <= TimeSpan.FromSeconds(60)) return timeSpan.Seconds + " seconds ago";
        if (timeSpan <= TimeSpan.FromMinutes(60)) return timeSpan.Minutes + " minutes ago";
        if (timeSpan <= TimeSpan.FromHours(24)) return timeSpan.Hours + " hours ago";
        if (timeSpan <= TimeSpan.FromDays(30)) return timeSpan.Days + " days ago";
        if (timeSpan <= TimeSpan.FromDays(365)) return timeSpan.Days / 30 + " months ago";
        return timeSpan.Days / 365 + " years ago";
    }
}