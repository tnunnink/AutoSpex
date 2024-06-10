namespace AutoSpex.Engine;

/// <summary>
/// An object containing the resulting data from running a <see cref="Spec"/> against a given <see cref="Source"/>.
/// </summary>
public class Outcome : IEquatable<Outcome>
{
    private List<Evaluation> _evaluations;

    private Outcome(Guid specId, string specName, Guid sourceId, string sourceName)
    {
        SpecId = specId;
        SpecName = specName;
        SourceId = sourceId;
        SourceName = sourceName;
        _evaluations = Enumerable.Empty<Evaluation>().ToList();
    }

    public Outcome(Spec spec, Source source, ICollection<Verification> verifications, long duration)
    {
        if (spec is null) throw new ArgumentNullException(nameof(spec));
        if (source is null) throw new ArgumentNullException(nameof(source));

        OutcomeId = Guid.NewGuid();
        SpecId = spec.SpecId;
        SpecName = spec.Name;
        SourceId = source.SourceId;
        SourceName = source.Name;
        Result = verifications.Count > 0 ? verifications.Max(r => r.Result) : ResultState.None;
        Duration = duration;
        _evaluations = verifications.SelectMany(v => v.Evaluations).ToList();
    }

    public Outcome(string specName, string sourceName, ICollection<Evaluation> evaluations)
    {
        SpecName = specName;
        SourceName = sourceName;
        Result = evaluations.Max(e => e.Result);
        _evaluations = evaluations.ToList();
    }

    public Guid OutcomeId { get; init; } = Guid.NewGuid();
    public Guid SpecId { get; init; } = Guid.Empty;
    public Guid SourceId { get; private set; } = Guid.Empty;
    public string SpecName { get; init; }
    public string SourceName { get; private set; }
    public ResultState Result { get; private set; }
    public long Duration { get; private set; }
    public IEnumerable<Evaluation> Evaluations => _evaluations.AsReadOnly();

    public static Outcome Empty(Node spec) => new(spec.NodeId, spec.Name, Guid.Empty, string.Empty);
    public static Outcome Empty(Node spec, Node source) => new(spec.NodeId, spec.Name, source.NodeId, source.Name);

    public void UpdateSource(Node node)
    {
        SourceId = node.NodeId;
        SourceName = node.Name;
    }

    public async Task Run(Source source, Spec spec, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(spec);

        if (SourceId != source.SourceId)
            throw new ArgumentException("The provided source id does not match the Outcome source id.");

        if (SpecId != spec.SpecId)
            throw new ArgumentException("The provided spec id does not match the Outcome spec id.");

        var outcome = await spec.Run(source, token);
        Result = outcome.Result;
        Duration = outcome.Duration;
        _evaluations = [..outcome.Evaluations];
    }

    public bool Equals(Outcome? other) => OutcomeId.Equals(other?.OutcomeId);
    public override bool Equals(object? obj) => obj is Outcome outcome && Equals(outcome);
    public override int GetHashCode() => OutcomeId.GetHashCode();
}