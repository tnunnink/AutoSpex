using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using L5Sharp.Core;

namespace AutoSpex.Client.Pages;

public partial class NewSourcePageModel : PageViewModel
{
    private readonly List<string> _sourceNames = [];

    [ObservableProperty] private FileInfo? _file;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    private SourceObserver? _source;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    [CustomValidation(typeof(NewSourcePageModel), nameof(ValidateSourceName))]
    [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    private string? _name;

    public override async Task Load()
    {
        var names = await Mediator.Send(new ListSourceNames());
        _sourceNames.AddRange(names);
    }

    [RelayCommand]
    private async Task Browse()
    {
        var path = await Shell.StorageProvider.SelectSourceFile();
        if (path is null) return;
        await LoadSource(path);
    }

    [RelayCommand]
    private async Task Drop(object? data)
    {
        if (data is not IEnumerable<IStorageItem> files) return;
        var file = files.FirstOrDefault();
        if (file is null) return;
        await LoadSource(file.Path);
    }

    [RelayCommand(CanExecute = nameof(CanAdd))]
    private void Add(Window window)
    {
        if (Source is null) return;
        if (string.IsNullOrEmpty(Name)) return;

        Source.Model.Name = Name;
        window.Close(Source);
    }

    private bool CanAdd() => Source is not null && !IsErrored;

    /// <summary>
    /// Attempts to read and load the L5X content from the provided Uri path. 
    /// </summary>
    private async Task LoadSource(Uri path)
    {
        try
        {
            var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var content = await L5X.LoadAsync(path.LocalPath, cancellation.Token);
            Source = new Source(content);
            Name = Source.Name;
            File = new FileInfo(path.LocalPath);
        }
        catch (Exception e)
        {
            Notifier.ShowError("Load Failure", $"Failed to load the selected source file with error {e.Message}.");
        }
    }

    /// <summary>
    /// Custom validation for the source name which must be unique in the application.
    /// </summary>
    public static ValidationResult? ValidateSourceName(object? value, ValidationContext context)
    {
        if (value is not string text)
            throw new InvalidOperationException("Expecting text name input.");

        if (context.ObjectInstance is not NewSourcePageModel page)
            throw new InvalidOperationException("Expecting NewSourcePageModel object instance.");

        if (page._sourceNames.Contains(text))
        {
            return new ValidationResult("Source name already exists in the application. Name must be unique.");
        }

        return ValidationResult.Success;
    }
}