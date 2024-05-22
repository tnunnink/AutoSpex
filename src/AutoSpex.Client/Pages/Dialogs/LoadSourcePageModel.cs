using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Persistence;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JetBrains.Annotations;
using L5Sharp.Core;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class LoadSourcePageModel(SourceObserver source) : PageViewModel
{
    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(LoadSourceCommand))]
    private L5X? _content;

    [ObservableProperty] private bool _scrubData = true;

    [ObservableProperty] private bool _updateVariables = true;


    [RelayCommand(CanExecute = nameof(CanLoadSource))]
    private async Task LoadSource(Window dialog)
    {
        if (Content is null) return;

        source.Update(Content, ScrubData);

        //todo SaveSource needs to incorporate the UpdateVariables once that is setup.
        var result = await Mediator.Send(new SaveSource(source));

        if (result.IsSuccess)
        {
            dialog.Close(source);
        }

        dialog.Close(null);
    }

    private bool CanLoadSource() => Content is not null;
}