using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Pages;

public partial class SpecsPageModel : PageViewModel,
    IRecipient<Observer.Created>,
    IRecipient<Observer.Deleted>
{
    /// <inheritdoc/>
    public SpecsPageModel(NodeObserver node)
    {
        Node = node;
    }

    public override string Route => $"{Node.Type}/{Node.Id}/{Title}";
    public override string Title => "Specs";
    public NodeObserver Node { get; }
    public ObserverCollection<Spec, SpecObserver> Specs { get; private set; } = [];
    public ObservableCollection<SpecObserver> Selected { get; } = [];

    [ObservableProperty] private string? _filter;

    public override async Task Load()
    {
        var result = await Mediator.Send(new ListSpecsIn(Node.Id));
        if (result.IsFailed) return;
        Specs = new ObserverCollection<Spec, SpecObserver>(result.Value.ToList(), s => new SpecObserver(s));
    }

    public void Receive(Observer.Created message)
    {
        if (message.Observer is not NodeObserver node || node.Type != NodeType.Spec) return;
    }

    public void Receive(Observer.Deleted message)
    {
        if (message.Observer is not NodeObserver node || node.Type != NodeType.Spec) return;
        Specs.RemoveAny(s => s.Id == node.Id);
    }

    partial void OnFilterChanged(string? value)
    {
        Specs.Filter(s => s.Node.Filter(value) || s.Element.Name.Satisfies(value));
    }
}