using System.Linq.Expressions;
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
    /// Creates a new default <see cref="Criterion"/> instance with the provided parent element type.
    /// </summary>
    public Criterion(Type type)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
    }

    /// <summary>
    /// Creates a new <see cref="Criterion"/> with the provided arguments.
    /// </summary>
    /// <param name="property">The name of the property for which to retrieve the value from the candidate.</param>
    /// <param name="operation">The operation to perform when evaluating.</param>
    /// <param name="argument">The argument value to use when evaluating.</param>
    public Criterion(Property? property, Operation operation, Argument? argument = default)
    {
        Type = property?.Origin ?? typeof(object);
        Property = property ?? Property.Default;
        Operation = operation;
        Argument = argument ?? Argument.Default;
    }

    /// <summary>
    /// Creates a new <see cref="Criterion"/> with the provided arguments.
    /// </summary>
    /// <param name="property">The name of the property for which to retrieve the value from the candidate.</param>
    /// <param name="operation">The operation to perform when evaluating.</param>
    /// <param name="arguments">The argument values to use when evaluating.</param>
    public Criterion(Property? property, Operation operation, params Argument[] arguments)
    {
        Type = property?.Origin ?? typeof(object);
        Property = property ?? Property.Default;
        Operation = operation;
        Argument = arguments.ToList();
    }

    /// <summary>
    /// The type this criterion represents. This is not used when evaluation is called, but
    /// is here to allow this data to be passed along with the object, so we know which properties can be resolved for this criterion. 
    /// </summary>
    [JsonConverter(typeof(JsonTypeConverter))]
    [JsonInclude]
    public Type Type { get; private set; } = typeof(object);

    /// <summary>
    /// The property of the provided object which will be the target or input value of the evaluation. If null,
    /// then <see cref="Evaluate"/> will simply use the provided candidate object itself as the target of the
    /// evaluation. This allows use to pass simple or complex objects to the criterion and specify which property to evaluate.
    /// </summary>
    [JsonConverter(typeof(JsonPropertyConverter))]
    public Property Property { get; set; } = Property.Default;

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
    /// The collection of argument values required for the operation to execute.
    /// </summary>
    public Argument Argument { get; set; } = Argument.Default;

    /// <summary>
    /// Represents a criterion that always evaluates to true.
    /// </summary>
    public static Criterion True => new() { Operation = Operation.None, Negation = Negation.Not };

    /// <summary>
    /// Represents a criterion that always evaluates to false.
    /// </summary>
    public static Criterion False => new() { Operation = Operation.None };

    /// <summary>
    /// Evaluates a candidate object using the current state/properties of the criterion object.
    /// </summary>
    /// <param name="candidate">The object to be evaluated.</param>
    /// <returns>An Evaluation object indicating the result of the evaluation.</returns>
    public Evaluation Evaluate(object candidate)
    {
        try
        {
            var value = Property != Property.Default ? Property.GetValue(candidate) : candidate;
            var valueType = value?.GetType();
            var argument = Argument.ResolveAs(valueType);
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
        if (Argument.Value is not Criterion other) return false;
        return ReferenceEquals(other, criterion) || other.Contains(criterion);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    public bool Contains(Argument argument)
    {
        if (ReferenceEquals(Argument, argument)) return true;
        if (Argument.Value is not IEnumerable<Argument> arguments) return false;
        return arguments.Any(a => ReferenceEquals(a, argument));
    }

    /// <summary>
    /// Creates a duplicate of the current <see cref="Criterion"/> instance.
    /// </summary>
    /// <returns>
    /// A new <see cref="Criterion"/> instance with the same property values but different unique identifiers.
    /// </returns>
    /// <remarks>
    /// This is indented to be used when exporting/importing content.
    /// </remarks>
    public Criterion Duplicate()
    {
        return new Criterion
        {
            Type = Type,
            Property = Property,
            Operation = Operation,
            Negation = Negation,
            Argument = Argument.Duplicate()
        };
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var final = GetExpected().ToList();

        var expected = final.Count switch
        {
            1 => final[0].ToText(),
            > 1 => $"[{string.Join(',', final.Select(x => x.ToText()))}]",
            _ => string.Empty
        };

        return $"{GetCriteria()} {expected}".Trim();
    }

    /// <summary>
    /// Gets the text containing the property, negation, and operation that this criterion is configured to evaluate.
    /// This includes all nested criterion argument values, thereby forming a chain of readable text that identifies
    /// what is being evaluated.
    /// </summary>
    /// <returns>A <see cref="string"/> containing the criteria text.</returns>
    public string GetCriteria()
    {
        var rootText = $"{Property.Path} {Negation} {Operation}";
        var innerText = Argument is { Value: Criterion criterion } ? criterion.GetCriteria() : string.Empty;
        return $"{rootText} {innerText}".Trim();
    }

    /// <summary>
    /// Gets the final values of the arguments in the <see cref="Criterion"/> instance.
    /// </summary>
    /// <returns>
    /// A collection of object values that represent the final resolved (and perhaps nested) argument values for this
    /// criterion instance.
    /// </returns>
    public IEnumerable<object> GetExpected() => Argument.Expected();

    public static implicit operator Func<object, bool>(Criterion criterion) => x => criterion.Evaluate(x);
    public static implicit operator Expression<Func<object, bool>>(Criterion criterion) => criterion.ToExpression();

    /// <summary>
    /// Converts the current <see cref="Criterion"/> instance into an expression tree.
    /// </summary>
    /// <returns>An expression tree representing the current <see cref="Criterion"/> instance.</returns>
    private Expression<Func<object, bool>> ToExpression()
    {
        var parameter = Expression.Parameter(typeof(object), "x");
        Func<object, bool> func = x => (bool)Evaluate(x);
        var call = Expression.Call(Expression.Constant(func.Target), func.Method, parameter);
        return Expression.Lambda<Func<object, bool>>(call, parameter);
    }
}