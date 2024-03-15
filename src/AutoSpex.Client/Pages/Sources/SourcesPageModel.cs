using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Shared;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JetBrains.Annotations;
using SourceObserver = AutoSpex.Client.Observers.SourceObserver;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class SourcesPageModel : PageViewModel
{
    public override string Title => "Sources";
    public override string Icon => "Sources";

    [ObservableProperty] private ObservableCollection<SourceObserver> _sources = [];

    public override async Task Load()
    {
        var result = await Mediator.Send(new ListSources());

        if (result.IsSuccess)
        {
            Sources = new ObservableCollection<SourceObserver>(result.Value.Select(s => new SourceObserver(s)));
        }
    }

    [RelayCommand]
    private async Task AddSource()
    {
        var source = await Prompter.Show<SourceObserver?>(new AddSourcePageModel());
        if (source is null) return;
        Sources.Add(source);
        await Navigator.Navigate(source);
    }
}