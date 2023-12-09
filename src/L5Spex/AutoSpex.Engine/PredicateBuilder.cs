using System.Linq.Expressions;

namespace AutoSpex.Engine;

/// <summary>
/// Enables the efficient, dynamic composition of query predicates.
/// </summary>
public static class PredicateBuilder
{
    /// <summary>
    /// Combines the first predicate with the second using the logical "and".
    /// </summary>
    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
    {
        return left.Compose(right, Expression.AndAlso);
    }

    /// <summary>
    /// Combines the first predicate with the second using the logical "or".
    /// </summary>
    public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right)
    {
        return left.Compose(right, Expression.OrElse);
    }

    /// <summary>
    /// Negates the predicate.
    /// </summary>
    public static Expression<Func<T, bool>> Not<T>(this Expression<Func<T, bool>> expression)
    {
        var negated = Expression.Not(expression.Body);
        return Expression.Lambda<Func<T, bool>>(negated, expression.Parameters);
    }

    public static Expression<Func<T, bool>> Chain<T>(this Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right, ChainType chain)
    {
        return chain == ChainType.And ? left.And(right) : left.Or(right);
    }

    /*public static Expression<Func<TTo, bool>> Cast<TFrom, TTo>(this Expression<Func<TFrom, bool>> expression)
    {
        var map = expression.Parameters.ToDictionary(x => x, x => Expression.Parameter(typeof(TTo), x.Name));
        var body = ParameterRebinder.Rebind(expression.Body, map);
        var converted = Expression.TypeAs()
        return Expression.Lambda<Func<TTo, bool>>(body, map.Values);
    }*/

    /// <summary>
    /// Combines the first expression with the second using the specified merge function.
    /// </summary>
    private static Expression<T> Compose<T>(this Expression<T> left, Expression<T> right,
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

    private class MemberRebinder : ExpressionVisitor
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
    }
}