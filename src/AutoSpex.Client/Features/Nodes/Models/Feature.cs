using Ardalis.SmartEnum;
using Ardalis.SmartEnum.Dapper;

namespace AutoSpex.Client.Features.Nodes;

[IgnoreCase]
public class Feature : SmartEnum<Feature, int>
{
    private Feature(string name, int value) : base(name, value)
    {
    }
    
    public static readonly Feature Specifications = new(nameof(Specifications), 1);
    public static readonly Feature Sources = new(nameof(Sources), 2);
}