using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Pages;

public partial class SuppressPageModel : PageViewModel,
    IRecipient<Observer.GetSelected>,
    IRecipient<Observer.Deleted>
{
    private readonly SourceObserver _source;

    /// <inheritdoc/>
    public SuppressPageModel(SourceObserver source) : base("Suppress")
    {
        _source = source;

        Suppressions = new ObserverCollection<Suppression, SuppressionObserver>(
            refresh: () => _source.Model.Suppressions.Select(s => new SuppressionObserver(s)).ToList(),
            add: (_, s) => _source.Model.AddSuppression(s),
            remove: (_, s) => _source.Model.RemoveSuppression(s),
            count: () => _source.Model.Suppressions.Count()
        );

        Track(Suppressions);
    }

    public override string Route => $"{nameof(Source)}/{_source.Id}/{Title}";
    public ObserverCollection<Suppression, SuppressionObserver> Suppressions { get; }
    public ObservableCollection<SuppressionObserver> Selected { get; } = [];


    [RelayCommand]
    private async Task AddSuppression()
    {
        var suppression = await Prompter.Show<SuppressionObserver?>(() => new AddSuppressionPageModel());
        if (suppression is null) return;
        Suppressions.Add(suppression);
    }

    [RelayCommand(CanExecute = nameof(CanAddNode))]
    private void AddNode(object? input)
    {
        if (input is not NodeObserver observer) return;

        //todo prompt the user for a reason
        var reason = "These don't apply";

        var suppressions = observer.Model.Descendants(NodeType.Spec).Select(n => new Suppression(n.NodeId, reason));
        Suppressions.AddRange(suppressions.Select(s => new SuppressionObserver(s)));
    }

    private static bool CanAddNode(object? input) => input is NodeObserver;


    public void Receive(Observer.GetSelected message)
    {
        if (message.Observer is not SuppressionObserver observer) return;
        if (!Suppressions.Any(s => s.Is(observer))) return;

        foreach (var item in Selected)
            message.Reply(item);
    }

    public void Receive(Observer.Deleted message)
    {
        switch (message.Observer)
        {
            case SuppressionObserver suppression when Suppressions.Any(s => s.Is(suppression)):
                Suppressions.Remove(suppression);
                Suppressions.AcceptChanges();
                break;
            case NodeObserver node when Suppressions.Any(s => s.Model.NodeId == node.Id):
                Suppressions.RemoveAny(s => s.Model.NodeId == node.Id);
                Suppressions.AcceptChanges();
                break;
        }
    }

    protected override void FilterChanged(string? filter)
    {
        Suppressions.Filter(filter);
    }
}