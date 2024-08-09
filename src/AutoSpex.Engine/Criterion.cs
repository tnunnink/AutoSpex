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
public class Criterion : IEquatable<Criterion>
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
    /// <param name="property">The name of the property for which to retrieve the the value from the candidate.</param>
    /// <param name="operation">The operation to perform when evaluating.</param>
    /// <param name="arguments">The set of <see cref="Argument"/> values to use when evaluating.</param>
    public Criterion(Property? property, Operation operation, params Argument[] arguments)
    {
        Type = property?.Origin ?? typeof(object);
        Property = property ?? Property.Default;
        Operation = operation;
        Arguments = arguments.ToList();
    }

    /// <summary>
    /// Creates a new <see cref="Criterion"/> with the provided arguments.
    /// </summary>
    /// <param name="operation">The operation to perform when evaluating.</param>
    /// <param name="arguments">The set of <see cref="Argument"/> values to use when evaluating.</param>
    public Criterion(Operation operation, params Argument[] arguments)
    {
        Operation = operation;
        Arguments = arguments.ToList();
    }

    /// <summary>
    /// A <see cref="Guid"/> that uniquely identifies this object.
    /// </summary>
    [JsonInclude]
    public Guid CriterionId { get; private init; } = Guid.NewGuid();

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
    /// The operation the evaluation will execute on the input and argument values.
    /// </summary>
    [JsonConverter(typeof(SmartEnumNameConverter<Operation, string>))]
    public Operation Operation { get; set; } = Operation.None;

    /// <summary>
    /// The collection of argument values required for the operation to execute.
    /// </summary>
    public List<Argument> Arguments { get; init; } = [];

    /// <summary>
    /// A flag to negate the result of the operation for this criterion. 
    /// </summary>
    [JsonConverter(typeof(SmartEnumNameConverter<Negation, bool>))]
    public Negation Negation { get; set; } = Negation.Is;

    public static implicit operator Func<object, bool>(Criterion criterion) => x => criterion.Evaluate(x);
    public static implicit operator Expression<Func<object, bool>>(Criterion criterion) => criterion.ToExpression();

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
            var args = Arguments.Select(a => a.ResolveAs(valueType)).ToArray();
            var result = Operation.Execute(value, args);

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
    /// Resolves all argument reference values to the scoped variables of the provided node instance.
    /// This method will travers any nested criterion or arguments to deeply resolve all argument reference values.
    /// If references are unresolvable from using the given node, they cause errors when the criterion is evaluated.
    /// </summary>
    /// <param name="node">The node providing the scoped variables for which to resolve argument references.</param>
    public void Resolve(Node node) => Resolve(this, node);

    /// <summary>
    /// Determines if the provided criterion is a nested object of this criterion, meaning that one of this criterion's
    /// arguments or descendent arguments is this criterion object. 
    /// </summary>
    /// <param name="other">The criterion to search for in the object graph.</param>
    /// <returns>
    /// <c>true</c> if this criterion contains the provided criterion within it's nested argument structure;
    /// Otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>This allows us to determine if one criterion "owns" another.</remarks>
    public bool Contains(Criterion other) =>
        Arguments.Any(a => a.Value is Criterion criterion &&
                           (criterion.CriterionId == other.CriterionId || criterion.Contains(other)));

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
    /// what is being checked.
    /// </summary>
    /// <returns>A <see cref="string"/> containing the criteria text.</returns>
    public string GetCriteria()
    {
        var rootText = $"{Property.Path} {Negation} {Operation}";
        var innerText = Arguments is [{ Value: Criterion criterion }] ? criterion.GetCriteria() : string.Empty;
        return $"{rootText} {innerText}".Trim();
    }

    /// <summary>
    /// Gets the final values of the arguments in the <see cref="Criterion"/> instance.
    /// </summary>
    /// <returns>
    /// A collection of object values that represent the final resolved (and perhaps nested) argument values for this
    /// criterion instance.
    /// </returns>
    public IEnumerable<object> GetExpected()
    {
        return Arguments.SelectMany(a => a.Expected()).ToList();
    }

    public bool Equals(Criterion? other)
    {
        if (ReferenceEquals(null, other)) return false;
        return ReferenceEquals(this, other) || CriterionId.Equals(other.CriterionId);
    }

    public override bool Equals(object? obj) => obj is Criterion other && Equals(other);
    public override int GetHashCode() => CriterionId.GetHashCode();

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

    /// <summary>
    /// Resolves the references in the given <see cref="Criterion"/> instance using the provided <see cref="Node"/>.
    /// </summary>
    private void Resolve(Criterion criterion, Node node)
    {
        foreach (var argument in criterion.Arguments)
        {
            ResolveReferences(argument, node);
        }
    }

    /// <summary>
    /// Resolves the argument reference values using the provided node instance.
    /// If the argument is a nested criterion or argument(s), then this method will forward call down the hierarchy to
    /// deeply resolve all argument references.
    /// </summary>
    /// <param name="argument">The argument to resolve.</param>
    /// <param name="node">The node that provides the context for resolving the argument.</param>
    private void ResolveReferences(Argument argument, Node node)
    {
        switch (argument.Value)
        {
            case Criterion nested:
                Resolve(nested, node);
                break;
            case IEnumerable<Argument> collection:
                collection.ToList().ForEach(a => ResolveReferences(a, node));
                break;
            case Reference reference:
                var value = node.Resolve(reference);
                reference.Value = value;
                break;
        }
    }
}