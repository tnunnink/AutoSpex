using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Observers;

/// <summary>
/// Base class for all step observer types.
/// This observer implments the common criteria collection functionality.
/// </summary>
public abstract partial class StepObserver : Observer<Step>,
    IRecipient<Observer.Deleted>,
    IRecipient<Observer.GetSelected>,
    IRecipient<Observer.Get<StepObserver>>
{
    protected StepObserver(Step model) : base(model)
    {
        Criteria = new ObserverCollection<Criterion, CriterionObserver>
        (
            refresh: () => Model.Criteria.Select(c => new CriterionObserver(c)).ToList(),
            add: (_, m) => Model.Add(m),
            remove: (_, m) => Model.Remove(m),
            move: (o, n) => Model.Move(o, n),
            clear: () => Model.Clear(),
            count: () => Model.Criteria.Count()
        );

        Track(Criteria);
    }

    public ObserverCollection<Criterion, CriterionObserver> Criteria { get; }
    public ObservableCollection<CriterionObserver> Selected { get; } = [];

    /// <summary>
    /// Adds a filter to the specification.
    /// </summary>
    [RelayCommand]
    private void AddCriteria()
    {
        Criteria.Add(new CriterionObserver(new Criterion()));
    }

    /// <summary>
    /// Command to add the criteria copied to the clipboard to the current step.
    /// </summary>
    [RelayCommand]
    private async Task PasteCriteria()
    {
        var criteria = await GetClipboardObservers<Criterion>();
        var copies = criteria.Select(c => new CriterionObserver(c.Duplicate()));
        Criteria.AddRange(copies);
    }

    /// <summary>
    /// If a criterion delete message is received we will delete all selected criterion from the list.
    /// </summary>
    public void Receive(Deleted message)
    {
        if (message.Observer is not CriterionObserver observer) return;
        Criteria.RemoveAny(x => x.Is(observer));
    }

    /// <summary>
    /// Handle reception of the get selected message by replying with the selected criteria.
    /// </summary>
    public void Receive(GetSelected message)
    {
        if (message.Observer is not CriterionObserver criterion) return;
        if (!Criteria.Any(x => x.Is(criterion))) return;

        foreach (var observer in Selected)
            message.Reply(observer);
    }
    
    /// <summary>
    /// Handles the request to get the observer that passes the provied predicate.
    /// This allows child criteria to have access to the step that contains them.
    /// </summary>
    public void Receive(Get<StepObserver> message)
    {
        if (message.HasReceivedResponse) return;

        if (message.Predicate.Invoke(this))
        {
            message.Reply(this);
        }
    }
}

/// <summary>
/// Base class for all step observer types.
/// This observer implements the common criteria collection functionality.
/// </summary>
public abstract class StepObserver<TStep> : StepObserver where TStep : Step
{
    /// <summary>
    /// Base class for all step observer types.
    /// This observer implements the common criteria collection functionality.
    /// </summary>
    protected StepObserver(TStep model) : base(model)
    {
        Model = model;
    }
    
    /// <summary>
    /// The underlying model object that is being wrapped by the observer.
    /// </summary>
    protected new TStep Model { get; }
}