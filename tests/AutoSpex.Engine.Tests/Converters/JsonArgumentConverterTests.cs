using System.Linq.Expressions;
using System.Text.Json;
using AutoSpex.Engine.Converters;
using FluentAssertions.Equivalency;

namespace AutoSpex.Engine.Tests.Converters;

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
        options.Converters.Add(new JsonTypeConverter());
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
        result.Should().BeEquivalentTo(argument, options => options.Excluding(a => a.ArgumentId));
    }

    [Test]
    public void RunSerializationForLogixAtomicTypeAndValidateResults()
    {
        var argument = new Argument(Radix.Decimal);
        
        var data = JsonSerializer.Serialize(argument, _options);
        data.Should().NotBeEmpty();

        var result = JsonSerializer.Deserialize(data, typeof(Argument), _options);
        result.Should().BeEquivalentTo(argument, options => options.Excluding(a => a.ArgumentId));
    }
    
    [Test]
    public void RunSerializationForCriterionAndValidateResults()
    {
        Expression<Func<IMemberInfo, bool>> exclude = m => m.Type == typeof(Guid);
        var criterion = new Criterion("SubProp", Operation.Equal, "Some Value");
        var argument = new Argument(criterion);
        
        var data = JsonSerializer.Serialize(argument, _options);
        data.Should().NotBeEmpty();

        var result = JsonSerializer.Deserialize(data, typeof(Argument), _options) as Argument;
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(argument, options => options.Excluding(exclude));
        result?.Type.Should().Be(typeof(Criterion));
        result?.Group.Should().Be(TypeGroup.Criterion);
        result?.Identifier.Should().Be(nameof(Criterion));
    }
}