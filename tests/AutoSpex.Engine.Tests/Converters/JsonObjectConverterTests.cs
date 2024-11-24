using System.Text;
using System.Text.Json;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine.Tests.Converters;

[TestFixture]
public class JsonObjectConverterTests
{
    private readonly JsonSerializerOptions _options = new() { Converters = { new JsonObjectConverter() } };

    [Test]
    public Task Serialize_NullValue_ShouldBeVerified()
    {
        object? value = null;

        var json = WriteJson(value);

        return VerifyJson(json);
    }

    [Test]
    public Task Serialize_BooleanValue_ShouldBeVerified()
    {
        const bool value = true;

        var json = WriteJson(value);

        return VerifyJson(json);
    }

    [Test]
    public Task Serialize_NumberValue_ShouldBeVerified()
    {
        const int value = 123;

        var json = WriteJson(value);

        return VerifyJson(json);
    }

    [Test]
    public Task Serialize_EnumValue_ShouldBeVerified()
    {
        var value = Radix.Decimal;

        var json = WriteJson(value);

        return VerifyJson(json);
    }

    [Test]
    public Task Serialize_ElementValue_ShouldBeVerified()
    {
        var value = new Tag("MyTag", 123);

        var json = WriteJson(value);

        return VerifyJson(json);
    }

    [Test]
    public Task Serialize_NumberCollectionValue_ShouldBeVerified()
    {
        var value = new List<int> { 1, 2, 3, 3, 4 };

        var json = WriteJson(value);

        return VerifyJson(json);
    }

    [Test]
    public Task Serialize_TextCollectionValue_ShouldBeVerified()
    {
        var value = new List<string> { "First", "Second", "Third" };

        var json = WriteJson(value);

        return VerifyJson(json);
    }

    [Test]
    public Task Serialize_CriterionValue_ShouldBeVerified()
    {
        var value = new Criterion(Element.Tag.Property("TagName"), Operation.Containing, "this is a test value");

        var json = WriteJson(value);

        return VerifyJson(json);
    }
    
    [Test]
    public Task Serialize_RangeValue_ShouldBeVerified()
    {
        var value = new Range(1, 20);

        var json = WriteJson(value);

        return VerifyJson(json);
    }
    
    /*[Test]
    public Task Serialize_PropertyValue_ShouldBeVerified()
    {
        var value = Property.This(typeof(Tag)).GetProperty("Description");

        var json = WriteJson(value);

        return VerifyJson(json);
    }*/

    [Test]
    public void Deserialize_NullValue_ShouldBeVerified()
    {
        object? value = null;
        var json = WriteJson(value);

        var result = JsonSerializer.Deserialize<object?>(json, _options);

        result.Should().BeNull();
    }

    [Test]
    public void Deserialize_BooleanValue_ShouldBeVerified()
    {
        const bool value = true;
        var json = WriteJson(value);

        var result = JsonSerializer.Deserialize<object?>(json, _options);

        result.Should().Be(value);
    }

    [Test]
    public void Deserialize_NumberValue_ShouldBeVerified()
    {
        const int value = 123;
        var json = WriteJson(value);

        var result = JsonSerializer.Deserialize<object?>(json, _options);

        result.Should().Be(value);
    }

    [Test]
    public void Deserialize_EnumValue_ShouldBeVerified()
    {
        var value = Radix.Decimal;
        var json = WriteJson(value);

        var result = JsonSerializer.Deserialize<object?>(json, _options);

        result.Should().Be(value);
    }

    [Test]
    public void Deserialize_ElementValue_ShouldBeVerified()
    {
        var value = new Tag("MyTag", 123);
        var json = WriteJson(value);

        var result = JsonSerializer.Deserialize<object?>(json, _options);

        result.Should().BeEquivalentTo(value, o => o.Excluding(t => t.Root));
    }

    [Test]
    public void Deserialize_NumberCollectionValue_ShouldBeVerified()
    {
        var value = new List<object> { 1, 2, 3, 3, 4 };
        var json = WriteJson(value);

        var result = JsonSerializer.Deserialize<object>(json, _options);

        result.Should().BeEquivalentTo(value);
    }

    [Test]
    public void Deserialize_TextCollectionValue_ShouldBeVerified()
    {
        var value = new List<string> { "First", "Second", "Third" };
        var json = WriteJson(value);

        var result = JsonSerializer.Deserialize<object>(json, _options);

        result.Should().BeEquivalentTo(value);
    }

    [Test]
    public void Deserialize_CriterionValue_ShouldBeVerified()
    {
        var value = new Criterion(Element.Tag.Property("TagName"), Operation.Containing, "this is a test value");
        var json = WriteJson(value);

        var result = JsonSerializer.Deserialize<object>(json, _options);

        result.Should().BeEquivalentTo(value);
    }

    private static string WriteJson(object? value)
    {
        var converter = new JsonObjectConverter();
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        converter.Write(writer, value, JsonSerializerOptions.Default);

        writer.Flush();
        return Encoding.UTF8.GetString(stream.ToArray());
    }
}