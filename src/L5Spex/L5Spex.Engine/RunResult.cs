using L5Spex.Engine.Enumerations;

namespace L5Spex.Engine;

public class RunResult
{
    private RunResult(Evaluation range, IEnumerable<Verification> verifications, RunConfig config, int total,
        int filtered, ResultType result)
    {
        Verifications = verifications;
        Config = config;
        Total = total;
        Candidates = filtered;
        Result = result;
        Range = range;
    }

    public Guid Id { get; } = Guid.NewGuid();

    public DateTime RanAt { get; } = DateTime.Now;
    public ResultType Result { get; }
    public RunConfig Config { get; }
    public int Total { get; }
    public int Candidates { get; }
    public Evaluation Range { get; }
    public IEnumerable<Verification> Verifications { get; }

    public static RunResult Process(Evaluation range, List<Verification> verifications, RunConfig config, int total, int filtered)
    {
        var result = range.Result;

        if (verifications.Count <= 0) return new RunResult(range, verifications, config, total, filtered, result);
        
        if (config is {VerificationInclusion: InclusionType.All, ResultInclusion: InclusionType.All})
            result |= verifications.Max(v => v.Count > 0 ? v.Max(e => e.Result) : ResultType.None);
        
        if (config is {VerificationInclusion: InclusionType.All, ResultInclusion: InclusionType.Any})
            result |= verifications.Max(v => v.Count > 0 ? v.Min(e => e.Result) : ResultType.None);
        
        if (config is {VerificationInclusion: InclusionType.Any, ResultInclusion: InclusionType.All})
            result |= verifications.Min(v => v.Count > 0 ? v.Max(e => e.Result) : ResultType.None);
        
        if (config is {VerificationInclusion: InclusionType.Any, ResultInclusion: InclusionType.Any})
            result |= verifications.Min(v => v.Count > 0 ? v.Min(e => e.Result) : ResultType.None);
        
        return new RunResult(range, verifications, config, total, filtered, result);
    }
}