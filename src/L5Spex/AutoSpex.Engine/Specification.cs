using System.Linq.Expressions;
using AutoSpex.Engine.Contracts;
using AutoSpex.Engine.Operations;
using L5Sharp.Core;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine;

public class Specification : ISpecification
{
    private readonly Type _type;

    public Specification(Type type)
    {
        _type = type;
        _query = c => c.Query(_type).ToList();
    }

    private readonly Func<L5X, IEnumerable<object>> _query;

    /// <summary>
    /// The <see cref="Func{T, TResult}"/> used to filter the results of the <see cref="_query"/>. This represents
    /// the second step of a common <c>Specification</c>. Once results are returns then we check the range and run
    /// the verification criterion.
    /// </summary>
    private Func<object, bool> _filter = _ => true;

    /// <summary>
    /// The <see cref="Criterion"/> used to check the range of the results of the filtered <see cref="_query"/>.
    /// </summary>
    private Criterion _range = Criterion.For<List<object>>("Count", Operation.GreaterThan, 0);

    /// <summary>
    /// 
    /// </summary>
    private readonly List<Criterion> _verifications = new();


    public Task<RunResult> Run(L5X file, RunConfig? config = default, CancellationToken token = default)
    {
        //todo handle cancellation and return inconclusive.
        return Task.Run(() =>
        {
            config ??= RunConfig.Default;

            //Query content
            var elements = _query(file).ToList();
            var total = elements.Count;

            //Filter content
            var candidates = elements.Where(_filter).ToList();
            var targets = candidates.Count;

            //Evaluate range
            var range = _range.Evaluate(candidates);

            //Generate verification results
            var results = _verifications.Select(v => new Verification(candidates.Select(v.Evaluate).ToList())).ToList();

            return RunResult.Process(range, results, config, total, targets);
        }, token);
    }

    public void ApplyFilter(Criterion criterion)
    {
        if (criterion is null)
            throw new ArgumentNullException(nameof(criterion));

        _filter = criterion.Compile();
    }

    public void ApplyFilter(IEnumerable<Criterion> filters, ChainType chainType)
    {
        if (filters is null)
            throw new ArgumentNullException(nameof(filters));

        _filter = filters.Aggregate(Criterion.All(), (current, filter) => current.Chain(filter, chainType)).Compile();
    }

    public void ApplyFilter(Expression<Func<object, bool>> expression)
    {
        if (expression is null)
            throw new ArgumentNullException(nameof(expression));

        _filter = expression.Compile();
    }

    public void ApplyRange(Operation operation, params object[] arguments)
    {
        if (operation is null) throw new ArgumentNullException(nameof(operation));
        if (arguments is null) throw new ArgumentNullException(nameof(arguments));
        _range = Criterion.For<List<object>>("Count", operation, arguments);
    }

    public void AddVerification(Criterion criterion)
    {
        _verifications.Add(criterion);
    }

    public void AddVerifications(IEnumerable<Criterion> filters)
    {
        _verifications.AddRange(filters);
    }
}