using L5Sharp.Core;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine;

public class Runner
{
    private readonly L5X _source;

    public Runner(L5X source)
    {
        _source = source;
    }

    public RunConfig Config { get; set; } = RunConfig.Default;

    public Task<RunResult> Run(Specification specification, CancellationToken token = default)
    {
        //todo handle cancellation and return inconclusive.
        return Task.Run(() =>
        {
            //Query content
            var elements = specification.Element.Query(_source).ToList();
            var found = elements.Count;

            //Filter content
            var filter = Config.CandidateInclusion == InclusionType.All
                ? Filter.All(specification.Filters).Compile()
                : Filter.Any(specification.Filters).Compile();
            var candidates = elements.Where(filter).ToList();
            var targets = candidates.Count;

            //Evaluate range
            var range = specification.Range.Evaluate(candidates.Count);

            //Generate verification results
            var results = specification.Verifications
                .Select(v => new Verification(candidates.Select(v.Evaluate).ToList())).ToList();

            return RunResult.Process(found, targets, range, results, Config);
        }, token);
    }
}