using L5Sharp.Core;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine;

public class SpecRunner
{
    public L5X Source { get; }
    public Dictionary<string, object> Variables { get; } = [];
    public Specification Specification { get; }

    public Task<RunResult> Run(CancellationToken token = default)
    {
        //todo handle cancellation and return inconclusive.
        return Task.Run(() =>
        {
            //1.Query content
            var elements = Query().ToList();
            var targets = elements.Count;
            
            //2.Filter content
            var filtered = elements.Where(Filter).ToList();
            var candidates = filtered.Count;

            //3.Evaluate range
            var range = Specification.Range.Evaluate(filtered.Count);

            //4.Verify candidates
            var verifications = filtered.Select(Verify).ToList();

            return RunResult.Process(targets, candidates, range, verifications, Specification.Options);
        }, token);
    }

    /// <summary>
    /// Executes a query using the specified source and returns the result as an enumerable collection of objects.
    /// </summary>
    /// <returns>
    /// An enumerable collection of objects that represents the result of the query.
    /// </returns>
    private IEnumerable<object> Query() => Specification.Element.Query(Source);

    private bool Filter(object candidate)
    {
        var evaluations = Specification.Filters.Select(f => Evaluate(f, candidate));

        return Specification.Options.FilterInclusion == InclusionType.All 
            ? evaluations.All(e => e.Result == ResultType.Passed) 
            : evaluations.Any(e => e.Result == ResultType.Passed);
    }

    private Verification Verify(object? candidate)
    {
        var evaluations = Specification.Verifications.Select(v => Evaluate(v, candidate));
        return new Verification(evaluations);
    }

    private Evaluation Evaluate(Criterion criterion, object? candidate)
    {
        try
        {
            var type = candidate?.GetType();
            var property = type?.Property(criterion.Property);
            var value = property?.Getter().Invoke(candidate) ?? candidate;
            var args = ResolveArguments(criterion.Arguments);
            
            var result = criterion.Operation.Execute(value, args);
            
            return result
                ? Evaluation.Passed(criterion, candidate, value)
                : Evaluation.Failed(criterion, candidate, value);
        }
        catch (Exception e)
        {
            return Evaluation.Error(criterion, candidate, e);
        }
    }

    private IEnumerable<object> ResolveArguments(List<Argument> arguments)
    {
        var values = new List<object>();

        foreach (var argument in arguments)
        {
            if (argument.Value is not Variable variable)
            {
                values.Add(argument.Value);
                continue;
            }

            if (Variables.TryGetValue(variable.Name, out var value))
            {
                values.Add(value);
                continue;
            }

            values.Add(variable.Default);
        }

        return values;
    }
}