using JetBrains.Annotations;

namespace AutoSpex.Engine;

/// <summary>
/// An object containing the resulting data from running a <see cref="Spec"/> against a given source L5X.
/// </summary>
[PublicAPI]
public class Outcome
{
    /// <summary>
    /// Creates an empty <see cref="Outcome"/> containing the provided spec id.
    /// </summary>
    /// <param name="specId">The <see cref="Guid"/> of the spec this outcome represents.</param>
    /// <remarks>
    /// A <see cref="Outcome"/> has a one-to-one relationship with a spec node. Allowing instantiated outcomes
    /// with just the spec id will allow us to pass in default outcomes to be used to know which specs to run.
    /// </remarks>
    public Outcome(Guid specId)
    {
    }
    
    /// <summary>
    /// Creates a new <see cref="Outcome"/> instance for the provided spec id and resulting collection of verifications.
    /// </summary>
    /// <param name="spec">The <see cref="Spec"/> that produced this outcome.</param>
    /// <param name="verifications">The collection of <see cref="Verification"/> that resulted from running the spec.</param>
    /// <param name="duration">The duration in milliseconds it took to run the spec.</param>
    public Outcome(Spec spec, ICollection<Verification> verifications, long duration = 0)
    {
        SpecId = spec.SpecId;
        SpecName = spec.Name;
        Duration = duration;
        Result = verifications.Count > 0 ? verifications.Max(v => v.Result) : ResultState.Passed;
        Total = verifications.Count;
        Passed = verifications.Count(v => v.Result == ResultState.Passed);
        Failed = verifications.Count(v => v.Result == ResultState.Failed);
        Errored = verifications.Count(v => v.Result == ResultState.Error);
        Evaluations = verifications.SelectMany(v => v.Evaluations).ToList();
    }

    public Guid OutcomeId { get; } = Guid.NewGuid();
    public Guid SpecId { get; } = Guid.Empty;
    public string SpecName { get; } = string.Empty;
    public ResultState Result { get; } = ResultState.None;
    public long Duration { get; }
    public int Total { get; }
    public int Passed { get; }
    public int Failed { get; }
    public int Errored { get; }
    public List<Evaluation> Evaluations { get; } = [];
}