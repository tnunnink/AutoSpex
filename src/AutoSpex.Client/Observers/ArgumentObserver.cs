using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
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
        Track(nameof(Value));
    }

    public ArgumentObserver() : this(new Argument())
    {
    }

    /// <inheritdoc />
    public override Guid Id => Model.ArgumentId;

    /// <summary>
    /// Indicates that the argument's value is an inner criterion object itself. This controls whether we will display
    /// a simple argument entry field or a nested criterion entry field.
    /// </summary>
    public bool IsCriterion => Value.Group == TypeGroup.Criterion;

    /// <summary>
    /// Indicates that the argument's value is an collection of values.
    /// </summary>
    public bool IsCollection => Value.Group == TypeGroup.Collection;

    /// <summary>
    /// The value of the argument which represents the data in which the criterion will evaluate against. This
    /// is ultimately the persisted value.
    /// </summary>
    public ValueObserver Value
    {
        get => new(Model.Value);
        set => SetProperty(new ValueObserver(Model.Value), value, Model, (a, v) => a.Value = v.Model);
    }

    /// <summary>
    /// The function that retrieves a collection of object values that are suggestions to the entry control.
    /// </summary>
    public Func<string?, CancellationToken, Task<IEnumerable<object>>> Suggestions => GetSuggestions;

    /// <inheritdoc />
    public override string ToString() => Value.ToString();

    /// <summary>
    /// Updates the argument <see cref="Value"/> based on the received value type from the entry field.
    /// If user enters simple text we would like to parse it as the strong type to let our data templates work.
    /// If the user enters a variable reference (text starting with '@') then we want to find and resolve the object.
    /// If user enters a complex object then it was selected from the suggestions, and we can just use that.
    /// Anything else just wrap in a value observer and set accordingly.
    /// </summary>
    [RelayCommand]
    private async Task UpdateValue(object? value)
    {
        var criterion = FindParent<CriterionObserver>();
        var group = criterion?.Property.Group;

        switch (value)
        {
            case string text when text.StartsWith(Reference.Prefix):
                var reference = await ResolveReference(text);
                Value = new ValueObserver(reference);
                return;
            case string text when group?.TryParse(text, out var parsed) is true && parsed is not null:
                Value = new ValueObserver(parsed);
                return;
            case ValueObserver observer:
                //Set to the variable reference and not the actual variable object itself.
                Value = observer.Model is Variable variable ? new ValueObserver(variable.Reference()) : observer;
                return;
            default:
                Value = new ValueObserver(value);
                break;
        }
    }

    /// <summary>
    /// Queries the database for a variable in scope with the specified name and returns it, or null if not found.
    /// </summary>
    private async Task<object?> ResolveReference(string name)
    {
        var scoped = await GetScopedVariables(string.Empty, CancellationToken.None);
        var match = scoped.FirstOrDefault(v => v.Name == name) as object;
        return match ?? new Reference(name);
    }

    /// <summary>
    /// Executes the code to retrieve possible suggestion values based on the current argument's type and scope,
    /// and returns a collection of values which are filtered based on the provided input text.
    /// </summary>
    private async Task<IEnumerable<object>> GetSuggestions(string? filter, CancellationToken token)
    {
        var suggestions = new List<object>();

        suggestions.AddRange(GetOptions(filter));

        var variables = await GetScopedVariables(filter, token);
        suggestions.AddRange(variables);

        return suggestions;
    }

    /// <summary>
    /// Query the database for variables that are in scope of this argument. These are variables that belong to or are
    /// inherited by the parent spec object. Therefore, we first request the parent spec and the use it's id to fetch
    /// variable objects as suggestions to plug into the argument value for the user.
    /// </summary>
    private async Task<IEnumerable<ValueObserver>> GetScopedVariables(string? filter, CancellationToken token)
    {
        var spec = FindParent<SpecObserver>();

        if (spec is null) return Enumerable.Empty<ValueObserver>();

        var result = await Mediator.Send(new GetScopedVariables(spec.Id), token);

        //Remove the prefix '@' so we can return all variables we want to reference.
        filter = filter?.TrimStart(Reference.Prefix);

        return result.IsSuccess
            ? result.Value.Select(v => new ValueObserver(v)).Where(v => v.Filter(filter))
            : Enumerable.Empty<ValueObserver>();
    }

    /// <summary>
    /// Gets potential option values for the parent criterion property which are potential values to the argument input. 
    /// </summary>
    private IEnumerable<ValueObserver> GetOptions(string? filter)
    {
        var criterion = FindParent<CriterionObserver>();
        var type = criterion?.Property.Type;
        var group = criterion?.Property.Group;

        if (type is null)
            return Enumerable.Empty<ValueObserver>();

        if (group == TypeGroup.Boolean)
            return type.GetOptions().Select(x => new ValueObserver(x));

        return group == TypeGroup.Enum
            ? type.GetOptions().Select(x => new ValueObserver(x)).Where(v => v.Filter(filter))
            : Enumerable.Empty<ValueObserver>();
    }

    public static implicit operator Argument(ArgumentObserver observer) => observer.Model;
    public static implicit operator ArgumentObserver(Argument model) => new(model);
}