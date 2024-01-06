using System.Linq.Expressions;

namespace AutoSpex.Engine;

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
    public Criterion()
    {
    }

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
    
    public string? Property { get; set; } = string.Empty;
    public Operation Operation { get; set; } = Operation.None;
    public List<Argument> Arguments { get; set; } = [];

    public Evaluation Evaluate(object? candidate)
    {
        try
        {
            var type = candidate?.GetType();
            var property = type?.Property(Property);
            var value = property?.Getter().Invoke(candidate) ?? candidate;
            var args = Arguments.Select(a => a.Value);
            var result = Operation.Execute(value, args);
            return result ? Evaluation.Passed(this, candidate, value) : Evaluation.Failed(this, candidate, value);
        }
        catch (Exception e)
        {
            return Evaluation.Error(this, candidate, e);
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