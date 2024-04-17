using JetBrains.Annotations;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine;

[PublicAPI]
public class Run
{
    private readonly List<Outcome> _outcomes = [];

    public Run()
    {
    }

    public Run(Guid nodeId, Guid sourceId)
    {
        NodeId = nodeId;
        SourceId
    }

    public Guid RunId { get; private set; } = Guid.NewGuid();
    public Guid NodeId { get; private set; } = Guid.Empty;
    public Guid SourceId { get; private set; } = Guid.Empty;
    public string Name { get; set; } = "Run";
    public ResultState Result { get; private set; } = ResultState.None;
    public DateTime RanOn { get; private set; } = DateTime.Now;
    public string RanBy { get; private set; } = Environment.UserName;
    public IReadOnlyCollection<Outcome> Outcomes => _outcomes;


    /// <summary>
    /// Executes the runner with the given input source.
    /// </summary>
    /// <param name="specs"></param>
    /// <param name="source"></param>
    /// <param name="progress">The progress object for tracking the execution progress. (optional)</param>
    /// <returns>The Run object that encapsulates the execution outcome.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the input source is null.</exception>
    public async Task Execute(IEnumerable<Spec> specs, Source source, IProgress<Outcome>? progress = default)
    {
        if (specs is null) throw new ArgumentNullException(nameof(specs));
        if (source is null) throw new ArgumentNullException(nameof(source));

        _outcomes.Clear();

        var content = source.L5X;

        foreach (var spec in specs)
        {
            var outcome = await spec.Run(content);
            progress?.Report(outcome);
            _outcomes.Add(outcome);
        }

        SourceId = source.SourceId;
        Result = _outcomes.Count > 0 ? _outcomes.Max(r => r.Result) : ResultState.None;
        RanOn = DateTime.Now;
        RanBy = Environment.UserName;
    }
}