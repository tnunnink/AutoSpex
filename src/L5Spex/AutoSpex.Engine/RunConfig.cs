namespace AutoSpex.Engine;

public class RunConfig
{
    public bool Enabled { get; set; } = true;
    public bool RunToEnd { get; set; } = true;
    public bool StopOnFirstFailure { get; set; } = false;
    public InclusionType ResultInclusion { get; set; } = InclusionType.All;
    public InclusionType VerificationInclusion { get; set; } = InclusionType.All;

    public static RunConfig Default => new();
}