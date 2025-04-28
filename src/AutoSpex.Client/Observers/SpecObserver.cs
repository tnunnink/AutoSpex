using System;
using System.Linq;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Observers;

public partial class SpecObserver : Observer<Spec>,
    IRecipient<Observer.Get<SpecObserver>>,
    IRecipient<Observer.Deleted>,
    IRecipient<StepObserver.GetInputTo>
{
    public SpecObserver(Spec model) : base(model)
    {
        Steps = new ObserverCollection<Step, StepObserver>(Model.Steps.ToList(), InstantiateStep);
        Track(nameof(Element));
        Track(Steps);
    }

    public override Guid Id => Model.SpecId;

    /// <summary>
    /// The element the query is configured to get.
    /// </summary>
    public Element Element
    {
        get => Model.Element;
        set => SetProperty(Model.Element, value, Model, (s, v) => s.Element = v);
    }

    /// <summary>
    /// The collection of steps that define the query. These can be either FilterObserver or SelectObserver, so we need
    /// to use a generic Observer type in the collection.
    /// </summary>
    public ObserverCollection<Step, StepObserver> Steps { get; }

    /// <summary>
    /// Updates the specification query element type and resets all the criteria. We may not do this in the future
    /// if I can get better error handling for the criterion observer.
    /// </summary>
    [RelayCommand]
    private void UpdateElement(Element? element)
    {
        if (element is null) return;
        Element = element;
    }

    /// <summary>
    /// Command to add a <see cref="Filter"/> step to this query.
    /// Each filter step will be automatically initialized with a default criterion instance.
    /// </summary>
    [RelayCommand]
    private void AddFilterStep()
    {
        Model.AddStep(new Filter(new Criterion()));
        Steps.Refresh();
    }

    /// <summary>
    /// Command to add a <see cref="Select"/> step to this query.
    /// </summary>
    [RelayCommand]
    private void AddSelectStep()
    {
        Model.AddStep(new Select(new Selection()));
        Steps.Refresh();
    }

    /// <summary>
    /// Command to add a <see cref="Select"/> step to this query.
    /// </summary>
    [RelayCommand]
    private void AddVerification()
    {
        Model.Verify(new Criterion());
        Steps.Refresh();
    }

    /// <summary>
    /// Handles the request to get the spec observer that passes the provided predicate. This allows child criteria
    /// to have access to the spec that contains them.
    /// </summary>
    public void Receive(Get<SpecObserver> message)
    {
        if (message.HasReceivedResponse) return;

        if (message.Predicate.Invoke(this))
        {
            message.Reply(this);
        }
    }

    /// <summary>
    /// Handle deletion of a given step instance for this query.
    /// </summary>
    public void Receive(Deleted message)
    {
        switch (message.Observer)
        {
            case FilterObserver filter when Steps.Has(filter):
                Steps.Remove(filter);
                break;
            case SelectObserver select when Steps.Has(select):
                Steps.Remove(select);
                break;
        }
    }

    /// <summary>
    /// Responds to the request for determining the step input property for this query observer.
    /// </summary>
    public void Receive(StepObserver.GetInputTo message)
    {
        if (message.HasReceivedResponse) return;
        if (!Steps.Has(message.Step)) return;
        var input = Model.GetInputTo(message.Step.Model);
        message.Reply(input);
    }

    /// <summary>
    /// Gets the step that contains the provided observer instance. This instance should either be a select step itself
    /// or a nested criterion observer contained by a filter step in the query.
    /// </summary>
    // ReSharper disable once UnusedMember.Local we may use this later
    private bool TryGetStep(Observer observer, out StepObserver step)
    {
        var target = observer switch
        {
            StepObserver self when Steps.Has(self) => self,
            CriterionObserver criterion => Steps.SingleOrDefault(s => s.Contains(criterion)),
            SelectionObserver selection => Steps.SingleOrDefault(s => s.Contains(selection)),
            ArgumentInput argument => Steps.SingleOrDefault(s => s.Contains(argument)),
            PropertyInput property => Steps.SingleOrDefault(s => s.Contains(property)),
            _ => null
        };

        if (target is null)
        {
            step = null!;
            return false;
        }

        step = target;
        return true;
    }

    /// <summary>
    /// Instantiates a StepObserver based on the type of Step provided.
    /// </summary>
    /// <param name="step">The Step object to instantiate an observer for.</param>
    /// <returns>The instantiated StepObserver for the provided Step object.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the Step type is not recognized.</exception>
    private static StepObserver InstantiateStep(Step step)
    {
        return step switch
        {
            Filter filter => new FilterObserver(filter),
            Select select => new SelectObserver(select),
            Verify verify => new VerifyObserver(verify),
            _ => throw new ArgumentOutOfRangeException(nameof(step))
        };
    }

    public static implicit operator SpecObserver(Spec model) => new(model);
    public static implicit operator Spec(SpecObserver observer) => observer.Model;
}