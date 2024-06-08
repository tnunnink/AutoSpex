using System.Collections.ObjectModel;

namespace AutoSpex.Engine;

/// <summary>
/// An object containing the resulting data from running a <see cref="Spec"/> against a given <see cref="Source"/>.
/// </summary>
public class Outcome
{
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
        Evaluations = new ReadOnlyCollection<Evaluation>(verifications.SelectMany(v => v.Evaluations).ToList());
    }

    public Outcome(string specName, string sourceName, ICollection<Evaluation> evaluations)
    {
        SpecName = specName;
        SourceName = sourceName;
        Result = evaluations.Max(e => e.Result);
        Evaluations = new ReadOnlyCollection<Evaluation>(evaluations.ToList());
    }

    public Guid OutcomeId { get; init; } = Guid.NewGuid();
    public Guid SpecId { get; init; } = Guid.Empty;
    public Guid SourceId { get; init; } = Guid.Empty;
    public string SpecName { get; init; }
    public string SourceName { get; init; }
    public ResultState Result { get; init; }
    public long Duration { get; init; }
    public IReadOnlyCollection<Evaluation> Evaluations { get; init; }

    public void Run(Source source, Spec spec)
    {
        
    }
}