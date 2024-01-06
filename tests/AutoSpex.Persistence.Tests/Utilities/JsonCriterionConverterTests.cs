using System.Text.Json;
using AutoSpex.Engine;
using FluentAssertions;

namespace AutoSpex.Persistence.Tests.Utilities;

public class JsonCriterionConverterTests
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
    public void RunSerializationForLogixAtomicTypeAndValidateResults()
    {
        var argument = new Criterion("Test", Operation.All, 1, 2, 3);
        
        var data = JsonSerializer.Serialize(argument, _options);
        data.Should().NotBeEmpty();

        var result = JsonSerializer.Deserialize(data, typeof(Criterion), _options);
        result.Should().BeEquivalentTo(argument);
    }
}