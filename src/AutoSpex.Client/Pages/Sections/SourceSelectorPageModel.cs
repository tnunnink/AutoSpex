using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class SourceSelectorPageModel : PageViewModel
{
    public override bool Reload => true;
    public ObserverCollection<Source, SourceObserver> Sources { get; } = [];

    public override async Task Load()
    {
        var result = await Mediator.Send(new ListSources());
        Sources.Bind(result.ToList(), x => new SourceObserver(x));
        RegisterDisposable(Sources);
    }

    [RelayCommand]
    private async Task AddSource()
    {
        var source = await Prompter.Show<SourceObserver?>(() => new NewSourcePageModel());
        if (source is null) return;

        var result = await Mediator.Send(new CreateSource(source));
        if (Notifier.ShowIfFailed(result)) return;

        Sources.Add(source);
        Messenger.Send(new Observer.Created<SourceObserver>(source));
        await Navigator.Navigate(source);
    }

    protected override void FilterChanged(string? filter)
    {
        Sources.Filter(filter);
    }
}