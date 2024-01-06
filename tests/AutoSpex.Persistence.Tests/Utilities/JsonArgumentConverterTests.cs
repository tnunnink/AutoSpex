using System.Text.Json;
using AutoSpex.Engine;
using FluentAssertions;
using L5Sharp.Core;
using Argument = AutoSpex.Engine.Argument;

namespace AutoSpex.Persistence.Tests.Utilities;

[TestFixture]
public class JsonArgumentConverterTests
{
    private JsonSerializerOptions? _options;

    [SetUp]
    public void Setup()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new JsonCriterionConverter());
        options.Converters.Add(new JsonArgumentConverter());
        _options = options;
    }
    
    [Test]
    [TestCase(true, typeof(bool))]
    [TestCase(123, typeof(int))]
    [TestCase(1.23, typeof(double))]
    [TestCase("This is a test", typeof(string))]
    public void RunSerializationForPrimitiveTypesAndValidateResults(object value, Type type)
    {
        var argument = new Argument(value);

        var data = JsonSerializer.Serialize(argument, _options);
        data.Should().NotBeEmpty();

        var result = JsonSerializer.Deserialize(data, typeof(Argument), _options);
        result.Should().BeEquivalentTo(argument);
    }

    [Test]
    public void RunSerializationForLogixAtomicTypeAndValidateResults()
    {
        var argument = new Argument(Radix.Decimal);
        
        var data = JsonSerializer.Serialize(argument, _options);
        data.Should().NotBeEmpty();

        var result = JsonSerializer.Deserialize(data, typeof(Argument), _options);
        result.Should().BeEquivalentTo(argument);
    }
    
    [Test]
    public void RunSerializationForCriterionAndValidateResults()
    {
        var argument = new Argument(new Criterion("SubProp", Operation.Equal, "Some Value"));
        
        var data = JsonSerializer.Serialize(argument, _options);
        data.Should().NotBeEmpty();

        var result = JsonSerializer.Deserialize(data, typeof(Argument), _options);
        result.Should().BeEquivalentTo(argument);
    }
}