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
        var step = DetermineStep(message.Observer);
        if (step is null) return;

        if (_source?.Model.Content is null)
        {
            message.Reply([]);
            return;
        }

        try
        {
            var index = Steps.IndexOf(step);
            var data = Model.ExecuteTo(_source.Model.Content, index);
            message.Reply(data);
        }
        catch (Exception)
        {
            message.Reply([]);
        }
    }

    /// <summary>
    /// Responds to an argument suggestion request for argument contained in filter criteria in this query instance.
    /// Since we may have an in-memory source context, we can use that to evaluate what are possible values that could
    /// be input to the criterion.
    /// </summary>
    public void Receive(ArgumentInput.SuggestionRequest message)
    {
        if (!Contains(message.Argument)) return;
        if (_source?.Model.Content is null) return;

        try
        {
            //todo this isn't quite right since the argument could be several steps in and idk what the base type is.
            //It might only make sense to execute the query to get context data and then select values based on argument (criterion) input.
            //For that we need the step associated with this argument (similar to GetDataTo).
            var property = message.Argument.Input;
            var elements = _source.Model.Content.Query(Element.This.Origin);
            var values = elements.Select(property.GetValue).Where(x => x is not null).Distinct();
            var suggestions = values.Select(v => new ValueObserver(v)).ToList();
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
    /// Gets the <see cref="Property"/> that is the input to the provided criterion or step based on where the object is
    /// in the spec configuration.
    /// </summary>
    private Property DetermineInput(Observer observer)
    {
        var property = Element.This;

        foreach (var step in Steps)
        {
            if (observer.Is(step)) return property;
            if (step is FilterObserver filter && filter.Criteria.Has(observer)) return property;
            if (step is not SelectObserver select) continue;
            property = property.GetProperty(select.Property.Path);
        }

        //We want to return a property that wraps the nested type and not one that has the nested path up to this point.
        //This is so the UI shows the correct path/property when rendering.
        return Property.This(property.Type);
    }

    /// <summary>
    /// Gets the step that contains the provided observer instance. This instance should either be a select step itslef
    /// or a nested criterion observer that is contained by a filter step in the query.
    /// </summary>
    private StepObserver? DetermineStep(Observer observer)
    {
        return observer switch
        {
            StepObserver step when Steps.Has(step) => step,
            CriterionObserver criterion when Contains(criterion) => Steps.Single(s =>
                s is FilterObserver filter && filter.Criteria.Has(observer)),
            _ => null
        };
    }

    /// <summary>
    /// Determines whether any configured (filter) step criteria contains the provided criterion.
    /// </summary>
    /// <param name="criterion">The criterion to check for.</param>
    /// <returns>True if the Steps collection contains the criterion, otherwise false.</returns>
    private bool Contains(CriterionObserver criterion)
    {
        return Steps.Where(s => s is FilterObserver).Cast<FilterObserver>().Any(f => f.Criteria.Has(criterion));
    }

    /// <summary>
    /// Determines if the specified ArgumentInput is contained in any criteria of the Steps that are FilterObservers.
    /// </summary>
    /// <param name="argument">The ArgumentInput to check for containment.</param>
    /// <returns>True if the ArgumentInput is contained in any criteria, false otherwise.</returns>
    private bool Contains(ArgumentInput argument)
    {
        return Steps
            .Where(s => s is FilterObserver)
            .Cast<FilterObserver>()
            .Any(f => f.Criteria.Any(c => c.Contains(argument)));
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