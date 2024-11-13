using System.Text.Json;

namespace AutoSpex.Engine.Tests.Converters;

[TestFixture]
public class JsonPropertyConverterTests
{
    private static readonly JsonSerializerOptions Options = new() { Converters = { new JsonPropertyConverter() } };

    [Test]
    public void Serialize_ValidProperty_ShouldBeVerified()
    {
        var property = Property.This(typeof(Tag)).GetProperty("Name");

        var json = JsonSerializer.Serialize(property, Options);

        json.Should().Be(string.Concat('"', property.Key, '"'));
    }

    [Test]
    public void Deserialize_ValidProperty_ShouldBeExpected()
    {
        var property = Property.This(typeof(Tag)).GetProperty("Name");

        var json = JsonSerializer.Serialize(property, Options);
        var result = JsonSerializer.Deserialize<Property>(json, Options);

        result.Should().BeEquivalentTo(property);
    }
}