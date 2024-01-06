namespace AutoSpex.Engine;

public class SpecificationOptions
{
    public static SpecificationOptions Default => new();
    
    public bool RunToEnd { get; set; } = true;

    public InclusionType FilterInclusion { get; set; } = InclusionType.All;
    public InclusionType CandidateInclusion { get; set; } = InclusionType.All;
    public InclusionType VerificationInclusion { get; set; } = InclusionType.All;
}