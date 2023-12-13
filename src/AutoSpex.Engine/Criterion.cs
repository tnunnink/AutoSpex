using System.Linq.Expressions;
using AgileObjects.ReadableExpressions;
using AutoSpex.Engine.Operations;

namespace AutoSpex.Engine;

public class Class
{
    
}

/// <summary>
/// The <see cref="Criterion"/> class represents a single criterion that can be used to evaluate a candidate object.
/// </summary>
/// <remarks>
/// This class wraps an <see cref="Expression{TDelegate}"/> taking an object and returning a boolean result.
/// I chose to avoid a generic type parameter since most of these will be defined at runtime and not compile time.
/// This class also relies on <see cref="Operation"/> to perform the evaluation of the candidate object.
/// This allow us to avoid having to build a complex expression tree for each criterion, and instead use a single
/// method call which handles the logic for each operation.
/// </remarks>
public class Criterion
{
    public Criterion(Element element, string property, Operation operation, params object[] arguments)
    {
        Element = element ?? throw new ArgumentNullException(nameof(element));
        Property = property ?? throw new ArgumentNullException(nameof(property));
        Operation = operation ?? throw new ArgumentNullException(nameof(operation));
        Arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
    }

    /// <summary>
    /// The object type the criterion is evaluating.
    /// </summary>
    /// <value>A <see cref="System.Type"/> object containing the type information.</value>
    public Element Element { get; }

    /// <summary>
    /// The property name of the type the criterion is evaluating.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> representing an immediate or nested property of the type.
    /// This mean you can specify complex properties, for example TagName.Path
    /// </value>
    public string Property { get; }

    /// <summary>
    /// The <see cref="Operation"/> to perform when evaluating the criterion using the property value and arguments.
    /// </summary>
    /// <value>An <see cref="Operations.Operation"/> type.</value>
    public Operation Operation { get; }

    /// <summary>
    /// An array of arguments to pass to the <see cref="Operation"/> when evaluating the criterion.
    /// </summary>
    /// <value>A array of <see cref="object"/>.</value>
    public object[] Arguments { get; }

    public Evaluation Evaluate(object? candidate)
    {
        if (candidate is null) return Evaluation.Failed(this);

        try
        {
            var getter = Element.Getter(Property);
            var value = getter(candidate);
            var result = Operation.Execute(value, Arguments) ? ResultType.Passed : ResultType.Failed;
            return Evaluation.Of(result, this, value);
        }
        catch (Exception e)
        {
            //todo maybe we can catch different exceptions and figure out how to better report them here but for now will pass message
            return Evaluation.Error(this, e.Message);
        }
    }

    public static implicit operator Func<object, bool>(Criterion criterion) => x => criterion.Evaluate(x);

    public static implicit operator Expression<Func<object, bool>>(Criterion criterion)
    {
        var parameter = Expression.Parameter(typeof(object), "x");
        Func<object, bool> func = x => (bool) criterion.Evaluate(x);
        var call = Expression.Call(Expression.Constant(func.Target), func.Method, parameter);
        return Expression.Lambda<Func<object, bool>>(call, parameter);
    }

    public override string ToString() =>
        $"{Element.Name}.{Property} Should Evaluate {Operation} Against '{string.Join(',', Arguments)}'";

    public string ToExpressionString() => ((Expression<Func<object, bool>>) this).ToReadableString();
}