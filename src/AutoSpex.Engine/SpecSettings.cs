using System.Text.Json.Serialization;
using Ardalis.SmartEnum.SystemTextJson;
using JetBrains.Annotations;

namespace AutoSpex.Engine;

[PublicAPI]
public class SpecSettings
{
    public static SpecSettings Default => new();
    public bool VerifyCount { get; set; }

    [JsonConverter(typeof(SmartEnumValueConverter<Operation, string>))]
    public Operation CountOperation { get; set; } = Operation.GreaterThan;

    public int CountValue { get; set; }
    public Inclusion FilterInclusion { get; set; } = Inclusion.All;
    public Inclusion VerificationInclusion { get; set; } = Inclusion.All;
}