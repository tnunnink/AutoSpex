using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using L5Sharp.Core;
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
    /// 
    /// </summary>
    public bool AllowsAdding => _criterion?.Operation == Operation.In;

    /// <summary>
    /// The value of the argument which represents the data in which the criterion will evaluate against. This
    /// is ultimately the persisted value.
    /// </summary>
    [Required]
    public object? Value
    {
        get => GetValue();
        set => SetProperty(Model.Value, value, Model, SetValue, true);
    }

    /// <summary>
    /// The function that retrieves a collection of object values that are suggestions to the entry control.
    /// </summary>
    public Func<string?, CancellationToken, Task<IEnumerable<object>>> Suggestions => GetSuggestions;

    /// <summary>
    /// The function that selects the text value for the current value object of the argument.
    /// </summary>
    public Func<object?, string> Selector => SelectText;

    /// <summary>
    /// The action that commits the provided object to the argument's <see cref="Value"/>.
    /// </summary>
    public Action<object?> Commit => CommitValue;


    public static implicit operator Argument(ArgumentObserver observer) => observer.Model;


    /// <summary>
    /// Selects the text to present to the entry when the user updates the value or the popup is launched.
    /// We are using a custom selector since we can enter so many different values here.
    /// </summary>
    private static string SelectText(object? item)
    {
        return item switch
        {
            string x => x,
            LogixEnum x => x.Name,
            VariableObserver x => x.ScopedReference,
            LogixComponent x => x.Name,
            _ => item?.ToString() ?? string.Empty
        };
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

        var variables = await GetScopedVariables(filter, token);
        suggestions.AddRange(variables);

        return suggestions;
    }

    /// <summary>
    /// Query the database for variables that are in scope of this argument. These are variables that belong to or are
    /// inherited by the parent spec object. Therefore, we first request the parent spec and the use it's id to fetch
    /// variable objects as suggestions to plug into the argument value for the user.
    /// </summary>
    private async Task<IEnumerable<object>> GetScopedVariables(string? filter, CancellationToken token)
    {
        var result = await Mediator.Send(new GetScopedVariables(Id), token);

        return result.IsSuccess
            ? result.Value.Select(v => new VariableObserver(v)).Where(v => v.ScopedReference.PassesFilter(filter))
            : Enumerable.Empty<ArgumentObserver>();
    }

    /// <summary>
    /// Gets potential option values for the parent criterion property which are potential values to the argument input. 
    /// </summary>
    private IEnumerable<object> GetOptions(string? filter)
    {
        var type = _criterion?.Property?.Type;

        return type is not null
            ? type.GetOptions().Where(x => x.ToString()?.PassesFilter(filter) is true)
            : Enumerable.Empty<ArgumentObserver>();
    }

    /// <summary>
    /// We need some special wrapping of the child values for Argument. It can be a nested criterion or variable, and
    /// if so we need to wrap those in observers. If not we can just return the simple value.
    /// </summary>
    private object GetValue()
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
        switch (value)
        {
            case null:
                break;
            case Argument other:
                SetValue(argument, other.Value);
                break;
            case CriterionObserver criterion:
                argument.SetValue(criterion.Model);
                break;
            case Criterion criterion:
                argument.SetValue(criterion);
                break;
            case VariableObserver variable:
                argument.SetValue(variable.Model);
                break;
            case Variable variable:
                argument.SetValue(variable);
                break;
            default:
                argument.SetValue(value);
                break;
        }
    }

    /// <summary>
    /// Updates the underlying argument value based on the received value type from the entry field.
    /// If user enters simple text we would like to parse it as the strong type to let our data templates work.
    /// If user enters a complex object then it was selected from the suggestions, and we can just use that.
    /// </summary>
    private void CommitValue(object? value)
    {
        var type = _criterion?.Property?.Type;

        switch (value)
        {
            case null:
                Value = string.Empty;
                return;
            case string text:
                Value = ResolveValue(text, type);
                return;
            default:
                Value = value;
                break;
        }
    }

    /// <summary>
    /// Since the user will enter text input as the argument, we need to take that and resolve the actual typed
    /// value it represents.
    /// </summary>
    private object ResolveValue(string value, Type? type)
    {
        //1. Null or empty should just return an empty string to default the UI.
        if (string.IsNullOrEmpty(value)) return string.Empty;

        //2. Text starting and ending with brackets should refer to a variable which we need to try and find.
        if (value.StartsWith('{') && value.EndsWith('}'))
        {
            return GetVariable(value[1..^1]);
        }

        //3. If our parent criterion has no type info or the target type is string, just return the string.
        if (type is null || type == typeof(string)) return value;

        //4. Otherwise, we rely on our LogixParser to do the work and if it can't we return the string.
        //todo this would be cool if part of the parsing
        if (type.IsAssignableTo(typeof(LogixElement)))
        {
            return XElement.Parse(value).Deserialize();
        }

        var parsed = type.IsParsable() ? value.TryParse(type) : default;
        return parsed ?? value;
    }

    private Task<VariableObserver> GetVariable(string name)
    {
        //todo query the database for the variable with this name in the scope of this argument.
        return Task.FromResult(new VariableObserver(new Variable(name)));
    }
}