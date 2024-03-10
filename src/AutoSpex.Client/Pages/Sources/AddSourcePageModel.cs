﻿using System;
using System.ComponentModel.DataAnnotations;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Client.Validation;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
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

    [ObservableProperty] private IStorageItem? _droppedItem;

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
            Messenger.Send(new SourceObserver.Created(Source));
            dialog.Close(Source);
        }

        dialog.Close(null);
    }

    private bool CanAddSource() => !HasErrors && Source is not null && !Source.HasErrors;

    partial void OnLocationChanged(string? value)
    {
        if (string.IsNullOrEmpty(value)) return;
        LoadSource(value);
    }

    partial void OnDroppedItemChanged(IStorageItem? value)
    {
        var path = value?.Path.LocalPath;
        if (string.IsNullOrEmpty(path)) return;
        LoadSource(path);
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