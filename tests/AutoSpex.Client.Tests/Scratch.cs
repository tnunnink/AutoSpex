using System.Text.Json;
using Avalonia.Styling;
using FluentAssertions;

namespace AutoSpex.Client.Tests;

[TestFixture]
public class Scratch
{
    [Test]
    public void ScratchTest()
    {
        var uri = new Uri($"//MyRootName/SubPathPerhaps/{Guid.NewGuid()}");

        uri.Should().NotBeNull();
    }

    [Test]
    public void SerializeThemeVariantTest()
    {
        // Test serialization
        var theme = ThemeVariant.Dark.ToString();
        var json = JsonSerializer.Serialize(theme);
        Console.WriteLine($"Serialized: {json}");

        // Test deserialization
        var deserialized = JsonSerializer.Deserialize<string>(json);
        Console.WriteLine($"Deserialized: {deserialized}");
    }
}