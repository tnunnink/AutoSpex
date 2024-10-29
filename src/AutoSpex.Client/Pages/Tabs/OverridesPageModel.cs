using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Pages;

public partial class OverridesPageModel : PageViewModel,
    IRecipient<Observer.GetSelected>,
    IRecipient<Observer.Deleted>
{
    private readonly SourceObserver _source;

    public OverridesPageModel(SourceObserver source) : base("Overrides")
    {
        _source = source;

        Overrides = new ObserverCollection<Node, OverrideObserver>(
            refresh: () => _source.Model.Overrides.Select(x => new OverrideObserver(x)).ToList(),
            add: (_, m) => _source.Model.AddOverride(m),
            remove: (_, m) => _source.Model.RemoveOverride(m),
            clear: () => _source.Model.ClearOverrides(),
            count: () => _source.Model.Overrides.Count()
        );

        Track(Overrides);
    }

    public override string Route => $"{nameof(Source)}/{_source.Id}/{Title}";
    public ObserverCollection<Node, OverrideObserver> Overrides { get; }
    public ObservableCollection<OverrideObserver> Selected { get; } = [];

    [RelayCommand]
    private async Task AddOverride()
    {
        var node = await Prompter.Show<NodeObserver?>(() => new SelectSpecPageModel());
        if (node is null) return;

        var result = await Mediator.Send(new LoadNode(node.Id));
        if (Notifier.ShowIfFailed(result)) return;

        var observer = new OverrideObserver(result.Value);
        Overrides.Add(observer);
    }

    public void Receive(Observer.GetSelected message)
    {
        if (message.Observer is not OverrideObserver observer) return;
        if (!Overrides.Any(s => s.Is(observer))) return;

        foreach (var item in Selected)
            message.Reply(item);
    }

    public void Receive(Observer.Deleted message)
    {
        switch (message.Observer)
        {
            case NodeObserver observer when Overrides.Any(x => x.Id == observer.Id):
                Overrides.RemoveAny(x => x.Id == observer.Id);
                Overrides.AcceptChanges();
                break;
            case OverrideObserver observer when !Overrides.Any(x => x.Is(observer)):
                Overrides.RemoveAny(x => x.Id == observer.Id);
                Overrides.AcceptChanges();
                break;
        }
    }

    protected override void FilterChanged(string? filter)
    {
        Overrides.Filter(filter);
    }
}