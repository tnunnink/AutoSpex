using System;
using System.Linq;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Observers;

public partial class QueryObserver : Observer<Query>,
    IRecipient<Observer.Deleted>,
    IRecipient<PropertyInput.GetInputTo>,
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
    /// </summary>
    [RelayCommand]
    private void AddFitlerStep()
    {
        Steps.Add(new FilterObserver(new Filter()));
    }

    /// <summary>
    /// Command to add a <see cref="Select"/> step to this query.
    /// </summary>
    [RelayCommand]
    private void AddSelectStep()
    {
        Steps.Add(new SelectObserver(new Select()));
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
    /// Handles the request message by replying if this query contains the provided instance.
    /// The received type can be either a step or criterion. If we contain either instance, we will determine the
    /// input to the observer using the current query configuration.
    /// </summary>
    public void Receive(PropertyInput.GetInputTo message)
    {
        if (message.HasReceivedResponse) return;

        if (message.Observer is StepObserver step && Steps.Has(step))
        {
            message.Reply(DetermineInput(step));
        }

        if (message.Observer is CriterionObserver criterion && Contains(criterion))
        {
            message.Reply(DetermineInput(criterion));
        }
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
        if (!Contains(message.Argument)) return;
        if (!TryGetStep(message.Argument, out var step)) return;

        try
        {
            var index = Steps.IndexOf(step);
            var data = Model.ExecuteTo(_source.Model.Content, index);
            var criterion = step.Criteria.Single(c => c.Contains(message.Argument));
            var property = criterion.Property.Value;
            var values = data.Select(property.GetValue).Where(x => x is not null).Distinct();
            var suggestions = values.Select(v => new ValueObserver(v)).Where(s => !s.IsEmpty).ToList();
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
    /// Gets the <see cref="Property"/> that is the input to the provided observer based on where the object is
    /// in the spec configuration.
    /// </summary>
    private Property DetermineInput(Observer observer)
    {
        var property = Element.This;

        foreach (var step in Steps)
        {
            if (step.Is(observer) || step.Criteria.Has(observer)) break;
            if (step is not SelectObserver select) continue;
            //When we get the child property, we want to handle a couple cases.
            //1. We want the "inner" type property (so collections/arrays is actually the type of the collection).
            //2. We want this new property to be standalone (not like a nested property).
            property = Property.This(property.GetProperty(select.Model.Property).InnerType);
        }
        
        return property;
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
            CriterionObserver criterion => Steps.SingleOrDefault(s => s.Criteria.Has(criterion)),
            ArgumentInput argument => Steps.SingleOrDefault(s => s.Criteria.Any(c => c.Contains(argument))),
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
    /// Determines whether any configured (filter) step criteria contains the provided criterion.
    /// </summary>
    /// <param name="criterion">The criterion to check for.</param>
    /// <returns>True if the Steps collection contains the criterion, otherwise false.</returns>
    private bool Contains(CriterionObserver criterion) => Steps.Any(s => s.Criteria.Has(criterion));

    /// <summary>
    /// Determines if the specified ArgumentInput is contained in any criteria of the Steps that are FilterObservers.
    /// </summary>
    /// <param name="argument">The ArgumentInput to check for containment.</param>
    /// <returns>True if the ArgumentInput is contained in any criteria, false otherwise.</returns>
    private bool Contains(ArgumentInput argument) => Steps.Any(s => s.Criteria.Any(c => c.Contains(argument)));

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