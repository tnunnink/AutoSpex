using System;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoSpex.Client.Pages;

public partial class RunPageModel(RunObserver run) : PageViewModel
{
    [ObservableProperty] private RunObserver _run = run;

    [ObservableProperty] private NodeObserver? _node;

    [ObservableProperty] private SourceObserver? _source;

    public override async Task Load()
    {
        throw new NotImplementedException();
    }

    [RelayCommand]
    private async Task Execute()
    {
        throw new NotImplementedException();
    }

    [RelayCommand]
    private Task Cancel()
    {
        throw new NotImplementedException();
    }
}