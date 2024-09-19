using System;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class SourceSettingsPageModel(SourceObserver source) : PageViewModel("Source Settings")
{
    public SourceObserver Source { get; } = source;

    [ObservableProperty] private string _name = source.Name;

    [ObservableProperty] private string _location = source.LocalPath;

    [RelayCommand]
    private async Task Commit()
    {
        Source.Name = Name;
    }
}