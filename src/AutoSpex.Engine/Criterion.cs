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
    public Criterion(Type? type)
    {
        Type = type ?? typeof(object);
    }

    /// <summary>
    /// Creates a new <see cref="Criterion"/> with the provided arguments.
    /// </summary>
    /// <param name="criterionId">The id of the cri</param>
    /// <param name="type">The type this criterion is configured for.</param>
    /// <param name="property">The name of the property for which to retrieve the the value from the candidate.</param>
    /// <param name="operation">The operation to perform when evaluating.</param>
    /// <param name="invert">Wether to invert the condition of the evaluation to false=pass (i.e. NOT).</param>
    /// <param name="arguments">The set of <see cref="Argument"/> values to use when evaluating.</param>
    public Criterion(Guid criterionId,
        Type? type = default,
        Property? property = default,
        Operation? operation = default,
        bool invert = false,
        params Argument[] arguments)
    {
        CriterionId = criterionId;
        Type = type ?? typeof(object);
        Property = property;
        Operation = operation;
        Arguments = [..arguments];
        Invert = invert;
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
        Property = property;
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
    public Guid CriterionId { get; } = Guid.NewGuid();

    /// <summary>
    /// The type this criterion represents. This is not used when evaluation is called, but
    /// is here to allow this data to be passed along with the object, so we know which properties can be resolved for this criterion. 
    /// </summary>
    public Type? Type { get; } = typeof(object);

    /// <summary>
    /// The property of the provided object which will be the target or input value of the evaluation. If null,
    /// then <see cref="Evaluate"/> will simply use the provided candidate object itself as the target of the
    /// evaluation. This allows use to pass simple or complex objects to the criterion and specify which property to evaluate.
    /// </summary>
    public Property? Property { get; set; }

    /// <summary>
    /// The operation the evaluation will execute on the input and argument values.
    /// </summary>
    public Operation? Operation { get; set; }

    /// <summary>
    /// The collection of argument values required for the operation to execute.
    /// </summary>
    public List<Argument> Arguments { get; init; } = [];

    /// <summary>
    /// A flag to invert the result of the operation.
    /// </summary>
    public bool Invert { get; set; }

    /// <summary>
    /// Evaluates a candidate object using the current state/properties of the criterion object.
    /// </summary>
    /// <param name="candidate">The object to be evaluated.</param>
    /// <returns>An Evaluation object indicating the result of the evaluation.</returns>
    public Evaluation Evaluate(object? candidate)
    {
        var operation = Operation ?? Operation.None;

        try
        {
            var value = Property is not null ? Property.GetValue(candidate) : candidate;
            var valueType = value?.GetType();
            var args = Arguments.Select(a => a.ResolveAs(valueType)).ToArray();
            var result = !Invert ? operation.Execute(value, args) : !operation.Execute(value, args);

            return result
                ? Evaluation.Passed(this, candidate, args, value)
                : Evaluation.Failed(this, candidate, args, value);
        }
        catch (Exception e)
        {
            return Evaluation.Error(this, candidate, e);
        }
    }

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

    public static implicit operator Func<object?, bool>(Criterion criterion) => x => criterion.Evaluate(x);
    public static implicit operator Expression<Func<object?, bool>>(Criterion criterion) => criterion.ToExpression();

    private Expression<Func<object?, bool>> ToExpression()
    {
        var parameter = Expression.Parameter(typeof(object), "x");
        Func<object, bool> func = x => (bool)Evaluate(x);
        var call = Expression.Call(Expression.Constant(func.Target), func.Method, parameter);
        return Expression.Lambda<Func<object?, bool>>(call, parameter);
    }
}