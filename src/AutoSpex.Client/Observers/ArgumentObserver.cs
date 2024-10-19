using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Argument = AutoSpex.Engine.Argument;

// ReSharper disable TailRecursiveCall

namespace AutoSpex.Client.Observers;

/// <summary>
/// A observer wrapper over the <see cref="Engine.Argument"/> class for a parent criterion object.
/// </summary>
public partial class ArgumentObserver : Observer<Argument>
{
    public ArgumentObserver(Argument argument) : base(argument)
    {
        Value = GetValue();
        Track(nameof(Value));
        Track(Value);
    }

    /// <inheritdoc />
    public override Guid Id => Model.ArgumentId;

    /// <summary>
    /// The value of the argument. This is expected to be some <see cref="ITrackable"/> observer or observer collection
    /// so that we can detect and respond to changes. The underlying value is initially set from the passed in mode
    /// depending on how it's parent criterion created it based on the selected operation. From there we either update
    /// this value directly if a binary type operations or update the nested object for inner criterion or observer collections.
    /// </summary>
    [ObservableProperty] private ITrackable _value;

    /// <summary>
    /// Gets the parent criterion that contains the argument. We need this to determine the property type and operation
    /// so that we can get suggesstable values and parse the input text correctly.
    /// </summary>
    public CriterionObserver? Criterion => GetObserver<CriterionObserver>(IsParent);

    /// <summary>
    /// The function that retrieves a collection of object values that are suggestions to the entry control.
    /// </summary>
    public Func<string?, CancellationToken, Task<IEnumerable<object>>> Suggestions => GetSuggestions;

    /// <summary>
    /// Gets a value indicating whether the Value property is empty.
    /// This is only used for basic input entries for binrary operations.
    /// </summary>
    public bool IsEmpty => Value is ValueObserver { IsEmpty: true };

    /// <summary>
    /// Updates the argument <see cref="Value"/> based on the received value type from the entry field.
    /// If user enters simple text we would like to parse it as the strong type to let our data templates work.
    /// If the user enters a variable reference (text starting with '@') then we want to find and resolve the object.
    /// If user enters a complex object, then it was selected from the suggestions, and we can just use that.
    /// Anything else just wrap in a value observer and set accordingly.
    /// </summary>
    [RelayCommand]
    private async Task UpdateValue(object? value)
    {
        var group = Criterion?.Property.Group;

        switch (value)
        {
            case string text when text.StartsWith(Reference.Prefix):
                var reference = await ResolveReference(text);
                Value = new ReferenceObserver(reference);
                return;
            case string text when group?.TryParse(text, out var parsed) is true && parsed is not null:
                Value = new ValueObserver(parsed);
                return;
            case ValueObserver observer:
                Value = observer.Model is Variable variable ? new ReferenceObserver(variable.Reference()) : observer;
                return;
            default:
                Value = new ValueObserver(value);
                break;
        }
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        //Sync the obsever value with that of the underlying model object.
        if (e.PropertyName is not nameof(Value)) return;
        SetValue(Value);
        OnPropertyChanged(nameof(IsEmpty));
    }

    /// <summary>
    /// Returns the wrapped model value if the model has a corresponding observer type.
    /// Collections are wrapped in the ObserverCollection since it implements ITrackable, so we can notify changes when the user updates the list.
    /// And default value will be wrapped in a value observer to be used with data templates
    /// </summary>
    private ITrackable GetValue()
    {
        return Model.Value switch
        {
            Reference reference => new ReferenceObserver(reference),
            Criterion criterion => new CriterionObserver(criterion),
            List<Argument> arguments => new ObserverCollection<Argument, ArgumentObserver>(arguments.ToList(),
                a => new ArgumentObserver(a)),
            _ => new ValueObserver(Model.Value)
        };
    }

