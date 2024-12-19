using System.Collections.Specialized;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;

namespace AutoSpex.Client.Observers;

public class SelectObserver : StepObserver<Select>
{
    public SelectObserver(Select model) : base(model)
    {
        Selections = new ObserverCollection<Selection, SelectionObserver>(Model.Selections,
            s => new SelectionObserver(s, DetermineInput)
        );

        Track(Selections);
        Selections.CollectionChanged += OnCriteriaChanged;
    }

    /// <summary>
    /// The collection of <see cref="Selection"/> that define the step.
    /// </summary>
    public ObserverCollection<Selection, SelectionObserver> Selections { get; }

    /// <inheritdoc />
    /// <remarks>Also unsubscribe from the change event.</remarks>
    protected override void OnDeactivated()
    {
        Selections.CollectionChanged -= OnCriteriaChanged;
        base.OnDeactivated();
    }

    /// <summary>
    /// When the criteria collection changes for a filter step we want to react by changing ths visuals.
    /// Refresh the binding for ShowMatch to allow the user to display the match button.
    /// If There are no longer any criteria automatically delete the entire step.
    /// </summary>
    private void OnCriteriaChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (Selections.Count == 0)
        {
            DeleteCommand.Execute(null);
        }
    }
}