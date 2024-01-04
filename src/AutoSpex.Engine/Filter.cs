using System.Linq.Expressions;

namespace AutoSpex.Engine;

public class Filter
{
    private readonly Expression<Func<object?, bool>> _expression;

    private Filter(Expression<Func<object?, bool>> expression)
    {
        _expression = expression;
    }

    /// <summary>
    /// Returns a filter that matches all elements.
    /// </summary>
    /// <returns>A filter that matches all elements.</returns>
    public static Filter All() => new(o => true);

    /// <summary>
    /// Creates a filter that always returns false for every input.
    /// </summary>
    /// <returns>A new instance of <see cref="Filter"/> with a lambda expression that always evaluates to false.</returns>
    public static Filter None() => new(o => false);
    
    public static Filter By(Expression<Func<object?, bool>> filter)
    {
        return new Filter(filter);
    }

    public static Filter By(Criterion criterion)
    {
        return new Filter(criterion);
    }

    public static Filter All(IEnumerable<Criterion> criteria)
    {
        return criteria.Aggregate(All(), (f, c) => f.Chain(c, ChainType.And));
    }

    public static Filter Any(IEnumerable<Criterion> criteria)
    {
        return criteria.Aggregate(None(), (f, c) => f.Chain(c, ChainType.Or));
    }

    public Filter And(Filter filter)
    {
        var expression = Chain(_expression, filter, ChainType.And);
        return new Filter(expression);
    }

    public Filter Or(Filter filter)
    {
        var expression = Chain(_expression, filter, ChainType.Or);
        return new Filter(expression);
    }

    public Filter Chain(Criterion criterion, ChainType chainType)
    {
        var expression = Chain(_expression, criterion, chainType);
        return new Filter(expression);
    }

    public Filter Chain(Filter filter, ChainType chainType)
    {
        var expression = Chain(_expression, filter, chainType);
        return new Filter(expression);
    }

    public Func<object, bool> Compile() => _expression.Compile();

    /// <summary>
    /// Implicitly converts a <see cref="Filter"/> object to an <see cref="Expression{TDelegate}"/> which represents
    /// a filter condition on objects.
    /// </summary>
    /// <param name="filter">The <see cref="Filter"/> object to be converted.</param>
    /// <returns>An <see cref="Expression{TDelegate}"/> object representing the filter condition.</returns>
    public static implicit operator Expression<Func<object?, bool>>(Filter filter) => filter._expression;

    /// <summary>
    /// Represents an implicit conversion operator that converts an expression of type <see cref="Expression{TDelegate}"/>
    /// to a <see cref="Filter"/> object.
    /// </summary>
    /// <param name="expression">The expression to convert.</param>
    /// <returns>A new instance of the Filter class.</returns>
    public static implicit operator Filter(Expression<Func<object?, bool>> expression) => new(expression);

    /// <summary>
    /// Chains two expressions together using the specified chain type.
    /// </summary>
    /// <typeparam name="T">The type of the parameter in the expression.</typeparam>
    /// <param name="left">The left expression to chain.</param>
    /// <param name="right">The right expression to chain.</param>
    /// <param name="chain">The type of the chain (And or Or).</param>
    /// <returns>An expression that represents the chained expressions.</returns>
    private static Expression<Func<T, bool>> Chain<T>(Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right, ChainType chain)
    {
        return chain == ChainType.And ? And(left, right) : Or(left, right);
    }

    /// <summary>
    /// Combines two expressions using the logical AND operator.
    /// </summary>
    /// <param name="left">The left expression.</param>
    /// <param name="right">The right expression.</param>
    /// <typeparam name="T">The type of the expression parameter.</typeparam>
    /// <returns>A new lambda expression that represents the logical AND of the input expressions.</returns>
    private static Expression<Func<T, bool>> And<T>(Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
    {
        return Compose(left, right, Expression.AndAlso);
    }

    /// <summary>
    /// Combines two lambda expressions using the logical OR operator.
    /// </summary>
    /// <param name="left">The first lambda expression.</param>
    /// <param name="right">The second lambda expression.</param>
    /// <typeparam name="T">The type of objects to be compared.</typeparam>
    /// <returns>A new lambda expression that represents the logical OR of the input expressions.</returns>
    private static Expression<Func<T, bool>> Or<T>(Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right)
    {
        return Compose(left, right, Expression.OrElse);
    }

    /// <summary>
    /// Negates the specified expression.
    /// </summary>
    /// <typeparam name="T">The type of elements being evaluated.</typeparam>
    /// <param name="expression">The expression to be negated.</param>
    /// <returns>The negated expression.</returns>
    private static Expression<Func<T, bool>> Not<T>(Expression<Func<T, bool>> expression)
    {
        var negated = Expression.Not(expression.Body);
        return Expression.Lambda<Func<T, bool>>(negated, expression.Parameters);
    }

    /// <summary>
    /// Composes two lambda expressions into a single lambda expression.
    /// </summary>
    /// <typeparam name="T">The type of the lambda expression.</typeparam>
    /// <param name="left">The left lambda expression.</param>
    /// <param name="right">The right lambda expression.</param>
    /// <param name="merge">A function that merges the body of the lambda expressions.</param>
    /// <returns>A new lambda expression resulting from the composition of the input expressions.</returns>
    private static Expression<T> Compose<T>(Expression<T> left, Expression<T> right,
        Func<Expression, Expression, Expression> merge)
    {
        // zip parameters (map from parameters of second to parameters of first)
        var map = left.Parameters
            .Zip(right.Parameters, (l, r) => new
            {
                Left = l,
                Right = r
            })
            .ToDictionary(x => x.Right, x => x.Left);

        // replace parameters in the second lambda expression with the parameters in the first
        var secondBody = ParameterRebinder.Rebind(right.Body, map);

        // create a merged lambda expression with parameters from the first expression
        return Expression.Lambda<T>(merge(left.Body, secondBody), left.Parameters);
    }

    private class ParameterRebinder : ExpressionVisitor
    {
        private readonly Dictionary<ParameterExpression, ParameterExpression> _map;

        private ParameterRebinder(Dictionary<ParameterExpression, ParameterExpression> map)
        {
            _map = map;
        }

        public static Expression Rebind(Expression expression,
            Dictionary<ParameterExpression, ParameterExpression> map)
        {
            return new ParameterRebinder(map).Visit(expression);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (_map.TryGetValue(node, out var replacement))
                node = replacement;

            return base.VisitParameter(node);
        }
    }

    /*private class MemberRebinder : ExpressionVisitor
    {
        private readonly Expression _replacement;

        private MemberRebinder(Expression replacement)
        {
            _replacement = replacement;
        }

        public static Expression Rebind(Expression expression, Expression replacement)
        {
            return new MemberRebinder(replacement).Visit(expression);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            node = Expression.MakeMemberAccess(null, node.Member);
            return base.VisitMember(node);
        }
    }*/
}