    /// <summary>
    /// Sets the underlying argument value based on the provided object. This object should either be a nested observer
    /// Criterion or Reference, nested observer collection of argument observers which in turn wrap single values,
    /// or some default value wrapped in a value observer.
    /// </summary>
    private void SetValue(object? value)
    {
        Model.Value = value switch
        {
            ReferenceObserver reference => reference.Model,
            CriterionObserver criterion => criterion.Model,
            ObserverCollection<Argument, ArgumentObserver> collection => collection.Select(x => x.Model).ToList(),
            ValueObserver observer => observer.Model,
            _ => value
        };
    }

    /// <summary>
    /// Queries the database for a variable in scope with the specified name and returns it, or null if not found.
    /// </summary>
    private async Task<Reference> ResolveReference(string name)
    {
        var scoped = await GetScopedVariables(string.Empty, CancellationToken.None);

        return scoped.FirstOrDefault(v => v.Name == name)?.Model is Variable match
            ? match.Reference()
            : new Reference(name);
    }

    /// <summary>
    /// Executes the code to retrieve possible suggestion values based on the current argument's type and scope,
    /// and returns a collection of values which are filtered based on the provided input text.
    /// </summary>
    private async Task<IEnumerable<object>> GetSuggestions(string? filter, CancellationToken token)
    {
        var suggestions = new List<object>();

        var options = GetOptions(filter);
        suggestions.AddRange(options);

        //todo get possible source values based on type/property of the parent criterion

        var variables = await GetScopedVariables(filter, token);
        suggestions.AddRange(variables);

        return suggestions;
    }

    /// <summary>
    /// Query the database for variables that are in scope of this argument. These are variables that belong to or are
    /// inherited by the parent node object. Therefore, we first request the parent node and the use it's id to fetch
    /// variable objects as suggestions to plug into the argument value for the user.
    /// </summary>
    private async Task<IEnumerable<ValueObserver>> GetScopedVariables(string? filter, CancellationToken token)
    {
        //Remove the prefix '@' so we can return all variables we want to reference.
        filter = filter?.TrimStart(Reference.Prefix);

        //Find the node to which this argument belongs.
        //This should be in memory since page rendering this argument contains the node,
        //but if not found will just return empty collection.
        //Remember we are doing in memory retrieval of this node id becuause this data may not yet be persisted.
        var node = GetObserver<NodeObserver>(x =>
            x.Model.Spec.Filters.Any(f => f.Contains(Id)) || x.Model.Spec.Verifications.Any(v => v.Contains(Id)));

        if (node is null) return [];

        var variables = await Mediator.Send(new GetScopedVariables(node.Id), token);
        return variables.Select(v => new ValueObserver(v)).Where(v => v.Filter(filter));
    }

    /// <summary>
    /// Gets potential option values for the parent criterion property which are potential values to the argument input. 
    /// </summary>
    private IEnumerable<ValueObserver> GetOptions(string? filter)
    {
        var type = Criterion?.Property.Type;
        var group = Criterion?.Property.Group;

        if (type is null) return [];

        if (group == TypeGroup.Boolean)
            return type.GetOptions().Select(x => new ValueObserver(x));

        return group == TypeGroup.Enum
            ? type.GetOptions().Select(x => new ValueObserver(x)).Where(v => v.Filter(filter))
            : [];
    }

    /// <summary>
    /// Determines whether the specified criterion is a parent of the current argument observer.
    /// </summary>
    /// <param name="criterion">The criterion to check if it is a parent.</param>
    /// <returns>True if the criterion is a parent, otherwise false.</returns>
    private bool IsParent(CriterionObserver criterion)
    {
        if (criterion.Argument.Id == Id) return true;
        if (criterion.Argument.Value is not ObserverCollection<Argument, ArgumentObserver> collection) return false;
        if (collection.Any(a => a.Id == Id)) return true;
        return false;
    }

    public static implicit operator Argument(ArgumentObserver observer) => observer.Model;
    public static implicit operator ArgumentObserver(Argument model) => new(model);
}