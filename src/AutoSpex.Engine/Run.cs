using JetBrains.Annotations;

namespace AutoSpex.Engine;

[PublicAPI]
public record Run
{
    [UsedImplicitly]
    private Run()
    {
    }

    public Run(Runner runner, Source source, ICollection<Outcome> outcomes)
    {
        if (runner is null) throw new ArgumentNullException(nameof(runner));
        if (source is null) throw new ArgumentNullException(nameof(source));

        RunnerId = runner.RunnerId;
        Runner = runner.Name;
        SourceId = source.SourceId;
        Source = source.Name;
        Outcomes = outcomes ?? throw new ArgumentNullException(nameof(outcomes));
        Result = outcomes.Max(r => r.Result);
    }

    public Guid RunId { get; private set; } = Guid.NewGuid();
    public Guid RunnerId { get; private set; } = Guid.Empty;
    public Guid SourceId { get; private set; } = Guid.Empty;
    public ResultState Result { get; private set; } = ResultState.None;
    public DateTime Ran { get; private set; } = DateTime.Now;
    public string Runner { get; private set; } = string.Empty;
    public string Source { get; private set; } = string.Empty;
    public IEnumerable<Outcome> Outcomes { get; private set; } = [];
    public int Total => Outcomes.Count();
    public int Passed => Outcomes.Count(o => o.Result == ResultState.Passed);
    public int Failed => Outcomes.Count(o => o.Result == ResultState.Failed);
    public int Errored => Outcomes.Count(o => o.Result == ResultState.Error);
    public long Duration => Outcomes.Sum(o => o.Duration);
    public long Average => Duration / Total;
}