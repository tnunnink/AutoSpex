using Ardalis.SmartEnum;

namespace AutoSpex.Persistence;

public class CriterionUsage : SmartEnum<CriterionUsage, int>
{
    private CriterionUsage(string name, int value) : base(name, value)
    {
    }
    
    public static readonly CriterionUsage None = new(nameof(None), 0);
    public static readonly CriterionUsage Filter = new(nameof(Filter), 1);
    public static readonly CriterionUsage Verification = new(nameof(Verification), 2);
    public static readonly CriterionUsage Range = new(nameof(Range), 3);
}