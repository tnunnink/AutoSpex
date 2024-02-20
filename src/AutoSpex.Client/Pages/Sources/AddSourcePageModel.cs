using System;
using System.ComponentModel.DataAnnotations;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Client.Validation;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JetBrains.Annotations;
using L5Sharp.Core;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class AddSourcePageModel : PageViewModel
{
    [ObservableProperty] [NotifyDataErrorInfo] [Required] [PathValidCharacters] [PathExists(PathType.File)]
    private string? _location;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(AddSourceCommand))]
    private SourceObserver? _source;

    [RelayCommand]
    private async Task SelectSource()
    {
        var uri = await Shell.StorageProvider.SelectL5XUri();
        if (uri is null) return;
        Location = uri.LocalPath;
        LoadSource(Location);
    }

    [RelayCommand(CanExecute = nameof(CanAddSource))]
    private async Task AddSource(Window dialog)
    {
        if (Source is null || Source.HasErrors) return;

        var result = await Mediator.Send(new CreateSource(Source.Model));

        if (result.IsSuccess)
        {
            dialog.Close(true);
        }

        dialog.Close(false);
    }

    [RelayCommand]
    private static void Cancel(Window dialog)
    {
        dialog.Close(false);
    }

    private bool CanAddSource() => !HasErrors && Source is not null && !Source.HasErrors;

    partial void OnLocationChanged(string? value)
    {
        if (string.IsNullOrEmpty(value)) return;
        LoadSource(value);
    }

    private void LoadSource(string location)
    {
        if (HasErrors)
        {
            Source = null;
            return;
        }

        try
        {
            var content = L5X.Load(location);
            var source = new Source(content);
            Source = new SourceObserver(source);
        }
        catch (Exception e)
        {
            var message = $"The source file '{location}' could not be loaded as a valid L5X file." +
                          $" Please try again or ensure the file is a valid L5X.";
            Notifier.Notify(new Notification("Failed to load source", message, NotificationType.Error));
            Source = null;
        }
    }
}