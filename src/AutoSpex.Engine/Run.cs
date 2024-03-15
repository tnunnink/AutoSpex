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
        Result = outcomes.Max(r => r.Result);
        Verified = outcomes.Count;
        Passed = outcomes.Count(o => o.Result == ResultState.Passed);
        Failed = outcomes.Count(o => o.Result == ResultState.Failed);
        Errored = outcomes.Count(o => o.Result == ResultState.Error);
        Duration = outcomes.Sum(o => o.Duration);
        Average = Duration / Verified;
    }

    public Guid RunId { get; private set; } = Guid.NewGuid();
    public Guid RunnerId { get; private set; } = Guid.Empty;
    public Guid SourceId { get; private set; } = Guid.Empty;
    public string Runner { get; private set; } = string.Empty;
    public string Source { get; private set; } = string.Empty;
    public ResultState Result { get; private set; } = ResultState.None;

    public DateTime RanOn { get; private set; } = DateTime.Now;

    public string RanBy { get; private set; } = Environment.UserName;
    public int Verified { get; private set; }
    public int Passed { get; private set; }
    public int Failed { get; private set; }
    public int Errored { get; private set; }
    public long Duration { get; private set; }
    public long Average { get; private set; }
}