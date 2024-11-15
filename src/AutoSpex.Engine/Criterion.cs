using System.Linq.Expressions;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
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
    public Criterion(Property? property, Operation operation, object? argument = default)
    {
        Type = property?.Origin ?? typeof(object);
        Property = property ?? Property.Default;
        Operation = operation;
        Argument = argument;
    }

    /// <summary>
    /// Creates a new <see cref="Criterion"/> with the provided arguments.
    /// </summary>
    /// <param name="property">The name of the property for which to retrieve the value from the candidate.</param>
    /// <param name="operation">The operation to perform when evaluating.</param>
    /// <param name="arguments">The argument values to use when evaluating.</param>
    public Criterion(Property? property, Operation operation, params object[] arguments)
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
    public Type Type { get; set; } = typeof(object);

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
    /// The value of the argument, which is just a generic object, since the user can enter primitive or complex types.
    /// This value is persisted and materialized using a custom JSON serializer.
    /// </summary>
    [JsonConverter(typeof(JsonObjectConverter))]
    public object? Argument { get; set; }

    /// <summary>
    /// Parses the given text to create a new <see cref="Criterion"/> instance based on the specified type and text.
    /// </summary>
    /// <param name="type">The type to be used for the Criterion instance.</param>
    /// <param name="text">The text containing the criteria information.</param>
    /// <returns>A new <see cref="Criterion"/> instance based on the provided type and text.</returns>
    public static Criterion Parse(Type type, string text)
    {
        var operations = string.Join("|", Operation.List.Select(o => o.Name));
        var pattern = $"(^[^\\ ]+) (Is|Not) ({operations})(.*?)$";

        var match = Regex.Match(text, pattern);
        if (!match.Success || match.Groups.Count < 3)
            throw new FormatException($"The input text '{text}' is not a valid Criterion pattern.");

        var property = Property.This(type).GetProperty(match.Groups[1].Value);
        var negation = Negation.FromName(match.Groups[2].Value);
        var operation = Operation.FromName(match.Groups[3].Value);
        var argument = ParseArgument(property, operation, match.Groups[4].Value.Trim());

        return new Criterion
        {
            Type = type,
            Property = property,
            Negation = negation,
            Operation = operation,
            Argument = argument
        };
    }

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
            var argument = ResolveArgument(Argument);
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
            Argument = Argument
        };
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var arguments = GetExpected().ToList();

        var expected = arguments.Count switch
        {
            1 => arguments[0].ToText(),
            > 1 => $"[{string.Join(',', arguments.Select(x => x.ToText()))}]",
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
        var innerText = Argument is Criterion criterion ? criterion.GetCriteria() : string.Empty;
        return $"{rootText} {innerText}".Trim();
    }

    /// <summary>
    /// Gets the final values of the arguments in the <see cref="Criterion"/> instance.
    /// </summary>
    /// <returns>
    /// A collection of object values that represent the final resolved (and perhaps nested) argument values for this
    /// criterion instance.
    /// </returns>
    public IEnumerable<object> GetExpected() => Expected();

    /// <summary>
    /// 
    /// </summary>
    private static object? ResolveArgument(object? argument)
    {
        var value = argument switch
        {
            Criterion criterion => criterion,
            Range range => new List<object?> { range.Min, range.Max },
            List<object> collection => collection.Select(ResolveArgument).ToList(),
            /*string text when type is not null && type != typeof(string) => text.TryParse(type),*/
            _ => argument
        };

        return value;
    }

    /// <summary>
    /// Traverses the argument value and retrieves the final expected argument value(s).
    /// Since an argument value may be a nested <see cref="Criterion"/> or collection of objects,
    /// we want to check them and get the values which are going to be used in the operation.
    /// </summary>
    /// <returns>A collection of object values that represent the final arguments.</returns>
    private IEnumerable<object> Expected()
    {
        return Argument switch
        {
            Criterion criterion => criterion.Expected(),
            _ => Argument is not null ? [Argument] : []
        };
    }

    public static implicit operator Func<object, bool>(Criterion criterion) => x => criterion.Evaluate(x);
    public static implicit operator Expression<Func<object, bool>>(Criterion criterion) => criterion.ToExpression();

    // ReSharper disable once ConvertIfStatementToSwitchStatement
    private static object? ParseArgument(Property property, Operation operation, string text)
    {
        if (operation is UnaryOperation || string.IsNullOrEmpty(text))
            return default;

        if (operation is BinaryOperation)
        {
            return property.Group.TryParse(text, out var parsed) ? parsed : text;
        }

        if (operation is TernaryOperation)
        {
            var values = text.Split(" and ", StringSplitOptions.RemoveEmptyEntries);
            return property.Group.TryParse(values[0], out var min) && property.Group.TryParse(values[1], out var max)
                ? new Range(min, max)
                : new Range();
        }

        if (operation is InOperation)
        {
            var values = text.TrimStart('[').TrimEnd(']').Split(',');
            var list = values.Select(v => property.Group.TryParse(v, out var parsed) ? parsed : v).ToList();
            return list;
        }

        if (operation is CollectionOperation)
        {
            return Parse(property.InnerType, text);
        }

        return default;
    }

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