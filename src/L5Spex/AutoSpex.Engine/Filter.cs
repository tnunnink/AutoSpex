using System.Linq.Expressions;
using AgileObjects.ReadableExpressions;

namespace AutoSpex.Engine;

public class Filter
{
    private readonly Expression<Func<object, bool>> _expression;

    private Filter(Expression<Func<object, bool>> expression)
    {
        _expression = expression;
    }

    public static Filter All() => new(PredicateBuilder.True<object>());
    
    public static Filter None() => new(PredicateBuilder.False<object>());
    
    public static Filter By(Criterion criterion)
    {
        return new Filter(criterion);
    }
    
    public Filter And(Criterion criterion)
    {
        var expression = _expression.Chain(criterion, ChainType.And);
        return new Filter(expression);
    }
    
    public Filter And(Filter filter)
    {
        var expression = _expression.Chain(filter, ChainType.And);
        return new Filter(expression);
    }
    
    public Filter Or(Criterion criterion)
    {
        var expression = _expression.Chain(criterion, ChainType.Or);
        return new Filter(expression);
    }
    
    public Filter Or(Filter filter)
    {
        var expression = _expression.Chain(filter, ChainType.Or);
        return new Filter(expression);
    }

    public Filter Chain(Criterion criterion, ChainType chainType)
    {
        var expression = _expression.Chain(criterion, chainType);
        return new Filter(expression);
    }
    
    public Filter Chain(Filter filter, ChainType chainType)
    {
        var expression = _expression.Chain(filter, chainType);
        return new Filter(expression);
    }

    /*public Filter Chain(IEnumerable<Tuple<Criterion, ChainType>> group, ChainType chainType)
    {
        var aggregate = group.Aggregate(PredicateBuilder.True<object>(),
            (current, filter) => current.Chain(filter.Item1, filter.Item2));

        _expression.Chain(aggregate, chainType);
        
        return new Filter(_expression);
    }*/

    public Func<object, bool> Compile() => _expression.Compile();
    
    public bool Passes(object candidate) => _expression.Compile()(candidate);

    public override string ToString() => _expression.ToReadableString();

    public static implicit operator Expression<Func<object, bool>>(Filter filter) => filter._expression;
    
    public static implicit operator Filter(Expression<Func<object, bool>> expression) => new(expression);
}