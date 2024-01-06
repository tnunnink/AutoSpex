using System.Text.Json;
using FluentAssertions;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace AutoSpex.Engine.Tests;

[TestFixture]
public class Scratch
{
    [Test]
    public void Testing()
    {
        var uri = new Uri("Project Name/Some Folder/Sub folder/My Specification Name");

        uri.Should().NotBeNull();
    }

    [Test]
    public void TypeTests()
    {
        var type = Type.GetType("System.String");

        type.Should().Be(typeof(string));
    }

    [Test]
    [TestCase("Test", typeof(string))]
    [TestCase(true, typeof(bool))]
    [TestCase(123, typeof(int))]
    [TestCase(1.23, typeof(double))]
    public void Serialization(object input, Type type)
    {
        var value = JsonSerializer.Serialize(input);
        value.Should().NotBeEmpty();

        var data = JsonSerializer.Deserialize(value, type);
        data.Should().Be(input);
    }

    [Test]
    public void Serialization_Complex()
    {
        var criterion = new Criterion("Test.Property", Operation.Contains, "some_value", "And perhaps another");

        var options = new JsonSerializerOptions();
        options.Converters.Add(new JsonTypeConverter());
        options.Converters.Add(new JsonCriterionConverter());

        var value = JsonSerializer.Serialize(criterion, options);
        value.Should().NotBeEmpty();

        /*var data = JsonSerializer.Deserialize(value, typeof(Criterion));
        data.Should().BeEquivalentTo(criterion);*/
    }
}