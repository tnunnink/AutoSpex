using System.Text.Json;
using System.Text.Json.Serialization;
using Ardalis.SmartEnum.SystemTextJson;

namespace AutoSpex.Engine;

/// <summary>
/// The <see cref="Criterion"/> class represents a single criterion that can be used to evaluate a candidate object.
/// </summary>
/// <remarks>
/// This class allows a criterion to be configured from basic inputs. This type is agnostic to the object type on which
/// it will be called, which makes it more flexible or reusable for any candidate object, as long as it is configured accordingly.
/// I chose to avoid a generic type parameter since most of these will be defined at runtime and not compile time.
/// This class relies on <see cref="Operation"/> to perform the <see cref="Evaluate"/> of the candidate object.
/// This allows us to avoid having to build a complex expression tree for each criterion, and instead use a single
/// method call which handles the logic for each operation. However, we can implicitly convert this to a predicate expression
/// using the local <see cref="Evaluate"/> method call so that this in theory could be used to build up a more complex
/// expression tree.
/// </remarks>
public class Criterion
{
    /// <summary>
    /// Creates a new default <see cref="Criterion"/> instance with an empty property, no operation and no arguments.
    /// </summary>
    public Criterion()
    {
    }

    /// <summary>
    /// Creates a new <see cref="Criterion"/> with the provided parameters.
    /// </summary>
    /// <param name="operation">The operation to perform when evaluating.</param>
    /// <param name="argument">The argument value to use when evaluating.</param>
    public Criterion(Operation operation, object? argument = default)
    {
        Operation = operation;
        Argument = argument;
    }

    /// <summary>
    /// Creates a new <see cref="Criterion"/> with the provided parameters.
    /// </summary>
    /// <param name="property">The name of the property for which to retrieve the value from the candidate.</param>
    /// <param name="operation">The operation to perform when evaluating.</param>
    /// <param name="argument">The argument value to use when evaluating.</param>
    public Criterion(string property, Operation operation, object? argument = default)
    {
        Property = property;
        Operation = operation;
        Argument = argument;
    }

    /// <summary>
    /// Creates a new <see cref="Criterion"/> with the provided parameters.
    /// </summary>
    /// <param name="property">The name of the property for which to retrieve the value from the candidate.</param>
    /// <param name="negation">The negation option to use on the operation result (Is/Not).</param>
    /// <param name="operation">The operation to perform when evaluating.</param>
    /// <param name="argument">The argument value to use when evaluating.</param>
    public Criterion(string property, Negation negation, Operation operation, object? argument = default)
    {
        Property = property;
        Negation = negation;
        Operation = operation;
        Argument = argument;
    }

    /// <summary>
    /// The property of the provided object which will be the target or input value of the evaluation. If null,
    /// then <see cref="Evaluate"/> will simply use the provided candidate object itself as the target of the
    /// evaluation. This allows use to pass simple or complex objects to the criterion and specify which property to evaluate.
    /// </summary>
    [JsonInclude]
    public string Property { get; set; } = string.Empty;

    /// <summary>
    /// A flag to negate the result of the operation for this criterion. 
    /// </summary>
    [JsonConverter(typeof(SmartEnumNameConverter<Negation, bool>))]
    public Negation Negation { get; set; } = Negation.Is;

    /// <summary>
    /// The operation the evaluation will execute on the input and argument values.
    /// </summary>
    [JsonConverter(typeof(SmartEnumNameConverter<Operation, string>))]
    public Operation Operation { get; set; } = Operation.None;

    /// <summary>
    /// The value of the argument, which is just a generic object, since the user can enter primitive or complex types.
    /// This value is persisted and materialized using a custom JSON serializer.
    /// </summary>
    [JsonConverter(typeof(JsonObjectConverter))]
    public object? Argument { get; set; }

    /// <summary>
    /// Evaluates a candidate object using the current state/properties of the criterion object.
    /// </summary>
    /// <param name="candidate">The object to be evaluated.</param>
    /// <returns>An Evaluation object indicating the result of the evaluation.</returns>
    public Evaluation Evaluate(object? candidate)
    {
        var origin = candidate is not null
            ? Engine.Property.This(candidate.GetType())
            : Engine.Property.Default;

        try
        {
            var property = origin.GetProperty(Property);
            var value = property.GetValue(candidate);
            var argument = ResolveArgument(Argument, candidate);
            var result = Operation.Execute(value, argument);

            return Negation.Satisfies(result)
                ? Evaluation.Passed(this, candidate, value)
                : Evaluation.Failed(this, candidate, value);
        }
        catch (Exception e)
        {
            return Evaluation.Errored(this, candidate, e);
        }
    }

    /// <summary>
    /// Determines if the provided criterion instance is this or a nested criterion object
    /// (meaning that one of this criterion's arguments or descendent arguments is the same criterion instance).
    /// </summary>
    /// <param name="criterion">The criterion to search for in the object graph.</param>
    /// <returns>
    /// <c>true</c> if this criterion contains the provided criterion instance within it's nested argument structure;
    /// Otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This allows us to determine if one criterion is or contains the provided instance.
    /// This uses reference equality.
    /// </remarks>
    public bool Contains(Criterion criterion)
    {
        if (ReferenceEquals(this, criterion)) return true;
        if (Argument is not Criterion other) return false;
        return ReferenceEquals(other, criterion) || other.Contains(criterion);
    }

    /// <summary>
    /// Creates a duplicate of the current <see cref="Criterion"/> instance.
    /// </summary>
    /// <returns>
    /// A new <see cref="Criterion"/> instance with the same property values but different unique identifiers.
    /// </returns>
    public Criterion Duplicate()
    {
        var data = JsonSerializer.Serialize(this);
        var instance = JsonSerializer.Deserialize<Criterion>(data);
        return instance ?? throw new InvalidOperationException("Could not materialize criterion object.");
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{GetCriteria()} {GetExpected()}".Trim();
    }

    /// <summary>
    /// Gets the text containing the property, negation, and operation that this criterion is configured to evaluate.
    /// This includes all nested criterion argument values, thereby forming a chain of readable text that identifies
    /// what is being evaluated.
    /// </summary>
    /// <returns>A <see cref="string"/> containing the criteria text.</returns>
    public string GetCriteria()
    {
        var rootText = $"{Property} {Negation} {Operation}";
        var innerText = Argument is Criterion criterion ? criterion.GetCriteria() : string.Empty;
        return $"{rootText} {innerText}".Trim();
    }

    /// <summary>
    /// Traverses the argument value and retrieves the text representation of the final expected argument value(s).
    /// </summary>
    /// <returns>A collection of object values that represent the final arguments.</returns>
    /// <remarks>
    /// This helps for the complete text 
    /// </remarks>
    public string GetExpected()
    {
        return Argument switch
        {
            Criterion criterion => criterion.GetExpected(),
            _ => Argument.ToText()
        };
    }

    /// <summary>
    /// Resolves the underlying argument value. Since an argument can be a complex object such as an inner criterion,
    /// range, list, property, or reference, we need to handle each case specifically to return the correct "argument"
    /// value for the operation.
    /// </summary>
    private static object? ResolveArgument(object? argument, object? candidate)
    {
        return argument switch
        {
            List<object> collection => collection.Select(x => ResolveArgument(x, candidate)).ToList(),
            Reference reference => reference.Resolve(candidate),
            _ => argument
        };
    }
}