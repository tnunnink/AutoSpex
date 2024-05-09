using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

// ReSharper disable TailRecursiveCall

namespace AutoSpex.Client.Observers;

/// <summary>
/// A observer wrapper over the <see cref="Engine.Argument"/> class for a parent criterion object.
/// </summary>
public class ArgumentObserver : Observer<Argument>
{
    /// <summary>
    /// Creates a new <see cref="ArgumentObserver"/> wrapping the provided <see cref="Argument"/>.
    /// </summary>
    /// <param name="model">The <see cref="Argument"/> object to wrap.</param>
    public ArgumentObserver(Argument model) : base(model)
    {
        Track(nameof(Value));
    }

    public override Guid Id => Model.ArgumentId;

    /// <summary>
    /// The owning <see cref="CriterionObserver"/> of the argument.
    /// </summary>
    public CriterionObserver? Criterion => RequestCriterion();

    /// <summary>
    /// The value of the argument which represents the data in which the criterion will evaluate against. This
    /// is ultimately the persisted value.
    /// </summary>
    public object Value
    {
        get => GetValue();
        set
        {
            var set = SetProperty(Model.Value, value, Model, SetValue);
            if (!set) return;
            OnPropertyChanged(Formatted);
        }
    }

    /// <summary>
    /// The value formatted as a string to be represented in the entry text field for the control.
    /// </summary>
    public string Formatted => Model.Formatted;

    /// <summary>
    /// The <see cref="Type"/> for the argument.
    /// </summary>
    public Type Type => Model.Type;

    /// <summary>
    /// The friendly type name for the underlying argument value.
    /// </summary>
    public string Identifier => Model.Identifier;

    /// <summary>
    /// The <see cref="TypeGroup"/> for the argument.
    /// </summary>
    public TypeGroup Group => Model.Group;

    /// <summary>
    /// Creates a new <see cref="ArgumentObserver"/> with an empty string as the initial value.
    /// </summary>
    /// <returns>A new <see cref="ArgumentObserver"/> object.</returns>
    public static ArgumentObserver Empty => new(Argument.Empty);

    /// <inheritdoc />
    public override string ToString() => Model.Formatted;

    /// <summary>
    /// Executes the code to retrieve possible suggestion values based on the current argument's parent criterion
    /// property, scoped variables, and active source data, and returns a collection of values wrapped in
    /// <see cref="Argument"/> objects and filtered based on the provided input text.
    /// </summary>
    public async Task<IEnumerable<ArgumentObserver>> GetSuggestions(string? filter = default)
    {
        var suggestions = await RequestSuggestions();

        if (string.IsNullOrEmpty(filter))
            return suggestions;

        return suggestions.Where(s => s.Formatted.Contains(filter, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Since the user will enter text input as the argument, we need to take that and resolve the actual type it represents
    /// depending on what was entered. If the user input a variable format meaning the text starts and ends with a bracket,
    /// then we need to find the matching variable object, assign it to the <see cref="Value"/> and determine if it is valid.
    /// If the user input a value matching an existing option for the criterion property (bool/enum), then we can...
    /// </summary>
    /// <param name="input"></param>
    public void ResolveInput(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            Value = string.Empty;
            return;
        }

        if (input.StartsWith('{') && input.EndsWith('}'))
        {
            /*Criterion.Spec;*/
        }

        Value = input;
    }

    /// <summary>
    /// We need some special wrapping of the child values for Argument. it can be a nested criterion or variable, and
    /// if so we need to wrap those in observers. If not we can just return the simple value. For Criterion we pass on
    /// the owning criterion property as the origin/parent property for the criterion since it only exists on the observer
    /// as meta data for which to present possible child property options.
    /// </summary>
    private object GetValue()
    {
        return Model.Value switch
        {
            Criterion criterion => new CriterionObserver(criterion, Criterion?.Property?.Type),
            Variable variable => new VariableObserver(variable),
            _ => Model.Value
        };
    }

    /// <summary>
    /// When we set argument with a criterion or variable observer we need to pass in the actual model object since that
    /// is what Argument expects. Anything else can be the plain value (text, number, enum, bool, etc.)
    /// </summary>
    private static void SetValue(Argument argument, object value)
    {
        switch (value)
        {
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
    /// Tries to get a collection of suggestions to display to the user as potential arguments values. This collection
    /// will contain either option values (bool/enum), variables in scope of this argument, or just simple string
    /// values that match the criterion property name/property type.
    /// </summary>
    private async Task<IEnumerable<ArgumentObserver>> RequestSuggestions()
    {
        var suggestions = new List<ArgumentObserver>();

        var options = GetOptions();
        suggestions.AddRange(options);

        var variables = await GetScopedVariables();
        suggestions.AddRange(variables);

        //todo perhaps also get the matching value suggestions from the active source

        return suggestions;
    }

    /// <summary>
    /// Query the database for variables that are in scope of this argument. These are variables that belong to or are
    /// inherited by the parent spec object. Therefore we first request the parent spec and the use it's id to fetch
    /// variable objects as suggestions to plug into the argument value for the user.
    /// </summary>
    private async Task<IEnumerable<ArgumentObserver>> GetScopedVariables()
    {
        var id = Criterion?.Spec?.Id ?? Guid.Empty;

        var result = await Mediator.Send(new GetScopedVariables(id));
        if (result.IsFailed) return Enumerable.Empty<ArgumentObserver>();

        return result.Value.Select(x => new ArgumentObserver(new Argument(x)));
    }

    /// <summary>
    /// Gets potential option values for the parent criterion property which are potential values to the argument input. 
    /// </summary>
    private IEnumerable<ArgumentObserver> GetOptions()
    {
        var property = Criterion?.Property;
        if (property is null) return Enumerable.Empty<ArgumentObserver>();
        return property.Options.Select(x => new ArgumentObserver(new Argument(x)));
    }

    /// <summary>
    /// Send the request to retrieve the parent <see cref="CriterionObserver"/> for this argument object.
    /// </summary>
    private CriterionObserver? RequestCriterion()
    {
        var request = Messenger.Send(new CriterionRequest(Id));
        if (!request.HasReceivedResponse) return default;
        return request.Response;
    }

    /// <summary>
    /// A request to retrieve the parent <see cref="CriterionObserver"/> of this argument.
    /// </summary>
    /// <param name="argumentId">The id of this argument for which to retrieve the parent criterion.</param>
    public class CriterionRequest(Guid argumentId) : RequestMessage<CriterionObserver>
    {
        public Guid ArgumentId { get; } = argumentId;
    }


    public static implicit operator Argument(ArgumentObserver observer) => observer.Model;
    public static implicit operator ArgumentObserver(Argument model) => new(model);
}