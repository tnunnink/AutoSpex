using System;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class CriteriaPageModel : PageViewModel,
    IRecipient<Observer.Deleted>,
    IRecipient<Observer.MakeCopy>
{
    private readonly NodeObserver _node;

    /// <inheritdoc/>
    public CriteriaPageModel(NodeObserver node) : base("Criteria")
    {
        _node = node;

        Specs = new ObserverCollection<Spec, SpecObserver>(
            refresh: () => _node.Model.Specs.Select(m => new SpecObserver(m)).ToList(),
            add: (_, m) => _node.Model.AddSpec(m),
            remove: (_, n) => _node.Model.RemoveSpec(n),
            clear: () => _node.Model.ClearSpecs(),
            count: () => _node.Model.Specs.Count()
        );

        Track(Specs);
    }

    public override string Route => $"Spec/{_node.Id}/{Title}";

    public ObserverCollection<Spec, SpecObserver> Specs { get; }

    /// <summary>
    /// Adds a new Spec to the Specs collection.
    /// </summary>
    /// <param name="element">The Element object to be used for creating the new Spec.</param>
    [RelayCommand]
    private void AddSpec(Element? element)
    {
        if (element is null) return;
        Specs.Add(new SpecObserver(new Spec(element)));
    }

    /// <summary>
    /// Receives a message of type Observer.Deleted and performs the necessary actions.
    /// </summary>
    /// <remarks>
    /// This method is a recipient of the Observer.Deleted message and is triggered when an Observer is deleted.
    /// It removes the specified SpecObserver from the Specs collection if its Id matches the Id of the deleted spec.
    /// </remarks>
    /// <param name="message">The Observer.Deleted message containing the Observer to be deleted.</param>
    public void Receive(Observer.Deleted message)
    {
        if (message.Observer is not SpecObserver spec) return;
        Specs.RemoveAny(x => x.Id == spec.Id);
    }

    /// <summary>
    /// Handle reception of messages to make a copy of a given spec instance. Check that the instance comes from
    /// this node object, and that it is indeed a spec, and then create and add the duplicate.
    /// </summary>
    public void Receive(Observer.MakeCopy message)
    {
        if (message.Observer is not SpecObserver spec) return;
        if (!Specs.Contains(spec)) return;

        var duplicate = new SpecObserver(spec.Model.Duplicate());
        Specs.Add(duplicate);
    }
}