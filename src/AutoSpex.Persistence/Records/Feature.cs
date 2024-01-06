using Ardalis.SmartEnum;

namespace AutoSpex.Persistence;

public class Feature : SmartEnum<Feature, int>
{
    private Feature(string name, int value) : base(name, value)
    {
    }
    
    public static readonly Feature Specifications = new(nameof(Specifications), 1);
    public static readonly Feature Sources = new(nameof(Sources), 2);
    public static readonly Feature Runners = new(nameof(Runners), 3);
}