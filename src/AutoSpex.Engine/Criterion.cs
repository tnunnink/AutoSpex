using System.Linq.Expressions;

namespace AutoSpex.Engine;

/// <summary>
/// The <see cref="Criterion"/> class represents a single criterion that can be used to evaluate a candidate object.
/// </summary>
/// <remarks>
/// This class allows a criterion to be configured from basic inputs. This type is agnostic to the object type on which
/// it will be called, which makes it more flexible or reusable for any candidate object, as long as it is configured accordingly.
/// I chose to avoid a generic type parameter since most of these will be defined at runtime and not compile time.
/// This class relies on <see cref="Operation"/> to perform the <see cref="Evaluate"/> of the candidate object.
/// This allow us to avoid having to build a complex expression tree for each criterion, and instead use a single
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
    /// Creates a new <see cref="Criterion"/> with the provided arguments.
    /// </summary>
    /// <param name="property">The name of the property for which to retrieve the the value from the candidate.</param>
    /// <param name="operation">The operation to perform when evaluating.</param>
    /// <param name="arguments">The set of <see cref="Argument"/> values to use when evaluating.</param>
    /// <exception cref="ArgumentNullException"><paramref name="operation"/> is null.</exception>
    public Criterion(string? property, Operation operation, params Argument[] arguments)
    {
        Property = property;
        Operation = operation ?? throw new ArgumentNullException(nameof(operation));
        Arguments = arguments.ToList();
    }

    public Criterion(Operation operation, params Argument[] arguments)
    {
        Operation = operation ?? throw new ArgumentNullException(nameof(operation));
        Arguments = arguments.ToList();
    }

    /// <summary>
    /// The property name of the provided object which will be the target or input value of the evaluation. If null or
    /// empty, then <see cref="Evaluate"/> will simply use the provided candidate object itself as the target of the
    /// evaluation. This allows use to pass simple or complex objects to the criterion and specify which property to evaluate.
    /// </summary>
    public string? Property { get; set; } = string.Empty;

    public Operation Operation { get; set; } = Operation.None;
    public List<Argument> Arguments { get; set; } = [];

    /// <summary>
    /// Evaluates a candidate object using the current state/properties of the criterion object.
    /// </summary>
    /// <param name="candidate">The object to be evaluated.</param>
    /// <returns>An Evaluation object indicating the result of the evaluation.</returns>
    public Evaluation Evaluate(object? candidate)
    {
        var type = candidate?.GetType();
        var property = type?.Property(Property);
        var inputType = property?.Type ?? type;
        var getter = property?.Getter();
        
        try
        {
            var value = getter is not null ? getter(candidate) : candidate;
            var args = Arguments.Select(a => a.Resolve(inputType)).ToArray();
            var result = Operation.Execute(value, args);

            return result
                ? Evaluation.Passed(this, type?.TypeIdentifier(), args, value)
                : Evaluation.Failed(this, type?.TypeIdentifier(), args, value);
        }
        catch (Exception e)
        {
            return Evaluation.Error(this, e, type?.TypeIdentifier());
        }
    }

    public static implicit operator Func<object?, bool>(Criterion criterion) => x => criterion.Evaluate(x);

    public static implicit operator Expression<Func<object?, bool>>(Criterion criterion) => criterion.GetExpression();

    private Expression<Func<object?, bool>> GetExpression()
    {
        var parameter = Expression.Parameter(typeof(object), "x");
        Func<object, bool> func = x => (bool) Evaluate(x);
        var call = Expression.Call(Expression.Constant(func.Target), func.Method, parameter);
        return Expression.Lambda<Func<object?, bool>>(call, parameter);
    }
}