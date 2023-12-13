namespace AutoSpex.Engine;

public class RunResult
{
    private RunResult(ResultType result, int found, int filtered, bool inRange, IEnumerable<Verification> verifications)
    {
        Result = result;
        Found = found;
        Candidates = filtered;
        InRange = inRange;
        Verifications = verifications;
    }

    public ResultType Result { get; }
    public int Found { get; }
    public int Candidates { get; }
    public bool InRange { get; }
    public IEnumerable<Verification> Verifications { get; }

    public static RunResult Process(int found, int candidates, bool inRange, List<Verification> verifications,
        RunConfig config)
    {
        var result = inRange ? ResultType.Passed : ResultType.Failed;

        if (verifications.Count <= 0) return new RunResult(result, found, candidates, inRange, verifications);

        if (config is {VerificationInclusion: InclusionType.All, CandidateInclusion: InclusionType.All})
            result |= verifications.Max(v => v.Count > 0 ? v.Max(e => e.Result) : ResultType.None);

        if (config is {VerificationInclusion: InclusionType.All, CandidateInclusion: InclusionType.Any})
            result |= verifications.Max(v => v.Count > 0 ? v.Min(e => e.Result) : ResultType.None);

        if (config is {VerificationInclusion: InclusionType.Any, CandidateInclusion: InclusionType.All})
            result |= verifications.Min(v => v.Count > 0 ? v.Max(e => e.Result) : ResultType.None);

        if (config is {VerificationInclusion: InclusionType.Any, CandidateInclusion: InclusionType.Any})
            result |= verifications.Min(v => v.Count > 0 ? v.Min(e => e.Result) : ResultType.None);

        return new RunResult(result, found, candidates, inRange, verifications);
    }
}