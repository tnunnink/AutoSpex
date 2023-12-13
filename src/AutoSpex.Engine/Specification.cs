using System.Linq.Expressions;
using AutoSpex.Engine.Operations;
using L5Sharp.Core;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine;

public class Specification
{
    private readonly Element _element;

    public Specification(Element element)
    {
        _element = element;
    }

    /// <summary>
    /// The <see cref="Func{T, TResult}"/> used to filter the results of the element query. This represents
    /// the second step of a common <c>Specification</c>. Once results are returns then we check the range and run
    /// the verification criterion.
    /// </summary>
    private Func<object, bool> _filter = _ => true;

    /// <summary>
    /// The <see cref="Criterion"/> used to check the range of the results of the filtered element query.
    /// </summary>
    private Func<int, bool> _range => i => i > 0;

    /// <summary>
    /// Holds a list of verifications.
    /// </summary>
    private readonly List<Criterion> _verifications = new();

    public Task<RunResult> Run(L5X file, RunConfig? config = default, CancellationToken token = default)
    {
        //todo handle cancellation and return inconclusive.
        return Task.Run(() =>
        {
            config ??= RunConfig.Default;

            //Query content
            var elements = _element.Query(file).ToList();
            var found = elements.Count;

            //Filter content
            var candidates = elements.Where(_filter).ToList();
            var targets = candidates.Count;

            //Evaluate range
            var range = _range(candidates.Count);

            //Generate verification results
            var results = _verifications.Select(v => new Verification(candidates.Select(v.Evaluate).ToList())).ToList();

            return RunResult.Process(found, targets, range, results, config);
        }, token);
    }

    public void UseFilter(Filter filter)
    {
        _filter = ((Expression<Func<object, bool>>) filter).Compile();
    }

    public void UseRange(Operation operation, params object[] values)
    {
        throw new NotImplementedException();
    }

    public void Verify(Criterion criterion)
    {
        _verifications.Add(criterion);
    }
}