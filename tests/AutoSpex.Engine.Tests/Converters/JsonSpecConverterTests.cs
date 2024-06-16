using System.Text.Json;
using FluentAssertions.Equivalency;

namespace AutoSpex.Engine.Tests.Converters;

[TestFixture]
public class JsonSpecConverterTests
{
    private JsonSerializerOptions? _options;

    [SetUp]
    public void Setup()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new JsonSpecConverter());
        options.Converters.Add(new JsonCriterionConverter());
        options.Converters.Add(new JsonArgumentConverter());
        options.Converters.Add(new JsonTypeConverter());
        _options = options;
    }

    [Test]
    public void Serialize_ValidSpec_ShouldBeExpected()
    {
        var spec = new Spec
        {
            Element = Element.Controller,
            Filters =
            {
                new Criterion(Element.Controller.Property("Value"), Operation.Equal, "Test"),
                new Criterion(Element.Controller.Property("Processor"), Operation.EndsWith, "74"),
            },
            Verifications =
            {
                new Criterion(Element.Controller.Property("Test"), Operation.All,
                    new Criterion(Element.Controller.Property("SubProp"), Operation.In, 1, 2, 3, 4)),
                new Criterion(Element.Controller.Property("Another"), Operation.Between, 1, 5),
            }
        };

        var result = JsonSerializer.Serialize(spec, _options);

        result.Should().NotBeEmpty();
    }

    [Test]
    public void Deserialize_ValidSpec_ShouldBeExpected()
    {
        var spec = new Spec
        {
            Element = Element.Controller,
            Filters =
            {
                new Criterion(Element.Controller.Property("Value"), Operation.Equal, "Test"),
                new Criterion(Element.Controller.Property("Processor"), Operation.EndsWith, "74"),
            },
            Verifications =
            {
                new Criterion(Element.Controller.Property("Test"), Operation.All,
                    new Criterion(Element.Controller.Property("SubProp"), Operation.In, 1, 2, 3, 4)),
                new Criterion(Element.Controller.Property("Another"), Operation.Between, 1, 5),
            }
        };

        var data = JsonSerializer.Serialize(spec, _options);

        var result = JsonSerializer.Deserialize(data, typeof(Spec), _options);
        result.Should().BeEquivalentTo(spec, options => options.Excluding(m => m.Type == typeof(Guid)));
    }
}