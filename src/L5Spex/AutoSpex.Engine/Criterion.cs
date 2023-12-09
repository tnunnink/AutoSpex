using System.Linq.Expressions;
using AgileObjects.ReadableExpressions;
using AutoSpex.Engine.Contracts;
using AutoSpex.Engine.Operations;

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
public class Criterion : ICriterion, IEquatable<Criterion>
{
    public Criterion(Type type, string property, Operation operation, params object[] arguments)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Property = property ?? throw new ArgumentNullException(nameof(property));
        Operation = operation ?? throw new ArgumentNullException(nameof(operation));
        Arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
    }

    /// <summary>
    /// The object type the criterion is evaluating.
    /// </summary>
    /// <value>A <see cref="System.Type"/> object containing the type information.</value>
    public Type Type { get; }

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
            var value = GetValue(candidate);
            var result = Operation.Evaluate(value, Arguments) ? ResultType.Passed : ResultType.Failed;
            return Evaluation.Of(result, this, value);
        }
        catch (Exception e)
        {
            //todo maybe we can catch different exceptions and figure out how to better report them here but for now will pass message
            return Evaluation.Error(this, e.Message);
        }
    }

    public Func<object, bool> Compile()
    {
        var parameter = Expression.Parameter(typeof(object), "x");
        var converted = Expression.TypeAs(parameter, Type);
        var member = GetMember(converted, Property);
        var expression = GetExpression<object>(parameter, member);
        return expression.Compile();
    }

    public Func<T, bool> Compile<T>()
    {
        if (typeof(T) != Type)
            throw new ArgumentException($"The criterion type '{Type}' does not match the supplied type '{typeof(T)}'.");

        var parameter = Expression.Parameter(Type, "x");
        var member = GetMember(parameter, Property);
        var expression = GetExpression<T>(parameter, member);
        return expression.Compile();
    }

    /// <summary>
    /// Returns a default always true expression predicate.
    /// </summary>
    /// <returns>An <see cref="Expression{TDelegate}"/> containing an always true delegate.</returns>
    public static Expression<Func<object, bool>> All() => o => true;

    /// <summary>
    /// Returns a default always false expression predicate.
    /// </summary>
    /// <returns>An <see cref="Expression{TDelegate}"/> containing an always false delegate.</returns>
    public static Expression<Func<object, bool>> None() => o => false;

    /// <summary>
    /// Creates a new <see cref="Criterion"/> for the specified type, property, operation, and arguments.
    /// </summary>
    /// <param name="property">The property of the type for which this criterion evaluates.</param>
    /// <param name="operation">The <see cref="Operations.Operation"/> the criterion performs.</param>
    /// <param name="arguments">The set of arguments the criterion uses for evaluation.</param>
    /// <typeparam name="T">The type of the candidate object supplied to the criterion.</typeparam>
    /// <returns>A new <see cref="Criterion"/> representing a check of the supplied parameters.</returns>
    public static Criterion For<T>(string property, Operation operation, params object[] arguments) =>
        new(typeof(T), property, operation, arguments);

    /// <summary>
    /// Implicitly converts this <see cref="Criterion"/> to an <see cref="Expression{TDelegate}"/> for use in creating
    /// more complex expressions.
    /// </summary>
    /// <param name="criterion">The criterion object to covert.</param>
    /// <returns>A <see cref="Expression{TDelegate}"/> representing the criterion predicate.</returns>
    public static implicit operator Expression<Func<object, bool>>(Criterion criterion) => criterion.GetExpression();

    private object? GetValue(object? candidate)
    {
        return candidate is not null ? GetValue(Type, Property).Compile().Invoke(candidate) : null;
    }

    public override string ToString() => GetExpression().ToReadableString();

    public bool Equals(Criterion? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Type == other.Type
               && Property.Equals(other.Property)
               && Operation.Equals(other.Operation)
               && Arguments.Equals(other.Arguments);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((Criterion) obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Type, Property, Operation, Arguments);
    }

    private Expression<Func<object, bool>> GetExpression()
    {
        var parameter = Expression.Parameter(typeof(object), "x");

        var converted = Expression.TypeAs(parameter, Type);
        var member = GetMember(converted, Property);

        var constants = Arguments.Select(v => Expression.TypeAs(Expression.Constant(v), typeof(object)))
            .Cast<Expression>().ToArray();
        var arguments = Expression.NewArrayInit(typeof(object), constants);

        var method = typeof(Operation).GetMethod("Evaluate")!;
        var instance = Expression.Constant(Operation);
        var evaluate = Expression.Call(instance, method, member, arguments);

        return Expression.Lambda<Func<object, bool>>(evaluate, parameter);
    }

    private Expression<Func<T, bool>> GetExpression<T>(ParameterExpression parameter, Expression member)
    {
        var constants = Arguments.Select(a => Expression.TypeAs(Expression.Constant(a), typeof(object)))
            .Cast<Expression>().ToArray();
        var arguments = Expression.NewArrayInit(typeof(object), constants);

        var method = typeof(Operation).GetMethod("Evaluate")!;
        var instance = Expression.Constant(Operation);
        var evaluate = Expression.Call(instance, method, member, arguments);

        return Expression.Lambda<Func<T, bool>>(evaluate, parameter);
    }

    private static Expression<Func<object, object?>> GetValue(Type type, string property)
    {
        var parameter = Expression.Parameter(typeof(object), "x");
        var converted = Expression.TypeAs(parameter, type);
        var member = GetMember(converted, property);
        return Expression.Lambda<Func<object, object?>>(member, parameter);
    }

    /// <summary>
    /// Recursively gets the member expression for the provided property name. This will also add null checks for
    /// each nested member to prevent null reference exceptions, and to allow null to be propagated through the
    /// member expression and returns to the operation's evaluation method.
    /// </summary>
    /// <param name="parameter">The current member access expression for the type.</param>
    /// <param name="property">The current property name to create member access to.</param>
    /// <returns>An <see cref="Expression{TDelegate}"/> that represents member access to a immediate or nested/complex
    /// member property or field, with corresponding conditional null checks for each member level.</returns>
    private static Expression GetMember(Expression parameter, string property)
    {
        if (!property.Contains('.'))
            return Expression.TypeAs(Expression.PropertyOrField(parameter, property), typeof(object));

        var index = property.IndexOf('.');
        var member = Expression.PropertyOrField(parameter, property[..index]);
        var notNull = Expression.NotEqual(member, Expression.Constant(null));
        return Expression.Condition(notNull, GetMember(member, property[(index + 1)..]), Expression.Constant(null),
            typeof(object));
    }
}