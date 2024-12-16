using System;
using System.Linq;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Observers;

public partial class QueryObserver : Observer<Query>,
    IRecipient<Observer.Deleted>,
    IRecipient<StepObserver.GetInputTo>,
    IRecipient<PropertyInput.GetDataTo>,
    IRecipient<ArgumentInput.SuggestionRequest>
{
    /// <summary>
    /// An optional source that can be used to get data by executing this query.
    /// This is needed to suggest data for property inputs.
    /// </summary>
    private readonly SourceObserver? _source;

    public QueryObserver(Query model, SourceObserver? source = default) : base(model)
    {
        _source = source;

        Steps = new ObserverCollection<Step, StepObserver>(Model.Steps, InstantiateStep);
        
        Track(nameof(Element));
        Track(Steps);
    }

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
    private void AddFitlerStep()
    {
        var step = new Filter(new Criterion());
        Steps.Add(new FilterObserver(step));
    }

    /// <summary>
    /// Command to add a <see cref="Select"/> step to this query.
    /// </summary>
    [RelayCommand]
    private void AddSelectStep()
    {
        var step = new Select(new Selection());
        Steps.Add(new SelectObserver(step));
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
    /// Responds to the request for determining the step input proeprty for this query observer.
    /// If this query contains the requesting step then reply with the current configured input to the step.
    /// </summary>
    public void Receive(StepObserver.GetInputTo message)
    {
        if (message.HasReceivedResponse) return;
        if (!Steps.Has(message.Step)) return;
        var input = Model.InputTo(message.Step.Model);
        message.Reply(input);
    }

    /// <summary>
    /// Responds to the request for data to a particular point in a query step.
    /// This is sent by a requesting <see cref="PropertyInput"/> observer.
    /// The expected observer type is either a select or criterion.
    /// </summary>
    public void Receive(PropertyInput.GetDataTo message)
    {
        if (_source?.Model.Content is null) return;
        if (!TryGetStep(message.Observer, out var step)) return;

        try
        {
            var index = Steps.IndexOf(step);
            var data = Model.ExecuteTo(_source.Model.Content, index).ToList();
            data.ForEach(message.Reply);
        }
        catch (Exception)
        {
            // Ignored because this is just optional.
            // It's only to suggest possible values based on a known source content and current property type.
            // If getting object value fails then it could be because the user configured the criterion incorrectly.
        }
    }

    /// <summary>
    /// Responds to an argument suggestion request for argument contained in filter criteria in this query instance.
    /// Since we may have an in-memory source context, we can use that to evaluate what are possible values that could
    /// be input to the criterion.
    /// </summary>
    public void Receive(ArgumentInput.SuggestionRequest message)
    {
        if (_source?.Model.Content is null) return;
        if (!TryGetStep(message.Argument, out var step)) return;
        if (!step.TryFind(c => c.Contains(message.Argument), out var criterion)) return;

        try
        {
            var index = Steps.IndexOf(step);
            var data = Model.ExecuteTo(_source.Model.Content, index);
            var values = criterion.ValuesFor(message.Argument, data);
            var suggestions = values.Select(v => new ValueObserver(v)).Where(x => !x.IsEmpty).ToList();
            suggestions.ForEach(message.Reply);
        }
        catch (Exception)
        {
            // Ignored because this is just optional.
            // It's only to suggest possible values based on a known source content and current property type.
            // If getting object value fails then it could be because the user configured the criterion incorrectly.
        }
    }

    /// <summary>
    /// Gets the step that contains the provided observer instance. This instance should either be a select step itslef
    /// or a nested criterion observer that is contained by a filter step in the query.
    /// </summary>
    private bool TryGetStep(Observer observer, out StepObserver step)
    {
        var target = observer switch
        {
            StepObserver self when Steps.Has(self) => self,
            CriterionObserver criterion => Steps.SingleOrDefault(s => s.Contains(criterion)),
            SelectionObserver selection => Steps.SingleOrDefault(s => s.Contains(selection)),
            ArgumentInput argument => Steps.SingleOrDefault(s => s.Contains(argument)),
            _ => default
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
            _ => throw new ArgumentOutOfRangeException(nameof(step))
        };
    }
}