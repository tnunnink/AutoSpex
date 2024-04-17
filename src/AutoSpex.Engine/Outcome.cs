using JetBrains.Annotations;

namespace AutoSpex.Engine;

[PublicAPI]
public class Outcome
{
    public Outcome(Guid specId, ICollection<Verification> verifications, long duration = 0)
    {
        SpecId = specId;
        Duration = duration;
        Result = verifications.Count > 0 ? verifications.Max(v => v.Result) : ResultState.Passed;
        Total = verifications.Count;
        Passed = verifications.Count(v => v.Result == ResultState.Passed);
        Failed = verifications.Count(v => v.Result == ResultState.Failed);
        Errored = verifications.Count(v => v.Result == ResultState.Error);
        Evaluations = verifications.SelectMany(v => v.Evaluations);
    }

    public Outcome(Guid outcomeId, Guid specId, ResultState result,
        long duration, int total, int passed, int failed, int errored, IEnumerable<Evaluation> evaluations)
    {
        OutcomeId = outcomeId;
        SpecId = specId;
        Result = result;
        Duration = duration;
        Total = total;
        Passed = passed;
        Failed = failed;
        Errored = errored;
        Evaluations = evaluations;
    }

    public Guid OutcomeId { get; } = Guid.NewGuid();
    public Guid SpecId { get; }
    public ResultState Result { get; }
    public long Duration { get; }
    public int Total { get; }
    public int Passed { get; }
    public int Failed { get; }
    public int Errored { get; }
    public IEnumerable<Evaluation> Evaluations { get; private set; }
}