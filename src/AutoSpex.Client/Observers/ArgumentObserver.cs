using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using Argument = AutoSpex.Engine.Argument;

// ReSharper disable TailRecursiveCall

namespace AutoSpex.Client.Observers;

/// <summary>
/// A observer wrapper over the <see cref="Engine.Argument"/> class for a parent criterion object.
/// </summary>
public class ArgumentObserver : Observer<Argument>
{
    /// <summary>
    /// The parent or owning <see cref="CriterionObserver"/> that this argument belongs to. We need reference to the
    /// parent in order to know which property type the argument should resolve to. This is specified on the criterion
    /// object itself.
    /// </summary>
    private readonly CriterionObserver? _criterion;

    public ArgumentObserver(Argument argument, CriterionObserver? criterion = default) : base(argument)
    {
        _criterion = criterion;
        Track(nameof(Value));
    }

    public ArgumentObserver(CriterionObserver criterion) : base(new Argument())
    {
        _criterion = criterion;
        Track(nameof(Value));
    }

    /// <inheritdoc />
    public override Guid Id => Model.ArgumentId;

    /// <summary>
    /// Indicates that the argument's value is an inner criterion object itself. This controls whether we will display
    /// a simple argument entry field or a nested criterion entry field.
    /// </summary>
    public bool IsCriterion => Model.Value is Criterion;

    /// <summary>
    /// The value of the argument which represents the data in which the criterion will evaluate against. This
    /// is ultimately the persisted value.
    /// </summary>
    public object? Value
    {
        get => GetValue();
        set => SetProperty(Model.Value, value, Model, SetValue);
    }

    /// <summary>
    /// The action that commits the provided object to the argument's <see cref="Value"/>.
    /// </summary>
    public Action<object?> Commit => UpdateValue;

    /// <summary>
    /// The function that selects the text value for the current value object of the argument.
    /// </summary>
    public Func<object?, string> Selector => x => x.ToText();

    /// <summary>
    /// The function that retrieves a collection of object values that are suggestions to the entry control.
    /// </summary>
    public Func<string?, CancellationToken, Task<IEnumerable<object>>> Suggestions => GetSuggestions;

    public static implicit operator Argument(ArgumentObserver observer) => observer.Model;

    /// <summary>
    /// We need some special wrapping of the child values for Argument. It can be a nested criterion or variable, and
    /// if so we need to wrap those in observers. If not we can just return the simple value.
    /// </summary>
    private object? GetValue()
    {
        return Model.Value switch
        {
            Criterion criterion => new CriterionObserver(criterion),
            Variable variable => new VariableObserver(variable),
            _ => Model.Value
        };
    }

    /// <summary>
    /// When we set argument with a criterion or variable observer we need to pass in the actual model object since that
    /// is what Argument expects. Anything else can be the plain value (text, number, enum, bool, etc.)
    /// </summary>
    private static void SetValue(Argument argument, object? value)
    {
        argument.Value = value switch
        {
            CriterionObserver criterion => criterion.Model,
            VariableObserver variable => variable.Model,
            _ => value
        };
    }

    /// <summary>
    /// Updates the underlying argument value based on the received value type from the entry field.
    /// If user enters simple text we would like to parse it as the strong type to let our data templates work.
    /// If user enters a complex object then it was selected from the suggestions, and we can just use that.
    /// </summary>
    private async void UpdateValue(object? value)
    {
        var group = _criterion?.Property?.Group;

        switch (value)
        {
            case string text when text.StartsWith('{') && text.EndsWith('}'):
                Value = await GetVariable(text) ?? text;
                return;
            case string text when group?.TryParse(text, out var parsed) is true && parsed is not null:
                Value = parsed;
                return;
            case ValueObserver observer:
                Value = observer.Model;
                return;
            default:
                Value = value;
                break;
        }
    }

    /// <summary>
    /// Queries the database for a variable in scope with the specified name and returns it, or null if not found.
    /// </summary>
    private async Task<object?> GetVariable(string name)
    {
        name = name.TrimStart('{').TrimEnd('}');
        var result = await Mediator.Send(new GetScopedVariable(Id, name));
        return result.IsSuccess ? new VariableObserver(result.Value) : default;
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
        var result = await Mediator.Send(new GetScopedVariables(Id), token);

        return result.IsSuccess
            ? result.Value.Select(v => new ValueObserver(new VariableObserver(v))).Where(v => v.Passes(filter))
            : Enumerable.Empty<ValueObserver>();
    }

    /// <summary>
    /// Gets potential option values for the parent criterion property which are potential values to the argument input. 
    /// </summary>
    private IEnumerable<ValueObserver> GetOptions(string? filter)
    {
        var type = _criterion?.Property?.Type;
        var group = _criterion?.Property?.Group;

        if (type is null)
            return Enumerable.Empty<ValueObserver>();

        if (group == TypeGroup.Boolean)
            return type.GetOptions().Select(x => new ValueObserver(x));

        return group == TypeGroup.Enum
            ? type.GetOptions().Select(x => new ValueObserver(x)).Where(v => v.Passes(filter))
            : Enumerable.Empty<ValueObserver>();
    }
}